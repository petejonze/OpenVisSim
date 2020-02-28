using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoveCursor : MonoBehaviour {
	
	// Use this for initialization
	void Start () {
	}

	// Update is called once per frame
	void Update () {
		
		// Validation (?)
		//FoveInterface2.GetFVRHeadset ().CheckEyesTracked();

		// Get convergence data
		Fove.Managed.SFVR_GazeConvergenceData convergence = FoveInterface2.GetFVRHeadset ().GetGazeConvergence ();

		// use Ray to get world space coordinate: 
		Vector3 o = new Vector3 (convergence.ray.origin.x, convergence.ray.origin.y, convergence.ray.origin.z);
		Vector3 d = new Vector3 (convergence.ray.direction.x, convergence.ray.direction.y, convergence.ray.direction.z);
		transform.position = o + d * 1f;

		//Debug.Log("x=" + transform.position.x + ",   y=" + transform.position.y);

		//Vector3 screenPos = Camera.main.WorldToScreenPoint(new Vector3(1f,1f,1f));
		//Debug.Log("target is " + screenPos.x + " pixels from the left");


		/*
		Fove.Managed.SFVR_Vec3 point = Fove.Managed.SFVR_Vec3(0.0f, 0.0f, 1.0f);
		Fove.Managed.SFVR_Vec3 normal = Fove.Managed.SFVR_Vec3(0.0f, 0.0f, -1.0f);
		Plane plane = PlaneFromPointAndNormal(point, normal);

		Fove::SFVR_Vec3 intersectionPoint;
		float dist;
		IntersectionRayPlane(convergence.ray.origin, convergence.ray.direction, plane, intersectionPoint, dist);
*/

		//transform.position = eyes.right.GetPoint(3.0f);

		/*

		//FoveInterface2.EyeRays eyes = FoveInterface2.GetEyeRays();
		RaycastHit hitLeft, hitRight;

        switch (FoveInterface.CheckEyesClosed())
        {
		case Fove.Managed.EFVR_Eye.Neither:

                Physics.Raycast(eyes.left, out hitLeft, Mathf.Infinity);
                Physics.Raycast(eyes.right, out hitRight, Mathf.Infinity);
                if (hitLeft.point != Vector3.zero && hitRight.point != Vector3.zero)
                {
                    transform.position = hitLeft.point + ((hitRight.point - hitLeft.point) / 2);
                } else
                {
                    transform.position = eyes.left.GetPoint(3.0f) + ((eyes.right.GetPoint(3.0f) - eyes.left.GetPoint(3.0f)) / 2);
                }

                break;
		case Fove.Managed.EFVR_Eye.Left:

                Physics.Raycast(eyes.right, out hitRight, Mathf.Infinity);
                if (hitRight.point != Vector3.zero) // Vector3 is non-nullable; comparing to null is always false
                {
                    transform.position = hitRight.point;
                }
                else
                {
                    transform.position = eyes.right.GetPoint(3.0f);
                }
                break;
		case Fove.Managed.EFVR_Eye.Right:  

                Physics.Raycast(eyes.left, out hitLeft, Mathf.Infinity);
                if (hitLeft.point != Vector3.zero) // Vector3 is non-nullable; comparing to null is always false
                {
                    transform.position = hitLeft.point;
                }
                else
                {
                    transform.position = eyes.left.GetPoint(3.0f);
                }
                break;
        }*/
	}
}
