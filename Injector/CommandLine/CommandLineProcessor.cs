using System.Collections.Generic;

namespace Injector.CommandLine
{
    public class CommandLineProcessor
    {
        public Dictionary<string, string> ArgumentList { get; set; }

        public CommandLineProcessor() => ArgumentList = new Dictionary<string, string>();

        public bool ParseArguments(string[] args)
        {
            if (args.Length != 8) return false;

            for (int i = 0; i < args.Length; i += 2)
                ArgumentList.Add(args[i], args[i + 1]);

            return (ArgumentList.ContainsKey("-m") && ArgumentList.ContainsKey("-c") && ArgumentList.ContainsKey("-i") && ArgumentList.ContainsKey("-o"));
        }

        public string GetValueFromKey(string key) => ArgumentList[key];
    }
}
