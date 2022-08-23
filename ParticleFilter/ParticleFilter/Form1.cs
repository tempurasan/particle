using OpenCvSharp;
using OpenCvSharp.Extensions;
using Classes;

namespace ParticleFilter {
    public partial class Form1 : Form {

        private Mat frame;
        private VideoCapture capture;
        private int timer_count = 0;
        PF pf;

        public Form1() {
            InitializeComponent();
            capture = new VideoCapture();
            frame = new Mat();
            timer1.Interval = 1;
        }

        private void button1_Click(object sender, EventArgs e) {
            capture.Open(0);
            if (!capture.IsOpened()) {
                MessageBox.Show("camera was not found!");
                this.Close();
            }
            capture.Read(frame);
            pictureBox1.Size = new System.Drawing.Size(frame.Width, frame.Height);
            toolStripStatusLabel1.Text = new System.Drawing.Size(frame.Width, frame.Height).ToString();
            pf = new PF(BitmapConverter.ToBitmap(frame), Color.FromArgb(36, 45, 47), 200, 30);
            pf.Next();
            timer1.Start();

        }

        private void timer1_Tick(object sender, EventArgs e) {
            capture.Read(frame);
            frame = frame.Flip(FlipMode.Y);
            pf.SetNewBitmap(BitmapConverter.ToBitmap(frame));
            pf.Next();
            if(timer_count == 10) {
                Console.WriteLine("a");
            }
            frame = BitmapConverter.ToMat(pf.GetBitmapDrawCircle());
            pictureBox1.Image = BitmapConverter.ToBitmap(frame);
            timer_count++;
            Application.DoEvents();
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e) {
            Color c = BitmapConverter.ToBitmap(frame).GetPixel(e.X, e.Y);
            string s = string.Format("h:{0},s:{1},b:{2}", c.GetHue(), c.GetSaturation(), c.GetBrightness());
            toolStripStatusLabel1.Text = c.ToString() + s;
        }
    }
}