using Microsoft.CodeAnalysis;
using System.IO;

namespace Injector
{
    internal class Program
    {
        private static readonly string _className = "TemporaryClass";
        private static readonly string _template = "public class {0} {{ {1} }}";

        static void Main(string[] args)
        {
            CommandLine.CommandLineProcessor processor = new CommandLine.CommandLineProcessor();

            if (processor.ParseArguments(args))
            {
                AssemblyInjector injector = new AssemblyInjector(processor.GetValueFromKey("-i"));
                string code = string.Format(_template, _className, File.ReadAllText(processor.GetValueFromKey("-c")));

                injector.InjectMethod(targetTypeName: processor.GetValueFromKey("-t"), methodCode: code, _className, processor.GetValueFromKey("-m"), OutputKind.ConsoleApplication);
                injector.InjectNewMethodCallInExistingMethod(processor.GetValueFromKey("-t"), "Bro", processor.GetValueFromKey("-m"), false);
                injector.SaveModifiedAssembly(processor.GetValueFromKey("-o"));
            }
        }
    }
}
