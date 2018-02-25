using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using DirectShowLib;
using Emgu.CV;
using Emgu.CV.Structure;

namespace SDPDesktop
{
    public partial class Form1 : Form
    {
        public RecognizerEngine Recognizer;
        public List<int> RecognizedLabels = new List<int>();
        public bool ExceptionCaught { get; set; }
        public DsDevice[] Cameras { get; set; }
        public static readonly int Fps = 30;
        public Capture CapTure;
        private readonly CascadeClassifier _classifier =
            new CascadeClassifier(Application.StartupPath + "/haarcascade_frontalface_alt_tree.xml");
        public ToolTip ToolTip = new ToolTip();
        public bool CaptureStopped;
        public bool TrainingDataAvailable;

        public struct VideoDevice
        {
            private readonly string _deviceName;
            public Guid Identifier;

            public VideoDevice(string name, Guid identity = new Guid())
            {
                _deviceName = name;
                Identifier = identity;
            }

            public override string ToString()
            {
                return $"{_deviceName}";
            }
        }

        public Form1()
        {
            InitializeComponent();
            comboBox1.MouseWheel += Forbid_MouseWheel;

            ToolTip.AutoPopDelay = 5000;
            ToolTip.InitialDelay = 1000;
            ToolTip.ReshowDelay = 500;
            ToolTip.ShowAlways = true;
            ToolTip.SetToolTip(pictureBox5, "Add Visitor");
            ToolTip.SetToolTip(pictureBox2, "Exit");
            ToolTip.SetToolTip(pictureBox1, "Minimize");
            ToolTip.SetToolTip(pictureBox6, "Add User");
            ToolTip.SetToolTip(pictureBox4, "Start the Camera");

            GetAvailableCameraDevices();
            FolderCreator();
        }

        public bool FolderCreator()
        {
            const string directory = @"C:\FRClient";
            if (!Directory.Exists(directory))
            {
                var di = Directory.CreateDirectory(directory);
                di.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
                File.Create($"{directory}\\TrainingData.yaml");
                TrainingDataAvailable = false;
                return false;
            }
            if (!File.Exists($"{directory}\\TrainingData.yaml"))
            {
                File.Create($"{directory}\\TrainingData.yaml");
            }
            if (new FileInfo($"{directory}\\TrainingData.yaml").Length == 0)
            {
                TrainingDataAvailable = false;
                return false;
            }
            Recognizer = new RecognizerEngine($"{directory}\\TrainingData.yaml");
            TrainingDataAvailable = true;
            return true;
        }

        /// <summary>
        /// Method which gets available video input devices.
        /// </summary>
        public void GetAvailableCameraDevices()
        {
            Cameras = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
            var webCams = new VideoDevice[Cameras.Length];
            for (var i = 0; i < Cameras.Length; i++)
            {
                webCams[i] = new VideoDevice(Cameras[i].Name, Cameras[i].ClassID);
                comboBox1.Items.Add(webCams[i].ToString());
            }
        }

