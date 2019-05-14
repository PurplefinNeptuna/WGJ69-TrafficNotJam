using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum Direction {
	Up,
	Down,
	Left,
	Right,
	Null
}

public class LData {
	public int num;
	public Direction dir;
	public Direction from;
	public LData() {
		num = 0;
		dir = Direction.Null;
	}
	public LData(int n, int d, int f = 4) {
		num = n;
		dir = (Direction) d;
		from = (Direction) f;
	}
}

public class GameController : MonoBehaviour {
	public static GameController main;
	public Grid grid;
	public Tilemap roadTile;
	[HideInInspector]
	public float unit2Meter;
	public Player player;
	public int score;
	public Direction lastMoveDir;
	public Vector3Int lastPlayerPos;
	public Vector3 lastPlayerPosInWorld;
	public Vector3Int lastTrackPos;
	public Vector3Int bottomLeft;
	public Dictionary<string, Tile> tiles = new Dictionary<string, Tile>();

	public List<List<LData>> level = new List<List<LData>>() {
		new List<LData>() { new LData(0, 4, 4), new LData(0, 4, 4), new LData(0, 4, 4), new LData(0, 4, 4), new LData(0, 4, 4) },
			new List<LData>() { new LData(0, 4, 4), new LData(0, 4, 4), new LData(0, 4, 4), new LData(0, 4, 4), new LData(0, 4, 4) },
			new List<LData>() { new LData(0, 4, 4), new LData(0, 4, 4), new LData(2, 0, 4), new LData(1, 0, 1), new LData(1, 0, 1) },
			new List<LData>() { new LData(0, 4, 4), new LData(0, 4, 4), new LData(0, 4, 4), new LData(0, 4, 4), new LData(0, 4, 4) },
			new List<LData>() { new LData(0, 4, 4), new LData(0, 4, 4), new LData(0, 4, 4), new LData(0, 4, 4), new LData(0, 4, 4) }
	};

	public List<List<LData>> oldLevel = new List<List<LData>>();

	private void Awake() {
		if (main == null) {
			main = this;
		}
		else if (main != this) {
			Destroy(gameObject);
		}

		Screen.SetResolution(600, 600, false);
		score = 0;
		lastTrackPos = new Vector3Int(2, 4, 0);
		bottomLeft = new Vector3Int(0, 0, 0);
		unit2Meter = 46.637624944f;
		lastMoveDir = Direction.Null;
		tiles = Resources.LoadAll<Tile>("Tiles").ToDictionary(x => x.name, x => x);
	}

	private void Start() {
		lastPlayerPos = grid.WorldToCell(player.transform.position);
		lastPlayerPosInWorld = player.transform.position;
		UIController.main.UpdateMinimap(level);
		DebugLevel(level);
	}

	private void DebugLevel(List<List<LData>> dLevel, string pre = "") {
		string log = pre + "";
		for (int j = 4; j >= 0; j--) {
			for (int i = 0; i < 5; i++) {
				if (dLevel[i][j].dir == Direction.Up)
					log += "^";
				else if (dLevel[i][j].dir == Direction.Right)
					log += ">";
				else if (dLevel[i][j].dir == Direction.Down)
					log += "v";
				else if (dLevel[i][j].dir == Direction.Left)
					log += "<";
				else
					log += "x";
				log += " ";
			}
			log += "\n";
		}
		Debug.Log(log);

	}

