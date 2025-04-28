using Newtonsoft.Json;
using Serilog;
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
            if (!Directory.Exists("Captures")) {
                Directory.CreateDirectory("Captures");
            }
            if (!Directory.Exists($"Captures/{saveName}")) {
                Directory.CreateDirectory($"Captures/{saveName}");
            }
            var detect = new DetectNumber();
            var time = DateTime.Now;
            while (true) {
                await Task.Delay(500);
                var jpg = adbWrapper.CaptureScreenshot();
                detect.SetImage(jpg);
                foreach (var e in detect.GetStringPoints("加入赛事")) {
                    adbWrapper.Tap(e.X, e.Y);
                    Log.Information($"{saveName}: 加入赛事");
                }
                foreach (var e in detect.GetStringPoints("返回主页")) {
                    adbWrapper.Tap(e.X, e.Y);
                    Log.Information($"{saveName}: 返回主页");
                }
                if (detect.Contains("竞猜对决") && detect.Contains("礼物对决") && detect.Contains("自娱自乐")) {
                    foreach (var e in detect.GetStringPoints("自娱自乐")) {
                        adbWrapper.Tap(e.X, e.Y);
                        Log.Information($"{saveName}: 自娱自乐");
                    }
                    foreach (var e in detect.GetStringPoints("开始")) {
                        adbWrapper.Tap(e.X, e.Y);
                        Log.Information($"{saveName}: 开始");
                    }
                }
                if (detect.Contains("WIN") && detect.Contains("LOSE")) {
                    await File.WriteAllBytesAsync($"Captures/{saveName}/{saveName}_{time.ToString("yyyyMMddHHmmss")}_Result.jpg", jpg);
                    time = DateTime.Now;
                    Log.Information($"{saveName}: 保存结果");
                    continue;
                }
                if (detect.Contains("本轮观望")) {
                    var p1 = detect.GetStringPoint("本轮观望");
                    adbWrapper.Tap(p1.X, p1.Y);
                    await File.WriteAllBytesAsync($"Captures/{saveName}/{saveName}_{time.ToString("yyyyMMddHHmmss")}_Start.jpg", jpg);
                    Log.Information($"{saveName}: 保存战斗序列");
                    continue;
                }
            }

        }


        public async Task ExecuteAsync(Action<int> SetScore) {

            await StartGame();
        }


    }
}
