using OpenCvSharp;
using OpenCvSharp.XFeatures2D;
using System;
using System.Collections.Generic;
using System.Linq;
namespace ArknightsBetting.Common;
public class DetectedObject {
    public Mat Image { get; set; }  // 对象图像
    public Rect Position { get; set; } // 在原图中的位置
}
public static class OpenCVHelper {
    static OpenCVHelper() {
        for (int i = 1; i < 10; i++) {
            templateImages.Add(i, Cv2.ImRead($"template/{i}.jpg"));
        }
    }
    public static Dictionary<int, Mat> templateImages { get; set; } = new ();
    public static int MatchToBestTemplate(Mat sourceImage) {
        // 创建 SURF 特征检测器和描述子提取器
        var surf = SURF.Create(300, 4, 2, true);

        // 提取所有模板图像的特征点和描述子
        List<KeyPoint> keyPoints = new List<KeyPoint>();
        List<Mat> descriptors = new List<Mat>();
        foreach (var kvp in templateImages) {
            KeyPoint[] kp;
            Mat desc = new Mat();
            surf.DetectAndCompute(kvp.Value, null, out kp, desc);
            keyPoints.AddRange(kp);
            descriptors.Add(desc);
        }

        // 提取源图像的特征点和描述子
        KeyPoint[] keyPointsSource;
        Mat descriptorsSource = new Mat();
        surf.DetectAndCompute(sourceImage, null, out keyPointsSource, descriptorsSource);

        // 使用暴力匹配器进行特征点匹配
        var matcher = new BFMatcher(NormTypes.L2, false);
        List<DMatch[]> matchesList = new List<DMatch[]>();
        foreach (var descriptor in descriptors) {
            DMatch[] matches = matcher.Match(descriptorsSource, descriptor);
            matchesList.Add(matches);
        }

        // 计算匹配得分
        double[] scores = new double[templateImages.Count];
        for (int i = 0; i < templateImages.Count; i++) {
            scores[i] = matchesList[i].Average(m => m.Distance);
        }

        // 找到得分最低的模板图索引
        int bestMatchIndex = Array.IndexOf(scores, scores.Min());

        // 返回对应的数字
        return templateImages.ElementAt(bestMatchIndex).Key;
    }
    public static List<DetectedObject> SplitObjectsWithPosition(Mat inputImage, int minArea = 1) {
        // 灰度转换
        using Mat gray = new Mat();
        Cv2.CvtColor(inputImage, gray, ColorConversionCodes.BGR2GRAY);

        // 二值化处理
        using Mat binary = new Mat();
        Cv2.Threshold(gray, binary, 0, 255, ThresholdTypes.BinaryInv | ThresholdTypes.Otsu);

        // 查找轮廓
        Point[][] contours;
        HierarchyIndex[] hierarchy;
        Cv2.FindContours(
            binary,
            out contours,
            out hierarchy,
            RetrievalModes.External,
            ContourApproximationModes.ApproxSimple);

        List<DetectedObject> results = new List<DetectedObject>();

        foreach (var contour in contours) {
            // 面积过滤
            double area = Cv2.ContourArea(contour);
            if (area < minArea) continue;

            // 获取包围矩形
            Rect rect = Cv2.BoundingRect(contour);

            // 创建对象掩膜
            using Mat mask = new Mat(inputImage.Size(), MatType.CV_8UC1, Scalar.Black);
            Cv2.DrawContours(mask, new[] { contour }, 0, Scalar.White, -1);

            // 提取并裁剪对象
            Mat objImage = new Mat(inputImage.Size(), inputImage.Type(), Scalar.Black);
            inputImage.CopyTo(objImage, mask);
            Mat cropped = new Mat(objImage, rect);

            // 存储结果（注意克隆图像数据）
            results.Add(new DetectedObject {
                Image = cropped.Clone(), // 必须克隆，否则释放mask后数据会失效
                Position = rect
            });
        }
        return results;
    }
    public static List<Mat> SplitObjects(Mat inputImage, int minArea = 1) {
        // 转换为灰度图像
        using Mat gray = new Mat();
        Cv2.CvtColor(inputImage, gray, ColorConversionCodes.BGR2GRAY);

        // 二值化处理（确保黑底白字）
        using Mat binary = new Mat();
        Cv2.Threshold(gray, binary, 0, 255, ThresholdTypes.BinaryInv | ThresholdTypes.Otsu);

        // 查找轮廓
        Point[][] contours;
        HierarchyIndex[] hierarchy;
        Cv2.FindContours(
            binary,
            out contours,
            out hierarchy,
            RetrievalModes.External,
            ContourApproximationModes.ApproxSimple);

        List<Mat> objects = new List<Mat>();

        foreach (var contour in contours) {
            // 过滤小面积区域
            double area = Cv2.ContourArea(contour);
            if (area < minArea) continue;

            // 创建对象掩膜
            Mat mask = Mat.Zeros(binary.Size(), MatType.CV_8UC1);
            Cv2.DrawContours(mask, new[] { contour }, 0, Scalar.White, -1);

            // 提取原始图像中的对象
            Mat result = new Mat();
            inputImage.CopyTo(result, mask);

            // 获取边界框并裁剪
            Rect rect = Cv2.BoundingRect(contour);
            Mat cropped = new Mat(result, rect);

            objects.Add(cropped);
        }


        return objects;
    }
    static int FindBestMatchingTemplate(Mat sourceImage) {
        // 记录每张模板图像的匹配结果
        Dictionary<int, double> matchScores = new ();

        // 循环匹配每张模板图像
        foreach (var templateImage in templateImages) {
            // 创建结果图像
            Mat resultImage = new Mat();
            
            // 进行模板匹配
            Cv2.MatchTemplate(
                sourceImage.CvtColor(ColorConversionCodes.BGR2HSV),
                templateImage.Value.CvtColor(ColorConversionCodes.BGR2HSV), 
                resultImage,
                TemplateMatchModes.CCoeffNormed
            );

            // 归一化处理
            //Cv2.Normalize(resultImage, resultImage, 0, 1, NormTypes.MinMax, MatType.CV_32F);

            // 获取最大值位置
            double minValue, maxValue;
            Point minLocation, maxLocation;
            Cv2.MinMaxLoc(resultImage, out minValue, out maxValue, out minLocation, out maxLocation);

            // 记录匹配分数
            matchScores.Add(templateImage.Key, maxValue);
        }

        var max = matchScores.First();
        foreach (var e in matchScores) {
            if (e.Value > max.Value) {
                max = e;
            }
        }
        // 返回最匹配的模板图像
        return max.Key;
    }

    public static NumberPhoto Detect(NumberPhoto image) {
        var number = FindBestMatchingTemplate(Mat.FromImageData(image.Photo));
        return new NumberPhoto() {
            Number = number,
            Photo = image.Photo,
            Points = image.Points
        };
    }
}