	private void Update() {
		if (player == null)
			return;
		Vector3Int playerPos = grid.WorldToCell(player.transform.position);
		if (playerPos != lastPlayerPos) {
			Direction movDir = GetDirection(lastPlayerPos, playerPos);
			if (CheckWay(lastMoveDir, movDir)) {
				if (GetLevelData(level, new Vector3Int(2, 2, 0) + Dir2Vec(movDir)).num == 0) {
					Destroy(player.gameObject);
					return;
				}
				else if (movDir == level[2][2].dir) {
					score++;
				}
				LData lastTrackData = GetLevelData(level, lastTrackPos);
				Direction lastTrackDir = lastTrackData.dir;
				bottomLeft += (playerPos - lastPlayerPos);
				lastTrackPos -= (playerPos - lastPlayerPos);
				lastPlayerPos = playerPos;
				lastMoveDir = movDir;
				oldLevel = CopyLevel();
				Move(movDir);
				CheckRealLastTrack(ref lastTrackPos, level);
				Debug.Log("CRLT found: " + lastTrackPos);
				level[2][2].num = 2;
				Fill(movDir);
				if (movDir == GetLevelData(level, lastTrackPos).dir) {
					Direction newRoadDir = Vec2Dir(Dir2Vec(movDir) * -1);
					while (newRoadDir == RotateLeft(RotateLeft(movDir))) {
						int idx = UnityEngine.Random.Range(0, 4);
						newRoadDir = (Direction) Enum.GetValues(typeof(Direction)).GetValue(idx);
					}
					Vector3Int newTrackPos = lastTrackPos + Dir2Vec(movDir);
					Vector3Int newRoadPos = bottomLeft + newTrackPos;
					if (newRoadDir == movDir) {
						level[newTrackPos.x][newTrackPos.y] = new LData(1, (int) newRoadDir, (int) RotateLeft(RotateLeft(movDir)));
						roadTile.SetTile(newRoadPos, tiles[GetRoadName(lastTrackDir, newRoadDir)]);
						lastTrackPos = newTrackPos;
						GetLevelData(level, lastTrackPos).dir = newRoadDir;
					}
					else if (newRoadDir == RotateLeft(movDir)) {
						level[newTrackPos.x][newTrackPos.y] = new LData(1, (int) newRoadDir, (int) RotateLeft(RotateLeft(movDir)));
						roadTile.SetTile(newRoadPos, tiles[GetRoadName(lastTrackDir, newRoadDir)]);
						lastTrackDir = newRoadDir;
						lastTrackPos = newTrackPos;
						newTrackPos = lastTrackPos + Dir2Vec(newRoadDir);
						newRoadPos = bottomLeft + newTrackPos;
						GetLevelData(level, lastTrackPos).dir = newRoadDir;
						while (newRoadDir != movDir && CheckValid(newTrackPos)) {
							int idx = UnityEngine.Random.Range(0, 2);
							newRoadDir = (Direction) new List<Direction>() {
								movDir,
								RotateLeft(movDir)
							}[idx];
							lastTrackPos = newTrackPos;
							if (newRoadDir == movDir) {
								level[newTrackPos.x][newTrackPos.y] = new LData(1, (int) newRoadDir, (int) RotateLeft(RotateLeft(lastTrackDir)));
								roadTile.SetTile(newRoadPos, tiles[GetRoadName(lastTrackDir, newRoadDir)]);
								GetLevelData(level, lastTrackPos).dir = newRoadDir;
								break;
							}
							level[newTrackPos.x][newTrackPos.y] = new LData(1, (int) newRoadDir, (int) RotateLeft(RotateLeft(lastTrackDir)));
							roadTile.SetTile(newRoadPos, tiles[GetRoadName(lastTrackDir, newRoadDir)]);
							GetLevelData(level, lastTrackPos).dir = newRoadDir;
							newTrackPos = lastTrackPos + Dir2Vec(newRoadDir);
							newRoadPos = bottomLeft + newTrackPos;
							lastTrackDir = newRoadDir;
						}
					}
					else if (newRoadDir == RotateRight(movDir)) {
						level[newTrackPos.x][newTrackPos.y] = new LData(1, (int) newRoadDir, (int) RotateLeft(RotateLeft(movDir)));
						roadTile.SetTile(newRoadPos, tiles[GetRoadName(lastTrackDir, newRoadDir)]);
						lastTrackDir = newRoadDir;
						lastTrackPos = newTrackPos;
						newTrackPos = lastTrackPos + Dir2Vec(newRoadDir);
						newRoadPos = bottomLeft + newTrackPos;
						GetLevelData(level, lastTrackPos).dir = newRoadDir;
						while (newRoadDir != movDir && CheckValid(newTrackPos)) {
							int idx = UnityEngine.Random.Range(0, 2);
							newRoadDir = (Direction) new List<Direction>() {
								movDir,
								RotateRight(movDir)
							}[idx];
							lastTrackPos = newTrackPos;
							if (newRoadDir == movDir) {
								level[newTrackPos.x][newTrackPos.y] = new LData(1, (int) newRoadDir, (int) RotateLeft(RotateLeft(lastTrackDir)));
								roadTile.SetTile(newRoadPos, tiles[GetRoadName(lastTrackDir, newRoadDir)]);
								GetLevelData(level, lastTrackPos).dir = newRoadDir;
								break;
							}
							level[newTrackPos.x][newTrackPos.y] = new LData(1, (int) newRoadDir, (int) RotateLeft(RotateLeft(lastTrackDir)));
							roadTile.SetTile(newRoadPos, tiles[GetRoadName(lastTrackDir, newRoadDir)]);
							GetLevelData(level, lastTrackPos).dir = newRoadDir;
							newTrackPos = lastTrackPos + Dir2Vec(newRoadDir);
							newRoadPos = bottomLeft + newTrackPos;
							lastTrackDir = newRoadDir;
						}
					}
				}
				Checker(ref level);
				UIController.main.UpdateMinimap(level);
			}
			else {
				player.transform.position = lastPlayerPosInWorld;
				player.rb2d.velocity = Vector2.zero;
				Vector2 barrierDir = new Vector2(Dir2Vec(movDir).x * -1, Dir2Vec(movDir).y * -1);
				player.rb2d.AddForce(barrierDir * player.acceleration * 2);
			}
		}
		else {
			lastPlayerPosInWorld = player.transform.position;
		}
	}

