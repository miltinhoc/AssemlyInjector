using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Mono.Cecil;
using System;
using System.IO;
using System.Linq;
using Mono.Cecil.Cil;

namespace Injector
{
    public class AssemblyInjector
    {
        private readonly AssemblyDefinition _originalAssembly;

        public AssemblyInjector(string originalAssemblyPath)
        {
            var assemblyResolver = new DefaultAssemblyResolver();
            var assemblyReaderParameters = new ReaderParameters { AssemblyResolver = assemblyResolver };
            _originalAssembly = AssemblyDefinition.ReadAssembly(originalAssemblyPath, assemblyReaderParameters);
        }

        public void InjectMethod(string targetTypeName, string methodCode, string className, string methodName)
        {
            var tempAssembly = CompileMethod(methodCode);
            var tempMethod = tempAssembly.MainModule.GetType(className).Methods.FirstOrDefault(m => m.Name == methodName);
            var targetType = _originalAssembly.MainModule.Types.FirstOrDefault(t => t.Name == targetTypeName);

            if (tempMethod != null && targetType != null)
            {
                InjectMethod(tempMethod, targetType);
            }
            else
            {
                throw new NullReferenceException();
            }
        }

        public void RemoveTemporaryResource()
        {
            for (int i = 0; i < _originalAssembly.MainModule.Resources.Count; i++)
            {
                if (_originalAssembly.MainModule.Resources[i].Name == "TempAssembly")
                {
                    _originalAssembly.MainModule.Resources.RemoveAt(i);
                    break;
                }
            }
        }

        public void SaveModifiedAssembly(string modifiedAssemblyPath)
        {
            using (FileStream modifiedAssemblyStream = new FileStream(modifiedAssemblyPath, FileMode.Create))
            {
                _originalAssembly.Write(modifiedAssemblyStream);
            }
        }

        private AssemblyDefinition CompileMethod(string methodCode)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(methodCode);
            var compilation = CSharpCompilation.Create("TempAssembly",
                new[] { syntaxTree },
                new[]
                {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Console).Assembly.Location)
                },
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            MemoryStream ms = new MemoryStream();
            var emitResult = compilation.Emit(ms);

            if (!emitResult.Success)
            {
                // Handle compilation errors
                foreach (var diagnostic in emitResult.Diagnostics)
                {
                    Console.WriteLine(diagnostic);
                }
                throw new InvalidOperationException("Compilation failed.");
            }

            ms.Seek(0, SeekOrigin.Begin);
            return AssemblyDefinition.ReadAssembly(ms);
        }

        private void InjectMethod(MethodDefinition tempMethod, TypeDefinition targetType)
        {
            var newMethod = new MethodDefinition(tempMethod.Name,
                tempMethod.Attributes,
                _originalAssembly.MainModule.ImportReference(tempMethod.ReturnType));

            var ilProcessor = newMethod.Body.GetILProcessor();

            foreach (var variable in tempMethod.Body.Variables)
            {
                newMethod.Body.Variables.Add(new VariableDefinition(_originalAssembly.MainModule.ImportReference(variable.VariableType)));
            }

            foreach (var instruction in tempMethod.Body.Instructions)
            {
                if (instruction.Operand is MethodReference methodReference)
                {
                    instruction.Operand = _originalAssembly.MainModule.ImportReference(methodReference);
                }
                else if (instruction.Operand is FieldReference fieldReference)
                {
                    instruction.Operand = _originalAssembly.MainModule.ImportReference(fieldReference);
                }
                else if (instruction.Operand is TypeReference typeReference)
                {
                    instruction.Operand = _originalAssembly.MainModule.ImportReference(typeReference);
                }
                ilProcessor.Append(instruction);
            }

            foreach (var exceptionHandler in tempMethod.Body.ExceptionHandlers)
            {
                newMethod.Body.ExceptionHandlers.Add(new ExceptionHandler(exceptionHandler.HandlerType)
                {
                    CatchType = exceptionHandler.CatchType == null ? null : _originalAssembly.MainModule.ImportReference(exceptionHandler.CatchType),
                    TryStart = GetCorrespondingInstruction(ilProcessor.Body.Instructions, tempMethod.Body.Instructions, exceptionHandler.TryStart),
                    TryEnd = GetCorrespondingInstruction(ilProcessor.Body.Instructions, tempMethod.Body.Instructions, exceptionHandler.TryEnd),
                    HandlerStart = GetCorrespondingInstruction(ilProcessor.Body.Instructions, tempMethod.Body.Instructions, exceptionHandler.HandlerStart),
                    HandlerEnd = GetCorrespondingInstruction(ilProcessor.Body.Instructions, tempMethod.Body.Instructions, exceptionHandler.HandlerEnd),
                    FilterStart = GetCorrespondingInstruction(ilProcessor.Body.Instructions, tempMethod.Body.Instructions, exceptionHandler.FilterStart)
                });
            }

            targetType.Methods.Add(newMethod);
        }

        private static Instruction GetCorrespondingInstruction(Mono.Collections.Generic.Collection<Instruction> newInstructions, Mono.Collections.Generic.Collection<Instruction> originalInstructions, Instruction originalInstruction)
        {
            if (originalInstruction == null)
            {
                return null;
            }

            int index = originalInstructions.IndexOf(originalInstruction);
            return newInstructions[index];
        }

    }
}
