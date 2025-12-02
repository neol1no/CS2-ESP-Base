using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Linq;

namespace MTRX_WARE
{
    public static class ConsoleUtils
    {
        private const int STD_OUTPUT_HANDLE = -11;
        private const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll")]
        private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll")]
        private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        public static void EnableVT()
        {
            var handle = GetStdHandle(STD_OUTPUT_HANDLE);
            uint mode;
            GetConsoleMode(handle, out mode);
            SetConsoleMode(handle, mode | ENABLE_VIRTUAL_TERMINAL_PROCESSING);
        }

        public static void WriteGradient(string text, Color start, Color end)
        {
            string[] lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            
            int maxLen = 0;
            foreach (var line in lines) if (line.Length > maxLen) maxLen = line.Length;

            if (maxLen == 0) return;

            StringBuilder sb = new StringBuilder();

            foreach (var line in lines)
            {
                for (int i = 0; i < line.Length; i++)
                {
                    float t = (float)i / maxLen;
                    int r = (int)(start.R + (end.R - start.R) * t);
                    int g = (int)(start.G + (end.G - start.G) * t);
                    int b = (int)(start.B + (end.B - start.B) * t);

                    sb.Append($"\x1b[38;2;{r};{g};{b}m{line[i]}");
                }
                sb.Append("\x1b[0m\n");
            }
            
            Console.Write(sb.ToString());
        }

        public static async Task DisplayLoadingScreen()
        {
            Console.Clear();
            Console.WriteLine();
            Console.WriteLine("  Initializing MTRX-WARE...");
            Console.WriteLine();
            string separator = string.Join(" ", Enumerable.Repeat("_", 44));
            Console.WriteLine(separator);
            Console.WriteLine();

            string[] steps = { "Checking fonts", "Checking offsets", "Initializing main", "Initializing ESP" };
            string[] spinnerFrames = { "|", "/", "-", "\\" };
            string[] loadingDots = { "   ", ".  ", ".. ", "..." };

            bool fontsFolderExists = Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "fonts"));
            bool fontFileExists = fontsFolderExists && File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "fonts", "Roboto-Regular.ttf"));
            bool offsetsFolderExists = Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "offsets"));
            bool offsetsFileExists = offsetsFolderExists && File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "offsets", "offsets.cs"));

            bool[] missing = new bool[2];
            missing[0] = !(fontsFolderExists && fontFileExists);
            missing[1] = !(offsetsFolderExists && offsetsFileExists);

            int row = 5; // +1 for extra line under underscore separator
            string[] bracketSymbols = new string[4];
            bracketSymbols[0] = missing[0] ? "\x1b[33m!\x1b[0m" : "\x1b[32m✓\x1b[0m";
            bracketSymbols[1] = missing[1] ? "\x1b[33m!\x1b[0m" : "\x1b[32m✓\x1b[0m";
            bracketSymbols[2] = "\x1b[32m✓\x1b[0m";
            bracketSymbols[3] = "\x1b[32m✓\x1b[0m";

            bool anyMissing = missing[0] || missing[1];

            for (int step = 0; step < steps.Length; step++) {
                for (int sub = 0; sub < 8; sub++) {
                    Console.SetCursorPosition(0, row + step);
                    string spinner = spinnerFrames[(sub + step * 2) % spinnerFrames.Length];
                    Console.Write($"  {steps[step],-30} [{spinner}]   ");
                    for (int k = step + 1; k < steps.Length; k++) Console.Write(new string(' ', 35));
                    string dotsAnim = loadingDots[sub % loadingDots.Length];
                    Console.SetCursorPosition(0, row + steps.Length + 2);
                    Console.Write($"Status: loading{dotsAnim}   ");
                    await Task.Delay(55);
                }
                Console.SetCursorPosition(0, row + step);
                Console.Write($"  {steps[step],-30} [{bracketSymbols[step]}]   ");
            }

            // final status
            Console.SetCursorPosition(0, row + steps.Length + 2);
            Console.Write(new string(' ', 33));
            Console.SetCursorPosition(0, row + steps.Length + 2);
            string statusMsg = anyMissing
                ? "Status: dependencies missing"
                : "Status: loaded successfully \x1b[32m✓\x1b[0m";
            Console.Write(statusMsg);
            Console.SetCursorPosition(0, row + steps.Length + 3);
            Console.WriteLine();
            Console.WriteLine(separator);
            Console.WriteLine();
            string continueMsg = anyMissing ? "Press Y to download dependencies" : "Press Y to continue...";
            if (anyMissing) {
                Console.WriteLine(continueMsg);
                while (true)
                {
                    var key = Console.ReadKey(true);
                    if (key.Key == ConsoleKey.Y) break;
                }
            }
        }
    }
}
