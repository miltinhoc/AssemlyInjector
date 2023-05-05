using System;
using System.IO;

namespace Injector
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Missing arguments.");
                return;
            }

            if (!File.Exists(args[0]))
            {
                Console.WriteLine("Target Assembly does not exist.");
                return;
            }

            string originalAssemblyPath = args[0];
            string modifiedAssemblyPath = args[1];

            AssemblyInjector injector = new AssemblyInjector(originalAssemblyPath);
            string methodCode = @"
public class DummyClass
{
    public static void NewMethod()
    {
    try { 
        string a = ""bb""; 
        System.Console.WriteLine(a);

    }catch{}

        
    }
}";

            injector.InjectMethod(targetTypeName: "Class1", methodCode: methodCode, "DummyClass", "NewMethod");
            injector.SaveModifiedAssembly(modifiedAssemblyPath);

        }
    }
}
