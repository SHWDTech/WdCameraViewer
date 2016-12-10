using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using WdVedioNet;

namespace WdCameraViewer
{
    public partial class WdCameraViewer : UserControl, IObjectSafety
    {
        private bool _sdkInited;

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

        private CameraInfo _cameraInfo;

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

        private bool _fSafeForScripting = true;
        private bool _fSafeForInitializing = true;

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

        public WdCameraViewer()
        {
            InitializeComponent();
            RsaCryptoServiceProvider.FromXmlString("<RSAKeyValue><Modulus>rskuXzA61uvVRcNt+f168ePkLhHB4PpdNnCn1dzRcayxiceBNoNqY1Qk1WwFklMCZz3YcKqKFh/VBkDfFL9tqkWXBQ1V7+9LCJ3hoV+RCXuJcyQt8mwe0Kh0R2VOE5rK2zbTLsovBjJt1j40lPpAbg9e1pVd7bQlGyfR3yOZZ8tBOFxGpWHKnxrKYJeyFZbPLE2BcSreFckqbcXnRGUqvUQMBJA2pyXpIdnRdFmMZpqJpSavYpPsm9jiu/8JUzVQgvnDIu7NPhNdUC5DqpYGjN/RGYcxBWIZWOR6T+JQ0s4aBXdUJsrHcA9XYhD62snN5urt+mbnoO5af0NdoC2b8scHcNqOzZxVg7jbFfCNhHWbYv4GVWgAlVyhcR2dTo1mZn8mE3Hl6WeotD7MZk8Inx67tfKCq6YcaEFHoqPTLTY0u9V6mtJCDe83yOUbrXeK464MF+M8bRwoBeX4GYr1mHdfoMtZeIc9vgVZK1byF/RFwm8yLbr4F/aEsWd5CaHivOJCwdobgATdFf6vO56DZjWBFGvJB+WTCvJmbIv2qL6WCCH8xL2tZIj9Jdf8BC8UUOSidH7ooXHrEiWhlu17+XDmE/Q71f5aRdPZz9CizRPg1rMXfogu8mUneq8hr2I+oBn3o8BxNRGqjbl5yZRNdi9ZtyBhOne+X0yR+5CNdsM=</Modulus><Exponent>AQAB</Exponent><P>5WqdOKItrhKsT92OdSET+rufrXotzTa5f7ruswUHdTVFSgka4yNWXBjs5ugATWFk+9weMqLIWAi6T6OsCUPdOgzENK9yJ9d9D97GVQ5IBDKs9sE3PZt2UMoCy09ceD8wHqeyjOlQj7yi+o8sNJ7O5Zy/z4NPtPY1DiyWebmF2jZCyxk+DJQlMGozQRIIc6Ilh9hpLAgbbI58vgonOMFU0O2+ugD978TJbXCv1ijy0J1r1FVq1f8HETxigpV+vGNT92AAZ8mzMUNY21tDF5Dh/I2dWZ3oI5DYLfhcCORxuAGBnNqMoVSHba3GDU4z5JsGLwX1ep1RTKhwGAKUGN34gw==</P><Q>wwoCu40FPr60sIjZNX7Owxn2uq+GyyogljKvAPsnDKQ3Gxi+6jDIsnLvcBhMm1nWJvRrS25OIsXMzJyIhdwXOTQJotN9cs6BBnSGbpfifhLnqOQFGcwdKqqZaLklUR7lKMgnaBQ4g4T9/84PBv/0JxLNhleuz5coZdpSVephu8/cK654/U8GXqDh5XMeybI3S+O8bir1K1c6BOhw8OtzzUjHChghSM6wqhe7jZJw4nn73qLj9Toehhdv4C8td0/oB4unVfJUfvNdWwDmD0e1l4TLir1bJRIC1fQKFvjQGfEAC85qNQaNxBwNTcNauRfP4XxI5A3TF1j4e+maZFO0wQ==</Q><DP>l1cCilaqLbgRxcnRbUE57eCR0J3V0xdzvWgyiRQbPF287L8e5pHsKWsj9Js9f85tEJy/qwWphjGTvm+pUJ9dNCsxz9OhSdkknjCGw5tdNK+9XDZP26tPnLH2r+oVhRmiA8b6yWwsgfWdyg5iyf+tWtlRy3HDRgxZKZWOWpRhUXcUDukC/sdH1S1pzFY6DxX7DidcEfjzJmTEs5T9FLqs2frMI+X9notBmZmJ1YxDygzfEj6a8LqBDgS4s44tAdfAj2LcQZtUQ347AtGsa8Je4f0FvRWnCrdFdOXuyMryncEYoMGnndGmWVsWWLarEvaVWLXkn1NiS2CeOaiRy+m6Qw==</DP><DQ>JINCQbRD0BxJnWbxKvejY5j/vLFRjcVENnokkw1xoQc5HcSDMTqSx/2GX7jc1pR55+8ICyYKUK4xCfkgAddTLa1VRHtNV+na88dqx1d92lZVsiOF5O92Yl9vutA2cTpUck8OOYjXj5+dIX+FBq1yGsKFYWoW2twUfwThNx5az5s5P6A5HEroCV0bDSaBFAdeHMH0q7c1ELkSroqJYkDh/ANs57HewU+YeS9aOEW7BlsJ0QMzo9wOjNHkatbKLzTxXkBBwnBMazvKNVg0uZWWJFiC9mU+o/D8QOuf2+8WnlSkypEJBwZEEfuibfVtjYssqqzmxHLmGs/YLPJtqeyVwQ==</DQ><InverseQ>ZRF0bEdyeQ0aCmVPg6zhY02v8dG6GCj/9LMt8JlAimqg65yqSDukRBWdomqNBwJiuJ8Zu75xutLahUSFJwjFoCN3eVZhIYFFsP5t9iMOCT8TvVRsXvfX9fLsc0jp6ruh3Em9iP66cXmnumL0/iF3bdgJKQiP/Xnw036WLch1ARg+sxMTv/wn5rlo1GB0Jgsec9upgrFooO/fYi+MgzYI9Ffmy4DhEeRT/ESSoVgYXuoXDRD65cQLuIfXcq5Y6geQhz4xjsOpyQkyBFjmkaT0v26SPhRc3Qa0xv8jWxQ5JIlgBMcENUDAP02cNKWLvxwTk4zvVfnM+3E1hVOf6BRiWQ==</InverseQ><D>UiVJPwF61eG2tXf46wH/00mIx0IfPa5NOrXNm4yRfvxr4FY8WzN+P7qfKRMAt1l+CqmdXK46AdXqF2tLrQOe9eSI6p3u4rozKJSTI3W3w54k5lF9qq63+NcC9z8cZ8hbSJXGwPnTCfWPe552tgG7YD6nEvDWWU5OFiorz9R6V7bGK0frB/Ui9o3vyV/iGZVsPuUaTeaYw+Jsp3TYkWN+p78gatCgbwQ5QmiNsUIY42wD/vNkgE8HZ+OSBEsDxfCLq1LZLETRfzg4peNod/bUk0bpjjbkiiQlx4pyFbNGyxyETEdd7HFnNDpxlixmyYcI11tYh1PvzsnXDHLLwOlOng8Bb16mVhvhXTQV2oYQ/e1R8CoKrCWIa8r53wWHDDWBf9DEZZb70f75uFs/qDxVaQVP7O+5JiNZEdSGXm0qIiY14Id47CZRmX0VPWW4RdkiPtqE6vAYFEQbrjQrZ8ZbgBN6fJfoU2zOc5Pi/mDJVGEXTlNfj1fi509G8IyzZbHveOTgSh+8Ka3KCg2OEU/vfFhP5vHnqnf+W0+F/65GDCkK2kloITWiDXZPJwZWDVbYcXye++MsOWhtp1Dc5RB10NuNl2gxvvMocWnSUQ8Bcw1jJmaAPtRJ8f8Hdk0zGs8zhyc3k7rctRRSGSUflX6rA1AbcqyP+GZEbCwx+GT9wYE=</D></RSAKeyValue>");
            cameraViewer.Paint += WdCameraViewerPaint;
            Disposed += OnDiposed;
            SdkInit();
        }

