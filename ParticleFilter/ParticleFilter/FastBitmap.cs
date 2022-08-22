using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Classes {
    public class FastBitmap {
        private Bitmap src;
        private byte[] srcpixels;
        private int width;
        private int height;
        private int Stride;

        public int Height
        {
            get { return height; }
        }
        public int Width
        {
            get { return width; }
        }


        public FastBitmap(Bitmap src) {
            this.src = (Bitmap)src.Clone();
            width = src.Width;
            height = src.Height;
            BitmapData srcBirmapData = src.LockBits(new Rectangle(0, 0, src.Width, src.Height), ImageLockMode.WriteOnly, src.PixelFormat);
            srcpixels = new byte[srcBirmapData.Stride * src.Height];
            this.Stride = srcBirmapData.Stride;
            Marshal.Copy(srcBirmapData.Scan0, srcpixels, 0, srcpixels.Length);
            src.UnlockBits(srcBirmapData);
        }
        public Color GetPixel(int x, int y) {
            Color c;
            int position = x * 3 + Stride * y;
            byte b = srcpixels[position + 0];
            byte g = srcpixels[position + 1];
            byte r = srcpixels[position + 2];

            c = Color.FromArgb(r, g, b);

            return c;
        }

        public void SetPixel(int x, int y, Color c) {
            int position = x * 3 + Stride * y;
            srcpixels[position + 0] = (byte)c.B;
            srcpixels[position + 1] = (byte)c.G;
            srcpixels[position + 2] = (byte)c.R;
        }

        public Bitmap ToBitmap() {
            Bitmap dst = (Bitmap)src.Clone();
            BitmapData dstData;
            dstData = dst.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, dst.PixelFormat);
            Marshal.Copy(srcpixels, 0, dstData.Scan0, srcpixels.Length);
            dst.UnlockBits(dstData);
            return dst;
        }
    }
}
