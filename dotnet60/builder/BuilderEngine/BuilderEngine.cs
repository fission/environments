using Builder.Model;
using Builder.Utility;
using NugetWorker;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using NuGet.Packaging;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Reflection;
using Microsoft.CodeAnalysis.Emit;
using System.Runtime.Loader;
using System.Text.Json;
using Fission.Functions;
using Fission.Common;

namespace Builder.Engine
{
   public class BuilderEngine
    {
        public string SRC_PKG = string.Empty;

        List<NugetWorker.DllInfo> dllInfos = new List<NugetWorker.DllInfo>();
        List<ExcludeDll> excludeDlls = new List<ExcludeDll>();
        List<IncludeNuget> includeNugets = new List<IncludeNuget>();
        List<string> compileErrors = new List<string>();
        List<string> compileInfo = new List<string>();

        public BuilderEngine()
        {
            SRC_PKG = Environment.GetEnvironmentVariable("SRC_PKG");
        }
    

        public async Task BuildPackage()
        {
            await BuildDllInfo();

            Console.WriteLine("Compiling");
            bool compiled = await TryCompile();
            if (compiled)
            {
                CopyToSourceDir();
                await BuildSpecs();
                Console.WriteLine("Compilation and building function specification done!");
            }
            else
            {
                Console.WriteLine("Compilation failed:");
                foreach(var error in compileErrors)
                {
                    Console.WriteLine($"COMPILATION ERROR: {error}");
                }
                throw new Exception($"COMPILATION FAILED! See builder logs for details, total errors: {compileErrors.Count}");
                
            }

        }

        public void CopyToSourceDir()
        {
            // create folder if it doesn't already exist
            string destinationFile = Path.Combine(SRC_PKG, CompilerHelper.Instance.builderSettings.DllDirectory, "dummy.txt");
            new FileInfo(destinationFile).Directory.Create();

            foreach (var dllinfo in dllInfos)
            {
                string filename = Path.GetFileName(dllinfo.path);
                destinationFile = Path.Combine(SRC_PKG, CompilerHelper.Instance.builderSettings.DllDirectory, filename);
                File.Copy(dllinfo.path, destinationFile,true);
            }
        }
        
        public async Task<bool> TryCompile()
        {
            string codeFile = Path.Combine(SRC_PKG, CompilerHelper.Instance.builderSettings.functionBodyFileName);
            if (!File.Exists(codeFile))
            {
                Console.WriteLine($"Source Code not found at : {codeFile} !" +
                    $" to use TryCompile() in Builder, make sure, your main function file name is " +
                    $"{CompilerHelper.Instance.builderSettings.functionBodyFileName} and " +
                    $"it is located at root of zip!" );
                return false;
            }
            return await Compile();
        }

        public async Task<bool> Compile()
        {
            var syntaxTrees = new List<SyntaxTree>();
            foreach (var codeInDirectory in CompilerHelper.GetCSharpSources(SRC_PKG)) 
            {
                syntaxTrees.Add(CSharpSyntaxTree.ParseText(File.ReadAllText(codeInDirectory)));
            }
            string assemblyName = Path.GetRandomFileName();

            var coreDir = Directory.GetParent(typeof(Enumerable).GetTypeInfo().Assembly.Location);

            List<MetadataReference> references = new List<MetadataReference>
            {
                MetadataReference.CreateFromFile(coreDir.FullName + Path.DirectorySeparatorChar + "mscorlib.dll"),
                MetadataReference.CreateFromFile(coreDir.FullName + Path.DirectorySeparatorChar + "netstandard.dll"),
                MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(Assembly.GetEntryAssembly().Location),
                MetadataReference.CreateFromFile(typeof(System.Runtime.Serialization.Json.DataContractJsonSerializer).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(path: typeof(IFissionFunction).GetTypeInfo().Assembly.Location),
            };

            foreach (var referencedAssembly in Assembly.GetEntryAssembly().GetReferencedAssemblies())
            {
                var assembly = Assembly.Load(referencedAssembly);
                references.Add(MetadataReference.CreateFromFile(assembly.Location));
                BuilderHelper.Instance.logger.Log($"Referring assembly-based dlls:  {assembly.Location}");
            }


            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            foreach (var dll in dllInfos)
            {
                BuilderHelper.Instance.logger.Log($"Referring nuget-based dll: {dll.path}");
                references.Add(MetadataReference.CreateFromFile(dll.path));
            }

            CSharpCompilation compilation = CSharpCompilation.Create(
                assemblyName,
                syntaxTrees: syntaxTrees.ToArray(),
                references: references,
                options: new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary,
                    optimizationLevel: OptimizationLevel.Release));

            var ms = new MemoryStream();
            EmitResult result = compilation.Emit(ms);

            if (!result.Success)
            {
                IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
                    diagnostic.IsWarningAsError ||
                    diagnostic.Severity == DiagnosticSeverity.Error).ToList();

                foreach (Diagnostic diagnostic in failures)
                {
                    compileErrors.Add($"{diagnostic.Id}: {diagnostic.GetMessage()}");
                    BuilderHelper.Instance.logger.Log($"COMPILE ERROR :{diagnostic.Id}: {diagnostic.GetMessage()}");
                }
                return false;
            }

