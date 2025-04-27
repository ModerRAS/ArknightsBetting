using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace ArknightsBetting.Common {
    public class AdbWrapper {
        private string adbPath;
        private string deviceSerial;

        public AdbWrapper(string adbPath) {
            this.adbPath = adbPath;
            this.deviceSerial = GetConnectedDevices().FirstOrDefault();
        }

        public AdbWrapper(string adbPath, string deviceSerial) {
            this.adbPath = adbPath;
            SetDeviceSerial(deviceSerial);
        }

        public void SetDeviceSerial(string deviceSerial) {
            this.deviceSerial = deviceSerial;
            Connect(deviceSerial);
        }

        public void CaptureScreenshot(string savePath) {
            ExecuteAdbCommand($"exec-out screencap -p", savePath);
        }

        public void Swipe(int startX, int startY, int endX, int endY) {
            ExecuteAdbCommand($"shell input swipe {startX} {startY} {endX} {endY}");
        }

        public void Tap(int X, int Y) {
            ExecuteAdbCommand($"shell input tap {X} {Y}");
        }

        private void ExecuteAdbCommand(string command, string savePath = "") {
            string args = $"{(deviceSerial != null ? "-s " + deviceSerial : "")} {command}";

            Process process = new Process();
            process.StartInfo.FileName = adbPath;
            process.StartInfo.Arguments = args;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.Start();

            if (!string.IsNullOrEmpty(savePath)) {
                using (var fileStream = new FileStream(savePath, FileMode.Create, FileAccess.Write)) {
                    process.StandardOutput.BaseStream.CopyTo(fileStream);
                }
            }

            process.WaitForExit();
        }

        private string[] GetConnectedDevices() {
            Process process = new Process();
            process.StartInfo.FileName = adbPath;
            process.StartInfo.Arguments = "devices";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.Start();

            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            return output
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Skip(1)
                .Select(line => line.Split('\t').First())
                .ToArray();
        }
        private void Connect(string deviceSerial) {
            Process process = new Process();
            process.StartInfo.FileName = adbPath;
            process.StartInfo.Arguments = $"connect {deviceSerial}";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.CreateNoWindow = true;
            process.Start();

            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
        }

        public void Pair(string deviceSerial, string code) {
            if (string.IsNullOrEmpty(deviceSerial)) {
                return;
            }
            if (string.IsNullOrEmpty(code)) {
                return;
            }
            ProcessStartInfo startInfo = new ProcessStartInfo {
                FileName = adbPath, // ADB可执行文件的路径（如果已将其添加到系统路径，则无需提供完整路径）
                Arguments = $"pair {deviceSerial}",
                CreateNoWindow = true, // 设置为true以隐藏命令行窗口
                UseShellExecute = false, // 设置为false以指示不要使用操作系统外壳程序启动进程
                RedirectStandardInput = true, // 设置为true以允许向进程标准输入流写入数据
                RedirectStandardOutput = true, // 设置为true以将命令输出重定向到StandardOutput流
                RedirectStandardError = true // 设置为true以将错误输出重定向到StandardError流
            };

            using (Process process = Process.Start(startInfo)) {
                // 读取命令输出和错误输出
                //string output = process.StandardOutput.ReadToEnd();
                //string error = process.StandardError.ReadToEnd();

                // 输出命令执行结果
                //Console.WriteLine("Output:");
                //Console.WriteLine(output);

                // 输出错误信息（如果有的话）
                //if (!string.IsNullOrEmpty(error)) {
                //Console.WriteLine("Error:");
                //Console.WriteLine(error);
                //}

                // 获取并输出需要继续输入的6位数字
                //if (output.Contains("Enter pairing code:")) {
                //}
                process.StandardInput.WriteLine(code);
                Console.WriteLine($"Inputting 6-digit code: {code}");


                process.WaitForExit(); // 等待进程退出

            }
        }
    }
}
