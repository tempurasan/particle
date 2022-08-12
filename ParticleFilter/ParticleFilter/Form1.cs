using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace ParticleFilter {
    public partial class Form1 : Form {

        private Mat frame;
        private VideoCapture capture;

        public Form1() {
            InitializeComponent();
            capture = new VideoCapture();
            frame = new Mat();
        }

        private void button1_Click(object sender, EventArgs e) {
            capture.Open(0);
            capture.Read(frame);
            pictureBox1.Size = new System.Drawing.Size(frame.Width, frame.Height);
            timer1.Start();
        }

        private void timer1_Tick(object sender, EventArgs e) {
            capture.Read(frame);
            pictureBox1.Image = BitmapConverter.ToBitmap(frame);
            Application.DoEvents();
        }
    }
}