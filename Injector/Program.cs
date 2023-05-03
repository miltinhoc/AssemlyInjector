using System;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Injector
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string originalAssemblyPath = "SimpleDll.dll";
            string modifiedAssemblyPath = "ModifiedAssembly.dll";

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

            injector.InjectMethod("Class1", methodCode);
            injector.SaveModifiedAssembly(modifiedAssemblyPath);

        }
    }
}
