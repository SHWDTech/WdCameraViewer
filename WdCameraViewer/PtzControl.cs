namespace WdCameraViewer
{
    /// <summary>
    /// 云台控制命令
    /// </summary>
    public enum PtzControl
    {
        /// <summary>
        /// 接通灯光电源
        /// </summary>
        LightPwron = 0x02,

        /// <summary>
        /// 接通雨刷开关
        /// </summary>
        WiperPwron = 0x03,

        /// <summary>
        /// 接通风扇开关
        /// </summary>
        FanPwron = 0x04,

        /// <summary>
        /// 接通加热器开关
        /// </summary>
        HeaterPwron = 0x05,

        /// <summary>
        /// 接通辅助设备开关1
        /// </summary>
        AuxPwron1 = 0x06,

        /// <summary>
        /// 接通辅助设备开关2
        /// </summary>
        AuxPwron2 = 0x07,

        /// <summary>
        /// 焦距变大（倍率变大）
        /// </summary>
        ZoomIn = 0x0B,

        /// <summary>
        /// 焦距变小（倍率变小）
        /// </summary>
        ZoomOut = 0x0C,

        /// <summary>
        /// 焦点前调
        /// </summary>
        FocusNear = 0x0D,

        /// <summary>
        /// 焦点后调
        /// </summary>
        FocusFar = 0x0E,

        /// <summary>
        /// 光圈扩大
        /// </summary>
        IrisOpen = 0x0F,

        /// <summary>
        /// 光圈缩小
        /// </summary>
        IrisClose = 0x10,

        /// <summary>
        /// 云台上仰
        /// </summary>
        TiltUp = 0x21,

        /// <summary>
        /// 云台下俯
        /// </summary>
        TiltDown = 0x15,

        /// <summary>
        /// 云台左转
        /// </summary>
        PanLeft = 0x17,

        /// <summary>
        /// 云台右转
        /// </summary>
        PanRight = 0x18,

        /// <summary>
        /// 云台上仰和左转
        /// </summary>
        UpLeft = 0x19,

        /// <summary>
        /// 云台上仰和右转
        /// </summary>
        UpRight = 0x1A,

        /// <summary>
        /// 云台下俯和左转
        /// </summary>
        DownLeft = 0x1B,

        /// <summary>
        /// 云台下俯和右转
        /// </summary>
        DownRight = 0x1C,

        /// <summary>
        /// 云台左右自动扫描
        /// </summary>
        PanAuto = 0x1D
    }
}
