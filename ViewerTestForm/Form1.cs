using System.Windows.Forms;

namespace ViewerTestForm
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            _previewBox.TryLogin("123");
        }
    }
}
