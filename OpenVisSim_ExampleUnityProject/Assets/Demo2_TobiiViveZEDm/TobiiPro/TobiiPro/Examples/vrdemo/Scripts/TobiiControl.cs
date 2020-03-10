//-----------------------------------------------------------------------
// Copyright © 2017 Tobii AB. All rights reserved.
//-----------------------------------------------------------------------

using System.IO;
using System.Xml;
using UnityEngine;

namespace Tobii.Research.Unity.Examples
{
    public sealed class ActiveObject
    {
        // The active GameObject.
        public GameObject HighlightedObject;

        // The previous material.
        public Material OriginalObjectMaterial;

        public ActiveObject()
        {
            HighlightedObject = null;
            OriginalObjectMaterial = null;
        }
    }

    public class TobiiControl : MonoBehaviour
    {
        // The Gaze sphere to follow the tracking.
        public GameObject _sphere;

        // The text about how to start the calibration.
        public GameObject _textCalibration;

        // The background of the text.
        public GameObject _textBackground;

        // The material to use for active objects.
        public Material _highlightMaterial;

        // The object that we hit.
        private ActiveObject _highlightInfo;

        // Whatever we need to run the calibration.
        private bool _calibratedSuccessfully;

        // When to start recording data.
        private bool _recordingData;

        // The controller component.
        private SteamVR_TrackedController _controllerRight;
        private SteamVR_TrackedController _controllerLeft;

        // Turn gaze sphere rendering on/off.
        private bool _visualizeGaze;

        // XML file to write the output.
        private XmlWriter _file;
        private XmlWriterSettings _fileSettings;

        // Quit the app.
        private bool _quitTime;

        // The Unity EyeTracker helper object.
        private VREyeTracker _eyeTracker;

        private void Start()
        {
            // Get EyeTracker unity object
            _eyeTracker = VREyeTracker.Instance;
            if (_eyeTracker == null)
            {
                Debug.Log("Failed to find eye tracker, has it been added to scene?");
            }

            _calibratedSuccessfully = false;
            _recordingData = false;
            _controllerRight = null;
            _controllerLeft = null;
            _visualizeGaze = true;
            _highlightInfo = new ActiveObject();
            _quitTime = false;
            _file = null;
            _fileSettings = null;
        }

        private void HandleTriggerClicked(object sender, ClickedEventArgs e)
        {
            if (_eyeTracker.Connected)
            {
                RunCalibration();
            }
        }

        private void RunCalibration()
        {
            if (_eyeTracker.EyeTrackerInterface.UpdateLensConfiguration())
            {
                Debug.Log("Updated lens configuration");
            }

            // Hide the sphere while calibrating.
            _sphere.SetActive(false);

            var calibrationStartResult = VRCalibration.Instance.StartCalibration(
                resultCallback: (calibrationResult) => {
                    // The calibration result is provided.
                    Debug.Log("Calibration was " + (calibrationResult ? "successful" : "unsuccessful"));

                    // Show the sphere again when the calibration is ready.
                    _sphere.SetActive(true);

                    _calibratedSuccessfully = calibrationResult;
                });

            Debug.Log("Calibration " + (calibrationStartResult ? "" : "not ") + "started");
        }

        private void HandleLeftTriggerClicked(object sender, ClickedEventArgs e)
        {
            _visualizeGaze = !_visualizeGaze;
        }

        private void HandlePadClicked(object sender, ClickedEventArgs e)
        {
            _quitTime = true;
        }

