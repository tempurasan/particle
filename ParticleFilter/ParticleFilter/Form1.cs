using OpenCvSharp;
using OpenCvSharp.Extensions;
using Classes;
using Point = System.Drawing.Point;

namespace ParticleFilter {
    public partial class Form1 : Form {

        private Mat frame;
        private VideoCapture capture;
        private int timer_count = 0;
        PF pf;
        private Point startTrimP;
        private Point endTrimP;
        private Color targetColor;
        private int viewMode = 0;

        public Form1() {
            InitializeComponent();
            capture = new VideoCapture();
            frame = new Mat();
            timer1.Interval = 1;
            targetColor = Color.White;
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
            pf = new PF(BitmapConverter.ToBitmap(frame), targetColor, 200, 30);
            pf.Next();
            timer1.Start();
        }

        private void timer1_Tick(object sender, EventArgs e) {
            capture.Read(frame);
            frame = frame.Flip(FlipMode.Y);
            pf.SetNewBitmap(BitmapConverter.ToBitmap(frame));
            pf.Next();

            if (viewMode == 0)
                pictureBox1.Image = BitmapConverter.ToBitmap(frame);
            else if (viewMode == 1)
                pictureBox1.Image = pf.GetBitmapDrawCircle();
            else if (viewMode == 2)
                pictureBox1.Image = pf.GetBitmapDrawRect();
                
            timer_count++;
            Application.DoEvents();
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e) {
            if (frame.Width > 10) {
                Color c = BitmapConverter.ToBitmap(frame).GetPixel(e.X, e.Y);
                string s = string.Format("h:{0},s:{1},b:{2}", c.GetHue(), c.GetSaturation(), c.GetBrightness());
                toolStripStatusLabel1.Text = c.ToString() + s;
            }
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e) {
            startTrimP = new Point(e.X, e.Y);
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e) {
            endTrimP = new Point(e.X, e.Y);
            Rectangle rect = new Rectangle(startTrimP.X, startTrimP.Y, endTrimP.X - startTrimP.X, endTrimP.Y - startTrimP.Y);

            if (rect.Width >= 0 || rect.Height >= 0) {
                Bitmap bmp = (Bitmap)pictureBox1.Image.Clone();
                FastBitmap dst = new FastBitmap(bmp.Clone(rect, bmp.PixelFormat));

                pictureBox2.Size = new System.Drawing.Size(dst.Width, dst.Height);
                pictureBox2.Image = dst.ToBitmap();

                float r = 0, g = 0, b = 0;
                for (int j = 0; j < dst.Height; j++) {
                    for (int i = 0; i < dst.Width; i++) {
                        Color c = dst.GetPixel(i, j);
                        r += c.R;
                        g += c.G;
                        b += c.B;
                    }
                }

                r /= (float)(dst.Width * dst.Height);
                g /= (float)(dst.Width * dst.Height);
                b /= (float)(dst.Width * dst.Height);

                targetColor = Color.FromArgb((int)r, (int)g, (int)b);
                pf.SetTargetColor(targetColor);
                label1.Text = "target:" + targetColor.ToString();

                bmp.Dispose();
                dst.Dispose();
            }

            else {
                MessageBox.Show("”ÍˆÍ‚ð‘I‘ð‚µ’¼‚µ‚Ä‚­‚¾‚³‚¢B");
            }
        }

        private void button2_Click(object sender, EventArgs e) {
            viewMode = 1;
        }

        private void button3_Click(object sender, EventArgs e) {
            viewMode = 2;
        }

        private void button4_Click(object sender, EventArgs e) {
            viewMode = 0;
        }
    }
}