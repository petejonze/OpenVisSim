//-----------------------------------------------------------------------
// Copyright © 2017 Tobii AB. All rights reserved.
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Valve.VR;

[assembly: InternalsVisibleTo("Assembly-CSharp-Editor")]

namespace Tobii.Research.Unity
{
    internal static class TobiiExtensionMethods
    {
        private static float _lastLcsMm;

        public static Vector3 InUnityCoord(this Point3D value)
        {
            return new Vector3(-value.X / 1000f, value.Y / 1000f, value.Z / 1000f);
        }

        public static Vector3 InUnityCoord(this NormalizedPoint3D value)
        {
            return new Vector3(-value.X, value.Y, value.Z);
        }

        /// <summary>
        /// Update lens configuration. Call periodically, not too often, or on changing the Vive IPD.
        /// Avoid calling this at the same time as doing a calibration.
        /// </summary>
        /// <param name="eyeTracker"></param>
        /// <returns>True if lens config was updated, false otherwise.</returns>
        internal static bool UpdateLensConfiguration(this IEyeTracker eyeTracker)
        {
            if ((eyeTracker.DeviceCapabilities & Capabilities.HasHMDLensConfig) == 0)
            {
                return false;
            }

            var lcsMm = VRUtility.LensCupSeparation * 1000f;

            if (lcsMm > 0 && Mathf.Abs(lcsMm - _lastLcsMm) > 0.1f)
            {
                var lensConfig = new HMDLensConfiguration(new Point3D(lcsMm / 2f, 0, 0), new Point3D(lcsMm / -2f, 0, 0));
                eyeTracker.SetHMDLensConfiguration(lensConfig);
                _lastLcsMm = lcsMm;

                return true;
            }

            return false;
        }

        internal static EyeTrackerOriginPose GetPose(this Transform transform, long timeStamp)
        {
            return new EyeTrackerOriginPose(timeStamp, transform);
        }

        internal static Transform ApplyPose(this Transform transform, EyeTrackerOriginPose pose)
        {
            transform.position = pose.Position;
            transform.rotation = pose.Rotation;
            return transform;
        }

        internal static bool Valid(this Validity validity)
        {
            return validity == Validity.Valid;
        }
    }

    internal static class VRUtility
    {
        #region Private

        /// <summary>
        /// A transform to manipulate for the historical eye tracker origin.
        /// Use <see cref="TemporaryTransformWithAppliedPose(EyeTrackerOriginPose)"/>
        /// to get this transfomr with a provided pose applied. It is
        /// temporary since the object will change to the next supplied pose
        /// on the next call.
        /// </summary>
        private static Transform _historicalEyeTrackerOrigin;

        /// <summary>
        /// Simpler get float property from Vive.
        /// </summary>
        /// <param name="prop">The property to get</param>
        /// <param name="error">An error reference</param>
        /// <returns>The float value</returns>
        private static float GetFloatProperty(ETrackedDeviceProperty prop, ref ETrackedPropertyError error)
        {
            return SteamVR.instance.hmd.GetFloatTrackedDeviceProperty(OpenVR.k_unTrackedDeviceIndex_Hmd, prop, ref error);
        }

        #endregion Private

        #region Helpers

        /// <summary>
        /// Get lens cup separation from HDM. A negative number indicates an error.
        /// </summary>
        internal static float LensCupSeparation
        {
            get
            {
                if (!SteamVR.active)
                {
                    return -1;
                }

                var error = ETrackedPropertyError.TrackedProp_Success;
                var result = GetFloatProperty(ETrackedDeviceProperty.Prop_UserIpdMeters_Float, ref error);

                if (error != ETrackedPropertyError.TrackedProp_Success)
                {
                    return -1;
                }

                return result;
            }
        }

        /// <summary>
        /// Get a <see cref="Transform"/> object with the provided pose applied.
        /// It is temporary since it will change to then next provided pose on
        /// the next call, or by direct manipulation.
        /// </summary>
        /// <param name="pose">The pose to apply</param>
        /// <returns>The temporary transform</returns>
        internal static Transform TemporaryTransformWithAppliedPose(EyeTrackerOriginPose pose)
        {
            return _historicalEyeTrackerOrigin.ApplyPose(pose);
        }

        /// <summary>
        /// Get the eye tracker origin when using a Vive with SteamVR in Unity3D. Slow, only use for first lookup.
        /// </summary>
        internal static Transform EyeTrackerOriginVive
        {
            get
            {
                var originGO = GameObject.Find("EyetrackerOrigin");
                if (originGO)
                {
                    return originGO.transform;
                }

                var cam = GameObject.Find("Camera (eye)"); // PJ: HACK !
                Debug.Log(">>>> "  + cam);
                if (cam == null)
                { 
                    cam = GameObject.Find("Camera (LeftEye) (eye)"); // PJ: HACK !
                    Debug.Log(">>>>>>>>>> "  + cam);
                }

                if (!cam)
                {
                    return null;
                }

                originGO = new GameObject("EyetrackerOrigin");
                var eyetrackerOrigin = originGO.transform;
                eyetrackerOrigin.parent = cam.transform;

                // Create a hidden game object with a transform to manipulate by pose information.
                var historicalEyeTrackerOriginObject = new GameObject("Historical Eye Tracker Origin");
                historicalEyeTrackerOriginObject.hideFlags = HideFlags.HideInHierarchy;
                _historicalEyeTrackerOrigin = historicalEyeTrackerOriginObject.transform;

                var zOffs = 0.015f;

                if (SteamVR.active)
                {
                    var error = ETrackedPropertyError.TrackedProp_Success;
                    var h2eDepth = GetFloatProperty(ETrackedDeviceProperty.Prop_UserHeadToEyeDepthMeters_Float, ref error);

                    if (error == ETrackedPropertyError.TrackedProp_Success)
                    {
                        zOffs = h2eDepth;
                        Debug.Log("Got head to eye z depth from HMD: " + zOffs);
                    }
                }

                eyetrackerOrigin.localPosition = new Vector3(0, 0, zOffs);
                eyetrackerOrigin.localRotation = Quaternion.identity;
                return eyetrackerOrigin;
            }
        }

