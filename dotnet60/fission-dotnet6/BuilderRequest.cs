using System;
using System.Collections.Generic;
using System.Text;

namespace Fission.DotNet
{
    public readonly record struct FunctionMetadata(
        string name, 
        string @namespace, 
        string selfLink, 
        string uid, 
        string resourceVersion, 
        int generation, 
        DateTime creationTimestamp
    );
    
    public readonly record struct BuilderRequest(
        string filepath, 
        string functionName, 
        string url, 
        FunctionMetadata FunctionMetadata
    );
}
