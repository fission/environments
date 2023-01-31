
// fission-dotnet6 - FissionCompiler.cs
// 
// Created by: Alistair J R Young(avatar) at 2020/12/29 9:08 AM.
// Modified by: Vsevolod Kvachev (Rasie1) at 2022.



using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Runtime.Serialization.Json;

using Fission.DotNet.Properties;
using Fission.Functions;
using Fission.Common;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;


namespace Fission.DotNet
{
    /// <summary>
    ///     The compiler which builds Fission functions from source text, when a builder container is not used.
    /// </summary>
    internal class FissionCompiler
    {
        /// <summary>
        ///     Compile C# source text(implementing <see cref="IFissionFunction" />) to an assembly stored in memory.
        /// </summary>
        /// <param name="source">The source code to compile.</param>
        /// <param name="errors">On exit, a list of compilation errors.</param>
        /// <returns>A <see cref="FunctionRef" /> referencing the compiled Fission function.</returns>

        // ReSharper disable once MemberCanBeMadeStatic.Global
        [SuppressMessage(category: "Performance",
                          checkId: "CA1822:Mark members as static",
                          Justification = "Instance members are expected in later iterations. -- AJRY 2020/12/30")]
        internal FunctionRef? Compile(string source, out List<string> errors)
        {
            errors = new List<string>();

            var options = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Latest);
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(source, options);

            var coreDir = Directory.GetParent(path: typeof(Enumerable).GetTypeInfo().Assembly.Location);

            var references = new List<MetadataReference>
                {
                    MetadataReference.CreateFromFile(path: $"{coreDir!.FullName}{Path.DirectorySeparatorChar}mscorlib.dll"),
                    MetadataReference.CreateFromFile(path: typeof(object).GetTypeInfo().Assembly.Location),
                    MetadataReference.CreateFromFile(path: typeof(IFissionFunction).GetTypeInfo().Assembly.Location),
                    MetadataReference.CreateFromFile(path: Assembly.GetEntryAssembly()!.Location),
                    MetadataReference.CreateFromFile(path: typeof(DataContractJsonSerializer).GetTypeInfo().Assembly.Location), 
                };

            foreach (var referencedAssembly in Assembly.GetEntryAssembly()!.GetReferencedAssemblies())
            {
                var loaded = Assembly.Load(assemblyRef: referencedAssembly);
                references.Add(item: MetadataReference.CreateFromFile(path: loaded.Location));
            }

            string assemblyName = Path.GetRandomFileName();
            CSharpCompilation compilation = CSharpCompilation.Create(assemblyName: assemblyName,
                                                                     syntaxTrees: new[] {syntaxTree,},
                                                                     references: references,
                                                                     options: new CSharpCompilationOptions(
                                                                     outputKind: OutputKind.DynamicallyLinkedLibrary,
                                                                     optimizationLevel: OptimizationLevel.Release));

            using var ms = new MemoryStream();

            EmitResult result = compilation.Emit(peStream: ms);

            if (!result.Success)
            {
                Console.WriteLine($"Compile failed, see pod logs for more details");
                IEnumerable<Diagnostic> failures = result.Diagnostics
                                                            .Where(predicate: diagnostic =>
                                                                                diagnostic.IsWarningAsError ||
                                                                                diagnostic.Severity ==
                                                                                DiagnosticSeverity.Error)
                                                            .ToList();

                foreach (Diagnostic diagnostic in failures) {
                    errors.Add(item: $"{diagnostic.Id}: {diagnostic.GetMessage()}");
                    Console.WriteLine($"COMPILE ERROR :{diagnostic.Id}: {diagnostic.GetMessage()}", "ERROR");
                }
                return null;
            }

            ms.Seek(offset: 0, loc: SeekOrigin.Begin);

            Assembly assembly = AssemblyLoadContext.Default.LoadFromStream(assembly: ms);

            Type? type = assembly.GetTypes()
                                    .FirstOrDefault(predicate: t => typeof(IFissionFunction).IsAssignableFrom(c: t));

            if (type == null)
            {
                errors.Add(item: Resources.FissionCompiler_Compile_NoEntrypoint);
                return null;
            }

            return new FunctionRef(assembly: assembly, type: type);
        }

        public FunctionRef? CompileV2(string packagePath, out List<string> errors, out List<string> outputInfo)
        {
            errors = new List<string>();
            outputInfo = new List<string>();
            var options = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Latest);

            var syntaxTrees = new List<SyntaxTree>();
            foreach (var codeInDirectory in CompilerHelper.GetCSharpSources(packagePath)) 
            {
                syntaxTrees.Add(CSharpSyntaxTree.ParseText(File.ReadAllText(codeInDirectory), options));
            }

            string assemblyName = Path.GetRandomFileName();

