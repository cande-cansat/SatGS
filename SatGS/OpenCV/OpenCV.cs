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
            string[] pathes =
            {
#if DEBUG
                "../../darknet/",
#endif
                "darknet/"
            };

            string darknetRoot = string.Empty;

            foreach(var path in pathes)
            {
                if (Directory.Exists(path))
                {
                    darknetRoot = new DirectoryInfo(path).FullName;
                    break;
                }
            }

            if (string.IsNullOrEmpty(darknetRoot))
            {
                MessageBox.Show("darknet model이 존재하지 않습니다.");
            }
            else
            {
                yoloCfg = $"{darknetRoot}yolov3.cfg";
                yoloModel = $"{darknetRoot}yolov3.weights";
                classNames = File.ReadAllLines($"{darknetRoot}yolov3.txt");
                darknet = Net.ReadNetFromDarknet(yoloCfg, yoloModel);
                modelLoaded = true;
            }
        }

        private bool modelLoaded = false;
        private string yoloCfg;
        private string yoloModel;
        private Net darknet;
        private string[] classNames;

        public BitmapSource ObjectDetectionFromImage(string imagePath)
        {
            if (!modelLoaded) 
                return new BitmapImage(new Uri(imagePath));

            var labels = new List<string>();
            var scores = new List<float>();
            var bboxes = new List<OpenCvSharp.Rect>();

            var image = new Mat(imagePath);
            var inputBlob = CvDnn.BlobFromImage(image, 1 / 255f, new OpenCvSharp.Size(416, 416), crop: false);

            darknet.SetInput(inputBlob);
            var outBlobNames = darknet.GetUnconnectedOutLayersNames();
            var outputBlobs = outBlobNames.Select(toMat => new Mat()).ToArray();

            darknet.Forward(outputBlobs, outBlobNames);

            foreach(var prob in outputBlobs)
            {
                for (int p = 0; p < prob.Rows; ++p)
                {
                    var confidence = prob.At<float>(p, 4);

                    // 여기가 어떤물체인지 확률, 현재 90% 초과하는 물체에 대해서만 rect를 그린다.
                    if (confidence <= 0.9) continue;

                    Cv2.MinMaxLoc(prob.Row(p).ColRange(5, prob.Cols), out _, out _, out _, out var classNumber);

                    var classes = classNumber.X;
                    var probability = prob.At<float>(p, classes + 5);

                    if (probability <= 0.9) continue;

                    var centerX = prob.At<float>(p, 0) * image.Width;
                    var centerY = prob.At<float>(p, 1) * image.Height;
                    var width   = prob.At<float>(p, 2) * image.Width;
                    var height  = prob.At<float>(p, 3) * image.Height;

                    labels.Add(classNames[classes]);
                    scores.Add(probability);
                    bboxes.Add(new OpenCvSharp.Rect(
                        (int)centerX - (int)width  / 2, 
                        (int)centerY - (int)height / 2, 
                        (int)width, 
                        (int)height
                    ));
                }
            }

            CvDnn.NMSBoxes(bboxes, scores, 0.9f, 0.5f, out var indices);

            foreach(var i in indices)
            {
                Cv2.Rectangle(image, bboxes[i], Scalar.Magenta, 2);
                Cv2.PutText(image, labels[i], bboxes[i].Location, HersheyFonts.HersheyComplex, 2, Scalar.Magenta, 2);
            }

            return image.ToBitmapSource();
        }
        public BitmapSource ObjectDetectionFromImage2(string imagePath)
        {
            if (!modelLoaded)
                return new BitmapImage(new Uri(imagePath));

            var labels = new List<string>();
            var scores = new List<float>();
            var bboxes = new List<OpenCvSharp.Rect>();

            var image = new Mat(imagePath);
            var inputBlob = CvDnn.BlobFromImage(image, 1 / 255f, new OpenCvSharp.Size(416, 416), crop: false);

            darknet.SetInput(inputBlob);
            var outBlobNames = darknet.GetUnconnectedOutLayersNames();
            var outputBlobs = outBlobNames.Select(toMat => new Mat()).ToArray();

            darknet.Forward(outputBlobs, outBlobNames);

            foreach (var prob in outputBlobs)
            {
                for (int p = 0; p < prob.Rows; ++p)
                {
                    var confidence = prob.At<float>(p, 4);

                    // 여기가 어떤물체인지 확률, 현재 90% 초과하는 물체에 대해서만 rect를 그린다.
                    if (confidence <= 0.4) continue;

                    Cv2.MinMaxLoc(prob.Row(p).ColRange(5, prob.Cols), out _, out _, out _, out var classNumber);

                    var classes = classNumber.X;
                    var probability = prob.At<float>(p, classes + 5);

                    var centerX = prob.At<float>(p, 0) * image.Width;
                    var centerY = prob.At<float>(p, 1) * image.Height;
                    var width = prob.At<float>(p, 2) * image.Width;
                    var height = prob.At<float>(p, 3) * image.Height;

                    if (probability > 0.9)
                    {
                        labels.Add(classNames[classes]);
                        scores.Add(probability);
                    }
                    else
                    {
                        labels.Add("Unknown");
                        scores.Add(1);
                    }

                    bboxes.Add(new OpenCvSharp.Rect(
                        (int)centerX - (int)width / 2,
                        (int)centerY - (int)height / 2,
                        (int)width,
                        (int)height
                    ));
                }
            }

            CvDnn.NMSBoxes(bboxes, scores, 0.9f, 0.5f, out var indices);

            foreach (var i in indices)
            {
                Cv2.Rectangle(image, bboxes[i], Scalar.Magenta, 2);
                Cv2.PutText(image, labels[i], bboxes[i].Location, HersheyFonts.HersheyComplex, 2, Scalar.Magenta, 2);
            }

            return image.ToBitmapSource();
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

        public BitmapSource ObjectDetectionFromImageWithContours(string imagePath)
        {
            var image = new Mat(imagePath);
            var contours = GetContourFromImage(image);

            foreach (var contour in contours)
            {
                var rect = Cv2.BoundingRect(contour);
                var cropped = image.SubMat(rect);
                var inputBlob = CvDnn.BlobFromImage(cropped, 1 / 255f, new OpenCvSharp.Size(416, 416), crop: false);

                Cv2.ImShow("a", cropped);
                MessageBox.Show("a", "a");

                darknet.SetInput(inputBlob);

                var outBlobNames = darknet.GetUnconnectedOutLayersNames();
                var outputBlobs = outBlobNames.Select(toMat => new Mat()).ToArray();

                darknet.Forward(outputBlobs, outBlobNames);

                var labels = new List<string>();
                var scores = new List<float>();
                var bboxes = new List<OpenCvSharp.Rect>();

                foreach (var prob in outputBlobs)
                {
                    var label = new List<string>();
                    var score = new List<float>();
                    var bbox = new List<OpenCvSharp.Rect>();

                    for (int p = 0; p < prob.Rows; ++p)
                    {
                        var confidence = prob.At<float>(p, 4);

                        // 여기가 어떤물체인지 확률, 현재 90% 초과하는 물체에 대해서만 rect를 그린다.
                        //if (confidence <= 0.9) continue;

                        Cv2.MinMaxLoc(prob.Row(p).ColRange(5, prob.Cols), out _, out _, out _, out var classNumber);

                        var classes = classNumber.X;
                        var probability = prob.At<float>(p, classes + 5);

                        var centerX = prob.At<float>(p, 0) * image.Width;
                        var centerY = prob.At<float>(p, 1) * image.Height;
                        var width = prob.At<float>(p, 2) * image.Width;
                        var height = prob.At<float>(p, 3) * image.Height;

                        if(probability >= 0.9)
                        {
                            labels.Add(classNames[classes]);
                            scores.Add(probability);
                            bboxes.Add(new OpenCvSharp.Rect(
                                (int)centerX - (int)width / 2,
                                (int)centerY - (int)height / 2,
                                (int)width,
                                (int)height
                            ));
                        }
                        else
                        {
                            labels.Add("Unknown");
                            scores.Add(1);
                            bboxes.Add(new OpenCvSharp.Rect(
                                rect.X + (int)centerX - (int)width / 2,
                                rect.Y + (int)centerY - (int)height / 2,
                                (int)width,
                                (int)height
                            ));
                        }
                    }
                }

                CvDnn.NMSBoxes(bboxes, scores, 0.9f, 0.5f, out var indices);

                var maxSize = 0;
                var maxIdx = -1;

                foreach (var i in indices)
                {
                    /*
                    Cv2.Rectangle(image, bboxes[i], Scalar.Magenta, 1);
                    Cv2.PutText(image, labels[i], bboxes[i].Location, HersheyFonts.HersheyComplex, 1, Scalar.Magenta, 1);
                    */
                    var size = bboxes[i].Width + bboxes[i].Height;
                    if (maxSize < size)
                    {
                        maxSize = size;
                        maxIdx = i;
                    }
                }
                if(maxIdx != -1)
                {
                    Cv2.Rectangle(image, bboxes[maxIdx], Scalar.Magenta, 1);
                    Cv2.PutText(image, labels[maxIdx], bboxes[maxIdx].Location, HersheyFonts.HersheyComplex, 1, Scalar.Magenta, 1);
                }
            }

            return image.ToBitmapSource();
        }

        public BitmapSource ContourDetectionFromImage(string imagePath)
        {
            var image = new Mat(imagePath);
            var grayImage = new Mat();
            var binaryImage = new Mat();

            // RGB Image to GrayScale Image
            Cv2.CvtColor(image, grayImage, ColorConversionCodes.BGR2GRAY);
            
            // GrayScale Image to Binary Image
            Cv2.Threshold(grayImage, binaryImage, 100, 255, ThresholdTypes.Binary);

            //Cv2.ImShow("a", binaryImage);

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

        
    }
}
