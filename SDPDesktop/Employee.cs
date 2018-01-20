using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace SDPDesktop
{
    public partial class Employee : UserControl
    {
        public int Id;

        public Employee(int id)
        {
            InitializeComponent();
            Id = id;
            label2.Text = Convert.ToString(Id);
        }

        public string LabelText
        {
            set => label1.Text = value;
        }

        public Bitmap Image
        {
            set => pictureBox1.Image = value;
        }

        private void pictureBox2_MouseEnter(object sender, EventArgs e)
        {
            pictureBox2.Cursor = Cursors.Hand;
            pictureBox2.BackColor = Color.FromArgb(68, 71, 90);
        }

        private void pictureBox2_MouseLeave(object sender, EventArgs e)
        {
            pictureBox2.Cursor = Cursors.Default;
            pictureBox2.BackColor = Color.FromArgb(40, 42, 54);
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            new Thread(() => new UserInfo(Id)
            {
                ShowInTaskbar = false
            }.ShowDialog()).Start();
        }
    }
}
