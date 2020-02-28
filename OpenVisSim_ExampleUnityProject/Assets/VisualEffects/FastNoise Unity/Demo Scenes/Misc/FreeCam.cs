using UnityEngine;

public class FreeCam : MonoBehaviour
{
	public float moveSpeed = 12.0f;
	public float shiftSpeedModifier = 3.0f;
	public float lookSensitivty = 0.8f;
	public float verticalLookMinMax = 80f;

	Vector3 lastMouse = new Vector3();

	// Update is called once per frame
	void Update()
	{
		UpdateRotation();
		UpdateMovement();
	}

	void UpdateRotation()
	{
		if (Input.GetMouseButtonDown(0))
		{
			lastMouse = Input.mousePosition;
		}

		if (!Input.GetMouseButton(0))
			return;

		Vector3 newRotation = transform.localEulerAngles;
		newRotation.x += (lastMouse.y - Input.mousePosition.y) * lookSensitivty;
		newRotation.y += (Input.mousePosition.x - lastMouse.x) * lookSensitivty;

		if (newRotation.x > 180f)
			newRotation.x -= 360f;

		newRotation.x = Mathf.Clamp(newRotation.x, -verticalLookMinMax, verticalLookMinMax);

		transform.localEulerAngles = newRotation;

		lastMouse = Input.mousePosition;
	}

	void UpdateMovement()
	{
		float modifiedSpeed = moveSpeed;

		if (Input.GetKey(KeyCode.LeftShift))
			modifiedSpeed *= shiftSpeedModifier;

		Vector3 movement = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

		movement = transform.rotation * movement;

		if (Input.GetKey(KeyCode.Q))
			movement.y = -1f;
		else if (Input.GetKey(KeyCode.E))
			movement.y = 1f;

		movement *= modifiedSpeed * Time.deltaTime;
		transform.Translate(movement, Space.World);
	}
}
