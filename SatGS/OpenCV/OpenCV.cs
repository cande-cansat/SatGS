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

        public BitmapSource DetectionWithYolo3(string imagePath)
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

            return WriteableBitmapConverter.ToWriteableBitmap(image);
        }

        public BitmapSource FindCountour(string imagePath)
        {
            var image = new Mat(imagePath);
            var grayImage = new Mat();
            var binaryImage = new Mat();

            // RGB Image to GrayScale Image
            Cv2.CvtColor(image, grayImage, ColorConversionCodes.BGR2GRAY);
            
            // 여기서 특정 색 제거

            
            // GrayScale Image to Binary Image
            Cv2.Threshold(grayImage, binaryImage, 100, 255, ThresholdTypes.Binary);

            //Cv2.ImShow("a", binaryImage);

            Cv2.FindContours(binaryImage, out var contours, out var hierachy, RetrievalModes.Tree, ContourApproximationModes.ApproxTC89KCOS);

            var newCountours = new List<OpenCvSharp.Point[]>();

            foreach(var p in contours)
            {
                var length = Cv2.ArcLength(p, true);
                if (length > 100)
                    Cv2.Rectangle(image, Cv2.BoundingRect(p), Scalar.Red, 2, LineTypes.AntiAlias);
            }

            //Cv2.DrawContours(image, newCountours, -1, Scalar.Red, 2, LineTypes.AntiAlias, null, 1);

            return WriteableBitmapConverter.ToWriteableBitmap(image);
        }
    }
}
