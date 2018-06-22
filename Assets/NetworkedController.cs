using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkedController : Photon.MonoBehaviour {
    public string _room = "Testing";

    public static NetworkedController Instance;

    void Start()
    {
        PhotonNetwork.ConnectUsingSettings("0.1");
		Debug.Log("Connected to Photon Network.");

		PhotonNetwork.autoJoinLobby = true;

		Debug.Log(PhotonNetwork.GetRoomList());
    }

    void OnJoinedLobby()
    {
        Debug.Log("Joined Lobby.");

        RoomOptions roomOptions = new RoomOptions() { };
        PhotonNetwork.JoinOrCreateRoom(_room, roomOptions, TypedLobby.Default);
    }

    void OnJoinedRoom()
    {
        Debug.Log("Joined Room.");
        // PhotonNetwork.Instantiate("Player", Vector3.zero, Quaternion.identity, 0);
        PhotonNetwork.Instantiate("PlayerNetworked", Vector3.zero, Quaternion.identity, 0);
        // PhotonNetwork.Instantiate("NetworkedPlayerRightHand", Vector3.zero, Quaternion.identity, 0);
        // PhotonNetwork.Instantiate("NetworkedPlayerLeftHand", Vector3.zero, Quaternion.identity, 0);
        // PhotonNetwork.Instantiate("NetworkedBody", Vector3.zero, Quaternion.identity, 0);

        Debug.Log("Room:\t" + PhotonNetwork.room.Name);
    }
}
