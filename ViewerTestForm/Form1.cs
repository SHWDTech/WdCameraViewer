using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using WdVedioNet;

namespace ViewerTestForm
{
    public partial class Form1 : Form
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

        public Form1()
        {
            InitializeComponent();
            DisplayMessage("正在初始化控件。");
            SdkInit();
            wdCameraViewer.Paint += WdCameraViewerPaint;
            Disposed += OnDisposed;
            CameraLogin();
        }

        /// <summary>
        /// 初始化海康SDK
        /// </summary>
        private void SdkInit()
        {
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
            wdCameraViewer.Invalidate();
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
                e.Graphics.DrawString(_displayMessage, myFont, _displayBrush, wdCameraViewer.ClientRectangle, _displayStringFormat);
            }
            _displayMessage = string.Empty;
        }

        private void OnDisposed(object sender, EventArgs e)
        {
            CHCNetSDK.NET_DVR_Cleanup();
        }

        private void CameraLogin()
        {
            var ddnsName = Encoding.Default.GetBytes("wdnvr0002");
            var ipAddress = new byte[16];
            uint dwPort = 0;
            if (
                !CHCNetSDK.NET_DVR_GetDVRIPByResolveSvr_EX("www.hik-online.com", 80, ddnsName, (ushort)ddnsName.Length,
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

            _loginUserId = CHCNetSDK.NET_DVR_Login_V30(_dvrAddress, _dvrPortNumber, "admin", "juli#406", ref _deviceInfo);

            if (_loginUserId < 0)
            {
                _lastError = CHCNetSDK.NET_DVR_GetLastError();
                DisplayMessage($"用户登录失败，错误号：{_lastError}");
            }
            else
            {
                DisplayMessage("用户登录成功，开始连接。");
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

            Preview();
        }

        private void Preview()
        {
            var lpPreviewInfo = new CHCNetSDK.NET_DVR_PREVIEWINFO
            {
                hPlayWnd = wdCameraViewer.Handle,
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
               DisplayMessage($"预览失败!");
            }
        }

        private void CameraUp(object sender, MouseEventArgs e)
        {
            CHCNetSDK.NET_DVR_PTZControl(_mLRealHandle, 21, 0);
        }

        private void CameraUpStop(object sender, MouseEventArgs e)
        {
            CHCNetSDK.NET_DVR_PTZControl(_mLRealHandle, 21, 1);
        }

        private void CameraDown(object sender, MouseEventArgs e)
        {
            CHCNetSDK.NET_DVR_PTZControl(_mLRealHandle, 22, 0);
        }

        private void CameraDownStop(object sender, MouseEventArgs e)
        {
            CHCNetSDK.NET_DVR_PTZControl(_mLRealHandle, 22, 1);
        }

        private void CameraLeft(object sender, MouseEventArgs e)
        {
            CHCNetSDK.NET_DVR_PTZControl(_mLRealHandle, 23, 0);
        }

        private void CameraLeftStop(object sender, MouseEventArgs e)
        {
            CHCNetSDK.NET_DVR_PTZControl(_mLRealHandle, 23, 1);
        }

        private void CameraRight(object sender, MouseEventArgs e)
        {
            CHCNetSDK.NET_DVR_PTZControl(_mLRealHandle, 24, 0);
        }

        private void CameraRightStop(object sender, MouseEventArgs e)
        {
            CHCNetSDK.NET_DVR_PTZControl(_mLRealHandle, 24, 1);
        }
    }
}
