using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Data.Entity;
using System.IO;

namespace SDPDesktop
{
    public partial class UserInfo : Form
    {
        public int Id { get; set; }

        public UserInfo(int id)
        {
            InitializeComponent();
            Id = id;
            using (var context = new FRModel())
            {
                var user = context.Users.Include(u => u.Gender).Include(u => u.Sector).Single(u => u.PersonalNumber == Id);
                Bitmap bmp;
                using (var stream = new MemoryStream(user.Avatar))
                {
                    bmp = new Bitmap(stream);
                }
                var resizedBmp = new Bitmap(bmp, new Size(200, 200));
                label1.Text = user.Name;
                label2.Text = user.Surname;
                label3.Text = Convert.ToString(Id);
                label4.Text = user.Gender.Name;
                label5.Text = user.Sector.Name;
                pictureBox1.Image = resizedBmp;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Dispose();
        }

        public const int WmNclbuttondown = 0xA1;
        public const int HtCaption = 0x2;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        private void UserInfo_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            ReleaseCapture();
            SendMessage(Handle, WmNclbuttondown, HtCaption, 0);
        }
    }
}
