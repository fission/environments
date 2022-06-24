using System;
using System.Collections.Generic;
using System.Text;

namespace Fission.Common
{
    public readonly record struct DllInfo(string name, string rootPackage, string framework, string processor, string path);
}
