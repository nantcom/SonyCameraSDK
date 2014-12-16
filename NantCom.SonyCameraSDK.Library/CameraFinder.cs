using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Xml.Linq;
using Windows.Web.Http;

namespace NantCom.SonyCameraSDK
{
    public class CameraFinder
    {
        /// <summary>
        /// Discovers the camera.
        /// </summary>
        /// <param name="maximumWaitTime">The maximum wait time.</param>
        /// <returns></returns>
        public async Task<CameraApiClient> DiscoverCamera( TimeSpan maximumWaitTime )
        {
            var cancel = new CancellationTokenSource();
            cancel.CancelAfter(maximumWaitTime);

            var result = await CameraFinder.SendSSDP(cancel.Token);
            var response = result.FirstOrDefault();

            if (response != null)
            {
                var location = (from lines in response.Split('\r', '\n')
                                where lines.StartsWith("location:", StringComparison.OrdinalIgnoreCase)
                                select lines.Substring("location:".Length)).FirstOrDefault();

                if (location == null)
                {
                    return null;
                }

                return await this.CreateClientFromSSDPResponse(location);
            }

            return null;
        }

        private async Task<CameraApiClient> CreateClientFromSSDPResponse( string xmlLocation )
        {
            HttpClient client = new HttpClient();
            var xml = await client.GetInputStreamAsync(new Uri(xmlLocation));

            XDocument doc = XDocument.Load( xml.AsStreamForRead() );
            XNamespace av = "urn:schemas-sony-com:av";
            XNamespace root = "urn:schemas-upnp-org:device-1-0";


            var services = doc.Root.Element(root + "device")
                            .Element(av + "X_ScalarWebAPI_DeviceInfo")
                            .Element(av + "X_ScalarWebAPI_ServiceList")
                            .Elements(av + "X_ScalarWebAPI_Service")
                            .ToDictionary(
                                e => e.Element(av + "X_ScalarWebAPI_ServiceType").Value,
                                e => e.Element(av + "X_ScalarWebAPI_ActionList_URL").Value);

            return new CameraApiClient( 
                doc.Root.Element(root + "device").Element( root + "friendlyName").Value,
                services["camera"], 
                services["system"], 
                services.ContainsKey("avContent") ? services["avContent"] : "" );
        }

        /// <summary>
        /// Send SSDP Packet to camera
        /// </summary>
        /// <param name="urn">The urn.</param>
        /// <param name="cancelToken">The cancel token.</param>
        private async static Task<List<string>> SendSSDP(CancellationToken cancelToken)
        {
            var responses = new List<string>();
            var multicastIP = new HostName("239.255.255.250");
            var port = "1900";
            var waitForRecevier = new ManualResetEventSlim(false);

            await Task.Run( async () =>
            {
                using (var socket = new DatagramSocket())
                {
                    socket.MessageReceived += (s, e) =>
                    {
                        var reader = e.GetDataReader();
                        uint count = reader.UnconsumedBufferLength;
                        string data = reader.ReadString(count);

                        responses.Add(data);
                        waitForRecevier.Set();
                    };

                    await socket.BindEndpointAsync(null, string.Empty);
                    socket.JoinMulticastGroup(multicastIP);


                    var request =
                        string.Concat("M-SEARCH * HTTP/1.1\r\n",
                        "Host: 239.255.255.250:1900\r\n",
                        "MAN: \"ssdp:discover\"\r\n",
                        "MX: 3\r\n",
                        "ST: urn:schemas-sony-com:service:ScalarWebAPI:1\r\n\r\n");

                    var bytes = Encoding.UTF8.GetBytes(request).AsBuffer();

                    while (cancelToken.IsCancellationRequested == false && responses.Count == 0)
                    {
                        using (var stream = await socket.GetOutputStreamAsync(multicastIP, port))
                        {
                            await stream.WriteAsync(bytes);
                        }

                        waitForRecevier.Wait(500);
                    }
                }

            });

            return responses;
        }
    }
}
