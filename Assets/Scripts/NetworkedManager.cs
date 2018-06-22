using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkedManager : Photon.PunBehaviour
{
    public string Room = "Testing";
    public string PlayerPrefabName;
    public int PlayerId;
    public static NetworkedManager Instance;

    private GameObject instance;

    void Awake()
    {
        // #Critical
        // we join the lobby automatically
        PhotonNetwork.autoJoinLobby = true;

        // #Critical
        // this makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically
        PhotonNetwork.automaticallySyncScene = false;
        PhotonNetwork.ConnectUsingSettings("0.1");
    }

    void Start()
    {
        Instance = this;
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("OnJoinedLobby()");

        RoomOptions roomOptions = new RoomOptions() { };
        PhotonNetwork.JoinOrCreateRoom(Room, roomOptions, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("OnJoinedRoom()\t" + string.Format("{0}/{1} in {2}", PhotonNetwork.room.PlayerCount, PhotonNetwork.room.MaxPlayers, PhotonNetwork.room.Name));
        PlayerId = PhotonNetwork.player.ID;
        
        if (NetworkedPlayer.LocalPlayerInstance == null)
        {
            Debug.Log("We are Instantiating LocalPlayer from " + SceneManagerHelper.ActiveSceneName);

            // we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
            PhotonNetwork.Instantiate(PlayerPrefabName, new Vector3(0f, 5f, 0f), Quaternion.identity, 0);
        }
        else
        {
            Debug.Log("Ignoring scene load for " + SceneManagerHelper.ActiveSceneName);
        }
    }

    public override void OnPhotonPlayerConnected(PhotonPlayer other)
    {
        Debug.Log("OnPhotonPlayerConnected()\t" + other.NickName); // not seen if you're the player connecting
        Debug.Log(PlayerId);
    }

    public override void OnPhotonPlayerDisconnected(PhotonPlayer other)
    {
        Debug.Log("OnPhotonPlayerDisconnected() " + other.NickName); // seen when other disconnects
        Debug.Log(PlayerId);
    }
}
