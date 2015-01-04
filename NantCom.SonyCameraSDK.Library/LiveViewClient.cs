using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NantCom.SonyCameraSDK
{
    public class ImageReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the image data
        /// </summary>
        /// <value>
        /// The data.
        /// </value>
        public byte[] Data { get; private set; }

        /// <summary>
        /// Gets the callback to call to notify that image has been shown
        /// if this method is not called, LiveViewClient will stop sending frames
        /// after 10 frames have passed
        /// </summary>
        /// <value>
        /// The report completed.
        /// </value>
        public Action ReportCompleted { get; private set; }

        public ImageReceivedEventArgs( byte[] data, Action report )
        {
            this.Data = data;
            this.ReportCompleted = report;
        }
    }

    /// <summary>
    /// Client for getting LiveView
    /// </summary>
    public class LiveViewClient
    {
        private string _Url;

        /// <summary>
        /// Initializes a new instance of the <see cref="LiveViewClient"/> class.
        /// </summary>
        /// <param name="url">The URL.</param>
        internal LiveViewClient( string url )
        {
            _Url = url;
        }

        /// <summary>
        /// Occurs when image received. Handler must call ReportCompleted on args
        /// otherwise the client will start skipping frame as it though that the handler function
        /// cannot keep up.
        /// </summary>
        public event EventHandler<ImageReceivedEventArgs> ImageReceived = delegate { };

        /// <summary>
        /// Occurs when live view was disconnected
        /// </summary>
        public event Action LiveViewDisconnected = delegate { };

        private int _CurrentFrame = 0;
        private int _FrameCompleted = 0;
        private CancellationToken _Token;

        private void ReportCompleted()
        {
            _FrameCompleted++;
        }

        private void OnImageReceived(byte[] data)
        {
            if (_CurrentFrame - _FrameCompleted > 5)
            {
                return;
            }

            _CurrentFrame++;
            this.ImageReceived( this,
                new ImageReceivedEventArgs( data, this.ReportCompleted ));
        }

        /// <summary>
        /// Starts the live view.
        /// </summary>
        /// <param name="getHttpStream">The get HTTP stream.</param>
        /// <param name="cancel">The cancel.</param>
        public void StartLiveView( Func<string, Stream> getHttpStream, CancellationToken cancel)
        {
            _Token = cancel;

            CancellationTokenSource watchDogCancel = new CancellationTokenSource();
            var currentFrame = 0;

            Action liveViewProcesor = () =>
            {
                var myWatchDogToken = watchDogCancel.Token;
                try
                {
                    var stream = getHttpStream(_Url);

                    byte[] headerBuffer = new byte[8 + 128];
                    while (cancel.IsCancellationRequested == false &&
                            myWatchDogToken.IsCancellationRequested == false)
                    {
                        currentFrame++;

                        // camera will flush data in sequence, dont have to store it in temp

                        // read common head and payload header
                        var bytesRead = stream.Read(headerBuffer, 0, 8 + 128);

                        var isJpeg = headerBuffer[1] == 0x01;

                        int startSize = 8 + 4;
                        int payloadSize = headerBuffer[startSize];
                        payloadSize <<= 8;
                        payloadSize += headerBuffer[startSize + 1];
                        payloadSize <<= 8;
                        payloadSize += headerBuffer[startSize + 2];

                        var paddingSize = headerBuffer[startSize + 3];

                        // DO NOT attempt to share this buffer, it has to be sent to
                        // UI thread for decoding JPEG
                        var dataBuffer = new byte[payloadSize + paddingSize];

                        int payloadRead = 0;
                        while (payloadRead != dataBuffer.Length)
                        {
                            var thisRead = stream.Read(dataBuffer, payloadRead, dataBuffer.Length - payloadRead);
                            payloadRead += thisRead;
                        }

                        if (isJpeg)
                        {
                            this.OnImageReceived(dataBuffer);
                        }

                    }
                }
                catch (Exception)
                {
                    // silently fail and wait for watchdog to resurrect
                }
            };

            var liveViewTask = Task.Run(liveViewProcesor, watchDogCancel.Token );

            Action watchDog = async () =>
            {
                var watchDogFrame = 0;
                while (cancel.IsCancellationRequested == false)
                {
                    await Task.Delay(1000);

                    if (watchDogFrame == currentFrame) // no new frame in the last second
                    {
                        watchDogCancel.Cancel();
                        watchDogCancel = new CancellationTokenSource();

                        currentFrame = 0;
                        watchDogFrame = 0;
                        liveViewTask = Task.Run(liveViewProcesor, watchDogCancel.Token);

                        await Task.Delay(2000); // wait for another 2 seconds for things to settled on another thread

                    }
                    else
                    {
                        watchDogFrame = currentFrame;
                    }

                    
                }
            };

            Task.Run(watchDog);
        }

    }
}
