using System;
using System.Collections.Generic;
using System.Text;

namespace Builder.Model
{
    public readonly record struct IncludeNuget(
        string packageName, string version
    );
}