            BuilderHelper.Instance.logger.Log("Compile success!",true);

            return true;
        }

        public async Task BuildSpecs()
        {
            var functionName = CompilerHelper.Instance.builderSettings.functionBodyFileName;
            var libraries = new List<Library>();

            foreach (var dllinfo in dllInfos)
            {
                string combinedPath = Path.Combine(CompilerHelper.Instance.builderSettings.DllDirectory, Path.GetFileName(dllinfo.path));
                string destinationFile = CompilerHelper.GetRelevantPathAsPerOS(combinedPath);

                var library = new Library()
                {
                    name = dllinfo.name,
                    nugetPackage = dllinfo.rootPackage,
                    path = destinationFile
                };
                libraries.Add(library);
            }
            var functionSpecification = new FunctionSpecification(functionName, libraries, "", "");
            string funcMetaJson = JsonSerializer.Serialize(functionSpecification);
            string funcMetaFile = Path.Combine(this.SRC_PKG, CompilerHelper.Instance.builderSettings.functionSpecFileName);
            BuilderHelper.Instance.WriteToFile(funcMetaFile, funcMetaJson);
        }

        public async Task BuildDllInfo()
        {
            includeNugets = BuilderHelper.Instance.GetNugetToInclude(SRC_PKG);

            foreach (var nuget in includeNugets)
            {
                NugetEngine nugetEngine = new NugetEngine();
                await nugetEngine.GetPackage(nuget.packageName, nuget.version);
                dllInfos.AddRange(nugetEngine.dllInfos);
            }

            dllInfos = System.Linq.Enumerable.DistinctBy(dllInfos, x => x.path).ToList();

#if DEBUG
            dllInfos.LogDllPathstoCSV("preFilter.CSV");
#endif

            excludeDlls = BuilderHelper.Instance.GetDllsToExclude(SRC_PKG);
            foreach (var excludedll in excludeDlls)
            {
                BuilderHelper.Instance.logger.Log($"Trying to remove excluded dll, if available: {excludedll.dllName} from package {excludedll.packageName}");
                dllInfos.RemoveAll(x => x.rootPackage.ToLower() == excludedll.packageName.ToLower() && x.name.ToLower() == excludedll.dllName.ToLower());
            }

#if DEBUG
            dllInfos.LogDllPathstoCSV("PostFilter.CSV");
#endif

        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            // This handler is called only when the common language runtime tries to bind to the assembly and fails.

            BuilderHelper.Instance.logger.Log($"Dynamically trying to load dll {(args.Name.Substring(0, args.Name.IndexOf(",")).ToString() + ".dll").ToLower()} in parent assembly");

            // Retrieve the list of referenced assemblies in an array of AssemblyName.
            Assembly MyAssembly = null, objExecutingAssemblies;
            string assemblyPath = "";

            objExecutingAssemblies = Assembly.GetExecutingAssembly();
            AssemblyName[] referencedAssemblyNames = objExecutingAssemblies.GetReferencedAssemblies();

            // Loop through the array of referenced assembly names.
            if (dllInfos.Any(x => x.name.ToLower() == (args.Name.Substring(0, args.Name.IndexOf(",")).ToString() + ".dll").ToLower()))
            {
                assemblyPath = dllInfos.Where(x => x.name.ToLower() == (args.Name.Substring(0, args.Name.IndexOf(",")).ToString() + ".dll").ToLower()).FirstOrDefault().path;

                BuilderHelper.Instance.logger.Log($"loading dll in parent assembly: {assemblyPath}");

                MyAssembly = Assembly.LoadFile(assemblyPath);
            }

            if (MyAssembly == null)
            {
                BuilderHelper.Instance.logger.Log($"WARNING! Unable to locate dll: {(args.Name.Substring(0, args.Name.IndexOf(",")).ToString() + ".dll").ToLower()} ", true);
            }

            return MyAssembly;
        }

    }
}
