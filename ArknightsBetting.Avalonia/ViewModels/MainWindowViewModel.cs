using ReactiveUI;
using System.Reactive;
using System.Threading.Tasks;
using System;
using ArknightsBetting.Common;
using System.IO;

namespace ArknightsBetting.Avalonia.ViewModels {
    public class MainWindowViewModel : ViewModelBase {
        private int Count { get; set; } = 0;
        public string _greeting = "填写ADB用的IP和端口后点击一键启动";
        public string Greeting {
            get => _greeting;
            set => this.RaiseAndSetIfChanged(ref _greeting, value);
        }

        public bool IsStart { get; set; }


        private string _inputText = "127.0.0.1:5555";

        public string InputText {
            get => _inputText;
            set => this.RaiseAndSetIfChanged(ref _inputText, value);
        }

        private string _adbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "adb", "adb.exe");

        public string AdbPath {
            get => _adbPath;
            set => this.RaiseAndSetIfChanged(ref _adbPath, value);
        }

        public string _pairCode = "123456";
        public string PairCode {
            get => _pairCode;
            set => this.RaiseAndSetIfChanged(ref _pairCode, value);
        }

        public string _pairAddress = "127.0.0.1:5666";
        public string PairAddress {
            get => _pairAddress;
            set => this.RaiseAndSetIfChanged(ref _pairAddress, value);
        }

        public ReactiveCommand<Unit, Unit> ReadInputCommand { get; }
        public ReactiveCommand<Unit, Unit> StopTaskCommand { get; }
        public ReactiveCommand<Unit, Unit> PairCommand { get; }

        public MainWindowViewModel() {
            ReadInputCommand = ReactiveCommand.CreateFromTask(ReadInput);
            StopTaskCommand = ReactiveCommand.CreateFromTask(StopTask);
            PairCommand = ReactiveCommand.CreateFromTask(PairTask);
        }
        private async Task ReadInput() {
            // 读取输入框的内容
            string input = InputText;
            // 处理输入的逻辑
            // ...
            Greeting = input;
            IsStart = true;
            while (IsStart) {
                var mainlogic = new MainLogic(AdbPath, input);
                await mainlogic.ExecuteAsync(new Action<int>((score) => {
                    Greeting += $",此次{score}分";
                }));
                Count++;
                Greeting = $"已完成{Count}次";
            }
        }

        private async Task StopTask() {
            IsStart = false;
            await Task.CompletedTask;
        }

        private async Task PairTask() {
            var adbWrapper = new AdbWrapper(AdbPath);
            adbWrapper.Pair(PairAddress, PairCode);
        }
    }
}
