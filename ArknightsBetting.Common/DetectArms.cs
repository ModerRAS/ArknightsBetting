using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArknightsBetting.Common {
    public class DetectArms {
        public static int DetectArmsNumber(byte[] ImageBytes) {
            // 高是665和685
            // 长是361-403/439-478/519-556/724-762/799-837/880-918
            var mat = ImageCropper.ByteArrayToMat(ImageBytes);
            var subMats = ImageCropper.BatchCropByRects(mat, new List<OpenCvSharp.Rect>() {
                new OpenCvSharp.Rect(361, 665, 42, 20),
                new OpenCvSharp.Rect(439, 685, 42, 20),
                new OpenCvSharp.Rect(519, 685, 42, 20),
                new OpenCvSharp.Rect(724, 685, 42, 20),
                new OpenCvSharp.Rect(799, 685, 42, 20),
                new OpenCvSharp.Rect(880, 685, 42, 20),
            } );

            return 0; // 返回检测到的数字
        }
    }
}
