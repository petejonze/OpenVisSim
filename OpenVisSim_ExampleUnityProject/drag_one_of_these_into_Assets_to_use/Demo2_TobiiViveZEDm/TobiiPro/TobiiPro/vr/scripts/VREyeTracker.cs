//-----------------------------------------------------------------------
// Copyright © 2017 Tobii AB. All rights reserved.
//-----------------------------------------------------------------------

using System.Collections;
using System.Threading;
using UnityEngine;

namespace Tobii.Research.Unity
{
    public class VREyeTracker : MonoBehaviour
    {
        #region Public Properties

        /// <summary>
        /// Get <see cref="VREyeTracker"/> instance. This is assigned
        /// in Awake(), so call earliest in Start().
        /// </summary>
        public static VREyeTracker Instance { get { return _instance; } }

        /// <summary>
        /// Get the IEyeTracker instance.
        /// </summary>
        public IEyeTracker EyeTrackerInterface { get { return _eyeTracker; } }

        /// <summary>
        /// Get the latest gaze data. If there are new arrivals,
        /// they will be processed before returning.
        /// </summary>
        public IVRGazeData LatestGazeData
        {
            get
            {
                if (UnprocessedGazeDataCount > 0)
                {
                    // We have more data.
                    ProcessGazeEvents();
                }

                return _latestGazeData;
            }
        }

        /// <summary>
        /// Get the latest processed processed gaze data.
        /// Don't care if there a newer one has arrived.
        /// </summary>
        public IVRGazeData LatestProcessedGazeData { get { return _latestGazeData; } }

        /// <summary>
        /// Pop and get the next gaze data object from the queue.
        /// </summary>
        public IVRGazeData NextData
        {
            get
            {
                if (_gazeDataQueue.Count < 1)
                {
                    return default(IVRGazeData);
                }

                return _gazeDataQueue.Next;
            }
        }

        /// <summary>
        /// Get the number of gaze data items left in the queue.
        /// </summary>
        public int GazeDataCount { get { return _gazeDataQueue.Count; } }

        /// <summary>
        /// Get how many unprocessed gaze data objects that are queued.
        /// </summary>
        public int UnprocessedGazeDataCount { get { return _originalGazeData.Count; } }

        /// <summary>
        /// Is the eye tracker connected?
        /// </summary>
        public bool Connected { get { return _eyeTracker != null; } }

        /// <summary>
        /// Connect or disconnect the gaze stream.
        /// </summary>
        public bool SubscribeToGazeData
        {
            get
            {
                return _subscribeToGaze;
            }

            set
            {
                _subscribeToGaze = value;
                UpdateSubscriptions();
            }
        }

        #endregion Public Properties

        #region Inspector Properties

        /// <summary>
        /// Flag to indicate if we want to subscribe to gaze data.
        /// </summary>
        [Tooltip("Checking this will subscribe to gaze at application startup.")]
        [SerializeField]
        private bool _subscribeToGaze = true;

        #endregion Inspector Properties

        #region Private Fields

        /// <summary>
        /// Static instance for easy access to this object.
        /// </summary>
        private static VREyeTracker _instance;

        /// <summary>
        /// The IEyeTracker instance.
        /// </summary>
        private IEyeTracker _eyeTracker = null;

        /// <summary>
        /// Flag to remember if we are subscribing to gaze data.
        /// </summary>
        private bool _subscribingToHMDGazeData;

        /// <summary>
        /// The eye tracker origin.
        /// </summary>
        private Transform _eyeTrackerOrigin;

        /// <summary>
        /// Max queue size for gaze data. Keep a little more than one second of gaze at 120 Hz, i.e. 130 events.
        /// </summary>
        private const int _maxGazeDataQueueSize = 130;

        /// <summary>
        /// Locked access and size management.
        /// </summary>
        private LockedQueue<HMDGazeDataEventArgs> _originalGazeData = new LockedQueue<HMDGazeDataEventArgs>(maxCount: _maxGazeDataQueueSize);

        /// <summary>
        /// Size managed queue.
        /// </summary>
        private SizedQueue<IVRGazeData> _gazeDataQueue = new SizedQueue<IVRGazeData>(maxCount: _maxGazeDataQueueSize);

        /// <summary>
        /// The list of eye tracker poses kept for each frame.
        /// Just keep roughly one second of poses at 90 fps. 100 is a nice round number.
        /// </summary>
        private PoseList _eyeTrackerOriginPoses = new PoseList(100);

        /// <summary>
        /// Hold the latest processed gaze data. Initialized to an invalid object.
        /// </summary>
        private IVRGazeData _latestGazeData = new VRGazeData();

        /// <summary>
        /// Thread for connection monitoring.
        /// </summary>
        private Thread _autoConnectThread;

        /// <summary>
        /// Lock for communication with the thread.
        /// </summary>
        private object _autoConnectLock = new object();

        /// <summary>
        /// The thread-running flag.
        /// </summary>
        private bool _autoConnectThreadRunning;

