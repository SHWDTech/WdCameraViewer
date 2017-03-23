using System;
using System.Drawing;
using System.Resources;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using WdVedioNet;

namespace WdCameraViewer
{
    [Guid("16eb10ad-b459-43a2-8d25-f8c5c2fb3a35"), ComVisible(true)]
    public partial class PreviewBox : UserControl, IObjectSafety
    {
        public PreviewBox()
        {
            InitializeComponent();
            cameraViewer.Paint += WdCameraViewerPaint;
            Disposed += OnDiposed;
        }

        private bool _sdkInited;

        private readonly ResourceManager _resource = new ResourceManager(typeof(PreviewBox));

        private string _displayMessage = string.Empty;

        private readonly StringFormat _displayStringFormat = new StringFormat
        {
            LineAlignment = StringAlignment.Center,
            Alignment = StringAlignment.Center
        };

        private readonly Brush _displayBrush = Brushes.MediumSpringGreen;

        private string _dvrAddress = string.Empty;

        private ushort _dvrPortNumber = 8000;

        private uint _lastError;

        private int _loginUserId;

        private uint _dwAChanTotalNum;

        private uint _dwDChanTotalNum;

        private int _mLRealHandle = -1;

        private readonly int[] _iIpDevId = new int[96];

        private readonly int[] _iChannelNum = new int[96];

        private CHCNetSDK.NET_DVR_DEVICEINFO_V30 _deviceInfo;

        private CHCNetSDK.NET_DVR_IPPARACFG_V40 _struIpParaCfgV40;

        private CHCNetSDK.NET_DVR_IPCHANINFO _struChanInfo;

        private CHCNetSDK.NET_DVR_IPCHANINFO_V40 _struChanInfoV40;

        private CameraLogin _cameraLogin;

        private static readonly RSACryptoServiceProvider RsaCryptoServiceProvider = new RSACryptoServiceProvider();

        #region IObjectSafety 成员
        private const string IidIDispatch = "{00020400-0000-0000-C000-000000000046}";
        private const string IidIDispatchEx = "{a6ef9860-c720-11d0-9337-00a0c90dcaa9}";
        private const string IidIPersistStorage = "{0000010A-0000-0000-C000-000000000046}";
        private const string IidIPersistStream = "{00000109-0000-0000-C000-000000000046}";
        private const string IidIPersistPropertyBag = "{37D84F60-42CB-11CE-8135-00AA004BB851}";

        private const int InterfacesafeForUntrustedCaller = 0x00000001;
        private const int InterfacesafeForUntrustedData = 0x00000002;
        private const int SOk = 0;
        private const int EFail = unchecked((int)0x80004005);
        private const int ENointerface = unchecked((int)0x80004002);

        private readonly bool _fSafeForScripting = true;
        private readonly bool _fSafeForInitializing = true;

        // ReSharper disable once RedundantAssignment
        public int GetInterfaceSafetyOptions(ref Guid riid, ref int pdwSupportedOptions, ref int pdwEnabledOptions)
        {
            int rslt;

            var strGuid = riid.ToString("B");
            pdwSupportedOptions = InterfacesafeForUntrustedCaller | InterfacesafeForUntrustedData;
            switch (strGuid)
            {
                case IidIDispatch:
                case IidIDispatchEx:
                    rslt = SOk;
                    pdwEnabledOptions = 0;
                    if (_fSafeForScripting)
                        pdwEnabledOptions = InterfacesafeForUntrustedCaller;
                    break;
                case IidIPersistStorage:
                case IidIPersistStream:
                case IidIPersistPropertyBag:
                    rslt = SOk;
                    pdwEnabledOptions = 0;
                    if (_fSafeForInitializing)
                        pdwEnabledOptions = InterfacesafeForUntrustedData;
                    break;
                default:
                    rslt = ENointerface;
                    break;
            }

            return rslt;
        }

        public int SetInterfaceSafetyOptions(ref Guid riid, int dwOptionSetMask, int dwEnabledOptions)
        {
            var rslt = EFail;
            var strGuid = riid.ToString("B");
            switch (strGuid)
            {
                case IidIDispatch:
                case IidIDispatchEx:
                    if (((dwEnabledOptions & dwOptionSetMask) == InterfacesafeForUntrustedCaller) && _fSafeForScripting)
                        rslt = SOk;
                    break;
                case IidIPersistStorage:
                case IidIPersistStream:
                case IidIPersistPropertyBag:
                    if (((dwEnabledOptions & dwOptionSetMask) == InterfacesafeForUntrustedData) && _fSafeForInitializing)
                        rslt = SOk;
                    break;
                default:
                    rslt = ENointerface;
                    break;
            }

            return rslt;
        }

