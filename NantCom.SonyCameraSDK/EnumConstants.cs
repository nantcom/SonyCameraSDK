using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NantCom.SonyCameraSDK
{

    public enum ZoomDirection
    {
        /// <summary>
        /// Zoom-in
        /// </summary>
        In,
        /// <summary>
        /// Zoom-out
        /// </summary>
        Out
    }

    public enum ZoomMovement
    {
        /// <summary>
        /// Long push
        /// </summary>
        Start,
        /// <summary>
        /// Stop
        /// </summary>
        Stop,
        /// <summary>
        /// Short push
        /// </summary>
        OneShot
    }

}
