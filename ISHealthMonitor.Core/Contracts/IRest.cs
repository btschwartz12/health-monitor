﻿using ISHealthMonitor.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISHealthMonitor.Core.Contracts
{
    public interface IRest
    {
        Task<string> GetHttpContent(string url);
        //string PatchHttpContentWithToken(string url, string bodycontent, string token, string contentType);
        Task<string> PutHttpContent(string url, string bodycontent, HttpContentTypes types);
        Task<string> PostHttpContent(string url, string bodycontent, HttpContentTypes types);
        Task<string> GetHttpContentWithToken(string url);
        //string PatchHttpContentWithToken(string url, string bodycontent, string token, string contentType);
        Task<string> PutHttpContentWithToken(string url, string bodycontent, HttpContentTypes types);
        Task<string> PostHttpContentWithToken(string url, string bodycontent, HttpContentTypes types);
    }
}
