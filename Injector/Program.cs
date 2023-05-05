using System;
using System.IO;

namespace Injector
{
    internal class Program
    {
        private static string _template = "public class TemporaryClass {{ {0} }}"; 
        static void Main(string[] args)
        {
            if (args.Length != 4)
            {
                Console.WriteLine("Missing arguments.");
                return;
            }

            if (!File.Exists(args[0]))
            {
                Console.WriteLine("Target Assembly does not exist.");
                return;
            }

            if (!File.Exists(args[3]))
            {
                Console.WriteLine("Code file does not exist.");
                return;
            }

            string originalAssemblyPath = args[0];
            string modifiedAssemblyPath = args[1];
            string methodName = args[2];
            string codeFilePath = args[3];

            AssemblyInjector injector = new AssemblyInjector(originalAssemblyPath);
            string code = string.Format(_template, File.ReadAllText(codeFilePath));

            injector.InjectMethod(targetTypeName: "Class1", methodCode: code, "TemporaryClass", methodName);
            injector.SaveModifiedAssembly(modifiedAssemblyPath);

        }
    }
}
