using OpenCvSharp;
using OpenCvSharp.Dnn;
using OpenCvSharp.WpfExtensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace SatGS.OpenCV
{
    internal class OpenCV
    {
        private static OpenCV instance;

        public static OpenCV Instance()
        {
            if(instance == null)
            {
                instance = new OpenCV();
            }
            return instance;
        }

        private OpenCV()
        {
            
        }


        public OpenCvSharp.Point[][] GetContourFromImage(Mat image)
        {
            var grayImage = new Mat();
            var binaryImage = new Mat();

            // RGB Image to GrayScale Image
            Cv2.CvtColor(image, grayImage, ColorConversionCodes.BGR2GRAY);

            // GrayScale Image to Binary Image
            Cv2.Threshold(grayImage, binaryImage, 100, 255, ThresholdTypes.Binary);

            //Cv2.ImShow("a", binaryImage);

            Cv2.FindContours(binaryImage, out var contours, out var hierachy, RetrievalModes.Tree, ContourApproximationModes.ApproxTC89KCOS);

            var results = new List<OpenCvSharp.Point[]>();

            var imgSize = image.Size();

            foreach (var p in contours)
            {
                var length = Cv2.ArcLength(p, true);
                var contourSize = Cv2.BoundingRect(p).Size;
                if (length > 100 && imgSize != contourSize)
                    results.Add(p);
            }

            return results.ToArray();
        }


        public BitmapSource ContourDetectionFromImage(Mat image)
        {
            var grayImage = new Mat();
            var binaryImage = new Mat();
            var gausianImage = new Mat();

            // RGB Image to GrayScale Image
            Cv2.CvtColor(image, grayImage, ColorConversionCodes.BGR2GRAY);

            Cv2.GaussianBlur(grayImage, gausianImage, new OpenCvSharp.Size(3, 3), 0);

            Cv2.ImShow("a", gausianImage);

            // GrayScale Image to Binary ImBage
            Cv2.Threshold(gausianImage, binaryImage, 100, 255, ThresholdTypes.Binary);

            

            Cv2.FindContours(binaryImage, out var contours, out var hierachy, RetrievalModes.Tree, ContourApproximationModes.ApproxTC89KCOS);

            //var newCountours = new List<OpenCvSharp.Point[]>();

            var imgSize = image.Size();

            foreach (var p in contours)
            {
                var length = Cv2.ArcLength(p, true);
                var contourSize = Cv2.BoundingRect(p).Size;
                if (length > 100 && imgSize != contourSize)
                    Cv2.Rectangle(image, Cv2.BoundingRect(p), Scalar.Red, 2, LineTypes.AntiAlias);
            }

            //Cv2.DrawContours(image, newCountours, -1, Scalar.Red, 2, LineTypes.AntiAlias, null, 1);

            return WriteableBitmapConverter.ToWriteableBitmap(image);
        }

        public BitmapSource ContourDetectionFromImage(string imagePath)
        {
            return ContourDetectionFromImage(new Mat(imagePath));
        }

        public Mat RemoveImageBackground(Mat image)
        {
            var img_rgb = new Mat();
            Cv2.CvtColor(image, img_rgb, ColorConversionCodes.BGR2RGB);
            return null;
        }

        public Mat RemoveImageBackground(string imagePath)
        {
            return RemoveImageBackground(new Mat(imagePath));
        }
    }
}
