using System;
using System.Text;
using System.Text.Json;

namespace Fission.DotNet.Common;

public class FissionHttpContext : FissionContext
{
    private string _method;

    public FissionHttpContext(Stream body, string method, Dictionary<string, object> arguments, Dictionary<string, string> headers, Dictionary<string, string> parameters) : base(body, arguments, headers, parameters)
    {
        _method = method;
    }

    public Dictionary<string, string> Headers => _headers;
    public string Url
    {
        get
        {
            var urlHeader = GetHeaderValue("X-Fission-Full-Url");

            if (urlHeader != null)
            {
                if (urlHeader.Contains("?"))
                {
                    urlHeader = urlHeader.Substring(0, urlHeader.IndexOf("?"));
                }

                return urlHeader;
            }
            else
            {
                return "/";
            }
        }
    }
    public string Method => _method;
    public string Host => GetHeaderValue("X-Forwarded-Host");
    public int Port => _headers.ContainsKey("X-Forwarded-Port") ? Int32.Parse(GetHeaderValue("X-Forwarded-Port")) : 0;
    public string UserAgent => GetHeaderValue("User-Agent");
}
