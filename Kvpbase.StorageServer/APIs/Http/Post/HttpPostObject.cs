﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SyslogLogging;
using WatsonWebserver;

using Kvpbase.Containers;
using Kvpbase.Classes;

namespace Kvpbase
{
    public partial class StorageServer
    {
        public static async Task HttpPostObject(RequestMetadata md)
        {
            string header = md.Http.Request.SourceIp + ":" + md.Http.Request.SourcePort + " ";

            #region Retrieve-Container

            ContainerClient client = null;
            ErrorCode error;

            if (!_ContainerMgr.GetContainerClient(md.Params.UserGuid, md.Params.ContainerName, out client))
            {
                _Logging.Warn(header + "HttpPostObject unable to find container " + md.Params.UserGuid + "/" + md.Params.ContainerName);
                md.Http.Response.StatusCode = 404;
                md.Http.Response.ContentType = "application/json";
                await md.Http.Response.Send(Common.SerializeJson(new ErrorResponse(5, 404, null, null), true));
                return;
            }
                 
            #endregion

            #region Authenticate-and-Authorize

            if (!client.Container.IsPublicWrite)
            {
                if (md.User == null || !(md.User.GUID.ToLower().Equals(md.Params.UserGuid.ToLower())))
                {
                    _Logging.Warn(header + "HttpPostObject unauthorized unauthenticated write attempt to object " + md.Params.UserGuid + "/" + md.Params.ContainerName + "/" + md.Params.ObjectKey);
                    md.Http.Response.StatusCode = 401;
                    md.Http.Response.ContentType = "application/json";
                    await md.Http.Response.Send(Common.SerializeJson(new ErrorResponse(3, 401, null, null), true));
                    return;
                }
            }

            if (md.Perm != null)
            {
                if (!md.Perm.WriteObject)
                {
                    _Logging.Warn(header + "HttpPostObject unauthorized write attempt to object " + md.Params.UserGuid + "/" + md.Params.ContainerName + "/" + md.Params.ObjectKey);
                    md.Http.Response.StatusCode = 401;
                    md.Http.Response.ContentType = "application/json";
                    await md.Http.Response.Send(Common.SerializeJson(new ErrorResponse(3, 401, null, null), true));
                    return;
                }
            }

            #endregion

            #region Check-if-Object-Exists

            if (client.Exists(md.Params.ObjectKey))
            {
                _Logging.Warn(header + "HttpPostObject object " + md.Params.UserGuid + "/" + md.Params.ContainerName + "/" + md.Params.ObjectKey + " already exists");
                md.Http.Response.StatusCode = 409;
                md.Http.Response.ContentType = "application/json";
                await md.Http.Response.Send(Common.SerializeJson(new ErrorResponse(7, 409, null, null), true));
                return; 
            }

            #endregion

            #region Verify-Transfer-Size

            if (_Settings.Server.MaxObjectSize > 0 && md.Http.Request.ContentLength > _Settings.Server.MaxObjectSize)
            {
                _Logging.Warn(header + "HttpPostObject object size too large (" + md.Http.Request.ContentLength + " bytes)");
                md.Http.Response.StatusCode = 413;
                md.Http.Response.ContentType = "application/json";
                await md.Http.Response.Send(Common.SerializeJson(new ErrorResponse(11, 413, null, null), true));
                return;
            }

            #endregion

            #region Write-and-Return
                  
            if (!_ObjectHandler.Create(md, client, md.Params.ObjectKey, md.Http.Request.ContentType, md.Http.Request.ContentLength, md.Http.Request.Data, out error))
            {
                _Logging.Warn(header + "HttpPostObject unable to write object " + md.Params.UserGuid + "/" + md.Params.ContainerName + "/" + md.Params.ObjectKey + ": " + error.ToString());

                int statusCode = 0;
                int id = 0;
                Helper.StatusFromContainerErrorCode(error, out statusCode, out id);

                md.Http.Response.StatusCode = statusCode;
                md.Http.Response.ContentType = "application/json";
                await md.Http.Response.Send(Common.SerializeJson(new ErrorResponse(id, statusCode, "Unable to write object.", error), true));
                return;
            }
            else
            {
                md.Http.Response.StatusCode = 201;
                await md.Http.Response.Send();
                return;
            }

            #endregion 
        }
    }
}