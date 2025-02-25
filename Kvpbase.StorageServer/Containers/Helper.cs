﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Kvpbase.Classes;

namespace Kvpbase.Containers
{ 
    public static class Helper
    { 
        public static void StatusFromContainerErrorCode(ErrorCode error, out int statusCode, out int id)
        {
            statusCode = 0;
            id = 0; 

            switch (error)
            {
                case ErrorCode.None:
                case ErrorCode.Success:
                    statusCode = 200;
                    break;

                case ErrorCode.Created:
                    statusCode = 201;
                    break;

                case ErrorCode.OutOfRange:
                    id = 2;
                    statusCode = 416;
                    break;

                case ErrorCode.NotFound:
                    id = 5;
                    statusCode = 404;
                    break;

                case ErrorCode.AlreadyExists:
                    id = 7;
                    statusCode = 409;
                    break;

                case ErrorCode.Locked:
                    id = 8;
                    statusCode = 409;
                    break;

                case ErrorCode.ServerError:
                case ErrorCode.IOError:
                case ErrorCode.PermissionsError:
                case ErrorCode.DiskFull:
                    id = 4;
                    statusCode = 500;
                    break;

                default:
                    throw new ArgumentException("Unknown error code: " + error.ToString());
            }

            return;
        }
    }
}
