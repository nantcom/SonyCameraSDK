using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NantCom.SonyCameraSDK
{
    public static class Utility
    {
        /// <summary>
        /// Converts JArray to Array of Given type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array">The array.</param>
        /// <returns></returns>
        public static string[] ToArray( JArray array )
        {
            return (from item in array
                    select item.ToString()).ToArray();
        }

    }
}
