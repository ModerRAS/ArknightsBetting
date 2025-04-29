using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArknightsBetting.Common.Test {
    [TestClass]
    public class DetectArmsTest {
        [TestMethod]
        public void TestDetectNumber() {
            var imageBytes = File.ReadAllBytes("testdata/192_168_121_147_5555_20250428154920_Start.jpg");
            var list = DetectArms.DetectArmsNumber(imageBytes);
            //Assert.AreEqual(6, list[0]);
            //Assert.AreEqual(8, list[1]);
            //Assert.AreEqual(4, list[2]);
            //Assert.AreEqual(2, list[3]);
            //Assert.AreEqual(13, list[4]);
            //Assert.AreEqual(3, list[5]);
        }
    }
}
