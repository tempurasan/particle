using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classes {
    public class PF {
        
        private class Particle {
            public Point Point;        //座標 
            public Point Velocity;         //速度
            public float Likelihood = 0;    //尤度
            private Random rand= new Random();
            
            public Particle() {
                Point = new Point();
                Velocity = new Point();
                Likelihood = 0;
            }

            public void RandomWalk(Size size) {
                Point.X += rand.Next((-1) * Velocity.X, Velocity.X);
                Point.Y += rand.Next((-1) * Velocity.Y, Velocity.Y);
                if(Point.X > size.Width -1)
                    Point.X = size.Width -1;
                if(Point.Y > size.Height -1)
                    Point.Y = size.Height -1;
                if (Point.X < 0)
                    Point.X = 0;
                if (Point.Y < 0)
                    Point.Y = 0;
            }

            public void Resampling(Point target, int len, Size size) {
                int x = target.X + rand.Next((-1) * len, len);
                int y = target.Y + rand.Next((-1) * len, len);
                Point.X = x;
                Point.Y = y;
                if (Point.X > size.Width - 1)
                    Point.X = size.Width - 1;
                if (Point.Y > size.Height - 1)
                    Point.Y = size.Height - 1;
                if(Point.X < 0)
                    Point.X = 0;
                if (Point.Y < 0)
                    Point.Y = 0;
            }

            public void SetLikelihood_RGB(Color src, Color target) {
                Likelihood = (Math.Abs(src.R - target.R) / 255.0F + Math.Abs(src.G - target.G) / 255.0F + Math.Abs(src.B - target.B) / 255.0F) / 3.0F;
            }

            public void SetLikelihood_HSV(Color src, Color target) {
                float h = Math.Abs(src.GetHue() - target.GetHue());
                float s = Math.Abs(src.GetSaturation() - target.GetSaturation());
                float v = Math.Abs(src.GetBrightness() - target.GetBrightness());
                if (h > 180) h -= 360;

                Likelihood = (h / 180.0F + s + v) / 3;
            }

            public void Probability(int dispersion) {
                float num = GaussFunction(dispersion);
                //Console.WriteLine("{0},{1}", Likelihood, num);
                if (rand.NextDouble() > num) Likelihood = -1;
            }
            private float GaussFunction(int dispersion) {
                return (float)(Math.Exp(-(Likelihood * Likelihood) / 2 * (dispersion * dispersion)));
            }

        }


        private Particle[] particles;
        private Random rand;
        private Color target_RGB;
        FastBitmap src;
        int vMax;
        private int gaussParam = 10;
        private Point Center;

        public PF(Bitmap src, Color target,int particleNum,int vMax) {
            this.src = new FastBitmap(src);
            this.target_RGB = target;
            particles = new Particle[particleNum];
            for (int i = 0; i < particleNum; i++) 
                particles[i] = new Particle();
            this.vMax = vMax;
            rand = new Random();
            SetFirstParticle();
            Center = new Point();
        }

        public void SetNewBitmap(Bitmap src) {
            this.src = new FastBitmap(src);
        }

        public void SetTargetColor(Color c) {
            target_RGB = c;
        }

        public Bitmap GetBitmapDrawCircle() {
            return DrawCircle(src.ToBitmap(), 10);
        }

        public Bitmap GetBitmapDrawRect() {
            return DrawRect(src.ToBitmap(), 30);
        }

        public void Next() {
            
            List<int> aliveIndex = new List<int>();
            List<int> deadIndex = new List<int>();

            for(int i = 0; i < particles.Length; i++) {
                particles[i].RandomWalk(new Size(src.Width, src.Height));
                Color c = src.GetPixel(particles[i].Point.X, particles[i].Point.Y);
                particles[i].SetLikelihood_RGB(c, target_RGB);
                //particles[i].SetLikelihood_HSV(c, target_RGB);

                //確率で消滅する粒子を決定、消滅したものはLikelihoodが-1になる
                particles[i].Probability(gaussParam);
                if (particles[i].Likelihood == -1) deadIndex.Add(i);
                else aliveIndex.Add(i);
            }

            //Console.WriteLine("alive{0},dead{1}", aliveIndex.Count, deadIndex.Count);

            //リサンプリング
            if(aliveIndex.Count > 10) {
                for(int i = 0;i < deadIndex.Count; i++) {
                    Point target = particles[aliveIndex[rand.Next(aliveIndex.Count - 1)]].Point;
                    particles[deadIndex[i]].Resampling(target, 30, new Size(src.Width, src.Height));
                }
            }

            else {
                SetFirstParticle();
                //Console.WriteLine("low particles");
            }

            //DrawCircle(src.ToBitmap(), 5);

        }

        private  void SetFirstParticle() {
            for(int i = 0; i < particles.Length; i++) {
                particles[i].Point = new Point(rand.Next(0, src.Width -1),rand.Next(0,src.Height));
                particles[i].Velocity = new Point(rand.Next(0, vMax), rand.Next(0, vMax));
            }
        }

        private Bitmap DrawRect(Bitmap src, int size) {
            Bitmap b = (Bitmap)src.Clone();
            Graphics g = Graphics.FromImage(b);
            int aveX = 0;
            int aveY = 0;
            
            foreach (var a in particles) {
                aveX += a.Point.X;
                aveY += a.Point.Y;
            }

            aveX /= particles.Length;
            aveY /= particles.Length;

            Rectangle rect = new Rectangle(aveX - size, aveY - size, size * 2, size * 2);

            g.DrawRectangle(new Pen(Brushes.Red,10), rect);

            return b;
        }

        private Bitmap DrawCircle(Bitmap src, int radius) {
            Bitmap b = (Bitmap)src.Clone();
            Graphics g = Graphics.FromImage(b);
            foreach(var a in particles) {
                int num = (int)(a.Likelihood * 5.0F);
                if (num < 0)
                    g.FillEllipse(Brushes.White, new Rectangle(a.Point.X - radius, a.Point.Y - radius, radius * 2, radius * 2));
                else
                    g.FillEllipse(Brushes.Red, new Rectangle(a.Point.X - radius, a.Point.Y - radius, radius * 2, radius * 2));
                //if(num == 5) {
                //    g.FillEllipse(Brushes.Red, new Rectangle(a.Point.X - radius, a.Point.Y - radius, radius * 2, radius * 2));
                //}
                //else if(num == 4) {
                //    g.FillEllipse(Brushes.Orange, new Rectangle(a.Point.X - radius, a.Point.Y - radius, radius * 2, radius * 2));

                //}
                //else if (num == 3) {
                //    g.FillEllipse(Brushes.Yellow, new Rectangle(a.Point.X - radius, a.Point.Y - radius, radius * 2, radius * 2));

                //}
                //else if (num == 2) {
                //    g.FillEllipse(Brushes.Green, new Rectangle(a.Point.X - radius, a.Point.Y - radius, radius * 2, radius * 2));

                //}
                //else if (num == 1) {
                //    g.FillEllipse(Brushes.Blue, new Rectangle(a.Point.X - radius, a.Point.Y - radius, radius * 2, radius * 2));

                //}
                //else {
                //    g.FillEllipse(Brushes.White, new Rectangle(a.Point.X - radius, a.Point.Y - radius, radius * 2, radius * 2));
                //}
            }
            g.Dispose();
            return b;
        }

    }
}
