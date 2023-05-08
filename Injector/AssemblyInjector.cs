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

        public void InjectMethod(string targetTypeName, string methodCode, string className, string methodName, string existingMethodName, OutputKind outputKind = OutputKind.DynamicallyLinkedLibrary)
        {
            var tempAssembly = CompileMethod(methodCode, outputKind);
            var tempMethod = tempAssembly.MainModule.GetType(className).Methods.FirstOrDefault(m => m.Name == methodName);
            var targetType = _originalAssembly.MainModule.Types.FirstOrDefault(t => t.Name == targetTypeName);

            if (tempMethod != null && targetType != null)
            {
                InjectMethod(tempMethod, targetType, true, existingMethodName);
            }
        }

        public void InjectMethod(string targetTypeName, string methodCode, string className, string methodName, OutputKind outputKind = OutputKind.DynamicallyLinkedLibrary)
        {
            var tempAssembly = CompileMethod(methodCode, outputKind);
            var tempMethod = tempAssembly.MainModule.GetType(className).Methods.FirstOrDefault(m => m.Name == methodName);
            var targetType = _originalAssembly.MainModule.Types.FirstOrDefault(t => t.Name == targetTypeName);

            if (tempMethod != null && targetType != null)
            {
                InjectMethod(tempMethod, targetType);
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

        private AssemblyDefinition CompileMethod(string methodCode, OutputKind outputKind = OutputKind.DynamicallyLinkedLibrary)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(methodCode);
            var compilationOptions = new CSharpCompilationOptions(outputKind == OutputKind.ConsoleApplication ? OutputKind.DynamicallyLinkedLibrary : outputKind);

            var compilation = CSharpCompilation.Create("TempAssembly", new[] { syntaxTree },
                new[]
                {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Windows.Forms.Form).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Drawing.Image).Assembly.Location),
                },
                compilationOptions);

            MemoryStream ms = new MemoryStream();
            var emitResult = compilation.Emit(ms);

            if (!emitResult.Success)
            {
                foreach (var diagnostic in emitResult.Diagnostics)
                {
                    Console.WriteLine(diagnostic);
                }
                throw new InvalidOperationException("Compilation failed.");
            }

            ms.Seek(0, SeekOrigin.Begin);
            return AssemblyDefinition.ReadAssembly(ms);
        }

        private void InjectMethod(MethodDefinition tempMethod, TypeDefinition targetType, bool injectCall = false, string existingMethodName = "")
        {
            var newMethod = new MethodDefinition(tempMethod.Name,
                tempMethod.Attributes,
                _originalAssembly.MainModule.ImportReference(tempMethod.ReturnType));

            foreach (var parameter in tempMethod.Parameters)
            {
                var newParameter = new ParameterDefinition(parameter.Name, parameter.Attributes, _originalAssembly.MainModule.ImportReference(parameter.ParameterType));
                newMethod.Parameters.Add(newParameter);
            }

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

            if (injectCall)
            {
                var existingMethod = targetType.Methods.FirstOrDefault(m => m.Name == existingMethodName);

                for (int i = 0; i < existingMethod.Parameters.Count; i++)
                {
                    newMethod.Parameters.Add(existingMethod.Parameters[i]);
                }
            }

            targetType.Methods.Add(newMethod);
        }

        public void InjectNewMethodCallInExistingMethod(string targetTypeName, string existingMethodName, string newMethodName, bool passArguments = false)
        {
            var targetType = _originalAssembly.MainModule.Types.FirstOrDefault(t => t.Name == targetTypeName);

            if (targetType == null)
            {
                throw new InvalidOperationException($"Type '{targetTypeName}' not found in the assembly.");
            }

            var existingMethod = targetType.Methods.FirstOrDefault(m => m.Name == existingMethodName);
            var newMethod = targetType.Methods.FirstOrDefault(m => m.Name == newMethodName);

            if (existingMethod == null)
            {
                throw new InvalidOperationException($"Method '{existingMethodName}' not found in the type '{targetTypeName}'.");
            }

            if (newMethod == null)
            {
                throw new InvalidOperationException($"Method '{newMethodName}' not found in the type '{targetTypeName}'.");
            }

            var ilProcessor = existingMethod.Body.GetILProcessor();
            var lastInstruction = existingMethod.Body.Instructions.Last();
            
            if (lastInstruction == null)
            {
                throw new InvalidOperationException("The method does not have a 'ret' instruction.");
            }

            Instruction firstInserted = null;

            if (passArguments)
            {
                if (existingMethod.Parameters.Count == 0)
                    return;

                Instruction last;

                for (int i = 0; i < existingMethod.Parameters.Count; i++)
                {
                    Instruction loadArgInstruction;

                    switch (i)
                    {
                        case 0:
                            loadArgInstruction = ilProcessor.Create(OpCodes.Ldarg_0);
                            firstInserted = loadArgInstruction;
                            break;
                        case 1:
                            loadArgInstruction = ilProcessor.Create(OpCodes.Ldarg_1);
                            break;
                        case 2:
                            loadArgInstruction = ilProcessor.Create(OpCodes.Ldarg_2);
                            break;
                        case 3:
                            loadArgInstruction = ilProcessor.Create(OpCodes.Ldarg_3);
                            break;
                        default:
                            loadArgInstruction = ilProcessor.Create(OpCodes.Ldarg_S, (byte)i);
                            break;
                    }

                    ilProcessor.InsertBefore(lastInstruction, loadArgInstruction);
                    last = loadArgInstruction;
                }
                firstInserted = ilProcessor.Create(OpCodes.Call, newMethod);
                ilProcessor.InsertBefore(lastInstruction, firstInserted);
            }
            else
            {
                firstInserted = ilProcessor.Create(OpCodes.Call, newMethod);
                ilProcessor.InsertBefore(lastInstruction, firstInserted);
            }

            var callInstruction = ilProcessor.Create(OpCodes.Call, newMethod);
            
            foreach (var handler in existingMethod.Body.ExceptionHandlers)
            {
                if (handler.HandlerType == ExceptionHandlerType.Catch && handler.HandlerEnd == lastInstruction)
                {
                    handler.HandlerEnd = firstInserted;
                }
            }

            foreach (var instruction in existingMethod.Body.Instructions)
            {
                if (instruction.OpCode == OpCodes.Leave_S && instruction.Operand == lastInstruction)
                {
                    instruction.Operand = firstInserted;
                }
            }
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
