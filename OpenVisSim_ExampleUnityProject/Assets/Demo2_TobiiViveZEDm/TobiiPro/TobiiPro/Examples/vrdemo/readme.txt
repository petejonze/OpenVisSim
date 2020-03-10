------------------------------------------------------------------------------
 Copyright © 2017 Tobii AB. All rights reserved.
------------------------------------------------------------------------------

Points 1 - 3 are needed to enable Steam VR and Tobii eyetracking in a project. 

1. Import the SteamVR package from the asset store.
2. Import the TobiiPro.SDK.Unity.Windows package.
3. Import the TobiiPro.SDK.Unity.VR package.


Points 4 - 7 show how to enable Steam VR in a scene

4. Remove any camera in the scene (the default camera is called "Main Camera").
   When creating a scene from scratch and importing Steam VR, there will be a
   conflict with the default camera in the scene.
5. Drag and drop the following prefabs into the scene:
	"SteamVR\Prefabs\[CameraRig]"
	"SteamVR\Prefabs\[SteamVR]"
6. Click the object "[CameraRig]\Controller (right)" GameObject and Add the
   component SteamVR_TrackedController
7. Click the object "[CameraRig]\Controller (left)" GameObject and Add the
   component SteamVR_TrackedController


Points 8 - 10 show how to enable Tobii eyetracking using the TobiiControl
package and show the gaze point on an object

8. Drag and drop the prefab "TobiiPro\Examples\VRDemo\Prefabs\TobiiControl"
   into the scene.
9. Place an object in the scene, such as a cube, and make sure it has a
   collider attached.
10. Play the game. Follow the instructions on the sign in the scene. Looking at
    the object should place the gaze point on it.


The tracking data for each session is stored in TrackData\data.xml. For each
hit, the app save: the origin, the position where the collision happens, the
distance from the origin to the collision and the time.

--

There are two example scenes that can be opened to see how to use the package:
CalibrationExample and InteractionExample.

CalibrationExample is a very basic scene. A simple room, with nothing on it.
The gaze point will collide with the walls, floor and roof.

InteractionExample is a more advanced example in which the objects in the scene
(cubes/cylinders) change colour when the user looks at them.
