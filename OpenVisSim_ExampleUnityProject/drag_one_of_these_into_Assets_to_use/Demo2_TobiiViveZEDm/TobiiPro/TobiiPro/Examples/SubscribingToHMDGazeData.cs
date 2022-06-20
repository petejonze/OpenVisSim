using System.Collections.Generic;
using System.Linq;
using Tobii.Research;
using UnityEngine;

namespace Tobii.Research.Unity.CodeExamples
{
    // The events in the SDK are called on a thread internal to the SDK. That thread can not safely set values
    // that are to be read on the main thread. The simplest way to make it safe is to enqueue the date, and dequeue it
    // on the main thread, e.g. via Update() in a MonoBehaviour.
    class SubscribingToHMDGazeData : MonoBehaviour
    {
        private IEyeTracker _eyeTracker;
        private Queue<HMDGazeDataEventArgs> _queue = new Queue<HMDGazeDataEventArgs>();

        void Awake()
        {
            var trackers = EyeTrackingOperations.FindAllEyeTrackers();
            _eyeTracker = trackers.FirstOrDefault(s => (s.DeviceCapabilities & Capabilities.HasHMDGazeData) != 0);
            if (_eyeTracker == null) 
            {
                Debug.Log("No HMD eye tracker detected!");    
            }
            else 
            {
                Debug.Log("Selected eye tracker with serial number {0}" + _eyeTracker.SerialNumber);
            }
        }

        void Update()
        {
            PumpGazeData();
        }

        void OnEnable()
        {
            if (_eyeTracker != null)
            {
                _eyeTracker.HMDGazeDataReceived += EnqueueEyeData;
            }
        }

        void OnDisable()
        {
            if (_eyeTracker != null)
            {
                _eyeTracker.HMDGazeDataReceived -= EnqueueEyeData;
            }
        }

        void OnDestroy()
        {
            EyeTrackingOperations.Terminate();
        }

        // This method will be called on a thread belonging to the SDK, and can not safely change values
        // that will be read from the main thread.
        private void EnqueueEyeData(object sender, HMDGazeDataEventArgs e)
        {
            lock (_queue)
            {
                _queue.Enqueue(e);
            }
        }

        private HMDGazeDataEventArgs GetNextGazeData()
        {
            lock (_queue)
            {
                return _queue.Count > 0 ? _queue.Dequeue() : null;
            }
        }

        private void PumpGazeData()
        {
            var next = GetNextGazeData();
            while (next != null)
            {
                HandleGazeData(next);
                next = GetNextGazeData();
            }
        }

        // This method will be called on the main Unity thread
        private void HandleGazeData(HMDGazeDataEventArgs e)
        {
            // Do something with gaze data
            // Debug.Log(string.Format(
            //     "Got gaze data with {0} left eye origin at point ({1}, {2}, {3}) in the HMD coordinate system.",
            //     e.LeftEye.GazeOrigin.Validity,
            //     e.LeftEye.GazeOrigin.PositionInHMDCoordinates.X,
            //     e.LeftEye.GazeOrigin.PositionInHMDCoordinates.Y,
            //     e.LeftEye.GazeOrigin.PositionInHMDCoordinates.Z));
        }
    }
}
