using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenCvSharp;
using System;
using System.Collections.Generic;

namespace ArknightsBetting.Common.Test {
    [TestClass]
    public class ImageCropperUnitTests {
        private Mat testImage;

        [TestInitialize]
        public void Setup() {
            // 创建一个100x100纯白色测试图片
            testImage = new Mat(new Size(100, 100), MatType.CV_8UC3, Scalar.White);
        }

        [TestCleanup]
        public void Cleanup() {
            testImage.Dispose();
        }

        [TestMethod]
        public void TestCropByRect_ValidRect_ReturnsCorrectSize() {
            // Arrange
            var roi = new Rect(10, 10, 30, 40);

            // Act
            Mat cropped = ImageCropper.CropByRect(testImage, roi);

            // Assert
            Assert.IsNotNull(cropped);
            Assert.AreEqual(30, cropped.Width);
            Assert.AreEqual(40, cropped.Height);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestCropByRect_InvalidRect_ThrowsException() {
            // Arrange
            var roi = new Rect(90, 90, 20, 20); // 超出边界

            // Act
            var cropped = ImageCropper.CropByRect(testImage, roi);

            // Assert - [ExpectedException] 会自动验证异常
        }

        [TestMethod]
        public void TestCropByCenter_ValidCenter_ReturnsCorrectSize() {
            // Arrange
            var center = new Point2f(50, 50);
            var size = new Size(20, 20);

            // Act
            Mat cropped = ImageCropper.CropByCenter(testImage, center, size);

            // Assert
            Assert.IsNotNull(cropped);
            Assert.AreEqual(20, cropped.Width);
            Assert.AreEqual(20, cropped.Height);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestCropByCenter_InvalidCenter_ThrowsException() {
            // Arrange
            var center = new Point2f(5, 5); // 太靠近边界
            var size = new Size(20, 20);

            // Act
            var cropped = ImageCropper.CropByCenter(testImage, center, size);

            // Assert - [ExpectedException] 会自动验证异常
        }

        [TestMethod]
        public void TestBatchCropByRects_ValidRects_ReturnsCorrectNumber() {
            // Arrange
            var rects = new List<Rect>
            {
                new Rect(0, 0, 20, 20),
                new Rect(30, 30, 20, 20),
                new Rect(60, 60, 30, 30)
            };

            // Act
            var croppedList = ImageCropper.BatchCropByRects(testImage, rects);

            // Assert
            Assert.AreEqual(3, croppedList.Count);
            Assert.AreEqual(20, croppedList[0].Width);
            Assert.AreEqual(20, croppedList[0].Height);
        }

        [TestMethod]
        public void TestBatchCropByCenters_ValidCenters_ReturnsCorrectNumber() {
            // Arrange
            var centers = new List<Point2f>
            {
                new Point2f(20, 20),
                new Point2f(50, 50),
                new Point2f(80, 80)
            };
            var size = new Size(20, 20);

            // Act
            var croppedList = ImageCropper.BatchCropByCenters(testImage, centers, size);

            // Assert
            Assert.AreEqual(3, croppedList.Count);
            foreach (var cropped in croppedList) {
                Assert.AreEqual(20, cropped.Width);
                Assert.AreEqual(20, cropped.Height);
            }
        }
    }
}