	public void CheckRealLastTrack(ref Vector3Int ltrackpos, List<List<LData>> lv) {
		DebugLevel(lv, "CRLT level: \n");
		Vector3Int lastValidPos = new Vector3Int(2, 2, 0);
		Vector3Int nextPos = Dir2Vec(GetLevelData(lv, lastValidPos).dir);
		LData nextValidData = GetLevelData(lv, lastValidPos + nextPos);
		while (CheckValid(lastValidPos + nextPos) && nextPos != Vector3Int.zero && nextValidData.dir != Direction.Null) {
			lastValidPos += nextPos;
			nextPos = Dir2Vec(GetLevelData(lv, lastValidPos).dir);
			nextValidData = GetLevelData(lv, lastValidPos + nextPos);
		}
		ltrackpos = lastValidPos;
	}

	public void Checker(ref List<List<LData>> lv) {
		foreach (var list in lv) {
			foreach (var item in list) {
				if (item.dir != Direction.Null && item.num == 0) {
					item.num = 1;
				}
			}
		}
	}

	public List<List<LData>> CopyLevel() {
		List<List<LData>> levelSnap = new List<List<LData>>();
		for (int i = 0; i < 5; i++) {
			levelSnap.Add(new List<LData>());
			for (int j = 0; j < 5; j++) {
				levelSnap[i].Add(new LData(level[i][j].num, (int) level[i][j].dir, (int) level[i][j].from));
			}
		}
		return levelSnap;
	}

	public LData GetLevelData(List<List<LData>> data, Vector3Int idx) {
		if (!CheckValid(idx)) return new LData(-1, 4);
		return data[idx.x][idx.y];
	}

	public void Fill(Direction dir) {
		if (dir == Direction.Up) {
			for (int i = 0; i < 5; i++) {
				Vector3Int newPos = bottomLeft + new Vector3Int(i, 4, 0);
				roadTile.SetTile(newPos, tiles["Road00"]);
			}
		}
		else if (dir == Direction.Down) {
			for (int i = 0; i < 5; i++) {
				Vector3Int newPos = bottomLeft + new Vector3Int(i, 0, 0);
				roadTile.SetTile(newPos, tiles["Road00"]);
			}
		}
		else if (dir == Direction.Left) {
			for (int i = 0; i < 5; i++) {
				Vector3Int newPos = bottomLeft + new Vector3Int(0, i, 0);
				roadTile.SetTile(newPos, tiles["Road00"]);
			}
		}
		else if (dir == Direction.Right) {
			for (int i = 0; i < 5; i++) {
				Vector3Int newPos = bottomLeft + new Vector3Int(4, i, 0);
				roadTile.SetTile(newPos, tiles["Road00"]);
			}
		}
	}

