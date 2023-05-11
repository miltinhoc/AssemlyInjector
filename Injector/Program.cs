using Injector.CommandLine;
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
            CommandLineProcessor processor = new CommandLineProcessor();

            if (processor.ParseArguments(args))
            {
                AssemblyInjector injector = new AssemblyInjector(processor.GetValueFromKey(CommandLineProcessor.InputArg));
                string code = string.Format(_template, _className, File.ReadAllText(processor.GetValueFromKey(CommandLineProcessor.CodeArg)));

                if (processor.KeyExists(CommandLineProcessor.EntryArg))
                {
                    injector.InjectMethodOnEntryPoint(code, _className, processor.GetValueFromKey(CommandLineProcessor.MethodArg));
                }
                else
                {
                    injector.InjectMethod(targetTypeName: processor.GetValueFromKey(CommandLineProcessor.TypeArg),
                    methodCode: code, _className, processor.GetValueFromKey(CommandLineProcessor.MethodArg));
                }

                if (processor.KeyExists(CommandLineProcessor.InjectArg))
                {
                    if (processor.KeyExists(CommandLineProcessor.EntryArg))
                    {
                        injector.InjectNewMethodCallInExistingMethod(
                            processor.GetValueFromKey(CommandLineProcessor.TypeArg),
                            processor.GetValueFromKey(CommandLineProcessor.MethodArg),
                            null,
                            false,
                            true
                        );
                    }
                    else
                    {
                        injector.InjectNewMethodCallInExistingMethod(
                            processor.GetValueFromKey(CommandLineProcessor.TypeArg), 
                            processor.GetValueFromKey(CommandLineProcessor.MethodArg),
                            processor.GetValueFromKey(CommandLineProcessor.InjectOnMethodArg)
                        );
                    }
                }

                injector.SaveModifiedAssembly(processor.GetValueFromKey(CommandLineProcessor.OutputArg));
            }
        }
    }
}
