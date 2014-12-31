using NantCom.SonyCameraSDK.JsonRPC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NantCom.SonyCameraSDK
{
    public class Camera
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
            public int ExposureCompensationMinimum { get; set; }

            /// <summary>
            /// Gets or sets the exposure compensation maximum.
            /// </summary>
            /// <value>
            /// The exposure compensation maximum.
            /// </value>
            public int ExposureCompensationMaximum { get; set; }

            /// <summary>
            /// Gets or sets the index of the exposure compensation step. 1 = 1/3EV, 2 = 1/2EV
            /// </summary>
            /// <value>
            /// The index of the exposure compensation step.
            /// </value>
            public int ExposureCompensationStepIndex { get; set; }

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
            public int ColorTemperatureMinimum { get; set; }

            /// <summary>
            /// Gets or sets the color temperature maximum.
            /// </summary>
            /// <value>
            /// The color temperature maximum.
            /// </value>
            public int ColorTemperatureMaximum { get; set; }

            /// <summary>
            /// Gets or sets the color temperature step.
            /// </summary>
            /// <value>
            /// The color temperature step.
            /// </value>
            public int ColorTemperatureStep { get; set; }

            /// <summary>
            /// Gets or sets the available zoom settings.
            /// </summary>
            /// <value>
            /// The zoom settings.
            /// </value>
            public string[] ZoomSettings { get; set; }
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
        public bool IsLiveViewReady { get; private set; }

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
        public int RecordableTime { get; private set; }

        /// <summary>
        /// Gets the number of recordable photos.
        /// </summary>
        /// <value>
        /// The recordable photos.
        /// </value>
        public int RecordablePhotos { get; private set; }

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
        private string _SelfTimer;
        private string _ShootMode;
        private string _ExposureCompensation;
        private string _FlashMode;
        private string _FNumber;
        private string _FocusMode;
        private string _ISO; 
        private string _ShutterSpeed;
        private string _WhiteBalance;
        private int _ColorTemperature;

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
        public string SelfTimer
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
        public string ExposureCompensation
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
        public bool IsProgramShifted
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
        public int ColorTemperature
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
        public bool IsTouchAFSet { get; private set; }

        /// <summary>
        /// Updates that status from given RPC Response;
        /// </summary>
        /// <param name="response">The response.</param>
        public void Update(SonyJsonRPCResponse response)
        {
            this.CameraOptions = new Options();

            this.Status = response.Result[1].cameraStatus;
            this.ZoomPercentage = response.Result[2].zoomPosition;
            this.IsLiveViewReady = response.Result[3].liveviewStatus;
            this.LiveViewOrientation = response.Result[4].liveviewOrientation;

            foreach (var item in response.Result[10])
            {
                if (item.recordTarget == true)
                {
                    this.RecordableTime = item.recordableTime;
                    this.RecordablePhotos = item.numberOfRecordableImages;
                }
            }

            this.CameraOptions.Functions = response.Result[12].cameraFunctionCandidates;
            this.CurrentFunction = response.Result[12].currentCameraFunction;

            this.CameraOptions.MovieQuality = response.Result[13].movieQualityCandidates;
            this.MovieQuality = response.Result[13].currentMovieQuality;

            this.PhotoAspect = response.Result[14].currentAspect;
            this.PhotoResolution = response.Result[14].currentSize;

            this.CameraOptions.OISModes = response.Result[16].steadyModeCandidates;
            this.OISMode = response.Result[16].currentSteadyMode;

            this.CameraOptions.ViewAngles = response.Result[17].viewAngleCandidates;
            this.ViewAngle = response.Result[17].currentViewAngle;

            this.CameraOptions.Modes = response.Result[18].exposureModeCandidates;
            this.Mode = response.Result[18].currentExposureMode;

            this.CameraOptions.PostViewImageSizes = response.Result[19].postviewImageSizeCandidates;
            this.PostViewImageSize = response.Result[19].currentPostviewImageSize;

            this.CameraOptions.SelfTimers = response.Result[20].selfTimerCandidates;
            this.SelfTimer = response.Result[20].currentSelfTimer;

            this.CameraOptions.ShootMode = response.Result[21].shootModeCandidates;
            this.ShootMode = response.Result[21].currentShootMode;

            this.CameraOptions.ExposureCompensationMinimum = response.Result[25].minExposureCompensation;
            this.CameraOptions.ExposureCompensationMaximum = response.Result[25].maxExposureCompensation;
            this.ExposureCompensation = response.Result[25].currentExposureCompensation;

            this.CameraOptions.FlashModes = response.Result[26].flashModeCandidates;
            this.FlashMode = response.Result[26].currentFlashMode;

            this.CameraOptions.FNumber = response.Result[27].fNumberCandidates;
            this.FNumber = response.Result[27].currentFNumber;

            this.CameraOptions.FocusModes = response.Result[28].focusModeCandidates;
            this.FocusMode = response.Result[28].currentFocusMode;

            this.CameraOptions.ISOSpeedCandidates = response.Result[29].isoSpeedRateCandidates;
            this.ISO = response.Result[29].currentIsoSpeedRate;

            this.IsProgramShifted = response.Result[31].isShifted;

            this.CameraOptions.ShutterSpeeds = response.Result[32].shutterSpeedCandidates;
            this.ShutterSpeed = response.Result[32].currentShutterSpeed;

            this.WhiteBalance = response.Result[33].currentWhiteBalanceMode;
            this.ColorTemperature = response.Result[33].currentColorTemperature;

            this.IsTouchAFSet = response.Result[34].currentSet;
        }

        private void SetColorTemperature(int value)
        {
            throw new NotImplementedException();
        }

        private void SetWhiteBalance(string value)
        {
            throw new NotImplementedException();
        }

        private void SetISO(string value)
        {
            throw new NotImplementedException();
        }
		
        private void SetFocusMode(string value)
        {
            throw new NotImplementedException();
        }
		
        private void SetFNumber(string value)
        {
            throw new NotImplementedException();
        }

        private void SetFlashMode(string value)
        {
            throw new NotImplementedException();
        }

        private void SetExposureCompensation(string value)
        {
            throw new NotImplementedException();
        }

        private void SetShootMode(string value)
        {
            throw new NotImplementedException();
        }	

        private void SetSelfTimer(string value)
        {
            throw new NotImplementedException();
        }

        private void SetCurrentFunction(string value)
        {
            throw new NotImplementedException();
        }

        private void SetMovieQuality(string value)
        {
            throw new NotImplementedException();
        }	

        private void SetPhotoAspect(string value)
        {
            throw new NotImplementedException();
        }

        private void SetPhotoResolution(string value)
        {
            throw new NotImplementedException();
        }
			
        private void SetOISMode(string value)
        {
            throw new NotImplementedException();
        }

        private void SetViewAngle(string value)
        {
            throw new NotImplementedException();
        }

        private void SetMode(string value)
        {
            throw new NotImplementedException();
        }

        private void SetPostViewImageSize(string value)
        {
            throw new NotImplementedException();
        }	
		

    }
}
