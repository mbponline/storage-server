﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Kvpbase.Classes
{
    /// <summary>
    /// Entry types for URL locks.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum LockType
    {
        [EnumMember(Value = "Read")]
        Read,
        [EnumMember(Value = "Write")]
        Write 
    }
}
