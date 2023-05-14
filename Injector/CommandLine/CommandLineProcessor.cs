using System.Collections.Generic;

namespace Injector.CommandLine
{
    public class CommandLineProcessor
    {
        /// <summary>
        /// Name for the new injected method (should match the method named on the code passed to the '-code' arg)
        /// </summary>
        public static readonly string MethodArg = "-method";

        /// <summary>
        /// Path for the file containing the c# code
        /// </summary>
        public static readonly string CodeArg = "-code";

        /// <summary>
        /// Path for the input assembly
        /// </summary>
        public static readonly string InputArg = "-input";

        /// <summary>
        /// Path for the output assembly
        /// </summary>
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
            return ArgumentList.TryGetValue(key, out _);
        }

        public string GetValueFromKey(string key)
        {
            return ArgumentList.TryGetValue(key, out var value) ? value : null;
        }
    }
}
