using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour {
	public static UIController main;
	public TextMeshProUGUI speedText;
	public TextMeshProUGUI scoreText;
	public TextMeshProUGUI driftText;
	public Slider jamSlider;
	public List<Image> minimap;
	private Sprite arrow;
	public GameController Main {
		get {
			return GameController.main;
		}
	}

	private void Awake() {
		if (main == null) {
			main = this;
		}
		else if (main != this) {
			Destroy(gameObject);
		}

		arrow = Resources.Load<Sprite>("Sprites/Pointer");
	}

	private void Update() {
		if (Main.player != null) {
			speedText.text = (Mathf.RoundToInt(Main.Unit2KMH(Main.player.speed))).ToString("D3") + " kmh";
			scoreText.text = "score: " + Main.score.ToString("D6");
			driftText.text = "style:    " + Main.player.styleRating.ToString("D3");
			jamSlider.value = Main.jamLevel;
		}
	}

	public void UpdateMinimap(List<List<LData>> level) {
		for (int i = 0; i < 5; i++) {
			for (int j = 0; j < 5; j++) {
				Quaternion fquat = Quaternion.Euler(0, 0, 0);
				LData now = level[i][j];
				Direction dir = now.dir;
				int idx = i * 5 + j;
				if (dir != Direction.Null) {
					if (dir == Direction.Up) {
						fquat = Quaternion.Euler(0, 0, 90);
					}
					else if (dir == Direction.Right) {
						fquat = Quaternion.Euler(0, 0, 0);
					}
					else if (dir == Direction.Down) {
						fquat = Quaternion.Euler(0, 0, 270);
					}
					else if (dir == Direction.Left) {
						fquat = Quaternion.Euler(0, 0, 180);
					}
					minimap[idx].sprite = arrow;
					minimap[idx].color = idx == 12 ? Color.blue : Color.white;
				}
				else {
					minimap[idx].sprite = null;
					minimap[idx].color = Color.clear;
				}
				minimap[idx].transform.rotation = fquat;
			}
		}
	}
}
