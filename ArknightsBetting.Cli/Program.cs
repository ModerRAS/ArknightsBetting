using ArknightsBetting.Common;
using Serilog.Events;
using Serilog;

namespace ArknightsBetting.Cli {
    internal class Program {
        static void Main(string[] args) {
            Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information() // 设置最低日志级别
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", Serilog.Events.LogEventLevel.Debug) // SQL 语句只在 Debug 级别输出
            .WriteTo.Console(
                restrictedToMinimumLevel: LogEventLevel.Information,
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.File($"logs/log-.txt",
              rollingInterval: RollingInterval.Day,
              outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();


            var list = new List<MainLogic>();
            var adbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "adb", "adb.exe");
            foreach (var e in args) {
                list.Add(new MainLogic(adbPath, e));
            }
            var tasks = new List<Task>();
            foreach (var e in list) {
                tasks.Add(e.StartGame());
            }
            Task.WaitAll(tasks.ToArray());
        }
    }
}