        #endregion

        /// <summary>
        /// 解析摄像头连接信息
        /// </summary>
        /// <param name="paramsStrig"></param>
        /// <returns></returns>
        public void TryLogin(string paramsStrig)
        {
            try
            {
                SdkInit();
                var oringinal = Encoding.UTF8.GetString(Convert.FromBase64String(paramsStrig));
                var jsonParams = DecryptString(oringinal);
                _cameraLogin = XmlSerializerHelper.DeSerialize<CameraLogin>(jsonParams);
                CameraLogin();
            }
            catch (Exception)
            {
                DisplayMessage("解析连接信息失败。");
            }
        }

        /// <summary>
        /// 初始化海康SDK
        /// </summary>
        private void SdkInit()
        {
            DisplayMessage("正在初始化控件。");
            var privateKey = _resource.GetString("RsaPrivateKey");
            if (string.IsNullOrEmpty(privateKey))
            {
                DisplayMessage("未找到资源文件，控件初始化失败。");
                return;
            }
            RsaCryptoServiceProvider.FromXmlString(privateKey);
            _sdkInited = CHCNetSDK.NET_DVR_Init();
            if (!_sdkInited)
            {
                DisplayMessage("控件初始化失败！");
                return;
            }

            DisplayMessage("控件初始化完成。");
        }

        /// <summary>
        /// 显示提示信息
        /// </summary>
        /// <param name="message"></param>
        private void DisplayMessage(string message)
        {
            _displayMessage = message;
            cameraViewer.Invalidate();
        }

        /// <summary>
        /// PICTUREBOX控件响应PAINT事件显示问题提示。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WdCameraViewerPaint(object sender, PaintEventArgs e)
        {
            if (string.IsNullOrEmpty(_displayMessage)) return;
            using (var myFont = new Font("simsun", 14))
            {
                e.Graphics.DrawString(_displayMessage, myFont, _displayBrush, cameraViewer.ClientRectangle, _displayStringFormat);
            }
            _displayMessage = string.Empty;
        }

        private void CameraLogin()
        {
            var ipAddress = new byte[16];
            uint dwPort = 0;
            if (
                !CHCNetSDK.NET_DVR_GetDVRIPByResolveSvr_EX(_cameraLogin.IpServerAddr, 7071, null, 0,
                    _cameraLogin.DomainBytes, (ushort)_cameraLogin.DomainBytes.Length, ipAddress, ref dwPort))
            {
                _lastError = CHCNetSDK.NET_DVR_GetLastError();
                DisplayMessage($"域名解析失败，错误号：{_lastError}");
                return;
            }

            _dvrAddress = Encoding.UTF8.GetString(ipAddress).TrimEnd('\0');
            _dvrPortNumber = (ushort)dwPort;
            _loginUserId = CHCNetSDK.NET_DVR_Login_V30(_dvrAddress, _dvrPortNumber, _cameraLogin.User, _cameraLogin.Password, ref _deviceInfo);

            if (_loginUserId < 0)
            {
                _lastError = CHCNetSDK.NET_DVR_GetLastError();
                DisplayMessage($"用户登录失败，错误号：{_lastError}");
                return;
            }
            DisplayMessage("用户登录成功，可以开始预览。");
            _dwAChanTotalNum = _deviceInfo.byChanNum;
            _dwDChanTotalNum = _deviceInfo.byIPChanNum + (256 * (uint)_deviceInfo.byHighDChanNum);

            InfoIpChannel();
        }

