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

        public Dictionary<string, string> ArgumentList { get; set; }

        public CommandLineProcessor() => ArgumentList = new Dictionary<string, string>();

        public bool ParseArguments(string[] args)
        {
            if (args.Length != 10) return false;

            for (int i = 0; i < args.Length; i += 2)
                ArgumentList.Add(args[i], args[i + 1]);

            return (ArgumentList.ContainsKey(MethodArg) 
                && ArgumentList.ContainsKey(CodeArg) 
                && ArgumentList.ContainsKey(InputArg) 
                && ArgumentList.ContainsKey(OutputArg) 
                && ArgumentList.ContainsKey(TypeArg));
        }

        public string GetValueFromKey(string key) => ArgumentList[key];
    }
}
