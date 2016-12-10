using System.Text;

namespace WdCameraViewer
{
    public class CameraInfo
    {
        public string DomainName { get; set; }

        public string User { get; set; }

        public string Password { get; set; }

        public byte[] DomainBytes => Encoding.UTF8.GetBytes(DomainName);
    }
}