	public bool CheckWay(Direction last, Direction now) {
		if (last == RotateLeft(RotateLeft(now)))
			return false;
		return true;
	}

	public bool CheckValid(Vector3Int idx) {
		if (idx.x < 0 || idx.y < 0 || idx.x > 4 || idx.y > 4)
			return false;
		return true;
	}

	public string GetRoadName(Direction from, Direction to) {
		if ((from == Direction.Up && to == Direction.Up) ||
			(from == Direction.Down && to == Direction.Down))
			return "Road10";
		if ((from == Direction.Left && to == Direction.Left) ||
			(from == Direction.Right && to == Direction.Right))
			return "Road05";
		if ((from == Direction.Left && to == Direction.Up) ||
			(from == Direction.Down && to == Direction.Right))
			return "Road12";
		if ((from == Direction.Right && to == Direction.Up) ||
			(from == Direction.Down && to == Direction.Left))
			return "Road09";
		if ((from == Direction.Left && to == Direction.Down) ||
			(from == Direction.Up && to == Direction.Right))
			return "Road06";
		if ((from == Direction.Right && to == Direction.Down) ||
			(from == Direction.Up && to == Direction.Left))
			return "Road03";
		return "Road08";
	}

	public static Vector3Int Dir2Vec(Direction dir) {
		if (dir == Direction.Up)
			return Vector3Int.up;
		if (dir == Direction.Down)
			return Vector3Int.down;
		if (dir == Direction.Left)
			return Vector3Int.left;
		if (dir == Direction.Right)
			return Vector3Int.right;
		return Vector3Int.zero;
	}

	public static Direction RotateLeft(Direction dir) {
		if (dir == Direction.Up)
			return Direction.Left;
		if (dir == Direction.Down)
			return Direction.Right;
		if (dir == Direction.Left)
			return Direction.Down;
		if (dir == Direction.Right)
			return Direction.Up;
		return Direction.Null;
	}

	public static Direction RotateRight(Direction dir) {
		if (dir == Direction.Up)
			return Direction.Right;
		if (dir == Direction.Down)
			return Direction.Left;
		if (dir == Direction.Left)
			return Direction.Up;
		if (dir == Direction.Right)
			return Direction.Down;
		return Direction.Null;
	}

	public static Direction Vec2Dir(Vector3Int dir) {
		if (dir == Vector3Int.up)
			return Direction.Up;
		if (dir == Vector3Int.down)
			return Direction.Down;
		if (dir == Vector3Int.left)
			return Direction.Left;
		if (dir == Vector3Int.right)
			return Direction.Right;
		return Direction.Null;
	}

	public static Direction GetDirection(Vector3Int from, Vector3Int to) {
		return Vec2Dir(to - from);
	}

	public void Move(Direction dir) {
		if (dir == Direction.Up) {
			foreach (var list in level) {
				list.RemoveAt(0);
				list.Add(new LData(0, (int) Direction.Null));
			}
		}
		else if (dir == Direction.Right) {
			level.RemoveAt(0);
			List<LData> newChunk = new List<LData>();
			for (int i = 0; i < 5; i++) {
				newChunk.Add(new LData(0, (int) Direction.Null));
			}
			level.Add(newChunk);
		}
		else if (dir == Direction.Down) {
			foreach (var list in level) {
				list.RemoveAt(4);
				list.Insert(0, new LData(0, (int) Direction.Null));
			}
		}
		else if (dir == Direction.Left) {
			level.RemoveAt(4);
			List<LData> newChunk = new List<LData>();
			for (int i = 0; i < 5; i++) {
				newChunk.Add(new LData(0, (int) Direction.Null));
			}
			level.Insert(0, newChunk);
		}
	}
}