        /// <summary>
        /// 解析摄像头连接信息
        /// </summary>
        /// <param name="paramsStrig"></param>
        /// <returns></returns>
        public void TryLogin(string paramsStrig)
        {
            try
            {
                var oringinal = Encoding.UTF8.GetString(Convert.FromBase64String(paramsStrig));
                var jsonParams = DecryptString(oringinal);
                _cameraInfo = XmlSerializerHelper.DeSerialize<CameraInfo>(jsonParams);
                CameraLogin();
            }
            catch (Exception ex)
            {
                DisplayMessage($"解析连接信息失败，错误号：{ex.Data["HResult"]}");
            }
        }

        /// <summary>
        /// 初始化海康SDK
        /// </summary>
        private void SdkInit()
        {
            DisplayMessage("正在初始化控件。");
            _sdkInited = CHCNetSDK.NET_DVR_Init();
            if (_sdkInited == false)
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
                !CHCNetSDK.NET_DVR_GetDVRIPByResolveSvr_EX("www.hik-online.com", 80, _cameraInfo.DomainBytes, (ushort)_cameraInfo.DomainBytes.Length,
                    null, 0, ipAddress, ref dwPort))
            {
                _lastError = CHCNetSDK.NET_DVR_GetLastError();
                DisplayMessage($"域名解析失败，错误号：{_lastError}");
            }
            else
            {
                _dvrAddress = Encoding.UTF8.GetString(ipAddress).TrimEnd('\0');
                _dvrPortNumber = (ushort)dwPort;
            }

