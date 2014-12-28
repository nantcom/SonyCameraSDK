using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using NantCom.SonyCameraSDK.JsonRPC;
using System.Threading.Tasks;
using PortableRest;
using System.Threading;
using System.Net.Http;
using System.Diagnostics;
using Newtonsoft.Json;

namespace NantCom.SonyCameraSDK
{
    /// <summary>
    /// DynamicObject class which handles sending JsonRPC request
    /// </summary>
    internal class ApiRequestor : DynamicObject
    {
        private string _BaseUrl;

        public ApiRequestor( string baseUrl )
        {
            _BaseUrl = baseUrl;
        }

        private RestClient _RestClient;

        /// <summary>
        /// Handles translation of method call to JsonRPC Request
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="args"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            if (_RestClient == null)
            {
                _RestClient = new RestClient();
                _RestClient.JsonSerializerSettings = new Newtonsoft.Json.JsonSerializerSettings()
                {
                    DateFormatHandling = Newtonsoft.Json.DateFormatHandling.IsoDateFormat,
                };
            }

            
            var requestInfo = new SonyJsonRPCRequest();
            requestInfo.RequestId = SonyJsonRPCRequest.LatestRequestId++;

            if (binder.Name.EndsWith("WithVersion"))
	        {
                requestInfo.Method = binder.Name.Replace("WithVersion", "");
                requestInfo.Parameters = args.Skip(1).ToArray();
                requestInfo.Version = (string)args[0];

	        } else
            {
                requestInfo.Method = binder.Name;
                requestInfo.Parameters = args;
                requestInfo.Version = "1.0";
            }


            var req = new RestRequest( _BaseUrl );
            req.Method = HttpMethod.Post;
            req.ContentType = ContentTypes.Json;
            req.AddParameter(requestInfo);
            
            result = TaskEx.Run(() =>
            {
                Debug.WriteLine("API Request: {0}, Body {1}", _BaseUrl, JsonConvert.SerializeObject(requestInfo));

                SonyJsonRPCResponse response = null;
                try
                {
                    var serverResponse = _RestClient.SendAsync<SonyJsonRPCResponse>(req).Result;
                    response = serverResponse.Content;
                }
                catch (Exception ex)
                {
                    // there is error - build our own response
                    response = new SonyJsonRPCResponse();
                    response.Error = new object [] { 500, ex.Message, ex };


                }

#if DEBUG
                if (response != null && response.IsSuccess == false)
                {
                    Debug.WriteLine("API Request Failed: {0}", (string)response.Error[1]);
                }
#endif

                return response;
            });

