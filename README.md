# Injector

## Example

Inject a Method on the assembly's entry point Type:
```bash
Injector.exe -code code.cs -input ConsoleApp1.exe -output a.exe -method NewMethod -entry
```

Inject a Method on the assembly's entry point Type and call it on the entry point Method:
```bash
Injector.exe -code code.cs -input ConsoleApp1.exe -output a.exe -method NewMethod -entry -injectcall
```

Inject a Method on an existing Type of the assembly:
```bash
Injector.exe -code code.cs -input ConsoleApp1.exe -output a.exe -method NewMethod -type Program
```

Inject a Method on an existing Type of the assembly and call it on a existing Method:
```bash
Injector.exe -code code.cs -input ConsoleApp1.exe -output a.exe -method NewMethod -type Program -injectcall -injectonmethod Verify
```

If there are more than 1 method with the same name (in case of overloading) you can choose which one to inject to using it's index:
```bash
Injector.exe -code code.cs -input ConsoleApp1.exe -output a.exe -method NewMethod -type Program -injectcall -injectonmethod Verify -index 1
```

code.cs file example:

```csharp

public static void NewMethod()
{
	int count = 0;
        Console.WriteLine(count.ToString());    
}
```

It's possible to copy the arguments of the method we inject the call into and pass them to our own. Note that is is not possible with arguments tho, manual tinkering needed.

## Todo
- Add a way to understand what references are necessary to add depending on the code passed.
- Find a better way  (via arguments) to copy/match arguments and pass them to the new injected method.
- Better exception handling.