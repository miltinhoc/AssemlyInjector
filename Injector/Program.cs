using Injector.CommandLine;
using Injector.Logging;
using System;
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

            if (!processor.ParseArguments(args))
            {
                return;
            }

            Header.Draw();

            AssemblyInjector injector = new AssemblyInjector(processor.GetValueFromKey(CommandLineProcessor.InputArg));
            string code = string.Format(_template, _className, File.ReadAllText(processor.GetValueFromKey(CommandLineProcessor.CodeArg)));
            int index = 0;

            if (processor.KeyExists(CommandLineProcessor.MethodIndexArg))
            {
                index = Convert.ToInt32(processor.GetValueFromKey(CommandLineProcessor.MethodIndexArg));
            }

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
                        0,
                        false,
                        true
                    );
                }
                else
                {
                    try
                    {
                        injector.InjectNewMethodCallInExistingMethod(
                            processor.GetValueFromKey(CommandLineProcessor.TypeArg),
                            processor.GetValueFromKey(CommandLineProcessor.MethodArg),
                            processor.GetValueFromKey(CommandLineProcessor.InjectOnMethodArg),
                            index
                        );
                    }
                    catch(Exception ex)
                    {
                        Logger.Print(ex.Message, LogType.ERROR);
                        Console.WriteLine("[*] Application exit.");

                        Environment.Exit(0);
                    }
                }
            }

            injector.SaveModifiedAssembly(processor.GetValueFromKey(CommandLineProcessor.OutputArg));

            Console.WriteLine("[*] Press any key to exit.");
            Console.Read();
        }
    }
}
