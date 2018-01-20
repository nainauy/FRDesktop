using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using DirectShowLib;
using Emgu.CV;
using Emgu.CV.Structure;

namespace SDPDesktop
{
    public partial class AddUser : Form
    {
        public byte[] Avatar { get; set; }
        public DsDevice[] Cameras { get; set; }
        public bool CaptureStopped;
        public Capture CapTure;
        public bool ExceptionCaught { get; set; }
        public ToolTip ToolTip = new ToolTip();
        public static readonly int Fps = 10;
        private readonly CascadeClassifier _classifier =
            new CascadeClassifier(Application.StartupPath + "/haarcascade_frontalface_alt_tree.xml");
        public List<Image<Gray, byte>> GrayFaces = new List<Image<Gray, byte>>();

        public AddUser()
        {
            InitializeComponent();
            InitializeOpenFileDialog();
            comboBox3.MouseWheel += Forbid_MouseWheel;

            var toolTip1 = new ToolTip
            {
                AutoPopDelay = 5000,
                InitialDelay = 1000,
                ReshowDelay = 500,
                ShowAlways = true
            };
            toolTip1.SetToolTip(pictureBox6, "Add Avatar");
            comboBox2.DataSource = new[]
            {
                new { Type = "Male", Number = 1 },
                new { Type = "Female", Number = 2 }
            };
            comboBox2.DisplayMember = "Type";
            comboBox2.ValueMember = "Number";
            comboBox1.DataSource = new[]
            {
                new { Type = "Sector 1", Number = 1 },
                new { Type = "Sector 2", Number = 2 },
                new { Type = "Sector 3", Number = 3 }
            };
            comboBox1.DisplayMember = "Type";
            comboBox1.ValueMember = "Number";
            GetAvailableCameraDevices();
        }

        /// <summary>
        /// Method which gets available video input devices.
        /// </summary>
        public void GetAvailableCameraDevices()
        {
            Cameras = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
            var webCams = new Form1.VideoDevice[Cameras.Length];
            for (var i = 0; i < Cameras.Length; i++)
            {
                webCams[i] = new Form1.VideoDevice(Cameras[i].Name, Cameras[i].ClassID);
                comboBox3.Items.Add(webCams[i].ToString());
            }
        }

        public void FolderCreator()
        {
            const string directory = @"C:\FRClient";
            if (Directory.Exists(directory)) return;
            var di = Directory.CreateDirectory(directory);
            di.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
            File.Create($"{directory}\\TrainingData.yaml");
        }

        public void Forbid_MouseWheel(object sender, EventArgs e)
        {
            var ee = (HandledMouseEventArgs)e;
            ee.Handled = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Dispose();
        }

        private void pictureBox6_MouseEnter(object sender, EventArgs e)
        {
            pictureBox6.Cursor = Cursors.Hand;
            pictureBox6.BackColor = Color.FromArgb(68, 71, 90);
        }

