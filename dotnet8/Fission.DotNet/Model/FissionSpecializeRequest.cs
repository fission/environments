using System;

namespace Fission.DotNet.Model;

public class FissionSpecializeRequest
{
    public string filepath { get; set; }
    public string functionName { get; set; }
    public string url { get; set; }
}