        public void Forbid_MouseWheel(object sender, EventArgs e)
        {
            var ee = (HandledMouseEventArgs)e;
            ee.Handled = true;
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        private void pictureBox1_MouseEnter(object sender, EventArgs e)
        {
            pictureBox1.Cursor = Cursors.Hand;
            pictureBox1.BackColor = Color.FromArgb(68, 71, 90);
        }

        private void pictureBox1_MouseLeave(object sender, EventArgs e)
        {
            pictureBox1.Cursor = Cursors.Default;
            pictureBox1.BackColor = Color.FromArgb(40, 42, 54);
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

        private void pictureBox5_MouseEnter(object sender, EventArgs e)
        {
            pictureBox5.Cursor = Cursors.Hand;
            pictureBox5.BackColor = Color.FromArgb(68, 71, 90);
        }

        private void pictureBox5_MouseLeave(object sender, EventArgs e)
        {
            pictureBox5.Cursor = Cursors.Default;
            pictureBox5.BackColor = Color.FromArgb(40, 42, 54);
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

        public int Count;

        /// <summary>
        /// Method which captures and processes frames, detects faces and recognize users.
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
                using (var grayFrame = colorFrame.Convert<Gray, byte>())
                {
                    var detectedFaces = _classifier.DetectMultiScale(grayFrame, 1.1, 10, new Size(250, 250), Size.Empty);
                    foreach (var face in detectedFaces)
                    {
                        colorFrame.Draw(face, new Bgr(Color.FromArgb(255, 184, 108)), 2);
                        if (Count == 5)
                        {
                            grayFrame.ROI = face;
                            var result = Recognizer.RecognizeUser(grayFrame.Copy());
                            if (result.Distance >= 1000) continue;
                            RecognizedLabels.Add(result.Label);
                            if (RecognizedLabels.Count == 3)
                            {
                                Displayer();
                            }
                            Count = 0;
                        }
                        Count++;
                    }
                    pictureBox3.Image = colorFrame.ToBitmap();
                }
            }
            catch (Exception e)
            {
                ExceptionCaught = true;
                MessageBox.Show($@"Cannot read from the chosen device. {e.Message}", @"Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
        }

        public void Displayer()
        {
            var result = RecognizedLabels.GroupBy(x => x)
                .ToDictionary(y => y.Key, y => y.Count())
                .OrderByDescending(z => z.Value);

            foreach (var x in result)
            {
                if (x.Value < 3) continue;
                using (var context = new FRModel())
                {
                    var person = context.Users.First(u => u.PersonalNumber == x.Key);
                    Bitmap bmp;
                    using (var stream = new MemoryStream(person.Avatar))
                    {
                        bmp = new Bitmap(stream);
                    }
                    var resizedBmp = new Bitmap(bmp, new Size(64, 64));
                    var employee = new Employee(person.PersonalNumber)
                    {
                        LabelText = $"{person.Name} {person.Surname}",
                        Image = resizedBmp
                    };
                    flowLayoutPanel1.Controls.Add(employee);
                    var entrance = new EntranceRegister
                    {
                        Time = DateTime.Now,
                        UserId = x.Key
                    };
                    context.EntranceRegisters.Add(entrance);
                    context.SaveChanges();
                }
            }
            RecognizedLabels.Clear();
        }

        private void pictureBox5_Click(object sender, EventArgs e)
        {
            new Thread(() => new AddVisitor
            {
                ShowInTaskbar = false
            }.ShowDialog()).Start();
        }

        private void pictureBox6_Click(object sender, EventArgs e)
        {
            var t = new Thread(() => new AddUser
            {
                ShowInTaskbar = false
            }.ShowDialog());
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
        }

        public bool IsClicked;
        private void pictureBox4_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == -1) return;
            if (ExceptionCaught) return;
            var f = FolderCreator();
            var timer = new System.Windows.Forms.Timer();
            if (f)
            {
                IsClicked = !IsClicked;
                if (IsClicked)
                {
                    try
                    {
                        CaptureStopped = false;
                        CapTure = new Capture(comboBox1.SelectedIndex);
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
                    comboBox1.Enabled = false;
                    ToolTip.SetToolTip(pictureBox4, "Stop the Camera");
                }
                else
                {
                    CaptureStopped = true;
                    CapTure.Stop();
                    timer.Dispose();
                    ToolTip.SetToolTip(pictureBox4, "Start the Camera");
                    comboBox1.Enabled = true;
                }
            }
            else
            {
                MessageBox.Show(@"There is no training data available. Please add at least one user.", @"Ooops",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public const int WmNclbuttondown = 0xA1;
        public const int HtCaption = 0x2;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool ReleaseCapture();


        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            ReleaseCapture();
            SendMessage(Handle, WmNclbuttondown, HtCaption, 0);
        }
    }
}
