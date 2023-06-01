using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISHealthMonitor.Core.Common
{
    public static class MediaTypes
    {
        public const string Json = @"application/json";
        public const string Stream = @"application/octet-stream";
        public const string JPeg = @"image/jpeg";
    }
    public static class GraphEndpoints
    {
        public const string BaseURL = "https://graph.microsoft.com/";
    }
    public enum HttpContentTypes
    {
        JsonString = 0,
        String = 1,
        Image = 2,
        ByteArray = 3
    }
    public static class CommonFunctions
    {
        public static string Base64Encode(string text)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(text);
            return System.Convert.ToBase64String(bytes);
        }
    }
}