        private void InfoIpChannel()
        {
            var dwSize = (uint)Marshal.SizeOf(_struIpParaCfgV40);

            var ptrIpParaCfgV40 = Marshal.AllocHGlobal((int)dwSize);
            Marshal.StructureToPtr(_struIpParaCfgV40, ptrIpParaCfgV40, false);

            uint dwReturn = 0;
            const int iGroupNo = 0; //该Demo仅获取第一组64个通道，如果设备IP通道大于64路，需要按组号0~i多次调用NET_DVR_GET_IPPARACFG_V40获取

            if (!CHCNetSDK.NET_DVR_GetDVRConfig(_loginUserId, CHCNetSDK.NET_DVR_GET_IPPARACFG_V40, iGroupNo, ptrIpParaCfgV40, dwSize, ref dwReturn))
            {
                _lastError = CHCNetSDK.NET_DVR_GetLastError();
                DisplayMessage($"获取录像机通道信息失败。错误码{_lastError}");
                return;
            }
            _struIpParaCfgV40 = (CHCNetSDK.NET_DVR_IPPARACFG_V40)Marshal.PtrToStructure(ptrIpParaCfgV40, typeof(CHCNetSDK.NET_DVR_IPPARACFG_V40));

            for (var i = 0; i < _dwAChanTotalNum; i++)
            {
                _iChannelNum[i] = i + _deviceInfo.byStartChan;
            }

            uint iDChanNum = 64;

            if (_dwDChanTotalNum < 64)
            {
                iDChanNum = _dwDChanTotalNum; //如果设备IP通道小于64路，按实际路数获取
            }

            for (var i = 0; i < iDChanNum; i++)
            {
                _iChannelNum[i + _dwAChanTotalNum] = i + (int)_struIpParaCfgV40.dwStartDChan;
                var byStreamType = _struIpParaCfgV40.struStreamMode[i].byGetStreamType;

                dwSize = (uint)Marshal.SizeOf(_struIpParaCfgV40.struStreamMode[i].uGetStream);
                switch (byStreamType)
                {
                    //目前NVR仅支持直接从设备取流 NVR supports only the mode: get stream from device directly
                    case 0:
                        var ptrChanInfo = Marshal.AllocHGlobal((int)dwSize);
                        Marshal.StructureToPtr(_struIpParaCfgV40.struStreamMode[i].uGetStream, ptrChanInfo, false);
                        _struChanInfo = (CHCNetSDK.NET_DVR_IPCHANINFO)Marshal.PtrToStructure(ptrChanInfo, typeof(CHCNetSDK.NET_DVR_IPCHANINFO));

                        _iIpDevId[i] = _struChanInfo.byIPID + (_struChanInfo.byIPIDHigh * 256) - (iGroupNo * 64) - 1;

                        Marshal.FreeHGlobal(ptrChanInfo);
                        break;
                    case 6:
                        var ptrChanInfoV40 = Marshal.AllocHGlobal((int)dwSize);
                        Marshal.StructureToPtr(_struIpParaCfgV40.struStreamMode[i].uGetStream, ptrChanInfoV40, false);
                        _struChanInfoV40 = (CHCNetSDK.NET_DVR_IPCHANINFO_V40)Marshal.PtrToStructure(ptrChanInfoV40, typeof(CHCNetSDK.NET_DVR_IPCHANINFO_V40));

                        _iIpDevId[i] = _struChanInfoV40.wIPID - (iGroupNo * 64) - 1;

                        Marshal.FreeHGlobal(ptrChanInfoV40);
                        break;
                }
            }
            Marshal.FreeHGlobal(ptrIpParaCfgV40);
        }

        public void Preview()
        {
            var lpPreviewInfo = new CHCNetSDK.NET_DVR_PREVIEWINFO
            {
                hPlayWnd = cameraViewer.Handle,
                lChannel = _iChannelNum[0],
                dwStreamType = 0,
                dwLinkMode = 0,
                bBlocked = true,
                dwDisplayBufNum = 15
            };
            //播放库显示缓冲区最大帧数

            var pUser = IntPtr.Zero;//用户数据 user data 

            //打开预览 Start live view 
            _mLRealHandle = CHCNetSDK.NET_DVR_RealPlay_V40(_loginUserId, ref lpPreviewInfo, null/*RealData*/, pUser);

            if (_mLRealHandle < 0)
            {
                DisplayMessage("开启预览失败!");
            }
        }

        private static string DecryptString(string sSource)
        {
            var byteEn = RsaCryptoServiceProvider.Encrypt(Encoding.ASCII.GetBytes("a"), false);
            var sBytes = sSource.Split(',');

            for (var j = 0; j < sBytes.Length; j++)
            {
                if (sBytes[j] != "")
                {
                    byteEn[j] = byte.Parse(sBytes[j]);
                }
            }
            var plaintbytes = RsaCryptoServiceProvider.Decrypt(byteEn, false);
            return Encoding.ASCII.GetString(plaintbytes);
        }

        /// <summary>
        /// 获取云台控制指令码
        /// </summary>
        /// <param name="cmdString"></param>
        /// <returns></returns>
        private static uint ControlCommand(string cmdString)
        {
            try
            {
                var command = (PtzControl)Enum.Parse(typeof(PtzControl), cmdString);
                return (uint)command;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        /// <summary>
        /// 控制云台
        /// </summary>
        /// <param name="cmdString"></param>
        /// <param name="stop"></param>
        public void PlatformControl(string cmdString, bool stop = false)
        {
            var cmd = ControlCommand(cmdString);
            if (cmd == 0) return;
            var isStop = (uint)(stop ? 1 : 0);
            CHCNetSDK.NET_DVR_PTZControl(_mLRealHandle, cmd, isStop);
        }

        private void OnDiposed(object sender, EventArgs e)
        {
            CHCNetSDK.NET_DVR_Cleanup();
        }
    }
}
