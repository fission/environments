using System;

namespace Fission.DotNet.Common;

public class FissionMqContext : FissionContext
{
    public FissionMqContext(Stream body, Dictionary<string, object> arguments, Dictionary<string, string> headers, Dictionary<string, string> parameters) : base(body, arguments, headers, parameters)
    {
        
    }

    public string Topic => GetHeaderValue("Topic");
    public string ErrorTopic => GetHeaderValue("Errortopic");
    public string ResponseTopic => GetHeaderValue("Resptopic");
}
