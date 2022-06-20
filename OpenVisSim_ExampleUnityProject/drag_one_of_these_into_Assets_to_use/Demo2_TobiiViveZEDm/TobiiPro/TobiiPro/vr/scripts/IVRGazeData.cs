//-----------------------------------------------------------------------
// Copyright © 2017 Tobii AB. All rights reserved.
//-----------------------------------------------------------------------

using UnityEngine;

namespace Tobii.Research.Unity
{
    /// <summary>
    /// A gaze data object contains the gaze data in Unity coordinates.
    /// </summary>
    public interface IVRGazeData
    {
        /// <summary>
        /// Data for the left eye in Unity coordinates.
        /// </summary>
        IVRGazeDataEye Left { get; }

        /// <summary>
        /// Data for the right eye in Unity coordinates.
        /// </summary>
        IVRGazeDataEye Right { get; }

        /// <summary>
        /// The combined gaze ray for this data in world coordinates.
        /// Based on the combined gaze origins and directions of the eyes.
        /// </summary>
        Ray CombinedGazeRayWorld { get; }

        /// <summary>
        /// The validity of the combined gaze ray. True is valid.
        /// </summary>
        bool CombinedGazeRayWorldValid { get; }

        /// <summary>
        /// A reference to the unprocessed gaze data received from the eye tracker.
        /// </summary>
        HMDGazeDataEventArgs OriginalGaze { get; }

        /// <summary>
        /// Tobii system time stamp for the data.
        /// </summary>
        long TimeStamp { get; }

        /// <summary>
        /// The approximate head pose at the time of the gaze data.
        /// </summary>
        EyeTrackerOriginPose Pose { get; }
    }
}
