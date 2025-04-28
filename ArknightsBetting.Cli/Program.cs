using ArknightsBetting.Common;

namespace ArknightsBetting.Cli {
    internal class Program {
        static void Main(string[] args) {
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
