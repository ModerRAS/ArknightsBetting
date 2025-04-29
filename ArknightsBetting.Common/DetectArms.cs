using OpenCvSharp;
using System.Text.RegularExpressions;

namespace ArknightsBetting.Common {
    public class DetectArms {
        static readonly string pattern = @"[×xX＊*][\s\-:：]*([0-9]{1,3})";
        public static int GetNumber(string text) {
            foreach (Match match in Regex.Matches(text, pattern)) {
                return int.Parse(match.Groups[1].Value);
            }
            throw new ArgumentNullException("Cannot detect number");
        }
        
        /// <summary>
        /// 为OCR优化的图像预处理：放大、灰度化、自适应二值化、可选膨胀
        /// </summary>
        /// <param name="src">原始图像（彩色或灰度）</param>
        /// <param name="scale">放大倍数（推荐 2~3）</param>
        /// <param name="applyDilation">是否进行膨胀操作（用于断笔连通）</param>
        /// <returns>预处理后的Mat（二值图）</returns>
        public static Mat PreprocessForOCR(Mat src, double scale = 20.0, bool applyDilation = true) {
            if (src.Empty())
                throw new ArgumentException("输入图像为空！");

            // 1. 放大图像
            Mat enlarged = new Mat();
            Cv2.Resize(src, enlarged, new OpenCvSharp.Size(), scale, scale, InterpolationFlags.Linear);

            // 2. 转为灰度
            Mat gray = new Mat();
            if (enlarged.Channels() == 3)
                Cv2.CvtColor(enlarged, gray, ColorConversionCodes.BGR2GRAY);
            else
                gray = enlarged.Clone();

            // 3. 自适应二值化
            Mat binary = new Mat();
            Cv2.AdaptiveThreshold(
                gray, binary,
                maxValue: 255,
                adaptiveMethod: AdaptiveThresholdTypes.GaussianC,
                thresholdType: ThresholdTypes.BinaryInv,
                blockSize: 11,
                c: 2
            );

            // 4. 可选：膨胀操作，填补断裂字符笔画
            if (applyDilation) {
                Mat kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new OpenCvSharp.Size(2, 2));
                Cv2.Dilate(binary, binary, kernel);
            }

            return binary;
        }
        /// <summary>
        /// 提取HSV空间中符合范围的白色区域
        /// </summary>
        /// <param name="inputMat">输入彩色Mat图像</param>
        /// <returns>白色区域二值化后的Mat</returns>
        public static Mat ExtractWhiteByHSV(Mat inputMat) {
            if (inputMat.Empty())
                throw new ArgumentException("输入的Mat是空的！");

            // 1. 转HSV
            Mat hsv = new Mat();
            Cv2.CvtColor(inputMat, hsv, ColorConversionCodes.BGR2HSV);

            // 2. 设定HSV白色范围
            Scalar lowerWhite = new Scalar(0, 0, 200);   // H, S, V 低界
            Scalar upperWhite = new Scalar(180, 30, 255); // H, S, V 上界

            // 这里根据实际情况调，如果白色带点其他色相，可以适当放宽H范围或S范围

            // 3. InRange 二值化
            Mat mask = new Mat();
            Cv2.InRange(hsv, lowerWhite, upperWhite, mask);

            return mask;
        }
        /// <summary>
        /// 将输入Mat转为灰度，并二值化提取白色部分
        /// </summary>
        /// <param name="inputMat">输入Mat图像</param>
        /// <param name="thresholdValue">二值化阈值，默认128</param>
        /// <returns>处理后的Mat（白色部分为255，黑色为0）</returns>
        public static Mat ExtractWhiteAreas(Mat inputMat, double thresholdValue = 128) {
            if (inputMat.Empty())
                throw new ArgumentException("输入的Mat是空的！");

            // 1. 转灰度
            Mat gray = new Mat();
            if (inputMat.Channels() == 3 || inputMat.Channels() == 4) {
                Cv2.CvtColor(inputMat, gray, ColorConversionCodes.BGR2GRAY);
            } else {
                gray = inputMat.Clone();
            }

            // 2. 二值化
            Mat binary = new Mat();
            Cv2.Threshold(gray, binary, thresholdValue, 255, ThresholdTypes.Binary);

            return binary;
        }
        public static int[] DetectArmsNumber(byte[] ImageBytes) {
            // 高是665和685
            // 长是361-403/439-478/519-556/724-762/799-837/880-918
            var mat = ImageCropper.ByteArrayToMat(ImageBytes);
            var subMats = ImageCropper.BatchCropByRects(mat, new List<OpenCvSharp.Rect>() {
                new OpenCvSharp.Rect(361, 665, 42, 20),
                new OpenCvSharp.Rect(439, 665, 42, 20),
                new OpenCvSharp.Rect(519, 665, 42, 20),
                new OpenCvSharp.Rect(724, 665, 42, 20),
                new OpenCvSharp.Rect(799, 665, 42, 20),
                new OpenCvSharp.Rect(880, 665, 42, 20),
            } );
            using var ocr = new OCRTool();
            var numberList = new int[6];
            for (int i = 0; i < subMats.Count; i++)
            {
                try {
                    var tmp = ExtractWhiteByHSV(subMats[i]);
                    Cv2.ImWrite(i + ".png", tmp); // 保存裁剪后的图像以供调试
                    var text = ocr.GetText(tmp);
                    var number = GetNumber(text);
                    numberList[i] = number;

                } catch (Exception ex) {
                    Console.WriteLine($"Error processing image {i}: {ex.Message}");
                    numberList[i] = 0;
                    continue; // 继续处理下一个图像
                }
                
            }
            return numberList; // 返回检测到的数字
        }
    }
}
