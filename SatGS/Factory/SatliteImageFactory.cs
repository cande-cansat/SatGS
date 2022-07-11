using SatGS.Socket;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SatGS.Factory
{
    internal static class SatliteImageFactory
    {
        const int ImgWidth = 640;
        const int ImgHeight = 480;
        public static Bitmap Create(PacketData packet)
        {
            var imgData = new byte[ImgWidth * ImgHeight * 4];

            for (int y = 0; y < ImgHeight; ++y)
            {
                for (int x = 0; x < ImgWidth; ++x)
                {
                    var value = packet.Data[y * ImgWidth + x + 1];
                    imgData[(y * 4) * ImgWidth + (x * 4 + 0)] = value;
                    imgData[(y * 4) * ImgWidth + (x * 4 + 1)] = value;
                    imgData[(y * 4) * ImgWidth + (x * 4 + 2)] = value;
                    imgData[(y * 4) * ImgWidth + (x * 4 + 3)] = 0;
                }
            }

            Bitmap image;
            unsafe
            {
                image = new Bitmap(ImgWidth, ImgHeight, PixelFormat.Format32bppRgb);
                BitmapData bmpData = image.LockBits(
                    new Rectangle(0, 0, image.Width, image.Height),
                    ImageLockMode.WriteOnly, image.PixelFormat);

                Marshal.Copy(imgData, 0, bmpData.Scan0, imgData.Length);

                image.UnlockBits(bmpData);
            }

            GC.Collect(0, GCCollectionMode.Forced);
            GC.WaitForFullGCComplete();


            return image;
        }
    }
}
