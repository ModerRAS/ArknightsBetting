using OpenCvSharp;
using Sdcb.PaddleInference;
using Sdcb.PaddleOCR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sdcb.PaddleOCR.Models.Online;

namespace ArknightsBetting.Common {
    public class OCRTool : IDisposable {
        public PaddleOcrAll all { get; set; }
        public OCRTool() {
            InitModel().Wait();
        }
        public async Task InitModel() {
            Sdcb.PaddleOCR.Models.Online.Settings.GlobalModelDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var model = await OnlineFullModels.ChineseV3.DownloadAsync();
            all = new PaddleOcrAll(model,
                PaddleDevice.Mkldnn()
                ) {
                AllowRotateDetection = true, /* 允许识别有角度的文字 */
                Enable180Classification = false, /* 允许识别旋转角度大于90度的文字 */
            };
        }
        public void Dispose() {
            all.Dispose();
        }
        public PaddleOcrResult GetOcrResult(byte[] image) {
            using (Mat src = Cv2.ImDecode(image, ImreadModes.Color)) {
                PaddleOcrResult result = all.Run(src);
                return result;
            }
        }
        public PaddleOcrResult GetOcrResult(Mat image) {
            PaddleOcrResult result = all.Run(image);
            return result;
        }
        public PaddleOcrResult Result { get; set; }
        public void SetImage(byte[] image) {
            Result = GetOcrResult(image);
        }
        public string GetText(Mat image) {
            Result = GetOcrResult(image);
            return Result.Text;
        }
        public Point GetStringPoint(string str) { 
            foreach (var region in Result.Regions) {
                if (region.Text.Equals(str)) {
                    return new Point() {
                        X = (int)region.Rect.Center.X,
                        Y = (int)region.Rect.Center.Y
                    };
                }
            }
            return new Point() { X = 0, Y = 0 };
        }

        public List<Point> GetStringPoints(string str) {
            var list = new List<Point>();
            foreach (var region in Result.Regions) {
                if (region.Text.Contains(str)) {
                    list.Add(new Point() {
                        X = (int)region.Rect.Center.X,
                        Y = (int)region.Rect.Center.Y
                    });
                }
            }
            return list;
        }

        public bool Contains(string str) {
            var list = new List<Point>();
            foreach (var region in Result.Regions) {
                if (region.Text.Contains(str)) {
                    list.Add(new Point() {
                        X = (int)region.Rect.Center.X,
                        Y = (int)region.Rect.Center.Y
                    });
                }
            }
            return list.Count > 0;
        }
        public int ConvertToResults(PaddleOcrResult paddleOcrResult) {
            foreach (var region in paddleOcrResult.Regions) {
                if (int.TryParse(region.Text, out var result)) {
                    return result;
                }
            }
            return (int)ErrorNumber.CannotDetectNumber;
        }
        public NumberPhoto Detect(NumberPhoto image) {
            var number = ConvertToResults(GetOcrResult(image.Photo));
            return new NumberPhoto() {
                Number = number,
                Photo = image.Photo,
                Points = image.Points
            };
        }
    }
}
