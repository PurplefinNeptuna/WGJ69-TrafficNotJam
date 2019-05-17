using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StyleLevel {
	private Queue<int> sQueue;
	private int avg;
	private int total;
	public int Score {
		get {
			return avg;
		}
	}
	public StyleLevel() {
		sQueue = new Queue<int>();
		avg = 0;
		total = 0;
	}
	public void Add(int score) {
		if (sQueue.Count < 60) {
			total += score;
			sQueue.Enqueue(score);
		}
		else {
			total += (score - sQueue.Dequeue());
			sQueue.Enqueue(score);
		}
		avg = Mathf.RoundToInt((float) total / (float) sQueue.Count);
	}
}

public class Player : MonoBehaviour {
	[HideInInspector]
	public Rigidbody2D rb2d;
	public AudioSource playerSound;
	public AudioSource playerSlide;
	public bool slidePlaying;
	public bool slidePlayingByBrake;
	public float turnSpeed;
	public float acceleration;
	public float reverseMult;
	public float maxSpeed;
	public float speed;
	public float angle;
	public float slideVelocity;
	public int slideInKMH;
	public int styleRating;
	public StyleLevel styleScore;
	public float slideScoreCooldown;
	public float slideScoreCooldownMax;
	public float delta;
	public float antiSlideMultiplier;
	public float slideTreshold;

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

	public float EnginePitch {
		get {
			return 0.3f + Mathf.Min((speed * Mathf.Sqrt((1f + (Mathf.Approximately(speed, 0f) ? 0f : (slideVelocity / speed))) * (2f - Mathf.Sqrt(Mathf.Sqrt(speed / maxSpeed)))) / maxSpeed), 1.3f) * 1.1f;
		}
	}

	public float AngleRating {
		get {
			return 0.2f + 0.25f * Mathf.Min((angle / 30f), 1.5f);
		}
	}

	private void Awake() {
		rb2d = GetComponent<Rigidbody2D>();
		playerSound = GetComponents<AudioSource>() [0];
		playerSlide = GetComponents<AudioSource>() [1];
		styleScore = new StyleLevel();
		turnSpeed = 0.09f;
		acceleration = 1.2f;
		maxSpeed = 1.095920133f;
		reverseMult = 0.8f;
		slideVelocity = 0f;
		slideInKMH = 0;
		styleRating = 0;
		slideScoreCooldownMax = 1f;
		slideScoreCooldown = slideScoreCooldownMax;
		delta = 0f;
		speed = 0f;
		antiSlideMultiplier = 5f;
		slideTreshold = 60f;
		slidePlaying = false;
		slidePlayingByBrake = false;
	}

	private void Update() {
		speed = rb2d.velocity.magnitude;
		playerSound.pitch = EnginePitch;
		delta = Time.deltaTime;
		Vector2 heading = transform.rotation * Vector2.up;
		slideVelocity = Vector3.Cross(heading, rb2d.velocity).magnitude / heading.magnitude;
		slideInKMH = Mathf.RoundToInt(Main.Unit2KMH(slideVelocity));
		if (!Mathf.Approximately(Main.Unit2KMH(speed), 0f))
			styleRating = Mathf.CeilToInt((float) slideInKMH * ((float) slideInKMH / Main.Unit2KMH(speed)) * ((float) slideInKMH / slideTreshold) * Main.jamMultiplier);

		styleScore.Add(styleRating);
		slideScoreCooldown -= delta;
		if (slideScoreCooldown <= 0f) {
			slideScoreCooldown = slideScoreCooldownMax + slideScoreCooldown;
			int styleScoreNow = styleScore.Score / 10;
			Main.score += styleScoreNow;
		}

		float slideDir = Mathf.Sign(Vector2.SignedAngle(rb2d.velocity, heading));
		angle = Vector2.Angle(rb2d.velocity, heading);
		float turnMult = Mathf.Sin(135 * (rb2d.velocity.magnitude / maxSpeed) * Mathf.Deg2Rad);
		turnMult *= angle >= 90f? - 1f : 1f;
		if (Input.GetAxisRaw("Vertical") > 0f || Input.GetButton("Fire1")) {
			rb2d.AddRelativeForce(Vector2.up * Acc);
		}
		if (Input.GetAxisRaw("Vertical") < 0f || Input.GetButton("Fire3")) {
			rb2d.AddRelativeForce(Vector2.down * acceleration * reverseMult);
			if (!slidePlayingByBrake && angle <= 90f) {
				playerSlide.volume = Mathf.Max(0.45f, playerSlide.volume);
				playerSlide.Play();
				slidePlayingByBrake = true;
			}
		}
		else if (slidePlayingByBrake) {
			playerSlide.Stop();
			slidePlaying = false;
			slidePlayingByBrake = false;
		}
		if (Input.GetAxisRaw("Horizontal") < 0f) {
			rb2d.AddTorque(turnSpeed * turnMult);
		}
		if (Input.GetAxisRaw("Horizontal") > 0f) {
			rb2d.AddTorque(-turnSpeed * turnMult);
		}
		if (angle > 5f) {
			rb2d.AddRelativeForce(antiSlideMultiplier * Vector2.left * slideDir * slideVelocity);
		}
		if (angle > 10f && angle < 135) {
			if (!slidePlaying) {
				playerSlide.volume = slidePlayingByBrake? Mathf.Max(0.45f, AngleRating) : AngleRating;
				playerSlide.Play();
				slidePlaying = true;
			}
			else {
				playerSlide.volume = slidePlayingByBrake? Mathf.Max(0.45f, AngleRating) : AngleRating;
			}
		}
		else if (slidePlaying) {
			playerSlide.Stop();
			slidePlaying = false;
			slidePlayingByBrake = false;
		}
		rb2d.velocity = Vector2.ClampMagnitude(rb2d.velocity, maxSpeed);
	}
}
