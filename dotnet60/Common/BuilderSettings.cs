using System;
using System.Collections.Generic;
using System.Text;

namespace Fission.Common
{
    public readonly record struct BuilderSettings(
        string NugetSpecsFile,
        string DllExcludeFile,
        string BuildLogDirectory,
        string NugetPackageRegEx,
        string ExcludeDllRegEx,
        bool RunningOnWindows,
        string functionBodyFileName,
        string functionSpecFileName,
        string DllDirectory
    );
}
