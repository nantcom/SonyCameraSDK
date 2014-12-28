using NantCom.SonyCameraSDK;
using NantCom.SonyCameraSDK.JsonRPC;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NantCom.SonyCameraSDK
{
    /// <summary>
    /// Represents the connected camera
    /// </summary>
    public class Camera : INotifyPropertyChanged
    {
        #region Status Properties

        /// <summary>
        /// Current function of camera
        /// </summary>
        public string CurrentFunction { get; private set; }

        /// <summary>
        /// Current function of camera
        /// </summary>
        public string ShootMode { get; private set; }
        
        /// <summary>
        /// Gets or sets the current camera status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        public string Status { get; private set; }

        /// <summary>
        /// Gets the zoom percentage.
        /// </summary>
        /// <value>
        /// The zoom percentage.
        /// </value>
        public int ZoomPercentage { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is live view ready.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is live view ready; otherwise, <c>false</c>.
        /// </value>
        public bool? IsLiveViewReady { get; private set; }

        /// <summary>
        /// Gets or sets the live view orientation.
        /// </summary>
        /// <value>
        /// The live view orientation.
        /// </value>
        public string LiveViewOrientation { get; private set; }

        /// <summary>
        /// Gets the recordable time (in minutes)
        /// </summary>
        /// <value>
        /// The recordable time.
        /// </value>
        public int? RecordableTime { get; private set; }

        /// <summary>
        /// Gets the number of recordable photos.
        /// </summary>
        /// <value>
        /// The recordable photos.
        /// </value>
        public int? RecordablePhotos { get; private set; }

        /// <summary>
        /// Gets the storage description.
        /// </summary>
        /// <value>
        /// The storage description.
        /// </value>
        public string StorageDescription { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance is touch af set.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is touch af set; otherwise, <c>false</c>.
        /// </value>
        public bool? IsTouchAFSet { get; private set; }

        public bool? IsProgramShifted { get; private set; }

        #endregion

        #region Setting Properties

        public CameraSetting PhotoResolution { get { return _Settings["PhotoResolution"]; } }
        public CameraSetting OISMode { get { return _Settings["OISMode"]; } }
        public CameraSetting Mode { get { return _Settings["Mode"]; } }
        public CameraSetting PostViewImageSize { get { return _Settings["PostViewImageSize"]; } }
        public CameraSetting MovieQuality { get { return _Settings["MovieQuality"]; } }
        public CameraSetting SelfTimer { get { return _Settings["SelfTimer"]; } }
        public CameraSetting ExposureCompensation { get { return _Settings["ExposureCompensation"]; } }
        public CameraSetting FlashMode { get { return _Settings["FlashMode"]; } }
        public CameraSetting FNumber { get { return _Settings["FNumber"]; } }
        public CameraSetting FocusMode { get { return _Settings["FocusMode"]; } }
        public CameraSetting ISO { get { return _Settings["ISO"]; } }
        public CameraSetting ShutterSpeed { get { return _Settings["ShutterSpeed"]; } }
        public CameraSetting ViewAngle { get { return _Settings["ViewAngle"]; } }
        public CameraSetting WhiteBalance { get { return _Settings["WhiteBalance"]; } }
        public CameraSetting ColorTemperature { get { return _Settings["ColorTemperature"]; } }

        #endregion

        /// <summary>
        /// Gets the available camera settings
        /// </summary>
        public IEnumerable<CameraSetting> AvailableSettings
        {
            get
            {
                return from s in _Settings.Values
                       where s.AvailableOptions != null && s.AvailableOptions.Length > 0
                       select s;
            }
        }

        /// <summary>
        /// Gets all Settings (use for sample data)
        /// </summary>
        public Dictionary<string, CameraSetting> AllSettings
        {
            get
            {
                return _Settings;
            }
#if DEBUG
            set
            {
                _Settings = value;
            }
#endif
        }

        /// <summary>
        /// Gets the API client.
        /// </summary>
        /// <value>
        /// The API client.
        /// </value>
        public CameraApiClient ApiClient
        {
            get
            {
                return _Client;
            }
        }

        /// <summary>
        /// Occurs when property changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        /// <summary>
        /// Occurs when live view image received.
        /// </summary>
        public event Action<ImageReceivedEventArgs> LiveViewImageReceived = delegate { };

        /// <summary>
        /// Fires when there is a long running process which require UI to be blocked
        /// </summary>
        public event Action<bool> LongRunningProcessOccured = delegate { };

        private bool _DisableUpdate = true;

        /// <summary>
        /// Disable Update
        /// </summary>
        internal bool DisableUpdate
        {
            get { return _DisableUpdate; }
        }
        internal SynchronizationContext Context
        {
            get { return _Context; }
        }

        private CameraApiClient _Client;
        private Dictionary<string, CameraSetting> _Settings;
        private SynchronizationContext _Context;


        /// <summary>
        /// Initializes a new instance of the <see cref="Camera"/> class.
        /// </summary>
        /// <param name="client">The camera api client.</param>
        public Camera(CameraApiClient client) : this()
        {
            _Client = client;
        }
        
        /// <summary>
        /// Create new instance of Camera
        /// </summary>
        public Camera()
        {
            _Context = SynchronizationContext.Current;
            this.InitializeSettings();
        }

        private void OnPropertyChanged( string name )
        {
            _Context.Post((o) =>
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(name));

            }, null);
        }

        private void OnPropertyChanged(IEnumerable<string> names)
        {
            _Context.Post((o) =>
            {
                foreach (var name in names)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs(name));
                }

            }, null);
        }

        /// <summary>
        /// Updates that status from given RPC Response;
        /// </summary>
        /// <param name="response">The response.</param>
        private IEnumerable<string> UpdateFromEventResponse(SonyJsonRPCResponse response)
        {
            if (response == null || response.IsSuccess == false)
            {
                yield break;
            }

            _DisableUpdate = true;

            if (response.Result[1] != null)
            {
                this.Status = response.Result[1].cameraStatus;
                yield return "Status";
            }

            if (response.Result[2] != null)
            {
                this.ZoomPercentage = (int)response.Result[2].zoomPosition;
                yield return "ZoomPercentage";
            }

            if (response.Result[3] != null)
            {
                this.IsLiveViewReady = (bool)response.Result[3].liveviewStatus;
                yield return "IsLiveViewReady";
            }

            if (response.Result[4] != null)
            {
                this.LiveViewOrientation = (string)response.Result[4].liveviewOrientation;
                yield return "LiveViewOrientation";
            }

            if (response.Result[10] != null)
            {
                foreach (var item in response.Result[10])
                {
                    if (item.recordTarget == true)
                    {
                        this.RecordableTime = (int)item.recordableTime;
                        this.RecordablePhotos = (int)item.numberOfRecordableImages;

                        if (this.RecordablePhotos == -1)
                        {
                            this.RecordablePhotos = null;
                        }

                        if (this.RecordableTime == -1)
                        {
                            this.RecordableTime = null;
                        }

                        yield return "RecordableTime";
                        yield return "RecordablePhotos";
                    }
                }
            }

            if (response.Result[12] != null)
            {
                this.CurrentFunction = (string)response.Result[12].currentCameraFunction;
                yield return "CurrentFunction";
            }

            if (response.Result[13] != null)
            {
                this.MovieQuality.Value = (string)response.Result[13].currentMovieQuality;
                this.MovieQuality.AvailableOptions = Utility.ToArray(response.Result[13].movieQualityCandidates);

                yield return "MovieQuality";                
            }

            if (response.Result[14] != null)
            {
                this.PhotoResolution.Value = (string)response.Result[14].currentAspect + " - " + (string)response.Result[14].currentSize;
                yield return "PhotoResolution";

                Task.Run(async () =>
                {
                    var stillSizeResponse = await _Client.GetAvailableStillSize();
                    var array = (JArray)stillSizeResponse.Result;

                    this.PhotoResolution.AvailableOptions = (from dynamic item in (JArray)array[1]
                                                                     select (string)item.aspect + " - " + (string)item.size).ToArray();
                    this.PhotoResolution.OnNewValue = (s) =>
                    {
                        var parts = s.Split('-');
                        _Client.SetStillSize(parts[0].Trim(), parts[1].Trim());
                    };

                    this.OnPropertyChanged("PhotoResolution");
                    this.OnPropertyChanged("AvailableSettings");
                });

            }

            if (response.Result[16] != null)
            {
                this.OISMode.Value = (string)response.Result[16].currentSteadyMode;
                this.OISMode.AvailableOptions = Utility.ToArray(response.Result[16].steadyModeCandidates);
                
                yield return "OISMode";
            }


            if (response.Result[17] != null)
            {
                this.ViewAngle.Value = (string)response.Result[17].currentViewAnglee;
                this.ViewAngle.AvailableOptions = Utility.ToArray(response.Result[17].viewAngleCandidates);

                yield return "ViewAngle";
            }

            if (response.Result[18] != null)
            {
                this.Mode.Value = (string)response.Result[18].currentExposureMode;
                this.Mode.AvailableOptions = Utility.ToArray(response.Result[18].exposureModeCandidates);

                yield return "Mode";
            }

            if (response.Result[19] != null)
            {
                this.PostViewImageSize.Value = (string)response.Result[19].currentPostviewImageSize;
                this.PostViewImageSize.AvailableOptions = Utility.ToArray(response.Result[19].postviewImageSizeCandidates);

                yield return "PostViewImageSize";
            }

            if (response.Result[20] != null)
            {
                this.SelfTimer.Value = (string)response.Result[20].currentSelfTimer;
                this.SelfTimer.AvailableOptions = Utility.ToArray(response.Result[20].selfTimerCandidates);

                yield return "SelfTimer";
            }

            if (response.Result[21] != null)
            {
                this.ShootMode = (string)response.Result[21].currentShootMode;
                yield return "ShootMode";
            }

            if (response.Result[25] != null)
            {

                var min = (int)response.Result[25].minExposureCompensation;
                var max = (int)response.Result[25].maxExposureCompensation;
                var stepType = (int)response.Result[25].stepIndexOfExposureCompensation;

                var divisor = stepType == 1 ? 3.0 : 2.0;

                this.ExposureCompensation.Value = string.Format("{0:0.0}", (int)response.Result[25].currentExposureCompensation / divisor);

                List<string> range = new List<string>();
                for (int i = min; i <= max; i++)
                {
                    range.Add( string.Format("{0:0.0}", i / divisor) );
                }

                this.ExposureCompensation.AvailableOptions = range.ToArray();

                this.ExposureCompensation.OnNewValue = (s) => _Client.SetExposureCompensation((int)(double.Parse(s) * divisor));

                yield return "ExposureCompensation";
            }

            if (response.Result[26] != null)
            {
                this.FlashMode.Value = (string)response.Result[26].currentFlashMode;
                this.FlashMode.AvailableOptions = Utility.ToArray(response.Result[26].flashModeCandidates);

                yield return "FlashMode";
            }

            if (response.Result[27] != null)
            {
                this.FNumber.Value = (string)response.Result[27].currentFNumber;
                this.FNumber.AvailableOptions = Utility.ToArray(response.Result[27].fNumberCandidates);

                yield return "FNumber";
            }

            if (response.Result[28] != null)
            {
                this.FocusMode.Value = (string)response.Result[28].currentFocusMode;
                this.FocusMode.AvailableOptions = Utility.ToArray(response.Result[28].focusModeCandidates);
                yield return "FocusMode";
            }

            if (response.Result[29] != null)
            {
                this.ISO.Value = (string)response.Result[29].currentIsoSpeedRate;
                this.ISO.AvailableOptions = Utility.ToArray(response.Result[29].isoSpeedRateCandidates);
                yield return "ISO";
            }

            if (response.Result[31] != null)
            {
                this.IsProgramShifted = (bool?)response.Result[31].isShifted;
                yield return "IsProgramShifted"; 
            }

            if (response.Result[32] != null)
            {
                this.ShutterSpeed.Value = (string)response.Result[32].currentShutterSpeed;
                this.ShutterSpeed.AvailableOptions = Utility.ToArray(response.Result[32].shutterSpeedCandidates);
                yield return "ShutterSpeed";
            }

            if (response.Result[33] != null)
            {
                this.WhiteBalance.Value = (string)response.Result[33].currentWhiteBalanceMode;
                this.ColorTemperature.Value = (string)response.Result[33].currentColorTemperature;

                yield return "WhiteBalance";
                yield return "ColorTemperature"; 

                Task.Run(async () =>
                {
                    var wbResponse = await _Client.GetAvailableWhiteBalance();
                    if (wbResponse.IsSuccess == false)
                    {
                        this.WhiteBalance.AvailableOptions = null;
                        return;
                    }

                    this.WhiteBalance.AvailableOptions = (from dynamic item in (JArray)(wbResponse.Result[1])
                                                          let mode = (string)item.whiteBalanceMode
                                                          where mode != "Color Temperature"
                                                          select mode).ToArray();

                    if (this.WhiteBalance.AvailableOptions.Contains("Color Temperature"))
                    {
                        var array = (JArray)wbResponse.Result;
                        var modes = (JArray)array[1];
                        var range = (from dynamic item in modes
                                    where item.whiteBalanceMode == "Color Temperature"
                                    select (JArray)item.colorTemperatureRange).FirstOrDefault();

                        if ( range != null )
                        {
                            this.ColorTemperature.AvailableOptions = Utility.ToArray(range);
                        }

                    }

                    this.OnPropertyChanged("WhiteBalance");
                    this.OnPropertyChanged("AvailableSettings");

                });
            }

            if (response.Result[34] != null)
            {
                this.IsTouchAFSet = (bool)response.Result[34].currentSet;
                yield return "IsTouchAFSet";
            }

            _DisableUpdate = false;
        }
        
        private Task _PollingTask;

        /// <summary>
        /// Starts the status polling.
        /// </summary>
        /// <param name="token">The token.</param>
        public async Task StartStatusPolling( CancellationToken token )
        {   
            if (_PollingTask != null)
            {
                if (_PollingTask.IsCompleted == false )
                {
                    return;
                }
            }

            await this.RefreshStatus();

            _PollingTask = Task.Run(async () =>
            {
                while (token.IsCancellationRequested == false)
                {
                    var start = DateTime.Now;

                    try
                    {
                        Debug.WriteLine("Polling Status...");

                        var newEvent = await _Client.GetEvent(true);
                        var changed2 = this.UpdateFromEventResponse(newEvent);
                        this.OnPropertyChanged(changed2);

                        Debug.WriteLine("Status has changed.");

                    }
                    catch (Exception)
                    {
                        Debug.WriteLine("No Change Detected.");
                    }

                    // try to update no frequent than 2 times/second
                    var end = DateTime.Now;
                    var passed = (start - end).TotalSeconds;

                    if (passed > 0.5)
                    {
                        continue;
                    }
                    await Task.Delay( TimeSpan.FromSeconds( 0.5 - passed ));
                }
            });

        }

        /// <summary>
        /// Refresh status, this will clear all menu
        /// </summary>
        /// <param name="token">The token.</param>
        public async Task RefreshStatus()
        {
            foreach (var item in _Settings.Values)
            {
                item.AvailableOptions = null;
            }

            var newEvent = await _Client.GetEvent(false);
            var changed = this.UpdateFromEventResponse(newEvent);

            this.OnPropertyChanged(changed);
            this.OnPropertyChanged("AvailableSettings");
        }

        /// <summary>
        /// Creates the live view client.
        /// </summary>
        /// <returns></returns>
        public async Task StartLiveView( Func<string, Stream> provideHTTPStream, CancellationToken token )
        {
            var liveViewUrl = (string)(await _Client.StartLiveview()).Result;
            var client = new LiveViewClient(liveViewUrl);

            client.ImageReceived += (sender, data) =>
            {
                this.LiveViewImageReceived(data);
            };

            client.StartLiveView(provideHTTPStream, token);
        }

        /// <summary>
        /// Take Photo with single press shutter
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Shutter( Action<string> handleTakenPhoto )
        {
            if (this.Status != "IDLE")
            {
                return false;
            }

            SonyJsonRPCResponse response = null;
            if (this.ShootMode == "still")
            {
                response = await this.ApiClient.ActTakePicture();
            }
            else
            {
                await this.ApiClient.SetShootMode("still");
                while (this.ShootMode != "still")
                {
                    await Task.Delay(500);
                }
                response = await this.ApiClient.ActTakePicture();
            }

            if (response.IsSuccess)
            {
                handleTakenPhoto((string)response.Result[0]);
            }

            return true;
        }

        /// <summary>
        /// Toggles the movie recording.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> ToggleMovieRecording()
        {
            if (this.Status == "MovieRecording")
            {
                this.ApiClient.StopMovieRec();
                return true;
            }

            if (this.Status != "IDLE")
            {
                return false;
            }

            if (this.ShootMode == "movie")
            {
                this.ApiClient.StartMovieRec();
            }
            else
            {
                await this.ApiClient.SetShootMode("movie");
                while (this.ShootMode != "movie")
                {
                    await Task.Delay(500);
                }
                this.ApiClient.StartMovieRec();
            }

            return true;
        }

        #region Function to send settings to camera

        /// <summary>
        /// Initializes the settings
        /// </summary>
        private void InitializeSettings()
        {
            _Settings = new Dictionary<string, CameraSetting>();

            _Settings.Add("ExposureCompensation", new CameraSetting()
            {
                Group = "Photographic",
                Name = "Exposure Compensation",
                Description = "Normally, camera will attempt to capture equal brightness of the whole scene. " +
                "Using Exposure compensation will allow you to make dark area brigther at the cost of detail in brighter areas.",

                // OnNewValue is set in status update due to complex setting
            });

            _Settings.Add("FlashMode", new CameraSetting()
            {
                Group = "Photographic",
                Name = "FlashMode",
                OnNewValue = (value) => _Client.SetFlashMode(value)
            });

            _Settings.Add("FNumber", new CameraSetting()
            {
                Group = "Photographic",
                Name = "FNumber",
                OnNewValue = (value) => _Client.SetFNumber(value)
            });

            _Settings.Add("FocusMode", new CameraSetting()
            {
                Group = "Photographic",
                Name = "FocusMode",
                OnNewValue = (value) => _Client.SetFocusMode(value)
            });

            _Settings.Add("ISO", new CameraSetting()
            {
                Group = "Photographic",
                Name = "ISO",
                OnNewValue = (value) => _Client.SetIsoSpeedRate(value)
            });

            _Settings.Add("Mode", new CameraSetting()
            {
                Name = "Exposure Mode",
                Description = "Set how you want to control the exposure settings of the camera. For example, in Auto mode the F Number would have no effect.",
                OnNewValue = async (value) =>
                {
                    _Context.Post((obj) =>
                    {
                        this.LongRunningProcessOccured(true);

                    }, null);

                    await Task.Run( ()=>{

                        _Client.SetExposureMode(value).Wait();
                        this.RefreshStatus().Wait();

                    });

                    _Context.Post((obj) =>
                    {
                        this.LongRunningProcessOccured(false);

                    }, null);
                }
            });

            _Settings.Add("MovieQuality", new CameraSetting()
            {
                Group = "Output",
                Name = "Movie Quality",
                OnNewValue = (value) => _Client.SetMovieQuality(value)
            });

            _Settings.Add("OISMode", new CameraSetting()
            {
                Group = "Photographic",
                Name = "SteadyShot Mode",
                OnNewValue = (value) => _Client.SetSteadyMode(value)
            });

            _Settings.Add("PhotoResolution", new CameraSetting()
            {
                Group = "Output",
                Name = "Captured Image Resolution",
                Description = "Adjust the dimension and resolution of captured photo. Higher resolution is usually always better if space is not a concern.",

                // OnNewValue is set in update status
            });

            _Settings.Add("PostViewImageSize", new CameraSetting()
            {
                Group = "Output",
                Name = "Review Image Resolution",
                Description = "Set the resolution of review image shown after each shot. " +
                "Higher resolution will make it slower to download from camera but allow you to save full resolution photo to your phone. " +
                "Note that Facebook only allow up to 2M size",
                OnNewValue = (value) => _Client.SetPostviewImageSize(value)
            });


            _Settings.Add("SelfTimer", new CameraSetting()
            {
                Name = "Self Timer",
                Description = "Set time of self Timer function",
                OnNewValue = (value) => _Client.SetSelfTimer(double.Parse(value))
            });

            _Settings.Add("ShutterSpeed", new CameraSetting()
            {
                Group = "Photographic",
                Name = "Shutter Speed",
                Description = "Adjust the length of time camera will let the light to hit the sensor." +
                "More time will allow brighter images in low light condition but will blur moving subject",
                OnNewValue = (value) => _Client.SetShutterSpeed(value)
            });

            _Settings.Add("ViewAngle", new CameraSetting()
            {
                Group = "Output",
                Name = "View Angle",
                Description = "Specify the recorded View Angle",
                OnNewValue = (value) => _Client.SetViewAngle(value)
            });

            _Settings.Add("WhiteBalance", new CameraSetting()
            {
                Group = "Photographic",
                Name = "White Balance",
                Description = "Adjust the color tone of captured image." +
                "Auto will let the camera attempt to make white color captured as natural white, while other" +
                " modes allow you to make it warmer (more yellow) or colder (more blue).",
                OnNewValue = (s) => _Client.SetWhiteBalance(s)
            });

            _Settings.Add("ColorTemperature", new CameraSetting()
            {
                Group = "Photographic",
                Name = "Color Temperature",
                Description = "Adjust the color tone of captured image." +
                "Special white balance mode for professional lighting or when you want to configure the" +
                " color temperature manually. More numbers is more blue while lower number is more yellow",
                OnNewValue = (s) =>
                {
                    _Client.SetWhiteBalance("Color Temperature", true, int.Parse(s));

                    _DisableUpdate = true;
                    this.WhiteBalance.Value = "Color Temperature";
                    _DisableUpdate = false;
                }
            });

            foreach (var item in _Settings.Values)
            {
                item.Owner = this;
            }
        }
        
        #endregion


    }
}
