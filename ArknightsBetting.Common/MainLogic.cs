using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArknightsBetting.Common {
    public class MainLogic {
        public AdbWrapper adbWrapper { get; set; }
        public MainLogic(string adbPath, string deviceSerial) {
            adbWrapper = new AdbWrapper(adbPath, deviceSerial);
            saveName = deviceSerial.Replace(":", "_").Replace(".", "_");
        }
        public string saveName { get; set; }
        public MainLogic(string deviceSerial) {
            var adbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "adb", "adb.exe");
            adbWrapper = new AdbWrapper(adbPath, deviceSerial);
            saveName = deviceSerial.Replace(":", "_").Replace(".", "_");
        }

        public async Task StartGame() {
            var detect = new DetectNumber();
            var time = DateTime.Now;
            while (true) {
                await Task.Delay(500);
                adbWrapper.CaptureScreenshot($"{saveName}.jpg");
                var jpg = await File.ReadAllBytesAsync($"{saveName}.jpg");
                detect.SetImage(jpg);
                foreach (var e in detect.GetStringPoints("加入赛事")) {
                    adbWrapper.Tap(e.X, e.Y);
                }
                foreach (var e in detect.GetStringPoints("返回主页")) {
                    adbWrapper.Tap(e.X, e.Y);
                }
                if (detect.Contains("竞猜对决") && detect.Contains("礼物对决") && detect.Contains("自娱自乐")) {
                    foreach (var e in detect.GetStringPoints("自娱自乐")) {
                        adbWrapper.Tap(e.X, e.Y);
                    }
                    foreach (var e in detect.GetStringPoints("开始")) {
                        adbWrapper.Tap(e.X, e.Y);
                    }
                }
                if (detect.Contains("WIN") && detect.Contains("LOSE")) {
                    await File.WriteAllBytesAsync($"{saveName}_{time.ToString("yyyyMMddHHmmss")}_Result.jpg", jpg);
                    time = DateTime.Now;
                    continue;
                }
                if (detect.Contains("本轮观望")) {
                    var p1 = detect.GetStringPoint("本轮观望");
                    adbWrapper.Tap(p1.X, p1.Y);
                    await File.WriteAllBytesAsync($"{saveName}_{time.ToString("yyyyMMddHHmmss")}_Start.jpg", jpg);
                    continue;
                }
            }

        }


        public async Task ExecuteAsync(Action<int> SetScore) {

            await StartGame();
        }


    }
}
