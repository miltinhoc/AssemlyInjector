using System.Collections.Generic;

namespace Injector.CommandLine
{
    public class CommandLineProcessor
    {
        public static readonly string MethodArg = "-method";
        public static readonly string CodeArg = "-code";
        public static readonly string InputArg = "-input";
        public static readonly string OutputArg = "-output";
        public static readonly string TypeArg = "-type";
        public static readonly string EntryArg = "-entry";
        public static readonly string InjectArg = "-injectcall";
        public static readonly string InjectOnMethodArg = "-injectonmethod";

        public Dictionary<string, string> ArgumentList { get; private set; }

        public CommandLineProcessor() => ArgumentList = new Dictionary<string, string>();

        public bool ParseArguments(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                // If this argument starts with "-"
                if (args[i].StartsWith("-"))
                {
                    // If the next argument also starts with "-" or doesn't exist, this is a flag without a value
                    if (i == args.Length - 1 || args[i + 1].StartsWith("-"))
                    {
                        ArgumentList[args[i]] = string.Empty;
                    }
                    else // This argument has a corresponding value
                    {
                        ArgumentList[args[i]] = args[++i];
                    }
                }
            }

            return AreAllArgumentsPresent();
        }

        private bool AreAllArgumentsPresent()
        {
            return (ArgumentList.ContainsKey(MethodArg)
                && ArgumentList.ContainsKey(CodeArg)
                && ArgumentList.ContainsKey(InputArg)
                && ArgumentList.ContainsKey(OutputArg));
        }

        public bool KeyExists(string key)
        {
            return ArgumentList.TryGetValue(key, out var value) ? true : false;
        }

        public string GetValueFromKey(string key)
        {
            return ArgumentList.TryGetValue(key, out var value) ? value : null;
        }
    }
}
