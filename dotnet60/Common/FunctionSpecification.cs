using System.Collections.Generic;

namespace Fission.Common
{
    public readonly record struct FunctionSpecification(
        string functionName,
        List<Library> libraries,
        string hash,
        string certificatePath
    );

    public readonly record struct Library(
        string name, 
        string path, 
        string nugetPackage
    );
}
