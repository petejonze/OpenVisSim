------------------------------------------------------------------------------
 Copyright © 2017 Tobii AB. All rights reserved.
------------------------------------------------------------------------------

The following describes how to create a simple VR scene with calibration and
gaze trail rendering.

0. Create a new scene.

Points 1 - 3 are needed to enable Steam VR and Tobii eyetracking in a project.

1. Import the SteamVR package from the asset store.
2. Import the TobiiPro.SDK.Unity.Windows package.
3. Import the TobiiPro.SDK.Unity.VR package.

Points 4 - 10 show how to create a scene with calibration and a gaze trail.

4. Remove any camera in the scene (the default camera is called "Main Camera").
   When creating a scene from scratch and importing Steam VR, there will be a
   conflict with the default camera in the scene.
5. Drag and drop the "SteamVR\Prefabs\[CameraRig]" prefab into the scene.
6. Drag and drop the "TobiiPro\VR\Prefabs\[VREyeTracker]" prefab into the scene.
7. Drag and drop the "TobiiPro\VR\Prefabs\[VRCalibration]" prefab into the scene.
   Select the [VRCalibration] prefab and in the inpsector, select a key to be
   used to start a calibration.
8. Drag and drop the "TobiiPro\VR\Prefabs\[VRGazeTrail]" prefab into the scene.
9. Right click in the hierarchy and select "3D Object -> Cube". Place the cube
   at position (0, 1, 3) in the scene.

10. Play the scene.
    * Press the calibration key selected earlier to perform a calibration.
    * Look at the cube. A gaze trail should be rendered on it.
