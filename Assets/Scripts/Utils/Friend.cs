using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Friend {
	public string playerId;
	public string playerName;

	public Friend(string id, string name) {
		this.playerId = id;
		this.playerName = name;
	}
}