            _loginUserId = CHCNetSDK.NET_DVR_Login_V30(_dvrAddress, _dvrPortNumber, _cameraInfo.User, _cameraInfo.Password, ref _deviceInfo);

            if (_loginUserId < 0)
            {
                _lastError = CHCNetSDK.NET_DVR_GetLastError();
                DisplayMessage($"用户登录失败，错误号：{_lastError}");
            }
            else
            {
                DisplayMessage("用户登录成功，可以开始预览。");
                _dwAChanTotalNum = _deviceInfo.byChanNum;
                _dwDChanTotalNum = _deviceInfo.byIPChanNum + 256 * (uint)_deviceInfo.byHighDChanNum;
            }

            InfoIpChannel();
        }

        private void InfoIpChannel()
        {
            var dwSize = (uint)Marshal.SizeOf(_struIpParaCfgV40);

            var ptrIpParaCfgV40 = Marshal.AllocHGlobal((int)dwSize);
            Marshal.StructureToPtr(_struIpParaCfgV40, ptrIpParaCfgV40, false);

            uint dwReturn = 0;
            var iGroupNo = 0;  //该Demo仅获取第一组64个通道，如果设备IP通道大于64路，需要按组号0~i多次调用NET_DVR_GET_IPPARACFG_V40获取

            if (!CHCNetSDK.NET_DVR_GetDVRConfig(_loginUserId, CHCNetSDK.NET_DVR_GET_IPPARACFG_V40, iGroupNo, ptrIpParaCfgV40, dwSize, ref dwReturn))
            {
                _lastError = CHCNetSDK.NET_DVR_GetLastError();
                DisplayMessage($"获取录像机通道信息失败。错误码{_lastError}");
            }
            else
            {
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

                            _iIpDevId[i] = _struChanInfo.byIPID + _struChanInfo.byIPIDHigh * 256 - iGroupNo * 64 - 1;

                            Marshal.FreeHGlobal(ptrChanInfo);
                            break;
                        case 6:
                            var ptrChanInfoV40 = Marshal.AllocHGlobal((int)dwSize);
                            Marshal.StructureToPtr(_struIpParaCfgV40.struStreamMode[i].uGetStream, ptrChanInfoV40, false);
                            _struChanInfoV40 = (CHCNetSDK.NET_DVR_IPCHANINFO_V40)Marshal.PtrToStructure(ptrChanInfoV40, typeof(CHCNetSDK.NET_DVR_IPCHANINFO_V40));

                            _iIpDevId[i] = _struChanInfoV40.wIPID - iGroupNo * 64 - 1;

                            Marshal.FreeHGlobal(ptrChanInfoV40);
                            break;
                    }
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
                var command = (PtzControl) Enum.Parse(typeof(PtzControl), cmdString);
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
            var isStop = (uint) (stop ? 0 : 1);
            CHCNetSDK.NET_DVR_PTZControl(_mLRealHandle, cmd, isStop);
        }

        private void OnDiposed(object sender, EventArgs e)
        {
            CHCNetSDK.NET_DVR_Cleanup();
        }
    }
}
