using System.Windows.Forms;

namespace ViewerTestForm
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            _previewBox.TryLogin("NDQsMTExLDI0LDk4LDE0OSwxMiwyMjIsMjA4LDE5OCwxNzQsMTcxLDQ0LDEzOSwxMjAsMTI0LDE3MiwxMjksMzIsMCwyMzQsMTIsNzQsMTUxLDE3MiwxNDEsMTg3LDgsNzgsMTMsMTM5LDE1NywyMjYsNiw5LDYwLDIzOCwyMTIsOTQsMTYxLDE3MiwyMDQsMjQsOTQsNDcsMTQwLDE5MywyNCw5OCwxOTcsMTI3LDE1OCwxNzUsNzEsMTUwLDE4Niw1Niw0NSwxNSwzNiwyMiwxODksMTUyLDE1NCwxNiwyMzAsMjA3LDExNywzOSwxNzMsMTY4LDIwMCwyMDcsOCwxMDgsMTYzLDEzNSwxMzAsMjQ4LDI0OCwyNDIsMTI1LDU4LDk5LDIwNCw3Niw0Myw1MywxMzIsMTgzLDE1MiwxMDEsMjUsMjQwLDI0Niw5OSwxNTAsMTg0LDE3MCwxMCwxODEsMzAsNDAsNDksNzQsMjU0LDE1LDIyMCwxMDIsMjQyLDE0MiwyMTMsNTcsMjA4LDIyMyw4OCwyNDgsNzcsMjAxLDE3LDE4LDI0NCwyMzMsMjQyLDE5LDE1MiwxMDksMTI0LDE1MywyNTEsMTU3LDIwOSwxNTQsMTgxLDE0NSw3Myw1MSwxOCwxMjAsMTU5LDEyOSwxNjUsNDksMTY2LDMyLDE3NCwxMywzMiw5MCwzNSw2NCwzOCwxMCw5NSwxMjgsMTIzLDEzMiw1MywyNTAsNjIsMTgyLDE3MiwxNTEsMTI3LDE1NywxMjEsMTQ4LDQ5LDEwOCwyMSwxMjUsMjE1LDE5MCw3MywyNDIsMjksMTcyLDE5LDE5Niw5MiwxODUsMTI0LDIzNCw4NiwxMjUsMjE4LDcxLDE2NCw1MCwyMjAsMjE5LDIyOCwxOTUsMTAzLDE4LDE1Niw3NCwyMTgsMzYsMTM2LDIsOTQsNTIsMTU4LDI2LDEyMywxNzksMTM1LDIxMywxMDksNjgsMzUsMTM3LDU2LDk1LDcsNjgsMTA1LDc4LDE0OSwyNDMsNDQsMTgsMTkzLDMzLDE3NSw5MCwyNDUsMTgyLDE1MiwxMDcsMTU4LDEzMiwzNiwxODUsMjM3LDE2NywyMDYsMTA4LDY2LDg4LDEyMSwxMTgsMTg3LDg3LDM5LDEzLDE2NiwyNTEsMTA1LDIwMyw4MiwyMDYsMjA2LDExOCwxMjAsMjM5LDE3NSwxMDYsMTI1LDIyMywzLDIzNSwxODUsMzAsNzMsMTM1LDUxLDE1MCwxMDYsMTI4LDI1Myw5OCwyMzEsNDcsMzMsMjEyLDE2NiwxMTAsMjM2LDEwNywxNiw3NCwxODEsNzAsMzgsMTc3LDIwOSwxOTAsNDUsMyw2OSwyMTEsMTcwLDExMywxMjEsNjgsMTE3LDcsMTY1LDEyNywxMTQsNzgsMzgsMjU0LDEzNiwxMSwxNjQsMjIzLDE0MSwxNDEsMTQ3LDE5Myw0OCwzNywzMiwxNywzMyw2OCwyMjUsMjQxLDEzOCwxMDgsOTMsMjIwLDExMSwxNzAsNjIsMTkzLDE3NCwxNzIsMjEyLDIwLDgwLDE3LDI0NCwxMjEsMTM4LDYzLDE0LDI0NiwxNjIsMjIxLDE5OCw1NCwxNTAsOTgsNjYsMjExLDEwMyw0NywyMDksMTcxLDE2Myw3OCwyMjAsNzMsNDksNDYsNywxMDYsMjM1LDE0MCwxNjcsMSwzLDExNCwxMTcsMjUzLDIzNSw1MSw4OCw3MCwxMzIsNjksNSwxNTAsMTUzLDE4NywzNiwxOTMsMTM4LDU4LDE2NCwyMTEsNDksMjIwLDI1NSwxOTYsMTM0LDQyLDE4NSwyMCwyMTYsMTgyLDI0LDIyLDIyNSwxNDQsMjI0LDEwNSwxODAsMjA0LDk1LDU4LDQ1LDQsMTQ5LDc5LDgwLDEwMyw2MCwyMTcsNjMsMTgwLDkzLDMwLDE2OCw3MCwyMzUsMjI3LDIyOCwyNTIsMjUwLDE0NiwyMzcsMjUyLDIzMSwyMDEsNDcsMzksMTk4LDEzMSwyMDUsMjIzLDMyLDIwNiwxODgsMTg3LDEsMjA4LDE4MSwxNjIsOTYsOTIsNDQsMTg3LDE1MSwxMzIsODgsMjcsNDQsMTY1LDE1NywxNjUsMTY4LDE5Myw4OSwyNDUsOTIsMTUxLDI0MSwxNjYsMjQyLDE1NCwxMiwxODgsOTksMTEzLDEzNywyMDEsMjM5LDE1Niw5NCwxOTcsMjQ1LDI0MCwxNTIsMjEwLDE1NSw3OSwxMTUsMTA1LDE4NCwxMTksMTU4LDI0OCwxMDYsNjMsNTYsMjAxLDU5LDEwNCwxNjIsMjQxLDI1NSwxNTAsMTA4LDEzNywyMzUsMTEzLDE3OSwyMzIsMTYzLDIzNSwxMjUsMjI3LDM3LDIwOCwyMTAsMjE0LDE1Nyw4NCw=");
            _previewBox.Preview();
        }

        private void button1_Click(object sender, System.EventArgs e)
        {
            _previewBox.PlatformControl("PanLeft");
        }

        private void button2_Click(object sender, System.EventArgs e)
        {
            _previewBox.PlatformControl("PanRight");
        }

        private void button3_Click(object sender, System.EventArgs e)
        {
            _previewBox.PlatformControl("TiltUp");
        }

        private void button4_Click(object sender, System.EventArgs e)
        {
            _previewBox.PlatformControl("TiltDown", true);
        }

        private void button5_Click(object sender, System.EventArgs e)
        {
            _previewBox.PlatformControl("FocusFar");
        }

        private void button6_Click(object sender, System.EventArgs e)
        {
            _previewBox.PlatformControl("FocusNear");
        }
    }
}
