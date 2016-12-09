using System;
using System.Drawing;
using System.Windows.Forms;
using WdVedioNet;

namespace WdCameraViewer
{
    public partial class WdCameraViewer : UserControl, IObjectSafety
    {
        private bool _sdkInited;

        public WdCameraViewer()
        {
            InitializeComponent();
            SdkInit();
            Disposed += OnDisosed;
        }

        private void SdkInit()
        {
            _sdkInited = CHCNetSDK.NET_DVR_Init();
            if (_sdkInited == false)
            {
                using (var myFont = new Font("Arial", 14))
                {
                    var grf = cameraPictureBox.CreateGraphics();
                    grf.DrawString("控件初始化失败。", myFont, Brushes.White, new PointF(2, 2));
                }
            }

        }

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

        private void OnDisosed(object sender, EventArgs e)
        {
            CHCNetSDK.NET_DVR_Cleanup();
        }
    }
}
