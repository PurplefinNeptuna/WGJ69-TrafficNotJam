using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Jam : MonoBehaviour {
	public GameController Main {
		get {
			return GameController.main;
		}
	}

	private void OnTriggerEnter2D(Collider2D other) {
		if (other.gameObject.layer == LayerMask.NameToLayer("Player")) {
			Main.jamLevel += 0.25f;
			Destroy(gameObject);
		}
	}

	private void Update() {
		Vector3Int jamPos = Main.grid.WorldToCell(transform.position);
		if (!Main.CheckValid(jamPos - Main.bottomLeft)) {
			Destroy(gameObject);
		}
	}
}
