using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkedManager : Photon.PunBehaviour
{
    public string Room = "Testing";
    public bool isTesting;
    public bool inRoomAndInitialized = false;
    public string PlayerPrefabName;
    public int PlayerId;
    public static NetworkedManager Instance;

    private GameObject instance;

    void Awake()
    {
        if (isTesting == true)
        {
            PlayerPrefs.SetString("PlayerId", "none");
            PlayerPrefs.SetString("PlayerName", "none");
        }

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

    public override void OnConnectedToPhoton()
    {
        Debug.Log("OnConnectedToPhoton()");
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("OnJoinedLobby()");
        InitIdentification();
        
        RoomOptions roomOptions = new RoomOptions() { };
        PhotonNetwork.JoinOrCreateRoom(Room, roomOptions, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        GameObject player;

        Debug.Log("OnJoinedRoom()\t" + string.Format("{0}/{1} in {2}", PhotonNetwork.room.PlayerCount, PhotonNetwork.room.MaxPlayers, PhotonNetwork.room.Name));
        PlayerId = PhotonNetwork.player.ID;

        //Debug.Log(string.Format("OnJoinedRoom()\tIs Joining Party: {0}", NetworkedSocial.isJoiningParty));
        //if (NetworkedSocial.photonPartyToJoin != null)
        //    NetworkedSocial.LocalPartyInstance = GameObject.Find(NetworkedSocial.photonPartyToJoin).GetComponent<NetworkedParty>();

        if (NetworkedPlayer.LocalPlayerInstance == null)
        {
            Debug.Log("We are Instantiating LocalPlayer from " + SceneManagerHelper.ActiveSceneName);

            // we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
            player = PhotonNetwork.Instantiate(PlayerPrefabName, new Vector3(0f, 5f, 0f), Quaternion.identity, 0);
            player.GetComponent<NetworkedClient>().Channels = new List<string> { PhotonNetwork.room.Name, "US-Global" };

            int playerId = player.GetComponent<PhotonView>().viewID;
            player.GetComponent<NetworkedActions>().NameChange(playerId, (string)PhotonNetwork.player.CustomProperties["UniqueId"]);

            ExitGames.Client.Photon.Hashtable playerData = new ExitGames.Client.Photon.Hashtable();
            playerData.Add("PlayerId", playerId);

            PhotonNetwork.player.SetCustomProperties(playerData);
        }
        else
        {
            Debug.Log("Ignoring scene load for " + SceneManagerHelper.ActiveSceneName);
        }

        Debug.Log((string)PhotonNetwork.player.CustomProperties["UniqueId"]);
        player = GameObject.Find((string)PhotonNetwork.player.CustomProperties["UniqueId"]);

        player.GetComponent<NetworkedSocial>().InitSocial();
        player.GetComponent<NetworkedClient>().InitClient();
        FindObjectOfType<InputHandler>().InitInput();

        player.GetComponent<NetworkedSocial>().hasJoinedRoom = true;
        inRoomAndInitialized = true;
    }

    public override void OnPhotonPlayerConnected(PhotonPlayer other)
    {
        Debug.Log("OnPhotonPlayerConnected()\t" + other.NickName); // not seen if you're the player connecting
        Debug.Log((string)PhotonNetwork.player.CustomProperties["UniqueId"]);
    }

    public override void OnPhotonPlayerDisconnected(PhotonPlayer other)
    {
        Debug.Log("OnPhotonPlayerDisconnected() " + other.NickName); // seen when other disconnects
        Debug.Log((string)PhotonNetwork.player.CustomProperties["UniqueId"]);
    }

    private void InitIdentification()
    {
        string photonNetworkId = PlayerPrefs.GetString("UniqueId", "none");
        string photonNetworkName = PlayerPrefs.GetString("PlayerName", "none");

        bool firstTimeLoad = (photonNetworkId == "none" || isTesting == true) ? true : false;

        if (firstTimeLoad == true)
        {
            photonNetworkId = DataUtilities.GetUniqueId();
            PlayerPrefs.SetString("UniqueId", photonNetworkId); // Unique ID class
        }

        UpdateNetworkIdentity(photonNetworkId, photonNetworkName);
    }

    private void UpdateNetworkIdentity(string networkId, string networkName)
    {
        ExitGames.Client.Photon.Hashtable playerData = new ExitGames.Client.Photon.Hashtable();
        playerData.Add("UniqueId", networkId);

        PhotonNetwork.player.SetCustomProperties(playerData);

        if (networkName == "none" || networkName == "")
            PhotonNetwork.playerName = networkId;
        else
            PhotonNetwork.playerName = networkName;
    }
}
