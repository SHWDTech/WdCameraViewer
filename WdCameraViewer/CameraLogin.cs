using System;
using System.Text;

namespace WdCameraViewer
{
    public class CameraLogin
    {
        public string SerialNumber { get; set; }

        public string User { get; set; }

        public string Password { get; set; }

        public Guid DeviceGuid { get; set; }

        public string IpServerAddr { get; set; }

        public byte[] DomainBytes => Encoding.UTF8.GetBytes(SerialNumber);
    }
}
