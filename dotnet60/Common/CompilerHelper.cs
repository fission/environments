using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Linq;

namespace Fission.Common
{

    public sealed class CompilerHelper
    {

        private static readonly Lazy<CompilerHelper> lazy =
            new Lazy<CompilerHelper>(() => new CompilerHelper());

        public string _logFileName = string.Empty;
        public static CompilerHelper Instance { get { return lazy.Value; } }

        private BuilderSettings? _builderSettings = null;

        public BuilderSettings builderSettings
        {
            get
            {
                if (_builderSettings == null)
                {
                    string builderSettingsjson = GetBuilderSettingsJson();
                    _builderSettings = JsonSerializer.Deserialize<BuilderSettings>(builderSettingsjson);
                }

                return _builderSettings.Value;
            }
            set
            {
                builderSettings = value;
            }
        }

        static CompilerHelper()
        {
        }

        private CompilerHelper()
        {
        }

        public static string GetRelevantPathAsPerOS(string currentPath)
        {
            if (CompilerHelper.Instance.builderSettings.RunningOnWindows)
            {
                return currentPath;
            }
            else
            {
                return currentPath.Replace("\\","/");
            }
        }

        private string GetBuilderSettingsJson()
        {
            var path = AppDomain.CurrentDomain.BaseDirectory + "builderSettings.json";
            return System.IO.File.ReadAllText(path);
        }

        public FunctionSpecification GetFunctionSpecs(string directoryPath)
        {
            string functionSpecsFilePath = Path.Combine(directoryPath, this.builderSettings.functionSpecFileName);
            if (!File.Exists(functionSpecsFilePath))
            {
                string specsJson = File.ReadAllText(functionSpecsFilePath);
                return JsonSerializer.Deserialize<FunctionSpecification>(specsJson);
            }
            
            throw new Exception($"Function Specification file not found at {functionSpecsFilePath}");
        }

        public static IEnumerable<string> GetDirectoryFiles(string rootPath, string patternMatch, SearchOption searchOption)
        {
            var foundFiles = Enumerable.Empty<string>();

            if (searchOption == SearchOption.AllDirectories)
            {
                try
                {
                    IEnumerable<string> subDirs = Directory.EnumerateDirectories(rootPath);
                    foreach (string dir in subDirs)
                    {
                        foundFiles = foundFiles.Concat(GetDirectoryFiles(dir, patternMatch, searchOption)); // Add files in subdirectories recursively to the list
                    }
                }
                catch (UnauthorizedAccessException) {}
                catch (PathTooLongException) {}
            }

            try
            {
                foundFiles = foundFiles.Concat(Directory.EnumerateFiles(rootPath, patternMatch)); // Add files from the current directory
            }
            catch (UnauthorizedAccessException) {}

            return foundFiles;
        }

        static public IEnumerable<string> GetCSharpSources(string path)
        {
            return GetDirectoryFiles(path, "*.cs", SearchOption.AllDirectories);
        }
    }

}
