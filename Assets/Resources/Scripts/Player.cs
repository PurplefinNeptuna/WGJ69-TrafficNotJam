using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {
	[HideInInspector]
	public Rigidbody2D rb2d;
	public float turnSpeed;
	public float acceleration;
	public float reverseMult;
	public float maxSpeed;
	public float slideVelocity;

	private void Awake() {
		rb2d = GetComponent<Rigidbody2D>();
		turnSpeed = 0.09f;
		acceleration = 1.2f;
		maxSpeed = 1.286514914f;
		reverseMult = 0.5f;
		slideVelocity = 0f;
	}

	private void Update() {
		Vector2 heading = transform.rotation * Vector2.up;
		slideVelocity = Vector3.Cross(heading, rb2d.velocity).magnitude / heading.magnitude;
		float slideDir = Mathf.Sign(Vector2.SignedAngle(rb2d.velocity, heading));
		float angle = Vector2.Angle(rb2d.velocity, heading);
		float turnMult = Mathf.Sin(135 * (rb2d.velocity.magnitude / maxSpeed) * Mathf.Deg2Rad);
		turnMult *= angle >= 90f? - 1f : 1f;
		if (Input.GetAxisRaw("Vertical") > 0f || Input.GetButton("Fire1")) {
			rb2d.AddRelativeForce(Vector2.up * acceleration);
		}
		if (Input.GetAxisRaw("Vertical") < 0f || Input.GetButton("Fire3")) {
			rb2d.AddRelativeForce(Vector2.down * acceleration * reverseMult);
		}
		if (Input.GetAxisRaw("Horizontal") < 0f) {
			rb2d.AddTorque(turnSpeed * turnMult);
		}
		if (Input.GetAxisRaw("Horizontal") > 0f) {
			rb2d.AddTorque(-turnSpeed * turnMult);
		}
		if (angle > 5f) {
			rb2d.AddRelativeForce(4f * Vector2.left * slideDir * slideVelocity);
		}
		rb2d.velocity = Vector2.ClampMagnitude(rb2d.velocity, maxSpeed);
	}
}
