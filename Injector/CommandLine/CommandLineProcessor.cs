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

        /// <summary>
        /// The Type to inject into
        /// </summary>
        public static readonly string TypeArg = "-type";

        /// <summary>
        /// Used to decide if we should inject on the entry point, if this is set, the argument '-type' is not needed
        /// </summary>
        public static readonly string EntryArg = "-entry";

        /// <summary>
        /// Used to decide if we should inject a call to the newly added method, if this is used with the argument '-entry', we will inject 
        /// our method into the entry point method, otherwise a method to inject the call needs to be defined on '-injecttomethod'
        /// </summary>
        public static readonly string InjectArg = "-injectcall";

        /// <summary>
        /// Used to specify in what method we should inject a call to our newly created one. This will inject it at the end of the method. 
        /// If the argument '-entry' is defined, this one will be ignored and the entry point method will be used instead.
        /// </summary>
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