            return true;
        }
    }
    
    /// <summary>
    /// Class which maps all Sony Camera SDK APIs
    /// </summary>
    public class CameraApiClient
    {
        private dynamic camera;
        private dynamic avContent;
        private dynamic system;

        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        /// <value>
        /// The model.
        /// </value>
        public string Model { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CameraApiClient"/> class.
        /// </summary>
        /// <param name="baseUrl">The base URL.</param>
        public CameraApiClient( string model, string cameraBaseUrl, string systemBaseUrl, string avContentBaseUrl )
        {
            this.Model = model;
            this.camera = new ApiRequestor(cameraBaseUrl + "/camera");
            this.avContent = new ApiRequestor(avContentBaseUrl + "/avContent");
            this.system = new ApiRequestor(systemBaseUrl + "/system");
        }

        /// <summary>
        /// This API provides a function to set a value of shooting mode.
        /// </summary>
        /// <param name="mode"></param>
        /// <returns>
        /// When the execution of the API is successful, 0 is set. If API is not successful, "result" member is not returned, and "error" member is returned. See Status code & Error for error detail.
        /// </returns>
        public Task<SonyJsonRPCResponse> SetShootMode(string mode)
        {
            return camera.setShootMode(mode);
        }

        /// <summary>
        /// This API provides a function to get current camera shooting mode.
        /// </summary>
        /// <returns>
        /// A list of supported shoot modes (See Shoot mode parameters of Parameter description)
        /// </returns>
        public Task<SonyJsonRPCResponse> GetShootMode()
        {
            return camera.getShootMode();
        }

        /// <summary>
        /// This API provides a function to take picture.
        /// </summary>
        /// <returns>
        /// Array of URLs of postview. The postview is captured image data by camera. The postview image can be used for storing it as the taken picture, and showing it to the client display.
        /// </returns>
        public Task<SonyJsonRPCResponse> ActTakePicture()
        {
            return camera.actTakePicture();
        }

        /// <summary>
        /// This API provides a function to wait while the camera is taking the picture.
        /// </summary>
        /// <returns>
        /// Array of URLs of postview. The postview is captured image data by camera. The postview image can be used for storing it as the taken picture, and showing it to the client display.
        /// </returns>
        public Task<SonyJsonRPCResponse> AwaitTakePicture()
        {
            return camera.awaitTakePicture();
        }

        /// <summary>
        /// This API provides a function to start continuous shooting.
        /// </summary>
        /// <returns>
        /// None.
        /// </returns>
        public Task<SonyJsonRPCResponse> StartContShooting()
        {
            return camera.startContShooting();
        }

        /// <summary>
        /// This API provides a function to stop continuous shooting.
        /// </summary>
        /// <returns>
        /// None.
        /// </returns>
        public Task<SonyJsonRPCResponse> StopContShooting()
        {
            return camera.stopContShooting();
        }

        /// <summary>
        /// This API provides a function to to start recording movie.
        /// </summary>
        /// <returns>
        /// Return parameter When the execution of the API is successful, 0 is set. If API is not successful, "result" member is not returned, and "error" member is returned. See Status code & Error for error detail.
        /// </returns>
        public Task<SonyJsonRPCResponse> StartMovieRec()
        {
            return camera.startMovieRec();
        }

        /// <summary>
        /// This API provides a function to stop recording movie.
        /// </summary>
        /// <returns>
        /// Reserved. Empty string will be set.
        /// </returns>
        public Task<SonyJsonRPCResponse> StopMovieRec()
        {
            return camera.stopMovieRec();
        }

        /// <summary>
        /// This API provides a function to start audio recording.
        /// </summary>
        /// <returns>
        /// When the execution of the API is successful, 0 is set. If API is not successful, "result" member is not returned, and "error" member is returned. See Status code & Error for error detail.
        /// </returns>
        public Task<SonyJsonRPCResponse> StartAudioRec()
        {
            return camera.startAudioRec();
        }

        /// <summary>
        /// This API provides a function to stop audio recording.
        /// </summary>
        /// <returns>
        /// When the execution of the API is successful, 0 is set. If API is not successful, "result" member is not returned, and "error" member is returned. See Status code & Error for error detail.
        /// </returns>
        public Task<SonyJsonRPCResponse> StopAudioRec()
        {
            return camera.stopAudioRec();
        }

        /// <summary>
        /// This API provides a function to start interval still recording.
        /// </summary>
        /// <returns>
        /// When the execution of the API is successful, 0 is set. If API is not successful, "result" member is not returned, and "error" member is returned. See Status code & Error for error detail.
        /// </returns>
        public Task<SonyJsonRPCResponse> StartIntervalStillRec()
        {
            return camera.StartIntervalStillRec();
        }

        /// <summary>
        /// This API provides a function to stop interval still recording.
        /// </summary>
        /// <returns>
        /// When the execution of the API is successful, 0 is set. If API is not successful, "result" member is not returned, and "error" member is returned. See Status code & Error for error detail.
        /// </returns>
        public Task<SonyJsonRPCResponse> StopIntervalStillRec()
        {
            return camera.stopIntervalStillRec();
        }

        /// <summary>
        /// This API provides a function to start liveview.
        /// </summary>
        /// <returns>
        /// Array of URL of liveview
        /// </returns>
        public Task<SonyJsonRPCResponse> StartLiveview()
        {
            return camera.startLiveview();
        }

        /// <summary>
        /// This API provides a function to stop liveview.
        /// </summary>
        /// <returns>
        /// When the execution of the API is successful, 0 is set. If API is not successful, "result" member is not returned, and "error" member is returned. See Status code & Error for error detail.
        /// </returns>
        public Task<SonyJsonRPCResponse> StopLiveview()
        {
            return camera.stopLiveview();
        }

        /// <summary>
        /// This API provides a function to start liveview with specific liveview size.
        /// </summary>
        /// <param name="size">Liveview size (See Liveview size parameter of Parameter description)</param>
        /// <returns> 
        /// Array of URL of liveview
        /// </returns>
        public Task<SonyJsonRPCResponse> StartLiveviewWithSize( string size)
        {
            return camera.startLiveviewWithSize();
        }

        /// <summary>
        /// This API provides a function to get current liveview size.
        /// </summary>
        /// <returns>
        /// Current liveview size (See Liveview size parameter of Parameter description)
        /// </returns>
        public Task<SonyJsonRPCResponse> GetLiveviewSize()
        {
            return camera.getLiveviewSize();
        }

        /// <summary>
        /// This API call is obsolete and not implemented, use GetAvailableLiveviewSize Instead
        /// </summary>
        /// <returns></returns>
        [Obsolete]
        public Task<SonyJsonRPCResponse> GetSupportedLiveviewSize()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This API provides a function to get current liveview size and the available liveview sizes at the moment. The available parameters can be changed by user operations and calling APIs.
        /// </summary>
        /// <returns>
        /// Index 0 - Current liveview size (See Liveview size parameter of Parameter description).
        /// Index 1 - A list of available liveview sizes
        /// </returns>
        public Task<SonyJsonRPCResponse> GetAvailableLiveviewSize()
        {
            return camera.getAvailableLiveviewSize();
        }

        /// <summary>
        /// This API provides a function to switch the liveview frame information transferring. The liveview frame information includes focus frames, face detection frames and tracking frames on the liveview.
        /// </summary>
        /// <param name="includeFrameInfo">true - Transfer the liveview frame information, false - Not transfer</param>
        /// <returns>
        /// Empty.
        /// </returns>
        public Task<SonyJsonRPCResponse> SetLiveviewFrameInfo(bool includeFrameInfo)
        {
            return camera.setLiveviewFrameInfo( new { frameInfo = includeFrameInfo });
        }

        /// <summary>
        /// This API provides a function to get current setting of the liveview frame information transferring.
        /// </summary>
        /// <returns>
        /// Object with One Attribute "frameInfo", if attribute is true - Transfer the liveview frame information.
        /// </returns>
        public Task<SonyJsonRPCResponse> GetLiveviewFrameInfo()
        {
            return camera.getLiveviewFrameInfo();
        }

        /// <summary>
        /// This API provides a function to zoom.
        /// </summary>
        /// <returns>
        /// When the execution of the API is successful, 0 is set. If API is not successful, "result" member is not returned, and "error" member is returned. See Status code & Error for error detail.
        /// </returns>
        public Task<SonyJsonRPCResponse> ActZoom(ZoomDirection direction, ZoomMovement movement)
        {
            return camera.actZoom( 
                direction.ToString().ToLowerInvariant(), 
                movement.ToString().ToLowerInvariant().Replace( "One", "1" ));
        }

        /// <summary>
        /// This API provides a function to set a value of zoom setting.
        /// </summary>
        /// <param name="setting">Zoom setting (See Zoom parameters of Parameter description)</param>
        /// <returns>None.</returns>
        public Task<SonyJsonRPCResponse> SetZoomSetting(  string parameter )
        {
            return camera.setZoomSetting( new { zoom = parameter } );
        }

        /// <summary>
        /// This API provides a function to get current zoom setting.
        /// </summary>
        /// <returns>
        /// An object with attribute "zoom" with value set to Current zoom setting (See Zoom parameters of Parameter description)
        /// </returns>
        public Task<SonyJsonRPCResponse> GetZoomSetting()
        {
            return camera.getZoomSetting();
        }

        /// <summary>
        /// This API is obsolete and was not implemented. The client should use "getAvailableZoomSetting" to get the available parameters at the moment.
        /// </summary>
        /// <returns></returns>
        [Obsolete]
        public Task<SonyJsonRPCResponse> GetSupportedZoomSetting()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This API provides a function to get current zoom setting and the available zoom settings at the moment. The available parameters can be changed by user operations and calling APIs.
        /// </summary>
        /// <returns>
        /// An object with attribute "zoom" with value set to Current zoom setting (See Zoom parameters of Parameter description),
        /// "candidate" with value set to A list of available zoom settings
        /// </returns>
        public Task<SonyJsonRPCResponse> GetAvailableZoomSetting()
        {
            return camera.getAvailableZoomSetting();
        }

        /// <summary>
        /// This API provides a function to half-press shutter.
        /// </summary>
        /// <returns>None.</returns>
        public Task<SonyJsonRPCResponse> ActHalfPressShutter()
        {
            return camera.actHalfPressShutter();
        }

        /// <summary>
        /// This API provides a function to cancel half-press shutter.
        /// </summary>
        /// <returns>None.</returns>
        public Task<SonyJsonRPCResponse> CancelHalfPressShutter()
        {
            return camera.cancelHalfPressShutter();
        }

        /// <summary>
        /// This API provides a function to enable touch AF and the position.
        /// </summary>
        /// <param name="x">X-Axis position in percentage of image width</param>
        /// <param name="y">Y-Axis position in percentage of image height</param>
        /// <returns>
        /// Array with 2 elements.
        /// Index 0 - 0 Indicate a successful operation,
        /// Index 1 - And object with attrbutes "AFResult" set to true if AF is successful or false otherwise,
        /// and attribute "AFType" set to "Touch" if focus is made to expact touch area or "Wide" if focus is made to wider area than specified area.
        /// </returns>
        public Task<SonyJsonRPCResponse> SetTouchAFPosition( double x, double y)
        {
            return camera.setTouchAFPosition( x, y);
        }

        /// <summary>
        /// This API provides a function to get current touch AF position.
        /// </summary>
        /// <returns>
        /// Object with attributes "set" set to true if AF is successful, "touchCoordinates" is reserved always empty array.
        /// </returns>
        public Task<SonyJsonRPCResponse> GetTouchAFPosition()
        {
            return camera.getTouchAFPosition();
        }

        /// <summary>
        /// This API provides a function to cancel Touch AF.
        /// </summary>
        /// <returns>None.</returns>
        public Task<SonyJsonRPCResponse> CancelTouchAFPosition()
        {
            return camera.cancelTouchAFPosition();
        }

        /// <summary>
        /// This API provides a function to start tracking focus.
        /// </summary>
        /// <param name="x">X-Axis position in percentage of image width</param>
        /// <param name="y">Y-Axis position in percentage of image height</param>
        /// <returns>None.</returns>
        public Task<SonyJsonRPCResponse> ActTrackingFocus( double x, double y)
        {
            return camera.actTrackingFocus();
        }

        /// <summary>
        /// This API provides a function to cancel tracking focus.
        /// </summary>
        /// <returns></returns>
        public Task<SonyJsonRPCResponse> CancelTrackingFocus()
        {
            return camera.cancelTrackingFocus();
        }

        /// <summary>
        /// This API provides a function to get current tracking focus setting.
        /// </summary>
        /// <returns>
        /// An object with attribute "tackingFocus", value is set to "On" if Tracking AF is activated.
        /// </returns>
        public Task<SonyJsonRPCResponse> GetTrackingFocus()
        {
            return camera.getTrackingFocus();
        }

        /// <summary>
        /// This API is obsolete was not implemented
        /// </summary>
        [Obsolete]
        public Task<SonyJsonRPCResponse> GetSupportedTrackingFocus()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This API provides a function to get current tracking focus setting and the available tracking focus settings at the moment. The available parameters can be changed by user operations and calling APIs.
        /// </summary>
        /// <returns>
        /// And object with 2 attributes - "trackingFocus" is set to "On" if Tracking AF is activated,
        /// "candidate" is set to available tracking focus settings ("On" and "Off")
        /// </returns>
        public Task<SonyJsonRPCResponse> GetAvailableTrackingFocus()
        {
            return camera.getAvailableTrackingFocus();
        }

        /// <summary>
        /// This API provides a function to set a value of continuous shooting mode.
        /// </summary>
        /// <returns></returns>
        public Task<SonyJsonRPCResponse> SetContShootingMode()
        {
            return camera.setContShootingMode();
        }

        /// <summary>
        /// This API provides a function to get current continuous shooting mode.
        /// </summary>
        /// <returns>
        /// An object with attribute "contShootingMode" which denotes the current mode.
        /// </returns>
        public Task<SonyJsonRPCResponse> GetContShootingMode()
        {
            return camera.getContShootingMode();
        }

        /// <summary>
        /// This API is obsolte, use GetAvailableContShootingMode instead.
        /// </summary>
        /// <returns></returns>
        [Obsolete]
        public Task<SonyJsonRPCResponse> GetSupportedContShootingMode()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This API provides a function to get current continuous shooting mode and the available continuous shooting modes at the moment. The available parameters can be changed by user operations and calling APIs.
        /// </summary>
        /// <returns>
        /// An object with 2 attributes: "contShootingMode" - current mode and "candidate" - the list of available modes
        /// </returns>
        public Task<SonyJsonRPCResponse> GetAvailableContShootingMode()
        {
            return camera.getAvailableContShootingMode();
        }

        /// <summary>
        /// This API provides a function to set a value of continuous shooting speed.
        /// </summary>
        /// <param name="contShootingSpeed">Continuous shooting speed (See Continuous shooting speed parameters of Parameter description)</param>
        /// <returns>
        /// None.
        /// </returns>
        public Task<SonyJsonRPCResponse> SetContShootingSpeed( string contShootingSpeed )
        {
            return camera.setContShootingSpeed( new { contShootingSpeed = contShootingSpeed } );
        }
        
        /// <summary>
        /// This API provides a function to get current continuous shooting speed.
        /// </summary>
        /// <returns>
        /// An Object with attribute "contShootingSpeed" which denotes the current continuous shooting speed (See Continuous shooting speed parameters of Parameter description)
        /// </returns>
        public Task<SonyJsonRPCResponse> GetContShootingSpeed()
        {
            return camera.getContShootingSpeed();
        }

        /// <summary>
        /// This API is obsolete and was not implemented, use GetAvailableContShootingSpeed instead.
        /// </summary>
        /// <returns></returns>
        [Obsolete]
        public Task<SonyJsonRPCResponse> GetSupportedContShootingSpeed()
        {
            throw new NotImplementedException();
        }
 
        /// <summary>
        /// This API provides a function to get current continuous shooting speed and the available continuous shooting speeds at the moment. The available parameters can be changed by user operations and calling APIs.
        /// </summary>
        /// <returns>
        /// An object with 2 attributes, "contShootingSpeed" - denotes the current shooting speed,
        /// and "candidate" - list of available continuous shooting speeds
        /// </returns>
        public Task<SonyJsonRPCResponse> GetAvailableContShootingSpeed()
        {
            return camera.getAvailableContShootingSpeed();
        }
 
        /// <summary>
        /// This API provides a function to set a value of self-timer.
        /// </summary>
        /// <param name="seconds">default is either 0, 2 or 10 seconds</param>
        /// <returns>
        /// When the execution of the API is successful, 0 is set. If API is not successful, "result" member is not returned, and "error" member is returned. See Status code & Error for error detail.
        /// </returns>
        public Task<SonyJsonRPCResponse> SetSelfTimer( double seconds)
        {
            return camera.setSelfTimer(seconds);
        }
 
        /// <summary>
        /// This API provides a function to get current self-timer setting.
        /// </summary>
        /// <returns>
        /// Current self-timer setting (unit: second) (See Self-timer parameters of Parameter description)
        /// </returns>
        public Task<SonyJsonRPCResponse> GetSelfTimer()
        {
            return camera.getSelfTimer();
        }
        
        /// <summary>
        /// This API is obsolete, use getAvailableSelfTimer instead.
        /// </summary>
        /// <param name="seconds"></param>
        /// <returns></returns>
        [Obsolete]
        public Task<SonyJsonRPCResponse> GetSupportedSelfTimer()
        {
            return camera.getSupportedSelfTimer();
        }

        /// <summary>
        /// This API provides a function to get current self-timer setting and the available self-timer settings at the moment. The available parameters can be changed by user operations and calling APIs.
        /// </summary>
        /// <returns>
        /// An array with 2 elements.
        /// Index 0 - current setting (unit: second),
        /// Index 1 - list of available self timer settings (unit: second)
        /// </returns>
        public Task<SonyJsonRPCResponse> GetAvailableSelfTimer()
        {
            return camera.getAvailableSelfTimer();
        }

        /// <summary>
        /// This API provides a function to set a value of exposure mode.
        /// </summary>
        /// <returns>
        /// When the execution of the API is successful, 0 is set. If API is not successful, "result" member is not returned, and "error" member is returned. See Status code & Error for error detail.
        /// </returns>
        public Task<SonyJsonRPCResponse> SetExposureMode( string mode )
        {
            return camera.setExposureMode( mode );
        }

        /// <summary>
        /// This API provides a function to get current exposure mode.
        /// </summary>
        /// <returns>
        /// Current exposure mode
        /// </returns>
        public Task<SonyJsonRPCResponse> GetExposureMode()
        {
            return camera.getExposureMode( );
        }
    
        /// <summary>
        /// This API is obsolete and was not implemented, use GetAvailableExposureMode instead
        /// </summary>
        /// <returns></returns>
        [Obsolete]
        public Task<SonyJsonRPCResponse> GetSupportedExposureMode()
        {
            throw new NotImplementedException();
        }
    
        /// <summary>
        /// This API provides a function to get current exposure mode and the available exposure modes at the moment. The available parameters can be changed by user operations and calling APIs.
        /// </summary>
        /// <returns>
        /// An array with 2 elements.
        /// Index 0 - Current exposure mode,
        /// Index 1 - A list of available exposure modes (See Exposure mode parameter of Parameter description)
        /// </returns>
        public Task<SonyJsonRPCResponse> GetAvailableExposureMode()
        {
            return camera.getAvailableExposureMode();
        }
    
        /// <summary>
        /// This API provides a function to set a value of focus mode.
        /// </summary>
        /// <returns>
        /// When the execution of the API is successful, 0 is set. If API is not successful, "result" member is not returned, and "error" member is returned. See Status code & Error for error detail.
        /// </returns>
        public Task<SonyJsonRPCResponse> SetFocusMode( string mode )
        {
            return camera.setFocusMode( mode );
        }

        /// <summary>
        /// This API provides a function to get current focus mode.
        /// </summary>
        /// <returns>Current focus mode</returns>
        public Task<SonyJsonRPCResponse> GetFocusMode()
        {
            return camera.getFocusMode( );
        }

        /// <summary>
        /// This API is obsolete and was not implemented, use getAvailableFocusMode instead.
        /// </summary>
        /// <returns></returns>
        [Obsolete]
        public Task<SonyJsonRPCResponse> GetSupportedFocusMode()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This API provides a function to get current focus mode and the available focus modes at the moment. The available parameters can be changed by user operations and calling APIs.
        /// </summary>
        /// <returns>
        /// An array with 2 elements.
        /// Index 0 - Current focus mode (See Focus mode parameter of Parameter description)
        /// Index 1 - A list of available focus modes (See Focus mode parameter of Parameter description)
        /// </returns>
        public Task<SonyJsonRPCResponse> GetAvailableFocusMode()
        {
            return camera.getAvailableFocusMode();
        }
        
        /// <summary>
        /// This API provides a function to set a value of exposure compensation.
        /// </summary>
        /// <param name="index">Index value of exposure compensation</param>
        /// <returns>
        /// When the execution of the API is successful, 0 is set. If API is not successful, "result" member is not returned, and "error" member is returned. See Status code & Error for error detail.
        /// </returns>
        public Task<SonyJsonRPCResponse> SetExposureCompensation( int index )
        {
            return camera.setExposureCompensation( index );
        }
 
        /// <summary>
        /// This API provides a function to get current exposure compensation value.
        /// </summary>
        /// <returns>
        /// Current index value of exposure compensation
        /// </returns>
        public Task<SonyJsonRPCResponse> GetExposureCompensation()
        {
            return camera.getExposureCompensation();
        }
 
        /// <summary>
        /// This API is Obsolete, use getSupportedExposureCompensation instead.
        /// </summary>
        [Obsolete]
        public Task<SonyJsonRPCResponse> GetSupportedExposureCompensation()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This API provides a function to get current exposure compensation value and the available exposure compensation values at the moment.
        /// </summary>
        /// <returns>
        /// An array with 4 elements.
        /// Index 0 - Current exposure compensation index value.
        /// Index 1 - Upper limit of available exposure compensation index value.
        /// Index 2 - Lower limit of available exposure compensation index value.
        /// Index 3 - Exposure compensation index step value (1 = 1/3 EV, 2 = 1/2 EV)
        /// </returns>
        public Task<SonyJsonRPCResponse> GetAvailableExposureCompensation()
        {
            return camera.getAvailableExposureCompensation();
        }
        
        /// <summary>
        /// This API provides a function to set a value of F number.
        /// </summary>
        /// <returns>When the execution of the API is successful, 0 is set. If API is not successful, "result" member is not returned, and "error" member is returned. See Status code & Error for error detail</returns>
        public Task<SonyJsonRPCResponse> SetFNumber( string fNumber)
        {
            return this.camera.setFNumber( fNumber );
        }

        /// <summary>
        /// This API provides a function to get current F number.
        /// </summary>
        /// <returns>
        /// Current F number
        /// </returns>
        public Task<SonyJsonRPCResponse> GetFNumber()
        {
            return this.camera.getFNumber();
        }

        /// <summary>
        /// This API is obsolete, use getAvailableFNumber instead.
        /// </summary>
        [Obsolete]
        public Task<SonyJsonRPCResponse> GetSupportedFNumber()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This API provides a function to get current F number and the available F numbers at the moment.
        /// </summary>
        /// <returns>
        /// Array of 2 elements.
        /// Index 1 - Current F number,
        /// Index 2 - List of possible F Number settings.
        /// </returns>
        public Task<SonyJsonRPCResponse> GetAvailableFNumber()
        {
            return this.camera.getAvailableFNumber();
        }

        /// <summary>
        /// This API provides a function to set a value of shutter speed.
        /// </summary>
        /// <returns>
        /// When the execution of the API is successful, 0 is set. If API is not successful, "result" member is not returned, and "error" member is returned. See Status code & Error for error detail.
        /// </returns>
        public Task<SonyJsonRPCResponse> SetShutterSpeed( string shutterSpeed)
        {
            return this.camera.setShutterSpeed( shutterSpeed );
        }

        /// <summary>
        /// This API provides a function to get current shutter speed.
        /// </summary>
        /// <returns>
        /// Current shutter speed
        /// </returns>
        public Task<SonyJsonRPCResponse> GetShutterSpeed()
        {
            return this.camera.getShutterSpeed();
        }

        /// <summary>
        ///  This API is obsolete, use getAvailableShutterSpeed instead.
        /// </summary>
        /// <returns></returns>
        [Obsolete]
        public Task<SonyJsonRPCResponse> GetSupportedShutterSpeed()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This API provides a function to get current shutter speed and the available shutter speeds at the moment.
        /// </summary>
        /// <returns>
        /// An array with 2 elements.
        /// Index 0 - Current shutter speed.
        /// Index 1 - List of available shutter speed settings.
        /// </returns>
        public Task<SonyJsonRPCResponse> GetAvailableShutterSpeed()
        {
            return this.camera.getAvailableShutterSpeed();
        }

        /// <summary>
        /// This API provides a function to set a value of ISO speed rate.
        /// </summary>
        /// <returns></returns>
        public Task<SonyJsonRPCResponse> SetIsoSpeedRate( string iso )
        {
            return this.camera.setIsoSpeedRate( iso );
        }

        /// <summary>
        /// This API provides a function to get current ISO speed rate value.
        /// </summary>
        /// <returns>
        /// Current ISO speed rate value
        /// </returns>
        public Task<SonyJsonRPCResponse> GetIsoSpeedRate()
        {
            return this.camera.getIsoSpeedRate();
        }

        /// <summary>
        ///  This API is obsolete, use getAvailableIsoSpeedRate instead.
        /// </summary>
        /// <returns></returns>
        [Obsolete]
        public Task<SonyJsonRPCResponse> GetSupportedIsoSpeedRate()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This API provides a function to get current ISO speed rate value and the available ISO speed rate values at the moment.
        /// </summary>
        /// <returns>
        /// An array with 2 elements.
        /// Index 0 - Current ISO speed rate value.
        /// Index 1 - A list of available ISO speed rate values.
        /// </returns>
        public Task<SonyJsonRPCResponse> GetAvailableIsoSpeedRate()
        {
            return this.camera.getAvailableIsoSpeedRate();
        }

        /// <summary>
        /// This API provides a function to set a value of white balance.
        /// </summary>
        /// <returns>
        /// When the execution of the API is successful, 0 is set. If API is not successful, "result" member is not returned, and "error" member is returned
        /// </returns>
        public Task<SonyJsonRPCResponse> SetWhiteBalance( string mode, bool useColorTemperature = false, int colorTemperature = 0)
        {
            return this.camera.setWhiteBalance( mode, useColorTemperature, colorTemperature);
        }

        /// <summary>
        /// This API provides a function to get current white balance.
        /// </summary>
        /// <returns>Current white balance</returns>
        public Task<SonyJsonRPCResponse> GetWhiteBalance()
        {
            return this.camera.getWhiteBalance();
        }

        /// <summary>
        ///  This API is obsolete, use getAvailableWhiteBalance instead.
        /// </summary>
        /// <returns></returns>
        [Obsolete]
        public Task<SonyJsonRPCResponse> GetSupportedWhiteBalance()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This API provides a function to get current white balance and the available white balances at the moment.
        /// </summary>
        /// <returns>
        /// Array with 2 elements.
        /// Index 0 - Current White balance settings: Object with attribute "whiteBalanceMode" and "colorTemperature"
        /// Index 1 - Supported white balance settings: Array of object with attributes "whiteBalanceMode" (name of the mode) and "colorTemperatureRange" (integer array)
        /// </returns>
        public Task<SonyJsonRPCResponse> GetAvailableWhiteBalance()
        {
            return this.camera.getAvailableWhiteBalance();
        }

        /// <summary>
        /// This API provides a function to set program shift. The client can change the aperture (F number) and shutter
        /// </summary>
        /// <param name="shiftAmount">Shift amount</param>
        /// <returns>
        /// When the execution of the API is successful, 0 is set. If API is not successful, "result" member is not returned, and "error" member is returned.
        /// </returns>
        public Task<SonyJsonRPCResponse> SetProgramShift( int shiftAmount )
        {
            return this.camera.setProgramShift(shiftAmount);
        }

        /// <summary>
        /// This API provides a function to get the supported program shift amounts.
        /// </summary>
        /// <returns>
        /// Array of Inteter, Range of supported shift amounts
        /// </returns>
        public Task<SonyJsonRPCResponse> GetSupportedProgramShift()
        {
            return this.camera.getSupportedProgramShift();
        }

        /// <summary>
        /// This API provides a function to set a value of flash mode.
        /// </summary>
        /// <param name="mode">Flash mode</param>
        /// <returns>
        /// When the execution of the API is successful, 0 is set. If API is not successful, "result" member is not returned, and "error" member is returned.
        /// </returns>
        public Task<SonyJsonRPCResponse> SetFlashMode( string mode )
        {
            return this.camera.setFlashMode(mode);
        }

        /// <summary>
        /// This API provides a function to get current flash mode.
        /// </summary>
        /// <returns>
        /// Current flash mode
        /// </returns>
        public Task<SonyJsonRPCResponse> GetFlashMode()
        {
            return this.camera.getFlashMode();
        }

        /// <summary>
        ///  This API is obsolete and was not implemented, use GetAvailableFlashMode instead.
        /// </summary>
        /// <returns></returns>
        [Obsolete]
        public Task<SonyJsonRPCResponse> GetSupportedFlashMode()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This API provides a function to get current flash mode and the available flash modes at the moment.
        /// </summary>
        /// <returns>
        /// Array with 2 Elements.
        /// Index 0 - Current flash mode.
        /// Index 1 - A list of available flash modes.
        /// </returns>
        public Task<SonyJsonRPCResponse> GetAvailableFlashMode()
        {
            return this.camera.getAvailableFlashMode();
        }

        /// <summary>
        /// This API provides a function to set a value of still size.
        /// </summary>
        /// <returns>
        /// When the execution of the API is successful, 0 is set. If API is not successful, "result" member is not returned, and "error" member is returned
        /// </returns>
        public Task<SonyJsonRPCResponse> SetStillSize( string aspect, string size )
        {
            return this.camera.setStillSize(aspect, size);
        }

        /// <summary>
        /// This API provides a function to get current still size.
        /// </summary>
        /// <returns>
        /// Object with attribute "aspect" (string) and "size" (string)
        /// </returns>
        public Task<SonyJsonRPCResponse> GetStillSize()
        {
            return this.camera.getStillSize();
        }

        /// <summary>
        ///  This API is obsolete and was not implemented, use getAvailableStillSize instead.
        /// </summary>
        /// <returns></returns>
        [Obsolete]
        public Task<SonyJsonRPCResponse> GetSupportedStillSize()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This API provides a function to get current still size and the available still sizes at the moment.
        /// </summary>
        /// <returns>
        /// Array with 2 elements.
        /// Index 0 - Current still size: object with attributes "aspect" and "size".
        /// Index 1 - A list of available still sizes: array of objects with attributes "aspect" and "size".
        /// </returns>
        public Task<SonyJsonRPCResponse> GetAvailableStillSize()
        {
            return this.camera.getAvailableStillSize();
        }

        /// <summary>
        /// This API provides a function to set a value of still quality.
        /// </summary>
        /// <returns>
        /// When the execution of the API is successful, 0 is set. If API is not successful, "result" member is not returned, and "error" member is returned
        /// </returns>
        public Task<SonyJsonRPCResponse> SetStillQuality( string quality )
        {
            return this.camera.setStillQuality( new { stillQuality = quality });
        }

        /// <summary>
        /// This API provides a function to get current still quality.
        /// </summary>
        /// <returns>
        /// Current still quality (string)
        /// </returns>
        public Task<SonyJsonRPCResponse> GetStillQuality()
        {
            return this.camera.getStillQuality();
        }

        /// <summary>
        ///  This API is obsolete and was not implemented, use getAvailableStillQuality instead.
        /// </summary>
        /// <returns></returns>
        [Obsolete]
        public Task<SonyJsonRPCResponse> GetSupportedStillQuality()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This API provides a function to get current still quality and the available still quality at the moment.
        /// </summary>
        /// <returns>
        /// Object with 2 attributes.
        /// "stillQuality" - Current still quality,
        /// "candidate" - A list of available still quality
        /// </returns>
        public Task<SonyJsonRPCResponse> GetAvailableStillQuality()
        {
            return this.camera.getAvailableStillQuality();
        }

        /// <summary>
        /// This API provides a function to set a value of postview image size. 
        /// The postview image can be used for storing it as the taken picture, and showing it to the client display.
        /// </summary>
        /// <returns>
        /// When the execution of the API is successful, 0 is set. If API is not successful, "result" member is not returned, and "error" member is returned
        /// </returns>
        public Task<SonyJsonRPCResponse> SetPostviewImageSize( string size)
        {
            return this.camera.setPostviewImageSize( size );
        }

        /// <summary>
        /// This API provides a function to get current postview image size.
        /// </summary>
        /// <returns> 
        /// Current postview image size (string)
        /// </returns>
        public Task<SonyJsonRPCResponse> GetPostviewImageSize()
        {
            return this.camera.getPostviewImageSize();
        }

        /// <summary>
        ///  This API is obsolete and was not implemented, use getAvailablePostviewImageSize instead.
        /// </summary>
        /// <returns></returns>
        [Obsolete]
        public Task<SonyJsonRPCResponse> GetSupportedPostviewImageSize()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This API provides a function to get current postview image size and the available postview image sizes at the moment.
        /// </summary>
        /// <returns>
        /// Array with 2 elements.
        /// Index 0 - Current postview image size
        /// Index 1 - A list of available postview image sizes
        /// </returns>
        public Task<SonyJsonRPCResponse> GetAvailablePostviewImageSize()
        {
            return this.camera.getAvailablePostviewImageSize();
        }

        /// <summary>
        /// This API provides a function to set a value of movie file format.
        /// </summary>
        /// <returns>
        /// When the execution of the API is successful, 0 is set. If API is not successful, "result" member is not returned, and "error" member is returned
        /// </returns>
        public Task<SonyJsonRPCResponse> SetMovieFileFormat( string format )
        {
            return this.camera.setMovieFileFormat( new { movieFileFormat = format } );
        }

        /// <summary>
        /// This API provides a function to get current movie file format.
        /// </summary>
        /// <returns>
        /// Current movie file format (string)
        /// </returns>
        public Task<SonyJsonRPCResponse> GetMovieFileFormat()
        {
            return this.camera.getMovieFileFormat();
        }

        /// <summary>
        ///  This API is obsolete and was not implemented, use getAvailableMovieFileFormat instead.
        /// </summary>
        /// <returns></returns>
        [Obsolete]
        public Task<SonyJsonRPCResponse> GetSupportedMovieFileFormat()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This API provides a function to get current movie file format and the available movie file formats at the moment.
        /// </summary>
        /// <returns>
        /// Object with 2 elements.
        /// "movieFileFormat" - Current movie file format.
        /// "candidate" - A list of available movie file formats.
        /// </returns>
        public Task<SonyJsonRPCResponse> GetAvailableMovieFileFormat()
        {
            return this.camera.getAvailableMovieFileFormat();
        }

        /// <summary>
        /// This API provides a function to set a value of movie quality.
        /// </summary>
        /// <returns>
        /// When the execution of the API is successful, 0 is set. If API is not successful, "result" member is not returned, and "error" member is returned
        /// </returns>
        public Task<SonyJsonRPCResponse> SetMovieQuality( string quality )
        {
            return this.camera.setMovieQuality( quality );
        }

        /// <summary>
        /// This API provides a function to get current movie quality.
        /// </summary>
        /// <returns>Current Quality (string)</returns>
        public Task<SonyJsonRPCResponse> GetMovieQuality()
        {
            return this.camera.getMovieQuality();
        }

        /// <summary>
        ///  This API is obsolete and was not implemented, use getAvailableMovieQuality instead.
        /// </summary>
        /// <returns></returns>
        [Obsolete]
        public Task<SonyJsonRPCResponse> GetSupportedMovieQuality()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This API provides a function to get current movie quality and the available movie qualities at the moment.
        /// </summary>
        /// <returns></returns>
        public Task<SonyJsonRPCResponse> GetAvailableMovieQuality()
        {
            return this.camera.getAvailableMovieQuality();
        }

        /// <summary>
        /// This API provides a function to set a value of steady mode.
        /// </summary>
        /// <returns>
        /// When the execution of the API is successful, 0 is set. If API is not successful, "result" member is not returned, and "error" member is returned
        /// </returns>
        public Task<SonyJsonRPCResponse> SetSteadyMode(string mode)
        {
            return this.camera.setSteadyMode(mode);
        }

        /// <summary>
        /// This API provides a function to get current steady mode.
        /// </summary>
        /// <returns>
        /// Current steady mode (string)
        /// </returns>
        public Task<SonyJsonRPCResponse> GetSteadyMode()
        {
            return this.camera.getSteadyMode();
        }

        /// <summary>
        ///  This API is obsolete and was not implemented, use getAvailableSteadyMode instead.
        /// </summary>
        /// <returns></returns>
        [Obsolete]
        public Task<SonyJsonRPCResponse> GetSupportedSteadyMode()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This API provides a function to get current steady mode and the available steady modes at the moment.
        /// </summary>
        /// <returns>
        /// Array with 2 elements.
        /// Index 0 - Current steady mode.
        /// Index 1 - A list of available steady modes.
        /// </returns>
        public Task<SonyJsonRPCResponse> GetAvailableSteadyMode()
        {
            return this.camera.getAvailableSteadyMode();
        }

        /// <summary>
        /// This API provides a function to set a value of view angle.
        /// </summary>
        /// <returns></returns>
        public Task<SonyJsonRPCResponse> SetViewAngle( string angle)
        {
            return this.camera.setViewAngle( angle );
        }

        /// <summary>
        /// This API provides a function to get current view angle.
        /// </summary>
        /// <returns>
        /// Current view angle (integer)
        /// </returns>
        public Task<SonyJsonRPCResponse> GetViewAngle()
        {
            return this.camera.getViewAngle();
        }

        /// <summary>
        ///  This API is obsolete and was not implemented, use getAvailableViewAngle instead.
        /// </summary>
        /// <returns></returns>
        [Obsolete]
        public Task<SonyJsonRPCResponse> GetSupportedViewAngle()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This API provides a function to get current view angle and the available view angles at the moment.
        /// </summary>
        /// <returns>
        /// Array of 2 elements.
        /// Index 0 - Current view angle (integer).
        /// Index 1 - A list of available view angles (integer).
        /// </returns>
        public Task<SonyJsonRPCResponse> GetAvailableViewAngle()
        {
            return this.camera.getAvailableViewAngle();
        }

        /// <summary>
        /// This API provides a function to set a value of scene selection.
        /// </summary>
        /// <returns></returns>
        public Task<SonyJsonRPCResponse> SetSceneSelection( string scene )
        {
            return this.camera.setSceneSelection( new { scene = scene } );
        }

        /// <summary>
        /// This API provides a function to get current scene selection.
        /// </summary>
        /// <returns>
        /// Current scene selection (string)
        /// </returns>
        public Task<SonyJsonRPCResponse> GetSceneSelection()
        {
            return this.camera.getSceneSelection();
        }

        /// <summary>
        ///  This API is obsolete and was not implemented, use getAvailableSceneSelection instead.
        /// </summary>
        /// <returns></returns>
        [Obsolete]
        public Task<SonyJsonRPCResponse> GetSupportedSceneSelection()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This API provides a function to get current scene selection and the available scene selections at the moment.
        /// </summary>
        /// <returns>
        /// Object with 2 attributes.
        /// "scene" - Current scene selection (string).
        /// "candidate" - A list of available scene selections ( string[] )
        /// </returns>
        public Task<SonyJsonRPCResponse> GetAvailableSceneSelection()
        {
            return this.camera.getAvailableSceneSelection();
        }

        /// <summary>
        /// This API provides a function to set a value of color setting.
        /// </summary>
        /// <returns></returns>
        public Task<SonyJsonRPCResponse> SetColorSetting( string colorSetting )
        {
            return this.camera.setColorSetting( new { colorSetting = colorSetting });
        }

        /// <summary>
        /// This API provides a function to get current color setting.
        /// </summary>
        /// <returns>
        /// Current color setting (string)
        /// </returns>
        public Task<SonyJsonRPCResponse> GetColorSetting()
        {
            return this.camera.getColorSetting();
        }

        /// <summary>
        ///  This API is obsolete and was not implemented, use getAvailableColorSetting instead.
        /// </summary>
        /// <returns></returns>
        [Obsolete]
        public Task<SonyJsonRPCResponse> GetSupportedColorSetting()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This API provides a function to get current color setting and the available color settings at the moment.
        /// </summary>
        /// <returns>
        /// Object with 2 attributes.
        /// "colorSetting" - Current color setting.
        /// "candidate" - A list of available color settings.
        /// </returns>
        public Task<SonyJsonRPCResponse> GetAvailableColorSetting()
        {
            return this.camera.getAvailableColorSetting();
        }

        /// <summary>
        /// This API provides a function to set a value of interval time.
        /// </summary>
        /// <returns>None.</returns>
        public Task<SonyJsonRPCResponse> SetIntervalTime( string intervalTimeSec)
        {
            return this.camera.setIntervalTime( new { intervalTimeSec = intervalTimeSec });
        }

        /// <summary>
        /// This API provides a function to get current interval time.
        /// </summary>
        /// <returns>
        /// Current interval time (unit: second) ( string )
        /// </returns>
        public Task<SonyJsonRPCResponse> GetIntervalTime()
        {
            return this.camera.getIntervalTime();
        }
        /// <summary>
        ///  This API is obsolete and was not implemented, use getAvailableColorSetting instead.
        /// </summary>
        /// <returns></returns>
        [Obsolete]
        public Task<SonyJsonRPCResponse> GetSupportedIntervalTime()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This API provides a function to get current interval time and the available interval times at the moment.
        /// </summary>
        /// <returns>
        /// Object with 2 attributes.
        /// "intervalTimeSec" - Current interval time (unit: second) (string).
        /// "candidate" - A list of available interval times (unit: second) (string).
        /// </returns>
        public Task<SonyJsonRPCResponse> GetAvailableIntervalTime()
        {
            return this.camera.getAvailableIntervalTime();
        }

        /// <summary>
        /// This API provides a function to set a value of flip setting.
        /// </summary>
        /// <returns>None.</returns>
        public Task<SonyJsonRPCResponse> SetFlipSetting( string flip )
        {
            return this.camera.setFlipSetting( new { flip = flip } );
        }

        /// <summary>
        /// This API provides a function to get current flip setting.
        /// </summary>
        /// <returns>Current flip setting (string)</returns>
        public Task<SonyJsonRPCResponse> GetFlipSetting()
        {
            return this.camera.getFlipSetting();
        }

        /// <summary>
        ///  This API is obsolete and was not implemented, use getAvailableFlipSetting instead.
        /// </summary>
        /// <returns></returns>
        [Obsolete]
        public Task<SonyJsonRPCResponse> GetSupportedFlipSetting()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This API provides a function to get current flip setting and the available flip settings at the moment. The
        /// </summary>
        /// <returns>
        /// Object with 2 attributes.
        /// "flip" - Current flip setting.
        /// "candidate" - A list of available flip settings.
        /// </returns>
        public Task<SonyJsonRPCResponse> GetAvailableFlipSetting()
        {
            return this.camera.getAvailableFlipSetting();
        }

        /// <summary>
        /// This API provides a function to set a value of TV color system.
        /// </summary>
        /// <returns>None.</returns>
        public Task<SonyJsonRPCResponse> SetTvColorSystem( string tvColorSystem )
        {
            return this.camera.setTvColorSystem( new { tvColorSystem = tvColorSystem });
        }

        /// <summary>
        /// This API provides a function to get current TV color system.
        /// </summary>
        /// <returns>
        /// Current TV color system (string)
        /// </returns>
        public Task<SonyJsonRPCResponse> GetTvColorSystem()
        {
            return this.camera.getTvColorSystem();
        }

        /// <summary>
        ///  This API is obsolete and was not implemented, use getAvailableFlipSetting instead.
        /// </summary>
        /// <returns></returns>
        [Obsolete]
        public Task<SonyJsonRPCResponse> GetSupportedTvColorSystem()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This API provides a function to get current TV color system and the available TV color systems at the moment
        /// </summary>
        /// <returns></returns>
        public Task<SonyJsonRPCResponse> GetAvailableTvColorSystem()
        {
            return this.camera.getAvailableTvColorSystem();
        }

        /// <summary>
        /// This API provides a function to set up camera for shooting function. Some camera models need this API call before starting liveview, capturing still image, recording movie, or accessing all other camera
        /// shooting functions.
        /// </summary>
        /// <returns>
        /// When the execution of the API is successful, 0 is set. If API is not successful, "result" member is not returned, and "error" member is returned.
        /// </returns>
        public Task<SonyJsonRPCResponse> StartRecMode()
        {
            return this.camera.startRecMode();
        }

        /// <summary>
        /// This API provides a function to stop shooting functions.
        /// </summary>
        /// <returns></returns>
        public Task<SonyJsonRPCResponse> StopRecMode()
        {
            return this.camera.stopRecMode();
        }

        /// <summary>
        /// This API provides a function to set a value of camera function.
        /// </summary>
        /// <returns>
        /// When the execution of the API is successful, 0 is set. If API is not successful, "result" member is not returned, and "error" member is returned.
        /// </returns>
        public Task<SonyJsonRPCResponse> SetCameraFunction( string function )
        {
            return this.camera.setCameraFunction( function );
        }

        /// <summary>
        /// This API provides a function to get current camera function.
        /// </summary>
        /// <returns>
        /// Current camera function (string)
        /// </returns>
        public Task<SonyJsonRPCResponse> GetCameraFunction()
        {
            return this.camera.getCameraFunction();
        }
        /// <summary>
        ///  This API is obsolete and was not implemented, use getAvailableCameraFunction instead.
        /// </summary>
        /// <returns></returns>
        [Obsolete]
        public Task<SonyJsonRPCResponse> GetSupportedCameraFunction()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This API provides a function to get current camera function and the available camera functions at the moment.
        /// </summary>
        /// <returns>
        /// Array with 2 elements.
        /// Index 0 - Current camera function (string),
        /// Index 1 - A list of available camera functions (string[])
        /// </returns>
        public Task<SonyJsonRPCResponse> GetAvailableCameraFunction()
        {
            return this.camera.getAvailableCameraFunction();
        }

        /// <summary>
        /// This API provides the list of schemes that device can handle. In Camera Remote API, standard URI structure, as defined by RFC 3986, is used for representing device's resources. Schemes are used to refer to device resources. URI is provided from the server.
        /// </summary>
        /// <returns>
        /// Array of objects. Each object contains attribute "scheme" which is the name of scheme.
        /// </returns>
        public Task<SonyJsonRPCResponse> GetSchemeList()
        {
            return this.avContent.getSchemeList();
        }

        /// <summary>
        /// This API provides the list of sources under the scheme. The source is included in URI to access stored contents in the camera. The camera supports specific source.
        /// </summary>
        /// <returns></returns>
        public Task<SonyJsonRPCResponse> GetSourceList()
        {
            return this.avContent.getSourceListW();
        }

        /// <summary>
        /// This API provides a function to get content count under specific URI.
        /// </summary>
        /// <param name="uri">URI to identify the content.</param>
        /// <param name="type">Optional parameter to narrow down result within specified URI in the request. 
        /// Following values are defined. Only for "date" view. Not available if "target" parameter is "all".
        /// "still" - Still image.
        /// "movie_mp4" - MP4 movie.
        /// "movie_xavcs" - XAVC S movie.
        /// </param>
        /// <param name="target">Optional parameter to widen result within specified URI in the request. 
        /// Following values are defined:
        /// "all" - Return the number of all contents.</param>
        /// <param name="view">View type
        /// "date" - Date view
        /// "flat" - Flat view
        /// </param>
        /// <returns>
        /// Object with attribute "count" (int)
        /// </returns>
        public Task<SonyJsonRPCResponse> GetContentCount( string uri, string type = null, string target = null, string view = "flat"  )
        {
            return this.avContent.getContentCountWithVersion( "1.2", new {
                uri = uri,
                type = type,
                target = target,
                view = view
            });
        }

        /// <summary>
        /// This API provides a function to get content list under specific URI.
        /// </summary>
        /// <param name="uri">URI to identify the content.</param>
        /// <param name="stIdx">Start index to get list items.</param>
        /// <param name="cnt">Count of the maximum number of items that can be listed, starting from "stIdx". Maximum number is 100.</param>
        /// <param name="type">Optional parameter to narrow down result within specified URI in the request. Following values are defined. Only for "date" view.
        /// "still" - Still image.
        /// "movie_mp4" - MP4 movie.
        /// "movie_xavcs" - XAVC S movie.
        /// null - Not specified.</param>
        /// <param name="view">View type
        /// "date" - Date view
        /// "flat" - Flat view</param>
        /// <param name="sort">
        /// Sort type
        /// "ascending" - Ascending
        /// "descending" - Descending
        /// "" - Not specified</param>
        /// <returns>
        /// Refer to Documentation for detailed format information
        /// </returns>
        public Task<SonyJsonRPCResponse> GetContentList( string uri, int stIdx = 0, int cnt = 100, string type = null, string view = "flat", string sort = "")
        {
            return this.avContent.getContentListWithVersion( "1.3", new {
                uri = uri,
                stIdx = stIdx,
                cnt = cnt,
                type = type,
                view = view,
            });
        }

        /// <summary>
        /// This API provides a function to set streaming content for remote playback.
        /// </summary>
        /// <param name="uri">URI of content.</param>
        /// <param name="remotePlayType">Remote playback type.
        /// "simpleStreaming" - Simple streaming
        /// "" - unknown</param>
        /// <returns>
        /// Object with attribute "playbackUrl" - URL for streaming
        /// </returns>
        public Task<SonyJsonRPCResponse> SetStreamingContent( string uri, string remotePlayType)
        {
            return this.avContent.setStreamingContent( new {
                uri = uri,
                remotePlayType = remotePlayType
            });
        }

        /// <summary>
        /// This API provides a function to start streaming.
        /// </summary>
        /// <returns>None.</returns>
        public Task<SonyJsonRPCResponse> StartStreaming()
        {
            return this.avContent.startStreaming();
        }

        /// <summary>
        /// This API provides a function to pause streaming.
        /// </summary>
        /// <returns>None.</returns>
        public Task<SonyJsonRPCResponse> PauseStreaming()
        {
            return this.avContent.pauseStreaming();
        }

        /// <summary>
        /// This API provides a function to seek streaming position while streaming content.
        /// </summary>
        /// <returns></returns>
        public Task<SonyJsonRPCResponse> SeekStreamingPosition( int positionMsec )
        {
            return this.avContent.seekStreamingPosition( new { positionMsec = positionMsec });
        }

        /// <summary>
        /// This API provides a function to stop streaming.
        /// </summary>
        /// <returns></returns>
        public Task<SonyJsonRPCResponse> StopStreaming()
        {
            return this.avContent.stopStreaming();
        }

        /// <summary>
        /// This API provides a function to get streaming status from the server.
        /// </summary>
        /// <returns>
        /// Object with 2 attributes.
        /// "status" - Streaming status.
        /// "factor" - Factor of streaming status.
        /// </returns>
        public Task<SonyJsonRPCResponse> RequestToNotifyStreamingStatus( bool polling)
        {
            return this.avContent.requestToNotifyStreamingStatus( new { polling = polling });
        }

        /// <summary>
        /// This API provides a function to delete contents.
        /// </summary>
        /// <param name="uris">List of URI to delete. Maximum number is 100.</param>
        /// <returns></returns>
        public Task<SonyJsonRPCResponse> DeleteContent( string[] uris)
        {
            return this.avContent.deleteContentWithVersion("1.1", new {
                uri = uris
            });
        }

        /// <summary>
        /// This API provides a function to set a value of IR remote control setting.
        /// </summary>
        /// <param name="infraredRemoteControl">IR remote control setting.</param>
        /// <returns></returns>
        public Task<SonyJsonRPCResponse> SetInfraredRemoteControl( string infraredRemoteControl)
        {
            return this.camera.setInfraredRemoteControl( new {
                infraredRemoteControl = infraredRemoteControl
            });
        }

        /// <summary>
        /// This API provides a function to get current IR remote control setting.
        /// </summary>
        /// <returns>
        /// Object with attribute "infraredRemoteControl" - Current IR remote control setting
        /// </returns>
        public Task<SonyJsonRPCResponse> GetInfraredRemoteControl()
        {
            return this.camera.getInfraredRemoteControl();
        }

        /// <summary>
        ///  This API is obsolete and was not implemented, use getAvailableInfraredRemoteControl instead.
        /// </summary>
        /// <returns></returns>
        [Obsolete]
        public Task<SonyJsonRPCResponse> GetSupportedInfraredRemoteControl()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This API provides a function to get current IR remote control setting and the available IR remote control settings at the moment.
        /// </summary>
        /// <returns>
        /// Object with 2 attributes.
        /// "infraredRemoteControl" - Current IR remote control setting.
        /// "candidate" - A list of available IR remote control settings.
        /// </returns>
        public Task<SonyJsonRPCResponse> GetAvailableInfraredRemoteControl()
        {
            return this.camera.getAvailableInfraredRemoteControl();
        }

        /// <summary>
        /// This API provides a function to set a value of auto power off time.
        /// </summary>
        /// <param name="autoPowerOff">Auto power off time (unit: second)</param>
        /// <returns></returns>
        public Task<SonyJsonRPCResponse> SetAutoPowerOff( string autoPowerOff )
        {
            return this.camera.setAutoPowerOff( new { autoPowerOff = autoPowerOff });
        }

        /// <summary>
        /// This API provides a function to get current auto power off time.
        /// </summary>
        /// <returns>
        /// Object with attribute "autoPowerOff" - Current auto power off time (unit: second)
        /// </returns>
        public Task<SonyJsonRPCResponse> GetAutoPowerOff()
        {
            return this.camera.getAutoPowerOff();
        }

        /// <summary>
        ///  This API is obsolete and was not implemented, use getAvailableFlipSetting instead.
        /// </summary>
        /// <returns></returns>
        [Obsolete]
        public Task<SonyJsonRPCResponse> GetSupportedAutoPowerOff()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This API provides a function to get current auto power off time and the available auto power off times at the moment
        /// </summary>
        /// <returns>
        /// Object with 2 attributes.
        /// "autoPowerOff" - Current auto power off time (unit: second).
        /// "candidate" - A list of available auto power off times.
        /// </returns>
        public Task<SonyJsonRPCResponse> GetAvailableAutoPowerOff()
        {
            return this.camera.getAvailableAutoPowerOff();
        }

        /// <summary>
        /// This API provides a function to set a value of beep mode.
        /// </summary>
        /// <returns>When the execution of the API is successful, 0 is set. If API is not successful, "result" member is not returned, and "error" member is returned.</returns>
        public Task<SonyJsonRPCResponse> SetBeepMode( string mode)
        {
            return this.camera.setBeepMode( mode );
        }

        /// <summary>
        /// This API provides a function to get current beep mode.
        /// </summary>
        /// <returns>Current beep mode (string)</returns>
        public Task<SonyJsonRPCResponse> GetBeepMode()
        {
            return this.camera.getBeepMode();
        }

        /// <summary>
        ///  This API is obsolete and was not implemented, use getAvailableBeepMode instead.
        /// </summary>
        /// <returns></returns>
        [Obsolete]
        public Task<SonyJsonRPCResponse> GetSupportedBeepMode()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This API provides a function to get current beep mode and the available beep modes at the moment.
        /// </summary>
        /// <returns>
        /// Array with 2 elements.
        /// Index 0 - Current beep mode (string)
        /// Index 1 - A list of available beep modes (string[])
        /// </returns>
        public Task<SonyJsonRPCResponse> GetAvailableBeepMode()
        {
            return this.camera.getAvailableBeepMode();
        }

        /// <summary>
        /// This API provides a function to set current time with timezone information.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <param name="timeZoneOffsetMinute">The time zone offset minute.</param>
        /// <param name="dstOffsetMinute">The DST offset minute.</param>
        /// <returns></returns>
        public Task<SonyJsonRPCResponse> SetCurrentTime( DateTime dateTime, int timeZoneOffsetMinute, int dstOffsetMinute)
        {
            return this.system.setCurrentTime( new {
                dateTime = dateTime,
                timeZoneOffsetMinute = timeZoneOffsetMinute,
                dstOffsetMinute = dstOffsetMinute
            });
        }

        /// <summary>
        /// This API provides a function to get storage information.
        /// </summary>
        /// <returns>
        /// Array of storage information objects.
        /// See documentation for details.</returns>
        public Task<SonyJsonRPCResponse> GetStorageInformation()
        {
            return this.camera.getStorageInformation();
        }

        /// <summary>
        /// This API provides a function to get event from the server.
        /// </summary>
        /// <param name="longPolling">if set to <c>true</c> Callback when timeout or change point detection.</param>
        /// <returns>
        /// Array of up to 59 elements. See documentation for details.
        /// </returns>
        public Task<SonyJsonRPCResponse> GetEvent( bool longPolling, string version = "1.0" )
        {
            return this.camera.getEventWithVersion( version, longPolling );
        }

        /// <summary>
        /// This API provides a function to get the available API names that the server supports at the moment.
        /// </summary>
        /// <returns>
        /// Array of api names (string[])
        /// </returns>
        public Task<SonyJsonRPCResponse> GetAvailableApiList()
        {
            return this.camera.getAvailableApiList();
        }

        /// <summary>
        /// This API provides a function to get name and "Camera Remote API" version of the server.
        /// </summary>
        /// <returns>
        /// Array with 2 elements.
        /// Index 0 - Application name of the server.
        /// Index 1 - "Camera Remote API" version of the server.
        /// </returns>
        public Task<SonyJsonRPCResponse> GetApplicationInfo()
        {
            return this.camera.getApplicationInfo();
        }

        /// <summary>
        /// This API provides supported versions on the "API service". The client can get the list of API names for specific version using "getMethodTypes" API. The client can get list of versions, which the server supports, using this API.
        /// </summary>
        /// <returns>
        /// Array of Supported versions on the API service.
        /// </returns>
        public Task<SonyJsonRPCResponse> GetCameraApiVersions()
        {
            return this.camera.getVersions();
        }

        /// <summary>
        /// This API provides supported versions on the "API service". The client can get the list of API names for specific version using "getMethodTypes" API. The client can get list of versions, which the server supports, using this API.
        /// </summary>
        /// <returns>
        /// Array of Supported versions on the API service.
        /// </returns>
        public Task<SonyJsonRPCResponse> GetAvContentApiVersions()
        {
            return this.avContent.getVersions();
        }
        
        /// <summary>
        /// This API provides supported versions on the "API service". The client can get the list of API names for specific version using "getMethodTypes" API. The client can get list of versions, which the server supports, using this API.
        /// </summary>
        /// <returns>
        /// Array of Supported versions on the API service.
        /// </returns>
        public Task<SonyJsonRPCResponse> GetSystemApiVersions()
        {
            return this.system.getVersions();
        }

        /// <summary>
        /// This API provides a function to get the supported APIs for the version. The client can get the list of API names for specific version using this API. The client can get list of versions, which the server supports, using "getVersions" API.
        /// </summary>
        /// <returns></returns>
        public Task<SonyJsonRPCResponse> GetCameraMethodTypes()
        {
            return this.camera.getMethodTypes();
        }
        
        /// <summary>
        /// This API provides a function to get the supported APIs for the version. The client can get the list of API names for specific version using this API. The client can get list of versions, which the server supports, using "getVersions" API.
        /// </summary>
        /// <returns></returns>
        public Task<SonyJsonRPCResponse> GetAvContentMethodTypes()
        {
            return this.avContent.getMethodTypes();
        }
        
        /// <summary>
        /// This API provides a function to get the supported APIs for the version. The client can get the list of API names for specific version using this API. The client can get list of versions, which the server supports, using "getVersions" API.
        /// </summary>
        /// <returns></returns>
        public Task<SonyJsonRPCResponse> GetSystemMethodTypes()
        {
            return this.system.getMethodTypes();
        }


    }

}