        private void pictureBox6_MouseLeave(object sender, EventArgs e)
        {
            pictureBox6.Cursor = Cursors.Default;
            pictureBox6.BackColor = Color.FromArgb(40, 42, 54);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var numbers = new List<int>();
            using (var con = new FRModel())
            {
                var users = con.Users.ToList();
                numbers.AddRange(users.Select(user => user.PersonalNumber));
            }
            textBox1.Enabled = false;
            textBox2.Enabled = false;
            textBox3.Enabled = false;
            comboBox1.Enabled = false;
            comboBox2.Enabled = false;
            pictureBox6.Enabled = false;
            if (GrayFaces.Count < 12)
            {
                MessageBox.Show(@"Please wait until the data set is ready", @"Wait a bit",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(textBox1.Text) && !string.IsNullOrWhiteSpace(textBox2.Text) &&
                !string.IsNullOrWhiteSpace(textBox3.Text) && Avatar != null)
                {
                    var exist = false;
                    foreach (var number in numbers)
                    {
                        if (number == Convert.ToInt32(textBox3.Text))
                        {
                            exist = true;
                        }
                    }
                    if (!exist)
                    {
                        try
                        {
                            using (var context = new FRModel())
                            {
                                var genreId = Convert.ToInt32(comboBox2.SelectedValue);
                                var sectorId = Convert.ToInt32(comboBox1.SelectedValue);
                                var personalNumber = Convert.ToInt32(textBox3.Text);
                                var gender = context.Genders.Single(g => g.Id == genreId);
                                var sector = context.Sectors.Single(s => s.Id == sectorId);
                                var user = new User
                                {
                                    Name = textBox1.Text,
                                    Surname = textBox2.Text,
                                    PersonalNumber = personalNumber,
                                    Gender = gender,
                                    Sector = sector,
                                    Avatar = Avatar
                                };
                                context.Users.Add(user);
                                context.SaveChanges();
                                foreach (var grayFace in GrayFaces)
                                {
                                    var converter = new ImageConverter();
                                    var image = (byte[])converter.ConvertTo(grayFace.ToBitmap(), typeof(byte[]));
                                    context.Images.Add(new Image
                                    {
                                        Face = image,
                                        UserId = Convert.ToInt32(textBox3.Text)
                                    });
                                    context.SaveChanges();
                                }
                            }
                        }
                        catch (SqlException)
                        {
                            MessageBox.Show(@"Failed saving a user. Check your connection or try again.", @"Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        var recognizer = new RecognizerEngine("C:\\FRClient\\TrainingData.yaml");
                        recognizer.TrainRecognizer();
                    }
                    else
                    {
                        MessageBox.Show(@"User with the same personal number already exists.", @"Warning", 
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }                   
                }
                else
                {
                    MessageBox.Show(@"You need to enter obligatory fields in order to add a user.", @"Warning",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            textBox1.Enabled = true;
            textBox2.Enabled = true;
            textBox3.Enabled = true;
            comboBox1.Enabled = true;
            comboBox2.Enabled = true;
            pictureBox6.Enabled = true;
            textBox1.Text = "";
            textBox2.Text = "";
            textBox3.Text = "";
            GrayFaces.Clear();
        }

        private void InitializeOpenFileDialog()
        {
            openFileDialog1.Filter =
                @"All images|*.bmp;*.jpg;*.jpeg;*.png|" +
                @"BMP|*.bmp|JPG|*.jpg;*.jpeg|PNG|*.png";

            openFileDialog1.Title = @"Image Dialog";
        }

        private void pictureBox6_Click(object sender, EventArgs e)
        {
            var dr = openFileDialog1.ShowDialog();
            if (dr != DialogResult.OK) return;
            var bmp = new Bitmap(openFileDialog1.FileName);
            var converter = new ImageConverter();
            Avatar = (byte[])converter.ConvertTo(bmp, typeof(byte[]));
        }

        public int Count;

        /// <summary>
        /// Event handler which captures and processes frames and detects faces.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="arg"></param>
        public void ProcessFrame(object sender, EventArgs arg)
        {
            try
            {
                if (ExceptionCaught) return;
                if (CaptureStopped) return;
                using (var colorFrame = CapTure.QueryFrame().ToImage<Bgr, byte>())
                {
                    var grayFrame = colorFrame.Convert<Gray, byte>();
                    var detectedFaces = _classifier.DetectMultiScale(grayFrame, 1.1, 10, new Size(250, 250), Size.Empty);
                    foreach (var face in detectedFaces)
                    {
                        grayFrame.ROI = face;
                        colorFrame.Draw(face, new Bgr(Color.FromArgb(255, 184, 108)), 2);
                        if (GrayFaces.Count == 12) continue;
                        if (Count == 5)
                        {
                            GrayFaces.Add(grayFrame.Copy());
                            Count = 0;
                        }
                        Count++;
                    }
                    pictureBox1.Image = colorFrame.ToBitmap();
                }
            }
            catch (Exception)
            {
                ExceptionCaught = true;
                MessageBox.Show(@"Cannot read from the chosen device.", @"Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
        }

        public bool IsClicked;
        private void pictureBox4_Click(object sender, EventArgs e)
        {
            if (comboBox3.SelectedIndex == -1) return;
            if (ExceptionCaught) return;
            var timer = new Timer();
            IsClicked = !IsClicked;
            if (IsClicked)
            {
                try
                {
                    CaptureStopped = false;
                    CapTure = new Capture(comboBox3.SelectedIndex);
                    timer.Interval = 1000 / Fps;
                    timer.Tick += ProcessFrame;
                    timer.Start();
                }
                catch (Exception)
                {
                    MessageBox.Show(@"Cannot read from the chosen device.", @"Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Application.Exit();
                }
                comboBox3.Enabled = false;
                ToolTip.SetToolTip(pictureBox4, "Stop the Camera");
            }
            else
            {
                CaptureStopped = true;
                CapTure.Stop();
                timer.Dispose();
                ToolTip.SetToolTip(pictureBox4, "Start the Camera");
                comboBox3.Enabled = true;
            }
        }

        private void pictureBox4_MouseEnter(object sender, EventArgs e)
        {
            pictureBox4.Cursor = Cursors.Hand;
            pictureBox4.BackColor = Color.FromArgb(68, 71, 90);
        }

        private void pictureBox4_MouseLeave(object sender, EventArgs e)
        {
            pictureBox4.Cursor = Cursors.Default;
            pictureBox4.BackColor = Color.FromArgb(40, 42, 54);
        }

        public const int WmNclbuttondown = 0xA1;
        public const int HtCaption = 0x2;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        private void AddUser_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            ReleaseCapture();
            SendMessage(Handle, WmNclbuttondown, HtCaption, 0);
        }
    }
}