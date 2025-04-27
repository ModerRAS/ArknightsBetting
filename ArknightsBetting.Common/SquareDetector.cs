using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;

namespace ArknightsBetting.Common {

    public class SquareDetector {
        private string imagePath;

        public SquareDetector(string imagePath) {
            this.imagePath = imagePath;
        }


        public List<Point[]> DetectAndDrawSquare(string outputPath) {
            // 读取图像
            Mat image = Cv2.ImRead(imagePath);

            // 将图像转换为HSV颜色空间
            Mat hsvImage = new Mat();
            Cv2.CvtColor(image, hsvImage, ColorConversionCodes.BGR2HSV);

            // 定义颜色阈值范围
            Scalar lowerGreen = new Scalar(140, 70, 40); // 偏绿色的阈值范围
            Scalar upperGreen = new Scalar(170, 90, 60);
            Scalar lowerWhite = new Scalar(0, 0, 200); // 偏白色的阈值范围
            Scalar upperWhite = new Scalar(180, 30, 255);

            // 创建掩模
            Mat greenMask = new Mat();
            Mat whiteMask = new Mat();
            Cv2.InRange(hsvImage, lowerGreen, upperGreen, greenMask);
            Cv2.InRange(hsvImage, lowerWhite, upperWhite, whiteMask);

            // 合并两个掩模
            Mat mask = greenMask + whiteMask;

            // 使用形态学操作找到正方形
            Mat kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(5, 5));
            Cv2.MorphologyEx(mask, mask, MorphTypes.Close, kernel);

            // 在掩模中寻找轮廓
            Point[][] contours;
            HierarchyIndex[] hierarchy;
            Cv2.FindContours(mask, out contours, out hierarchy, RetrievalModes.List, ContourApproximationModes.ApproxSimple);

            var Curves = new List<Point[]>();

            // 寻找正方形
            foreach (var contour in contours) {
                var approxCurve = Cv2.ApproxPolyDP(contour, Cv2.ArcLength(contour, true) * 0.02, true);
                if (approxCurve.Length == 4 && Cv2.IsContourConvex(approxCurve)) {
                    Curves.Add(approxCurve);
                }
            }

            var FilteredCurves = new List<Point[]>();

            foreach (var CurvePoints in Curves) {
                var isInternal = false;
                foreach (var points in Curves) {
                    var pointsTest = CurvePoints.Sum(curvePoint => {
                        return Cv2.PointPolygonTest(points, curvePoint, false);
                    });
                    if (pointsTest > 0) {
                        isInternal = true;
                    }

                }
                //var result = Parallel.ForEach(Curves, points => {
                //    var pointsTest = approxCurve.Select(curvePoint => {
                //        var num = Cv2.PointPolygonTest(points, curvePoint, false);
                //        Console.WriteLine(num);
                //        return num;
                //    });

                //});
                // 检测到正方形，绘制轮廓
                if (!isInternal) {
                    Cv2.DrawContours(image, new Point[][] { CurvePoints }, 0, Scalar.Red, 2);

                    //Cv2.DrawMarker(image, GetCenter(CurvePoints), Scalar.Blue);
                    if (Math.Abs(Utils.GetCenter(CurvePoints).X - CurvePoints[0].X) > 20 && 
                        Math.Abs(Utils.GetCenter(CurvePoints).Y - CurvePoints[0].Y) > 20) {
                        FilteredCurves.Add(CurvePoints);
                    }
                }
            }



            // 保存结果图像
            Cv2.ImWrite(outputPath, image);

            return FilteredCurves;
        }
        public List<NumberPhoto> GetSmallImagesAsByteArrays(List<Point[]> boxes) {
            Mat image = Cv2.ImRead(imagePath);
            List<NumberPhoto> imageBytesList = new List<NumberPhoto>();

            for (int i = 0; i < boxes.Count; i++) {
                Rect boundingRect = Cv2.BoundingRect(boxes[i]);
                Mat smallImage = image[boundingRect];
                // 将图像编码为字节数组
                //Cv2.ImWrite($"D:\\WorkSpace\\CSharp\\ArknightsBetting\\TestResults\\Deploy_ModerRAS 20240504T192923\\Out\\{DateTime.Now.Millisecond}.jpg", smallImage);
                Cv2.ImEncode(".jpg", smallImage, out byte[] imageBytes);
                imageBytesList.Add(new NumberPhoto { Points = boxes[i], Photo = imageBytes });
            }

            return imageBytesList;
        }
    }

}
