using UnityEngine;
using System.Collections;

public class PositionTarget : MonoBehaviour 
{

// Hello
void Start()
{
	Debug.Log(GameObject.Find("Sphere (1)").transform.position);
	Vector3 x = Random.onUnitSphere * 1.5f;
	Debug.Log(x);
	GameObject.Find("Sphere (1)").transform.position = x;
}
 
void Update()
{
}

}