            var coreDir = Directory.GetParent(typeof(Enumerable).GetTypeInfo().Assembly.Location);

            List<MetadataReference> references = new List<MetadataReference>
                {
                    MetadataReference.CreateFromFile(coreDir.FullName + Path.DirectorySeparatorChar + "mscorlib.dll"),
                    MetadataReference.CreateFromFile(coreDir.FullName + Path.DirectorySeparatorChar + "netstandard.dll"),
                    MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location),
                    MetadataReference.CreateFromFile(Assembly.GetEntryAssembly().Location),
                    MetadataReference.CreateFromFile(typeof(System.Runtime.Serialization.Json.DataContractJsonSerializer).GetTypeInfo().Assembly.Location)
                };

            foreach (var referencedAssembly in Assembly.GetEntryAssembly().GetReferencedAssemblies())
            {
                var loaded = Assembly.Load(referencedAssembly);
                references.Add(MetadataReference.CreateFromFile(loaded.Location));
            }

            functionSpec = CompilerHelper.Instance.GetFunctionSpecs(packagePath);

            foreach (var library in functionSpec.libraries)
            {
                string dllCompletePath = CompilerHelper.GetRelevantPathAsPerOS(Path.Combine(packagePath, library.path));
                references.Add(MetadataReference.CreateFromFile(dllCompletePath));
            }

            AppDomain currentDomain = AppDomain.CurrentDomain;
            this.packagePath = packagePath;
            currentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            CSharpCompilation compilation = CSharpCompilation.Create(
                assemblyName,
                syntaxTrees: syntaxTrees,
                references: references,
                options: new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary,
                    optimizationLevel: OptimizationLevel.Release));

            var ms = new MemoryStream();
            EmitResult result = compilation.Emit(ms);

            if (!result.Success)
            {
                Console.WriteLine($"Compile failed, see pod logs for more details");
                IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
                    diagnostic.IsWarningAsError ||
                    diagnostic.Severity == DiagnosticSeverity.Error).ToList();

                foreach (Diagnostic diagnostic in failures)
                {
                    errors.Add($"{diagnostic.Id}: {diagnostic.GetMessage()}");
                    Console.WriteLine($"COMPILE ERROR :{diagnostic.Id}: {diagnostic.GetMessage()}", "ERROR");
                }

                return null;
            }

            Console.WriteLine($"COMPILE SUCCESS!");

            ms.Seek(0, SeekOrigin.Begin);

            Assembly assembly = AssemblyLoadContext.Default.LoadFromStream(ms);
            
            Type? type = assembly.GetTypes()
                                    .FirstOrDefault(predicate: t => typeof(IFissionFunction).IsAssignableFrom(c: t));
            if (type == null)
            {
                errors.Add(item: Resources.FissionCompiler_Compile_NoEntrypoint);
                return null;
            }

            return new FunctionRef(assembly, type);
        }

        private string? packagePath;
        FunctionSpecification functionSpec;

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            // This handler is called only when the common language runtime tries to bind to the assembly and fails.

            Console.WriteLine($"Dynamically trying to load dll {(args.Name.Substring(0, args.Name.IndexOf(",")).ToString() + ".dll").ToLower()} in parent assembly");

            Assembly myAssembly = null, objExecutingAssemblies;
            string assemblyPathRelative = "", assemblyPathAbsolute = "";

            objExecutingAssemblies = Assembly.GetExecutingAssembly();
            AssemblyName[] referencedAssemblyNames = objExecutingAssemblies.GetReferencedAssemblies();

            // load all available dlls from  deployment folder in dllinfo object         
            if (functionSpec.libraries.Any(x => x.name.ToLower() ==(args.Name.Substring(0, args.Name.IndexOf(",")).ToString() + ".dll").ToLower()))
            {
                assemblyPathRelative = functionSpec.libraries.Where(x => x.name.ToLower() ==(args.Name.Substring(0, args.Name.IndexOf(",")).ToString() + ".dll").ToLower()).FirstOrDefault().path;
                assemblyPathAbsolute = Path.Combine(packagePath, assemblyPathRelative);
                Console.WriteLine($"loading dll in parent assembly: {CompilerHelper.GetRelevantPathAsPerOS(assemblyPathAbsolute)}");
                myAssembly = Assembly.LoadFile(CompilerHelper.GetRelevantPathAsPerOS(assemblyPathAbsolute));
                Console.WriteLine($"Load success for: {CompilerHelper.GetRelevantPathAsPerOS(assemblyPathAbsolute)}");
            }

            if (myAssembly == null)
            {
                Console.WriteLine($"WARNING! Unable to locate dll: {(args.Name.Substring(0, args.Name.IndexOf(",")).ToString() + ".dll").ToLower()} ", "WARNING");
            }
            return myAssembly;
        }
    }
}
