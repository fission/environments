using System;
using System.Collections.Generic;
using Fission.DotNet.Common;

namespace Fission.DotNet.Services;

public class CorsPolicy : ICorsPolicy
{
    private HashSet<string> _origins = new HashSet<string>();
    private HashSet<string> _headers = new HashSet<string>();
    private HashSet<string> _methods = new HashSet<string>();
    private bool _allowAnyOrigin = false;
    private bool _allowAnyHeader = false;
    private bool _allowAnyMethod = false;
    private bool _allowCredentials = false;

    public void AllowAnyOrigin()
    {
        _allowAnyOrigin = true;
    }

    public void AllowAnyHeader()
    {
        _allowAnyHeader = true;
    }

    public void AllowAnyMethod()
    {
        _allowAnyMethod = true;
    }

    public void AllowCredentials()
    {
        _allowCredentials = true;
    }

    public void WithOrigin(string[] origins)
    {
        foreach (var origin in origins)
        {
            _origins.Add(origin);
        }
    }

    public void WithHeader(string[] headers)
    {
        foreach (var header in headers)
        {
            _headers.Add(header);
        }
    }

    public void WithMethod(string[] methods)
    {
        foreach (var method in methods)
        {
            _methods.Add(method);
        }
    }

    public Dictionary<string, string> GetCorsHeaders()
    {
        var headers = new Dictionary<string, string>();

        if (_allowAnyOrigin)
        {
            headers["Access-Control-Allow-Origin"] = "*";
        }
        else if (_origins.Count > 0)
        {
            headers["Access-Control-Allow-Origin"] = string.Join(", ", _origins);
        }

        if (_allowAnyHeader)
        {
            headers["Access-Control-Allow-Headers"] = "*";
        }
        else if (_headers.Count > 0)
        {
            headers["Access-Control-Allow-Headers"] = string.Join(", ", _headers);
        }

        if (_allowAnyMethod)
        {
            headers["Access-Control-Allow-Methods"] = "*";
        }
        else if (_methods.Count > 0)
        {
            headers["Access-Control-Allow-Methods"] = string.Join(", ", _methods);
        }

        if (_allowCredentials)
        {
            headers["Access-Control-Allow-Credentials"] = "true";
        }

        return headers;
    }

    public IDictionary<string, string> GetRequestCorsHeaders()
    {
        var headers = new Dictionary<string, string>();

        if (_allowAnyOrigin)
        {
            headers["Access-Control-Allow-Origin"] = "*";
        }
        else if (_origins.Count > 0)
        {
            headers["Access-Control-Allow-Origin"] = string.Join(", ", _origins);
        }
        
        return headers;
    }
}