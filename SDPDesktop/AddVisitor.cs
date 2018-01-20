using System;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Forms;

namespace SDPDesktop
{
    public partial class AddVisitor : Form
    {
        public AddVisitor()
        {
            InitializeComponent();
            comboBox2.DataSource = new[]
            {
                new { Type = "Male", Number = 1 },
                new { Type = "Female", Number = 2 }
            };
            comboBox2.DisplayMember = "Type";
            comboBox2.ValueMember = "Number";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Dispose();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            textBox1.Enabled = false;
            textBox2.Enabled = false;
            textBox3.Enabled = false;
            comboBox2.Enabled = false;
            if (!string.IsNullOrWhiteSpace(textBox1.Text) && !string.IsNullOrWhiteSpace(textBox2.Text) &&
                !string.IsNullOrWhiteSpace(textBox3.Text))
            {
                try
                {
                    using (var context = new FRModel())
                    {
                        var genreId = Convert.ToInt32(comboBox2.SelectedValue);
                        var gender = context.Genders.Single(g => g.Id == genreId);
                        var visitor = new Visitor
                        {
                            Name = textBox1.Text,
                            Surname = textBox2.Text,
                            CardId = textBox3.Text,
                            Gender = gender
                        };
                        context.Visitors.Add(visitor);
                        context.SaveChanges();
                    }
                }
                catch (SqlException)
                {
                    MessageBox.Show(@"Failed saving a visitor. Check your connection or try again.", @"Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show(@"You need to enter obligatory fields in order to add a visitor.", @"Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            textBox1.Enabled = true;
            textBox2.Enabled = true;
            textBox3.Enabled = true;
            comboBox2.Enabled = true;
            textBox1.Text = "";
            textBox2.Text = "";
            textBox3.Text = "";
        }

        public const int WmNclbuttondown = 0xA1;
        public const int HtCaption = 0x2;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        private void AddVisitor_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            ReleaseCapture();
            SendMessage(Handle, WmNclbuttondown, HtCaption, 0);
        }
    }
}