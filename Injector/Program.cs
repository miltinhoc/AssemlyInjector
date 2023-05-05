using System.IO;

namespace Injector
{
    internal class Program
    {
        private static string _template = "public class TemporaryClass {{ {0} }}"; 
        static void Main(string[] args)
        {
            CommandLine.CommandLineProcessor processor = new CommandLine.CommandLineProcessor();

            if (processor.ParseArguments(args))
            {
                AssemblyInjector injector = new AssemblyInjector(processor.OriginalAssemblyPath);
                string code = string.Format(_template, File.ReadAllText(processor.CodeFilePath));

                injector.InjectMethod(targetTypeName: "Class1", methodCode: code, "TemporaryClass", processor.MethodName);
                injector.SaveModifiedAssembly(processor.ModifiedAssemblyPath);
            }
        }
    }
}
