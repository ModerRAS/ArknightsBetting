using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;

namespace ArknightsBetting.Common {

    public static class ImageCropper {

        public static Mat ByteArrayToMat(byte[] imageBytes) {
            if (imageBytes == null || imageBytes.Length == 0) {
                throw new ArgumentException("图像数据为空！");
            }

            Mat img = Cv2.ImDecode(imageBytes, ImreadModes.Color); // 解码成彩色图片
            if (img.Empty()) {
                throw new InvalidOperationException("图像解码失败，可能是无效的图片数据！");
            }
            return img;
        }

        /// <summary>
        /// 根据左上角和宽高裁剪图片（标准矩形裁剪）
        /// </summary>
        /// <param name="src">源图</param>
        /// <param name="roi">裁剪区域(Rectangle)</param>
        /// <returns>裁剪后的子图</returns>
        public static Mat CropByRect(Mat src, Rect roi) {
            if (roi.X >= 0 && roi.Y >= 0 && roi.Right <= src.Width && roi.Bottom <= src.Height) {
                return new Mat(src, roi);
            } else {
                throw new ArgumentException("裁剪区域超出图片范围！");
            }
        }

        /// <summary>
        /// 根据中心点裁剪图片（GetRectSubPix方式）
        /// </summary>
        /// <param name="src">源图</param>
        /// <param name="center">中心点</param>
        /// <param name="size">裁剪的尺寸</param>
        /// <returns>裁剪后的子图</returns>
        public static Mat CropByCenter(Mat src, Point2f center, Size size) {
            if (center.X - size.Width / 2 < 0 || center.Y - size.Height / 2 < 0 ||
                center.X + size.Width / 2 > src.Width || center.Y + size.Height / 2 > src.Height) {
                throw new ArgumentException("中心裁剪区域超出图片范围！");
            }

            Mat cropped = new Mat();
            Cv2.GetRectSubPix(src, size, center, cropped);
            return cropped;
        }

        /// <summary>
        /// 批量裁剪多个矩形区域
        /// </summary>
        /// <param name="src">源图</param>
        /// <param name="rectList">矩形列表</param>
        /// <returns>裁剪结果列表</returns>
        public static List<Mat> BatchCropByRects(Mat src, List<Rect> rectList) {
            var result = new List<Mat>();
            foreach (var rect in rectList) {
                result.Add(CropByRect(src, rect));
            }
            return result;
        }

        /// <summary>
        /// 批量裁剪多个中心点区域
        /// </summary>
        /// <param name="src">源图</param>
        /// <param name="centerList">中心点列表</param>
        /// <param name="cropSize">每个裁剪块的尺寸</param>
        /// <returns>裁剪结果列表</returns>
        public static List<Mat> BatchCropByCenters(Mat src, List<Point2f> centerList, Size cropSize) {
            var result = new List<Mat>();
            foreach (var center in centerList) {
                result.Add(CropByCenter(src, center, cropSize));
            }
            return result;
        }
    }

}
