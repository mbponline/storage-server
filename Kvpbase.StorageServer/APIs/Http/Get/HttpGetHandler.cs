﻿using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SyslogLogging;
using WatsonWebserver;

using Kvpbase.Classes;

namespace Kvpbase
{
    public partial class StorageServer
    {
        public static async Task HttpGetHandler(RequestMetadata md)
        {
            string header = md.Http.Request.SourceIp + ":" + md.Http.Request.SourcePort + " ";

            if (md.Http.Request.RawUrlEntries.Count == 1)
            {
                await HttpGetContainerList(md);
                return;
            }
            else if (md.Http.Request.RawUrlEntries.Count == 2)
            {
                await HttpGetContainer(md);
                return;
            }
            else if (md.Http.Request.RawUrlEntries.Count >= 3)
            {
                await HttpGetObject(md);
                return;
            }
            else
            {
                _Logging.Warn(header + "HttpGetHandler container URL does not conform to required structure");
                md.Http.Response.StatusCode = 400;
                md.Http.Response.ContentType = "application.json";
                await md.Http.Response.Send(Common.SerializeJson(new ErrorResponse(2, 400, "URL path must be of the form /[user]/[container]/[key].", null), true));
                return;
            } 
        }
    }
}