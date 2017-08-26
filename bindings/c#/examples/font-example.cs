using rpi_rgb_led_matrix_sharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace font_example
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("font-example.exe <fontpath> text");
                return;
            }
            string text = "Hello\nWorld!";
            if (args.Length == 2)
            {
                text = args[1];
            }

            var matrix = new RGBLedMatrix(32, 2, 1);
            var canvas = matrix.CreateOffscreenCanvas();
            var font = matrix.LoadFont(args[0]);

            canvas.DrawText(font, 0, 0, text, Color.White);
            matrix.SwapOnVsync(canvas);

            while (!Console.KeyAvailable)
            {
                Thread.Sleep(250);
            }
        }
    }
}
