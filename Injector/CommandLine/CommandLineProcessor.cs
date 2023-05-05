namespace Injector.CommandLine
{
    public class CommandLineProcessor
    {
        public string OriginalAssemblyPath { get; set; }
        public string ModifiedAssemblyPath { get; set; }
        public string MethodName { get; set; }
        public string CodeFilePath { get; set; }

        public bool ParseArguments(string[] args)
        {
            if (args.Length != 8) return false;

            for (int i = 0; i < args.Length; i += 2)
            {
                switch (args[i])
                {
                    case "-m":
                        MethodName = args[i + 1];
                        break;
                    case "-c":
                        CodeFilePath = args[i + 1];
                        break;
                    case "-i":
                        OriginalAssemblyPath = args[i + 1];
                        break;
                    case "-o":
                        ModifiedAssemblyPath = args[i + 1];
                        break;
                    default:
                        //Console.WriteLine($" [*] Invalid argument(s).\n{_usage}");
                        return false;
                }
            }

            if (string.IsNullOrEmpty(CodeFilePath) || string.IsNullOrEmpty(OriginalAssemblyPath) || string.IsNullOrEmpty(ModifiedAssemblyPath) || string.IsNullOrEmpty(MethodName) ) 
            {
                return false;
            }

            return true;
        }
    }
}
