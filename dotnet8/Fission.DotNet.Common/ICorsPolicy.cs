using System;

namespace Fission.DotNet.Common
{
    public interface ICorsPolicy
    {
        void AllowAnyOrigin();
        void AllowAnyHeader();
        void AllowAnyMethod();
        void AllowCredentials();

        void WithOrigin(string[] origin);
        void WithHeader(string[] header);
        void WithMethod(string[] method);
    }
}
