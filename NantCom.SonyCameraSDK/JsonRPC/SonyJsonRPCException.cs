using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NantCom.SonyCameraSDK.JsonRPC
{
    /// <summary>
    /// Represents an error from Sony JsonRPC Server
    /// </summary>
    public class SonyJsonRPCException : Exception
    {
        public int Code { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SonyJsonRPCException"/> class.
        /// </summary>
        /// <param name="error">The error.</param>
        public SonyJsonRPCException(JArray error)
            : base((string)error[1])
        {
            this.Code = (int)error[0];
        }
    }
}
