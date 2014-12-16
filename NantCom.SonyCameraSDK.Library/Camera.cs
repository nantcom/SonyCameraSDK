using NantCom.SonyCameraSDK;
using NantCom.SonyCameraSDK.JsonRPC;
using System;
using System.Collections.Generic;
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
        /// <summary>
        /// Possible options for this camera
        /// </summary>
        public class Options
        {
            /// <summary>
            /// Gets the available camera functions.
            /// </summary>
            /// <value>
            /// The functions.
            /// </value>
            public string[] Functions { get; set; }

            /// <summary>
            /// Gets or sets the movie quality.
            /// </summary>
            /// <value>
            /// The movie quality.
            /// </value>
            public string[] MovieQuality { get; set; }

            /// <summary>
            /// Gets or sets the ois modes.
            /// </summary>
            /// <value>
            /// The ois modes.
            /// </value>
            public string[] OISModes { get; set; }

            /// <summary>
            /// Gets or sets the view angles.
            /// </summary>
            /// <value>
            /// The view angles.
            /// </value>
            public string[] ViewAngles { get; set; }

            /// <summary>
            /// Gets or sets the modes.
            /// </summary>
            /// <value>
            /// The modes.
            /// </value>
            public string[] Modes { get; set; }

            /// <summary>
            /// Gets or sets the post view image sizes.
            /// </summary>
            /// <value>
            /// The post view image sizes.
            /// </value>
            public string[] PostViewImageSizes { get; set; }

            /// <summary>
            /// Gets or sets the self timers.
            /// </summary>
            /// <value>
            /// The self timers.
            /// </value>
            public string[] SelfTimers { get; set; }

            /// <summary>
            /// Gets or sets the shoot mode.
            /// </summary>
            /// <value>
            /// The shoot mode.
            /// </value>
            public string[] ShootMode { get; set; }

            /// <summary>
            /// Gets or sets the exposure compensation minimum.
            /// </summary>
            /// <value>
            /// The exposure compensation minimum.
            /// </value>
            public int? ExposureCompensationMinimum { get; set; }

            /// <summary>
            /// Gets or sets the exposure compensation maximum.
            /// </summary>
            /// <value>
            /// The exposure compensation maximum.
            /// </value>
            public int? ExposureCompensationMaximum { get; set; }

            /// <summary>
            /// Gets or sets the index of the exposure compensation step. 1 = 1/3EV, 2 = 1/2EV
            /// </summary>
            /// <value>
            /// The index of the exposure compensation step.
            /// </value>
            public int? ExposureCompensationStepIndex { get; set; }

            /// <summary>
            /// Gets or sets the flash modes.
            /// </summary>
            /// <value>
            /// The flash modes.
            /// </value>
            public string[] FlashModes { get; set; }

            /// <summary>
            /// Gets or sets the f number.
            /// </summary>
            /// <value>
            /// The f number.
            /// </value>
            public string[] FNumber { get; set; }

            /// <summary>
            /// Gets or sets the focus modes.
            /// </summary>
            /// <value>
            /// The focus modes.
            /// </value>
            public string[] FocusModes { get; set; }

            /// <summary>
            /// Gets or sets the iso speed candidates.
            /// </summary>
            /// <value>
            /// The iso speed candidates.
            /// </value>
            public string[] ISOSpeedCandidates { get; set; }

            /// <summary>
            /// Gets or sets the shutter speeds.
            /// </summary>
            /// <value>
            /// The shutter speeds.
            /// </value>
            public string[] ShutterSpeeds { get; set; }

            /// <summary>
            /// Gets or sets the color temperature minimum.
            /// </summary>
            /// <value>
            /// The color temperature minimum.
            /// </value>
            public int? ColorTemperatureMinimum { get; set; }

            /// <summary>
            /// Gets or sets the color temperature maximum.
            /// </summary>
            /// <value>
            /// The color temperature maximum.
            /// </value>
            public int? ColorTemperatureMaximum { get; set; }

            /// <summary>
            /// Gets or sets the color temperature step.
            /// </summary>
            /// <value>
            /// The color temperature step.
            /// </value>
            public int? ColorTemperatureStep { get; set; }

            /// <summary>
            /// Gets or sets the available zoom settings.
            /// </summary>
            /// <value>
            /// The zoom settings.
            /// </value>
            public string[] ZoomSettings { get; set; }

            #region Supported Flags

            /// <summary>
            /// Gets a value indicating whether camera supports setting color temperature 
            /// </summary>
            /// <value>
            /// <c>true</c> if color temperature setting supported; otherwise, <c>false</c>.
            /// </value>
            public bool IsColorTemperatureSupported
            {
                get
                {
                    return                        
                        this.ColorTemperatureMaximum != null &&
                        this.ColorTemperatureMinimum != null &&
                        this.ColorTemperatureStep != null;
                }
            }

            #endregion
        }

        /// <summary>
        /// Gets the camera options.
        /// </summary>
        /// <value>
        /// The camera options.
        /// </value>
        public Options CameraOptions { get; private set; }

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

        private string _CurrentFunction;
        private string _PhotoAspect;
        private string _PhotoResolution;
        private string _OISMode;
        private string _ViewAngle;
        private string _Mode;
        private string _PostViewImageSize;
        private string _MovieQuality;
        private int _SelfTimer;
        private string _ShootMode;
        private int _ExposureCompensation;
        private string _FlashMode;
        private string _FNumber;
        private string _FocusMode;
        private string _ISO;
        private string _ShutterSpeed;
        private string _WhiteBalance;
        private int? _ColorTemperature;

        ///<summary>
        ///Get or set the value of CurrentFunction
        ///</summary>
        public string CurrentFunction
        {
            get
            {
                return _CurrentFunction;
            }
            set
            {
                this.SetCurrentFunction(value);
            }
        }

        ///<summary>
        ///Get or set the value of MovieQuality
        ///</summary>
        public string MovieQuality
        {
            get
            {
                return _MovieQuality;
            }
            set
            {
                this.SetMovieQuality(value);
            }
        }

        /// <summary>
        /// Get or set the value of PhotoAspect
        /// </summary>
        public string PhotoAspect
        {
            get
            {
                return _PhotoAspect;
            }
            set
            {
                this.SetPhotoAspect(value);
            }
        }

        ///<summary>
        ///Get or set the value of PhotoResolution
        ///</summary>
        public string PhotoResolution
        {
            get
            {
                return _PhotoResolution;
            }
            set
            {
                this.SetPhotoResolution(value);
            }
        }

        ///<summary>
        ///Get or set the value of OISMode
        ///</summary>
        public string OISMode
        {
            get
            {
                return _OISMode;
            }
            set
            {
                this.SetOISMode(value);
            }
        }

        ///<summary>
        ///Get or set the value of ViewAngle
        ///</summary>
        public string ViewAngle
        {
            get
            {
                return _ViewAngle;
            }
            set
            {
                this.SetViewAngle(value);
            }
        }

        ///<summary>
        ///Get or set the value of Mode
        ///</summary>
        public string Mode
        {
            get
            {
                return _Mode;
            }
            set
            {
                this.SetMode(value);
            }
        }

        ///<summary>
        ///Get or set the value of PostViewImageSize
        ///</summary>
        public string PostViewImageSize
        {
            get
            {
                return _PostViewImageSize;
            }
            set
            {
                this.SetPostViewImageSize(value);
            }
        }

        ///<summary>
        ///Get or set the value of SelfTimer
        ///</summary>
        public int SelfTimer
        {
            get
            {
                return _SelfTimer;
            }
            set
            {
                this.SetSelfTimer(value);
            }
        }

        ///<summary>
        ///Get or set the value of ShootMode
        ///</summary>
        public string ShootMode
        {
            get
            {
                return _ShootMode;
            }
            set
            {
                this.SetShootMode(value);
            }
        }

        ///<summary>
        ///Get or set the value of ExposureCompensation
        ///</summary>
        public int ExposureCompensation
        {
            get
            {
                return _ExposureCompensation;
            }
            set
            {
                this.SetExposureCompensation(value);
            }
        }

        ///<summary>
        ///Get or set the value of FlashMode
        ///</summary>
        public string FlashMode
        {
            get
            {
                return _FlashMode;
            }
            set
            {
                this.SetFlashMode(value);
            }
        }

        ///<summary>
        ///Get or set the value of FNumber
        ///</summary>
        public string FNumber
        {
            get
            {
                return _FNumber;
            }
            set
            {
                this.SetFNumber(value);
            }
        }

        ///<summary>
        ///Get or set the value of FocusMode
        ///</summary>
        public string FocusMode
        {
            get
            {
                return _FocusMode;
            }
            set
            {
                this.SetFocusMode(value);
            }
        }

        ///<summary>
        ///Get or set the value of ISO
        ///</summary>
        public string ISO
        {
            get
            {
                return _ISO;
            }
            set
            {
                this.SetISO(value);
            }
        }

        ///<summary>
        ///Get or set the value of IsProgramShifted
        ///</summary>
        public bool? IsProgramShifted
        {
            get;
            private set;
        }

        ///<summary>
        ///Get or set the value of ShutterSpeed
        ///</summary>
        public string ShutterSpeed
        {
            get
            {
                return _ShutterSpeed;
            }
            set
            {
                _ShutterSpeed = value;
            }
        }

        ///<summary>
        ///Get or set the value of WhiteBalance
        ///</summary>
        public string WhiteBalance
        {
            get
            {
                return _WhiteBalance;
            }
            set
            {
                this.SetWhiteBalance(value);
            }
        }

        ///<summary>
        ///Get or set the value of ColorTemperature
        ///</summary>
        public int? ColorTemperature
        {
            get
            {
                return _ColorTemperature;
            }
            set
            {
                this.SetColorTemperature(value);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is touch af set.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is touch af set; otherwise, <c>false</c>.
        /// </value>
        public bool? IsTouchAFSet { get; private set; }

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

        private bool _DisableUpdate = true;
        private CameraApiClient _Client;

        /// <summary>
        /// Initializes a new instance of the <see cref="Camera"/> class.
        /// </summary>
        /// <param name="client">The camera api client.</param>
        public Camera(CameraApiClient client)
        {
            _Client = client;
            this.CameraOptions = new Options();
        }

        /// <summary>
        /// Updates that status from given RPC Response;
        /// </summary>
        /// <param name="response">The response.</param>
        private void UpdateFromEventResponse(SonyJsonRPCResponse response)
        {
            if (response.IsSuccess == false)
            {
                return;
            }

            _DisableUpdate = true;

            if (response.Result[1] != null)
            {
                this.Status = response.Result[1].cameraStatus;
            }

            if (response.Result[2] != null)
            {
                this.ZoomPercentage = (int)response.Result[2].zoomPosition;
            }

            if (response.Result[3] != null)
            {
                this.IsLiveViewReady = (bool)response.Result[3].liveviewStatus;
            }

            if (response.Result[4] != null)
            {
                this.LiveViewOrientation = (string)response.Result[4].liveviewOrientation;
            }

            if (response.Result[10] != null)
            {
                foreach (var item in response.Result[10])
                {
                    if (item.recordTarget == true)
                    {
                        this.RecordableTime = (int)item.recordableTime;
                        this.RecordablePhotos = (int)item.numberOfRecordableImages;

                        if (this.RecordableTime == -1)
                        {
                            this.RecordableTime = null;
                        }
                    }
                }
            }

            if (response.Result[12] != null)
            {
                this.CameraOptions.Functions = Utility.ToArray(response.Result[12].cameraFunctionCandidates);
                this.CurrentFunction = (string)response.Result[12].currentCameraFunction;
            }

            if (response.Result[13] != null)
            {
                this.CameraOptions.MovieQuality = Utility.ToArray(response.Result[13].movieQualityCandidates);
                this.MovieQuality = (string)response.Result[13].currentMovieQuality;
            }

            if (response.Result[14] != null)
            {
                this.PhotoAspect = (string)response.Result[14].currentAspect;
                this.PhotoResolution = (string)response.Result[14].currentSize;
            }

            if (response.Result[16] != null)
            {
                this.CameraOptions.OISModes = Utility.ToArray(response.Result[16].steadyModeCandidates);
                this.OISMode = (string)response.Result[16].currentSteadyMode;
            }


            if (response.Result[17] != null)
            {
                this.CameraOptions.ViewAngles = Utility.ToArray(response.Result[17].viewAngleCandidates);
                this.ViewAngle = (string)response.Result[17].currentViewAngle;
            }

            if (response.Result[18] != null)
            {
                this.CameraOptions.Modes = Utility.ToArray(response.Result[18].exposureModeCandidates);
                this.Mode = (string)response.Result[18].currentExposureMode;
            }

            if (response.Result[19] != null)
            {
                this.CameraOptions.PostViewImageSizes = Utility.ToArray(response.Result[19].postviewImageSizeCandidates);
                this.PostViewImageSize = (string)response.Result[19].currentPostviewImageSize;
            }

            if (response.Result[20] != null)
            {
                this.CameraOptions.SelfTimers = Utility.ToArray(response.Result[20].selfTimerCandidates);
                this.SelfTimer = (int)response.Result[20].currentSelfTimer;
            }

            if (response.Result[21] != null)
            {
                this.CameraOptions.ShootMode = Utility.ToArray(response.Result[21].shootModeCandidates);
                this.ShootMode = (string)response.Result[21].currentShootMode;
            }

            if (response.Result[25] != null)
            {
                this.CameraOptions.ExposureCompensationMinimum = (int)response.Result[25].minExposureCompensation;
                this.CameraOptions.ExposureCompensationMaximum = (int)response.Result[25].maxExposureCompensation;
                this.CameraOptions.ExposureCompensationStepIndex = (int)response.Result[25].stepIndexOfExposureCompensation;
                this.ExposureCompensation = (int)response.Result[25].currentExposureCompensation;
            }

            if (response.Result[26] != null)
            {
                this.CameraOptions.FlashModes = Utility.ToArray(response.Result[26].flashModeCandidates);
                this.FlashMode = (string)response.Result[26].currentFlashMode;
            }

            if (response.Result[27] != null)
            {
                this.CameraOptions.FNumber = Utility.ToArray(response.Result[27].fNumberCandidates);
                this.FNumber = (string)response.Result[27].currentFNumber;
            }

            if (response.Result[28] != null)
            {
                this.CameraOptions.FocusModes = Utility.ToArray(response.Result[28].focusModeCandidates);
                this.FocusMode = (string)response.Result[28].currentFocusMode;
            }

            if (response.Result[29] != null)
            {
                this.CameraOptions.ISOSpeedCandidates = Utility.ToArray(response.Result[29].isoSpeedRateCandidates);
                this.ISO = (string)response.Result[29].currentIsoSpeedRate;
            }

            if (response.Result[31] != null)
            {
                this.IsProgramShifted = (bool?)response.Result[31].isShifted;
            }

            if (response.Result[32] != null)
            {
                this.CameraOptions.ShutterSpeeds = Utility.ToArray(response.Result[32].shutterSpeedCandidates);
                this.ShutterSpeed = (string)response.Result[32].currentShutterSpeed;
            }

            if (response.Result[33] != null)
            {
                this.WhiteBalance = (string)response.Result[33].currentWhiteBalanceMode;
                this.ColorTemperature = (int)response.Result[33].currentColorTemperature;
            }

            if (response.Result[34] != null)
            {
                this.IsTouchAFSet = (bool?)response.Result[34].currentSet;
            }
            
            _DisableUpdate = false;
            this.PropertyChanged(this, new PropertyChangedEventArgs(string.Empty));
        }
        
        /// <summary>
        /// Updates the status.
        /// </summary>
        /// <returns></returns>
        public async Task UpdateStatus()
        {
            var result = await _Client.GetEvent(false);
            this.UpdateFromEventResponse(result);
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

            var context = System.Threading.SynchronizationContext.Current;
            _PollingTask = Task.Run(() =>
            {
                while (token.IsCancellationRequested == false)
                {
                    try
                    {
                        //Debug.WriteLine("Polling Status...");

                        var newEvent = _Client.GetEvent(true).Result;

                        context.Post((state) =>
                        {
                            this.UpdateFromEventResponse(newEvent);

                        }, null);

                        //Debug.WriteLine("Status has changed.");
                    }
                    catch (Exception)
                    {
                        //Debug.WriteLine("No Change Detected.");
                    }

                    Task.Delay(1000).Wait();
                }
            });

            await _PollingTask;
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

        #region Function to send settings to camera

        private void SetColorTemperature(int? value)
        {
            if (_ColorTemperature == value)
            {
                return;
            }
            _ColorTemperature = value;

            if (_DisableUpdate)
                return;

            _Client.SetWhiteBalance("Color Temperature", true, value.Value);
        }

        private void SetWhiteBalance(string value)
        {
            if (_WhiteBalance == value)
            {
                return;
            }
            _WhiteBalance = value;

            if (_DisableUpdate)
                return;

            _Client.SetWhiteBalance(value);
        }

        private void SetISO(string value)
        {
            if (_ISO == value)
            {
                return;
            }
            _ISO = value;

            if (_DisableUpdate)
                return;

            _Client.SetIsoSpeedRate(value);
        }

        private void SetFocusMode(string value)
        {
            if (_FocusMode == value)
            {
                return;   
            }
            _FocusMode = value;

            if (_DisableUpdate)
                return;

            _Client.SetFocusMode(value);
        }

        private void SetFNumber(string value)
        {
            if (_FNumber == value)
            {
                return;
            }
            _FNumber = value;

            if (_DisableUpdate)
                return;

            _Client.SetFNumber(value);
        }

        private void SetFlashMode(string value)
        {
            if (_FlashMode == value)
            {
                return;
            }
            _FlashMode = value;

            if (_DisableUpdate)
                return;

            _Client.SetFlashMode(value);
        }

        private void SetExposureCompensation(int value)
        {
            if (_ExposureCompensation == value)
            {
                return;
            }
            _ExposureCompensation = value;

            if (_DisableUpdate)
                return;

            _Client.SetExposureCompensation(value);
        }

        private void SetShootMode(string value)
        {
            if (_ShootMode == value)
            {
                return;
            }
            _ShootMode = value;

            if (_DisableUpdate)
                return;

            _Client.SetShootMode(value);
        }

        private void SetSelfTimer(int value)
        {
            if (_SelfTimer == value)
            {
                return;
            }
            _SelfTimer = value;

            if (_DisableUpdate)
                return;

            _Client.SetSelfTimer(value);
        }

        private void SetCurrentFunction(string value)
        {
            if (_CurrentFunction == value)
            {
                return;
            }
            _CurrentFunction = value;

            if (_DisableUpdate)
                return;

            _Client.SetCameraFunction(value);
        }

        private void SetMovieQuality(string value)
        {
            if (_MovieQuality == value)
            {
                return;
            }
            _MovieQuality = value;

            if (_DisableUpdate)
                return;

            _Client.SetMovieQuality(value);
        }

        private void SetPhotoAspect(string value)
        {
            if (_PhotoAspect == value)
            {
                return;
            }
            _PhotoAspect = value;

            if (_DisableUpdate)
                return;

            _Client.SetStillSize(value, _PhotoResolution);
        }

        private void SetPhotoResolution(string value)
        {
            if (_PhotoResolution == value)
            {
                return;
            }
            _PhotoResolution = value;

            if (_DisableUpdate)
                return;

            _Client.SetStillSize(_PhotoAspect, value);
        }

        private void SetOISMode(string value)
        {
            if (_OISMode == value)
            {
                return;
            }
            _OISMode = value;

            if (_DisableUpdate)
                return;

            _Client.SetSteadyMode(value);
        }

        private void SetViewAngle(string value)
        {
            if (_ViewAngle == value)
            {
                return;
            }
            _ViewAngle = value;

            if (_DisableUpdate)
                return;

            _Client.SetViewAngle(value);
        }

        private void SetMode(string value)
        {
            if (_Mode == value)
            {
                return;
            }
            _Mode = value;

            if (_DisableUpdate)
                return;

            _Client.SetExposureMode(value);
        }

        private void SetPostViewImageSize(string value)
        {
            if (_PostViewImageSize == value)
            {
                return;
            }
            _PostViewImageSize = value;

            if (_DisableUpdate)
                return;

            _Client.SetPostviewImageSize(value);
        }

        #endregion

    }
}