        private void Update()
        {
            if (_quitTime == true)
            {
                // Check if we need to close the result file.
                if (_file != null)
                {
                    CloseResultFile();
                }

                // And quit!
                if (!Application.isEditor)
                {
                    Application.Quit();
                }

                return;
            }

            if (_controllerRight == null)
            {
                var obj = GameObject.Find("Controller (right)");
                if (obj != null)
                {
                    _controllerRight = obj.GetComponent<SteamVR_TrackedController>();
                    _controllerRight.TriggerClicked += HandleTriggerClicked;
                    _controllerRight.PadClicked += HandlePadClicked;
                }
            }

            if (_controllerLeft == null)
            {
                var obj = GameObject.Find("Controller (left)");
                if (obj != null)
                {
                    _controllerLeft = obj.GetComponent<SteamVR_TrackedController>();
                    _controllerLeft.TriggerClicked += HandleLeftTriggerClicked;
                    _controllerLeft.PadClicked += HandlePadClicked;
                }
            }

            if (_eyeTracker.Connected)
            {
                if (Input.GetKeyDown(KeyCode.F1))
                {
                    RunCalibration();
                }

                // Check if the calibration already finish.
                if (_recordingData == false && _calibratedSuccessfully == true)
                {
                    // if that is the case, we enable the recording of
                    // of the data and attach a new callback for when
                    // new data arrive from the tracking device.
                    _recordingData = true;

                    // Open the result file.
                    OpenResultFile();
                }

                // Check if we are recording. If so, store data to file in chronological order
                if (_recordingData == true)
                {
                    while (_eyeTracker.GazeDataCount > 0)
                    {
                        // Save the information.
                        WriteResult(_eyeTracker.NextData);
                    }
                }

                // Reset any priviously set active object and remove its highlight
                if (_highlightInfo.HighlightedObject != null)
                {
                    var renderer = _highlightInfo.HighlightedObject.GetComponent<MeshRenderer>();
                    if (renderer != null)
                    {
                        renderer.material = _highlightInfo.OriginalObjectMaterial;
                    }

                    _highlightInfo.HighlightedObject = null;
                    _highlightInfo.OriginalObjectMaterial = null;
                }

                var latestData = _eyeTracker.LatestGazeData;

                if (latestData.CombinedGazeRayWorldValid)
                {
                    RaycastHit hit;
                    if (Physics.Raycast(latestData.CombinedGazeRayWorld, out hit))
                    {
                        // Update the gaze point in the scene.
                        _sphere.transform.position = hit.point;

                        if (hit.collider != null)
                        {
                            if (hit.collider.gameObject.name.StartsWith("Cube") == true || hit.collider.gameObject.name.StartsWith("Cylinder") == true)
                            {
                                MeshRenderer renderer = hit.collider.gameObject.GetComponent<MeshRenderer>();
                                if (renderer != null)
                                {
                                    _highlightInfo.HighlightedObject = hit.collider.gameObject;
                                    _highlightInfo.OriginalObjectMaterial = renderer.material;
                                    renderer.material = _highlightMaterial;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void OpenResultFile()
        {
            if (!Directory.Exists("TrackData"))
            {
                Directory.CreateDirectory("TrackData");
            }

            _fileSettings = new XmlWriterSettings();
            _fileSettings.Indent = true;
            _file = XmlWriter.Create(Path.Combine("TrackData", "data.xml"), _fileSettings);
            _file.WriteStartDocument();
            _file.WriteStartElement("Data");
        }

        private void CloseResultFile()
        {
            _file.WriteEndElement();
            _file.WriteEndDocument();
            _file.Flush();
            _file.Close();
            _file = null;
            _fileSettings = null;
        }

        private void WriteResult(IVRGazeData gazeData)
        {
            _file.WriteStartElement("Gaze");
            _file.WriteStartElement("Left");
            _file.WriteStartElement("Ray");
            _file.WriteAttributeString("valid", gazeData.Left.GazeRayWorldValid.ToString());
            _file.WriteAttributeString("timestamp", gazeData.TimeStamp.ToString());
            _file.WriteAttributeString("origin", gazeData.Left.GazeRayWorld.origin.ToString("0000.00000000"));
            _file.WriteAttributeString("direction", gazeData.Left.GazeRayWorld.direction.ToString("0000.00000000"));
            _file.WriteEndElement();
            _file.WriteEndElement();
            _file.WriteEndElement();
        }
    }
}
