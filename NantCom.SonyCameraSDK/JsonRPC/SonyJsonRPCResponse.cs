using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NantCom.SonyCameraSDK.JsonRPC
{

    /// <summary>
    /// Basic Json RPC Response
    /// </summary>
    public class SonyJsonRPCResponse
    {
        /// <summary>
        /// Gets or sets the result.
        /// </summary>
        /// <value>
        /// The result.
        /// </value>
        [JsonProperty("result")]
        public dynamic RawResult { get; set; }

        /// <summary>
        /// Gets the result.
        /// If Raw Result is Array with length 1 result is exposed as first item in the array.
        /// If there is an error, Exception Object will be returned
        /// Otherwise, the raw result is returned.
        /// </summary>
        /// <value>
        /// The result.
        /// </value>
        [JsonIgnore]
        public dynamic Result
        {
            get
            {
                if (this.Error != null)
                {
                    return new SonyJsonRPCException( (JArray)this.Error );
                }

                if (this.RawResult == null)
                {
                    return null;
                }

                if (this.RawResult is JArray &&
                    ((JArray)this.RawResult).Count == 1)
                {
                    return this.RawResult[0];
                }

                return this.RawResult;
            }
        }

        /// <summary>
        /// Gets or sets the error.
        /// </summary>
        /// <value>
        /// The error.
        /// </value>
        [JsonProperty("error")]
        public dynamic Error { get; set; }

        /// <summary>
        /// Gets or sets the request identifier.
        /// </summary>
        /// <value>
        /// The request identifier.
        /// </value>
        [JsonProperty("id")]
        public int RequestId { get; set; }

        /// <summary>
        /// Gets a value indicating whether this response represents a success call.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is success; otherwise, <c>false</c>.
        /// </value>
        public bool IsSuccess
        {
            get
            {
                return this.Error == null;
            }
        }
    }
}
