using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArknightsBetting.Common {
    public class Utils {
        public static Point GetCenter(Point[] points) {
            var sum = new Point();
            foreach (var e in points) {
                sum.X += e.X;
                sum.Y += e.Y;
            }
            sum.X /= points.Length;
            sum.Y /= points.Length;
            return sum;
        }
    }
}
