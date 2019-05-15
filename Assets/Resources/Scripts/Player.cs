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
	public float speed;
	public float slideVelocity;
	public int slideInKMH;
	public int slideScore;
	public int ntick;
	public float slideScoreCooldown;
	public float slideScoreCooldownMax;
	public float delta;
	
	public GameController Main {
		get {
			return GameController.main;
		}
	}

	public float Acc {
		get {
			return acceleration * Mathf.Sin((40f + (speed / maxSpeed) * 90f) * Mathf.Deg2Rad);
		}
	}

	private void Awake() {
		rb2d = GetComponent<Rigidbody2D>();
		turnSpeed = 0.09f;
		acceleration = 1.2f;
		maxSpeed = 1.286514914f;
		reverseMult = 0.5f;
		slideVelocity = 0f;
		slideInKMH = 0;
		ntick = 0;
		slideScore = 0;
		slideScoreCooldownMax = 1f;
		slideScoreCooldown = slideScoreCooldownMax;
		delta = 0f;
		speed = 0f;
	}

	private void Update() {
		speed = rb2d.velocity.magnitude;
		delta = Time.deltaTime;
		Vector2 heading = transform.rotation * Vector2.up;
		slideVelocity = Vector3.Cross(heading, rb2d.velocity).magnitude / heading.magnitude;
		slideInKMH = Mathf.RoundToInt(Main.Unit2KMH(slideVelocity));

		ntick++;
		slideScore += slideInKMH;
		slideScoreCooldown -= delta;
		if (slideScoreCooldown <= 0f) {
			slideScoreCooldown = slideScoreCooldownMax + slideScoreCooldown;
			Main.score += (slideScore / (20 * ntick));
			slideScore = 0;
			Debug.Log(ntick);
			ntick = 0;
		}

		float slideDir = Mathf.Sign(Vector2.SignedAngle(rb2d.velocity, heading));
		float angle = Vector2.Angle(rb2d.velocity, heading);
		float turnMult = Mathf.Sin(135 * (rb2d.velocity.magnitude / maxSpeed) * Mathf.Deg2Rad);
		turnMult *= angle >= 90f? - 1f : 1f;
		if (Input.GetAxisRaw("Vertical") > 0f || Input.GetButton("Fire1")) {
			rb2d.AddRelativeForce(Vector2.up * Acc);
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
			rb2d.AddRelativeForce(3f * Vector2.left * slideDir * slideVelocity);
		}
		rb2d.velocity = Vector2.ClampMagnitude(rb2d.velocity, maxSpeed);
	}
}
