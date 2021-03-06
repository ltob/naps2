﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;

namespace NAPS2.Scan.Images.Transforms
{
    [Serializable]
    public class HueTransform : Transform
    {
        public int HueShift { get; set; }

        public override Bitmap Perform(Bitmap bitmap)
        {
            int bytesPerPixel;
            if (bitmap.PixelFormat == PixelFormat.Format24bppRgb)
            {
                bytesPerPixel = 3;
            }
            else if (bitmap.PixelFormat == PixelFormat.Format32bppArgb)
            {
                bytesPerPixel = 4;
            }
            else
            {
                // No need to handle 1bpp since hue shifts are null transforms
                return bitmap;
            }

            double hueShiftAdjusted = HueShift / 2000.0 * 360;
            if (hueShiftAdjusted < 0)
            {
                hueShiftAdjusted += 360;
            }
            
            var data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
            var stride = Math.Abs(data.Stride);
            for (int y = 0; y < data.Height; y++)
            {
                for (int x = 0; x < data.Width; x++)
                {
                    int r = Marshal.ReadByte(data.Scan0 + stride * y + x * bytesPerPixel);
                    int g = Marshal.ReadByte(data.Scan0 + stride * y + x * bytesPerPixel + 1);
                    int b = Marshal.ReadByte(data.Scan0 + stride * y + x * bytesPerPixel + 2);

                    Color c = Color.FromArgb(255, r, g, b);
                    double h, s, v;
                    ColorHelper.ColorToHSV(c, out h, out s, out v);

                    h = (h + hueShiftAdjusted) % 360;

                    c = ColorHelper.ColorFromHSV(h, s, v);

                    Marshal.WriteByte(data.Scan0 + stride * y + x * bytesPerPixel, c.R);
                    Marshal.WriteByte(data.Scan0 + stride * y + x * bytesPerPixel + 1, c.G);
                    Marshal.WriteByte(data.Scan0 + stride * y + x * bytesPerPixel + 2, c.B);
                }
            }
            bitmap.UnlockBits(data);
            
            return bitmap;
        }

        public override bool IsNull => HueShift == 0;
    }
}