        #endregion Helpers
    }

    #region Queues and Lists

    /// <summary>
    /// Simple lock-protected queue. Will not grow above max count.
    /// </summary>
    /// <typeparam name="T">The class type for the queue</typeparam>
    internal sealed class LockedQueue<T>
    {
        private SizedQueue<T> _sizedQueue;

        /// <summary>
        /// Create a locked queue with size management.
        /// </summary>
        /// <param name="maxCount">Max size of the queue</param>
        internal LockedQueue(int maxCount)
        {
            _sizedQueue = new SizedQueue<T>(maxCount);
        }

        /// <summary>
        /// Enqueue or dequeue.
        /// </summary>
        internal T Next
        {
            get
            {
                lock (_sizedQueue)
                {
                    return _sizedQueue.Next;
                }
            }

            set
            {
                lock (_sizedQueue)
                {
                    _sizedQueue.Next = value;
                }
            }
        }

        /// <summary>
        /// Get queue size.
        /// </summary>
        internal int Count
        {
            get
            {
                lock (_sizedQueue)
                {
                    return _sizedQueue.Count;
                }
            }
        }
    }

    /// <summary>
    /// Simple size managed queue. Avoids overgrowing.
    /// </summary>
    /// <typeparam name="T">The class type for the queue</typeparam>
    internal sealed class SizedQueue<T>
    {
        private Queue<T> _queue = new Queue<T>();
        private int _maxCount;

        /// <summary>
        /// Create a size managed queue.
        /// </summary>
        /// <param name="maxCount">Max size of the queue</param>
        internal SizedQueue(int maxCount)
        {
            _maxCount = maxCount;
        }

        /// <summary>
        /// Enqueue or dequeue.
        /// </summary>
        internal T Next
        {
            get
            {
                if (_queue.Count < 1)
                {
                    return default(T);
                }

                return _queue.Dequeue();
            }

            set
            {
                _queue.Enqueue(value);

                // Manage queue size.
                while (_queue.Count > _maxCount)
                {
                    _queue.Dequeue();
                }
            }
        }

        /// <summary>
        /// Get queue size.
        /// </summary>
        internal int Count
        {
            get
            {
                return _queue.Count;
            }
        }
    }

    /// <summary>
    /// Size managed list of poses.
    /// </summary>
    internal sealed class PoseList
    {
        private List<EyeTrackerOriginPose> _list = new List<EyeTrackerOriginPose>();
        private int _maxCount;

        internal PoseList(int maxCount)
        {
            _maxCount = maxCount;
        }

        internal void Add(EyeTrackerOriginPose pose)
        {
            // Save the current pose for the current time.
            _list.Add(pose);

            while (_list.Count > _maxCount)
            {
                _list.RemoveAt(0);
            }
        }

        internal EyeTrackerOriginPose this[int index]
        {
            get
            {
                return _list[index];
            }

            set
            {
                _list[index] = value;
            }
        }

        internal int Count
        {
            get
            {
                return _list.Count;
            }
        }

        /// <summary>
        /// Look up the best matching pose corresponding to the provided time stamp.
        /// If the list is empty, an invalid pose will be returned.
        /// If the time stamp is:
        ///  - before the first pose, the first pose will be returned.
        ///  - after the last pose, the last pose will be returned.
        ///  - a perfect match, the corresponding pose will be returned.
        ///  - between two poses, the interpolated pose will be returned.
        /// </summary>
        /// <param name="timeStamp">The gaze time stamp in the system clock</param>
        /// <returns>The best matching pose</returns>
        internal EyeTrackerOriginPose GetBestMatchingPose(long timeStamp)
        {
            var comparer = new EyeTrackerOriginPose(timeStamp);
            var index = _list.BinarySearch(comparer);

            if (_list.Count == 0)
            {
                // No poses to compare. Return invalid object.
                return comparer;
            }

            if (index < 0)
            {
                // No direct hit. This should be the common case.
                // Bitwise complement gives the index of the next larger item.
                index = ~index;

                if (index > 0)
                {
                    if (index == _list.Count)
                    {
                        // There is no larger time stamp. Return the last item.
                        return _list[_list.Count - 1];
                    }

                    // Interpolate a new pose and return it.
                    return _list[index - 1].Interpolate(_list[index], timeStamp);
                }

                // If index is zero, then the time stamp we provided is before
                // the first item in the poses list. This is normally long ago.
                // Anyway, return the first item.
                return _list[0];
            }

            // Direct hit. Could happen, but should be very rare.
            return _list[index];
        }
    }

    #endregion Queues and Lists
}
