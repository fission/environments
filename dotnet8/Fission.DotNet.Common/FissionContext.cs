using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Fission.DotNet.Common;

public class FissionContext
{
    protected Stream _content;
    protected Dictionary<string, object> _arguments;
    protected Dictionary<string, string> _headers;
    protected Dictionary<string, string> _parameters;

    public FissionContext(Stream body, Dictionary<string, object> arguments, Dictionary<string, string> headers, Dictionary<string, string> parameters)
    {
        if (body == null) throw new ArgumentNullException(nameof(body));
        if (arguments == null) throw new ArgumentNullException(nameof(arguments));
        if (headers == null) throw new ArgumentNullException(nameof(headers));
        if (parameters == null) throw new ArgumentNullException(nameof(parameters));

        _content = body;
        _arguments = arguments;
        _headers = headers;
        _parameters = parameters;
    }

    protected string GetHeaderValue(string key, string defaultValue = null)
    {
        return _headers.ContainsKey(key) ? _headers[key] : defaultValue;
    }

    public Dictionary<string, object> Arguments => _arguments;
    public Dictionary<string, string> Parameters => _parameters;

    public string TraceID => GetHeaderValue("traceparent", Guid.NewGuid().ToString());
    public string FunctionName => GetHeaderValue("X-Fission-Function-Name");
    public string Namespace => GetHeaderValue("X-Fission-Function-Namespace");
    public string ResourceVersion => GetHeaderValue("X-Fission-Function-Resourceversion");
    public string UID => GetHeaderValue("X-Fission-Function-Uid");
    public string Trigger => GetHeaderValue("Source-Name");
    public string ContentType => GetHeaderValue("Content-Type");
    public Int32 ContentLength => GetHeaderValue("Content-Length") != null ? Int32.Parse(GetHeaderValue("Content-Length")) : 0;
    public Stream Content => _content;

    public async Task<string?> ContentAsString()
    {
        if (_content == null)
        {
            return null;
        }

        _content.Position = 0;
        using (StreamReader reader = new StreamReader(_content, Encoding.UTF8, leaveOpen: true))
        {
            return await reader.ReadToEndAsync();
        }
    }

    public async Task<T> ContentAs<T>(JsonSerializerOptions? options = null)
    {
        if (_content == null)
        {
            return default;
        }

        _content.Position = 0;
        using (StreamReader reader = new StreamReader(_content, Encoding.UTF8, leaveOpen: true))
        {
            string content = await reader.ReadToEndAsync();
            return JsonSerializer.Deserialize<T>(content, options);
        }
    }
}
