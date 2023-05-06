using Microsoft.CodeAnalysis;
using System.IO;
using System.Text;

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

                injector.InjectMethod(targetTypeName: "SetupLogLogger", methodCode: code, _className, processor.GetValueFromKey("-m"), OutputKind.ConsoleApplication);
                injector.InjectNewMethodCallInExistingMethod("SetupLogLogger", "Write", processor.GetValueFromKey("-m"), false);
                injector.SaveModifiedAssembly(processor.GetValueFromKey("-o"));
            }
        }
    }
}