        /// <summary>
        /// Locked access to the thread-runnign flag.
        /// </summary>
        private bool AutoConnectThreadRunning
        {
            get
            {
                lock (_autoConnectLock)
                {
                    return _autoConnectThreadRunning;
                }
            }

            set
            {
                lock (_autoConnectLock)
                {
                    _autoConnectThreadRunning = value;
                }
            }
        }

        private IEyeTracker _foundEyeTracker;

        private IEyeTracker FoundEyeTracker
        {
            get
            {
                lock (_autoConnectLock)
                {
                    return _foundEyeTracker;
                }
            }

            set
            {
                lock (_autoConnectLock)
                {
                    _foundEyeTracker = value;
                }
            }
        }

        #endregion Private Fields

        #region Unity Methods

        private void Awake()
        {
            _instance = this;
        }

        private void Start()
        {
            // The eye tracker origin is not exactly in the camera position when using the SteamVR plugin in Unity.
            _eyeTrackerOrigin = VRUtility.EyeTrackerOriginVive;

            // Init autoconnect
            StartCoroutine(AutoConnectMonitoring());
        }

        private void Update()
        {
            // Save the current pose for the current time.
            _eyeTrackerOriginPoses.Add(_eyeTrackerOrigin.GetPose(EyeTrackingOperations.GetSystemTimeStamp()));

            // Check for state transitions to or from subscribing.
            UpdateSubscriptions();

            if (_subscribeToGaze)
            {
                ProcessGazeEvents();
            }
        }

        private void OnDestroy()
        {
            StopAutoConnectThread();
        }

        private void OnApplicationQuit()
        {
            EyeTrackingOperations.Terminate();
        }

        #endregion Unity Methods

        #region Private Eye Tracking Methods

        private void ProcessGazeEvents()
        {
            const int maxIterations = 20;

            var gazeData = _latestGazeData;

            for (int i = 0; i < maxIterations; i++)
            {
                var originalGaze = _originalGazeData.Next;

                // Queue empty
                if (originalGaze == null)
                {
                    break;
                }

                var bestMatchingPose = _eyeTrackerOriginPoses.GetBestMatchingPose(originalGaze.SystemTimeStamp);
                if (!bestMatchingPose.Valid)
                {
                    Debug.Log("Did not find a matching pose");
                    continue;
                }

                gazeData = new VRGazeData(originalGaze, bestMatchingPose);
                _gazeDataQueue.Next = gazeData;
            }

            var queueCount = UnprocessedGazeDataCount;
            if (queueCount > 0)
            {
                Debug.LogWarning("We didn't manage to empty the queue: " + queueCount + " items left...");
            }

            _latestGazeData = gazeData;
        }

        private IEnumerator AutoConnectMonitoring()
        {
            StartAutoConnectThread();

            while (true)
            {
                if (_eyeTracker == null && FoundEyeTracker != null)
                {
                    _eyeTracker = FoundEyeTracker;
                    FoundEyeTracker = null;
                    UpdateSubscriptions();
                    StopAutoConnectThread();
                    Debug.Log("Connected to Eye Tracker: " + _eyeTracker.SerialNumber);
                    yield break;
                }

                yield return new WaitForSeconds(0.1f);
            }
        }

        private void StartAutoConnectThread()
        {
            if (_autoConnectThread != null)
            {
                return;
            }

            _autoConnectThread = new Thread(() => {
                AutoConnectThreadRunning = true;

                while (AutoConnectThreadRunning)
                {
                    var eyeTrackers = EyeTrackingOperations.FindAllEyeTrackers();

                    foreach (var eyeTrackerEntry in eyeTrackers)
                    {
                        if (eyeTrackerEntry.SerialNumber.StartsWith("VR"))
                        {
                            FoundEyeTracker = eyeTrackerEntry;
                            AutoConnectThreadRunning = false;
                            return;
                        }
                    }

                    Thread.Sleep(200);
                }
            });

            _autoConnectThread.IsBackground = true;
            _autoConnectThread.Start();
        }

        private void StopAutoConnectThread()
        {
            if (_autoConnectThread != null)
            {
                AutoConnectThreadRunning = false;
                _autoConnectThread.Join(1000);
                _autoConnectThread = null;
            }
        }

        private void UpdateSubscriptions()
        {
            if (_eyeTracker == null)
            {
                return;
            }

            if (_subscribeToGaze && !_subscribingToHMDGazeData)
            {
                _eyeTracker.HMDGazeDataReceived += HMDGazeDataReceivedCallback;
                _subscribingToHMDGazeData = true;
            }
            else if (!_subscribeToGaze && _subscribingToHMDGazeData)
            {
                _eyeTracker.HMDGazeDataReceived -= HMDGazeDataReceivedCallback;
                _subscribingToHMDGazeData = false;
            }
        }

        private void HMDGazeDataReceivedCallback(object sender, HMDGazeDataEventArgs eventArgs)
        {
            _originalGazeData.Next = eventArgs;
        }

        #endregion Private Eye Tracking Methods
    }
}
