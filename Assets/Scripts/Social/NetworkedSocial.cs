using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

using UnityEngine.UI;

public class NetworkedSocial : MonoBehaviour
{
    public bool isJoiningParty = false;
    public bool hasJoinedRoom = false;
    public string photonPartyToJoin = null;
    private string photonNetworkId = null;
    public string photonNetworkName = null;

    private NetworkedClient _client;
    public Party _partyInstance;

    private bool nameIsSet = false;

    void Awake()
    {
    }

    public void InitSocial()
    {
        photonNetworkId = (string)PhotonNetwork.player.CustomProperties["UniqueId"];
        photonNetworkName = PhotonNetwork.player.NickName;

        nameIsSet = photonNetworkName == photonNetworkId || photonNetworkName == "none" || photonNetworkName == "";

        _client = GetComponent<NetworkedClient>();

        Debug.Log("PhotonId:\t" + photonNetworkId);
        Debug.Log("PlayerName:\t" + photonNetworkName);
    }

    public bool SetName(string name)
    {
        if (nameIsSet)
        {
            photonNetworkName = name;
            PhotonNetwork.playerName = photonNetworkName;
            PlayerPrefs.SetString("PlayerName", photonNetworkName);
            return true;
        }
        else
            Debug.Log(string.Format("Player name has already been set.\t{0}", photonNetworkName));

        return false;
    }

    public bool AddFriend(string id, string name)
    {
        // null-check
        if (name == null || id == null)
            return false;

        if (_client.FriendsStatus == null)
            _client.FriendsStatus = new List<Friend>();

        // is friend already in list
        foreach (Friend friend in _client.FriendsStatus)
        {
            if (friend.playerId == id)
                return false;
        }

        // otherwise, add to friends list
        _client.FriendsStatus.Add(new Friend(id, name, UserStatusCode.Online, false, false, PhotonNetwork.room.Name));
        SaveFriendsList((string)PhotonNetwork.player.CustomProperties["UniqueId"]);

        // Pass to chat client to receive updates for the newly added friend
        _client.chatClient.AddFriends(new string[] { id });
        _client.chatClient.PublishMessage(PhotonNetwork.room.Name, string.Format("{0} has added a new friend!", photonNetworkId));

        return true;
    }

    public bool RemoveFriend(string id, string name)
    {
        Debug.Log("RemoveFriend()\n" + id + "\n" + name);

        // null-check
        if (name == null || id == null)
            return false;

        if (_client.FriendsStatus == null)
            _client.FriendsStatus = new List<Friend>();

        // otherwise, remove from friends list
        foreach (Friend friend in _client.FriendsStatus)
        {
            if (friend.playerId == id)
            {
                _client.FriendsStatus.Remove(friend);
                break;
            }
        }
        SaveFriendsList((string)PhotonNetwork.player.CustomProperties["UniqueId"]);

        // Pass to chat client to stop recieving updates for this player
        _client.chatClient.RemoveFriends(new string[] { id });
        return true;
    }

    void CreateParty(string creator, List<string> invited, string customName)
    {
        gameObject.AddComponent<Party>();
        _partyInstance = GetComponent<Party>();
        _partyInstance.InitParty(
            new List<string> { creator },
            invited,
            creator,
            customName,
            false,
            this._client,
            true,
            null
            );
    }

    // Request to join party
    public void JoinParty(string inviterId)
    {
        object[] partyJoinRequest = new object[] { _client.UserName, inviterId, UserStatusCode.PartyJoinRequest };
        _client.chatClient.SendPrivateMessage(inviterId, partyJoinRequest);
    }

    public void OnInviteToParty(string playerId)
    {
        if (_partyInstance == null)
        {
            Debug.Log(string.Format("{0}No Party associated with this player. We need to create one, then invite players.", (string)PhotonNetwork.player.CustomProperties["UniqueId"]));

            CreateParty(
                (string)PhotonNetwork.player.CustomProperties["UniqueId"],
                new List<string> { playerId },
                "TestingParty"
                );
        }
        else
        {
            Debug.Log("Inviting this player to the party");

            _partyInstance.Invite(playerId, this._client);
        }
    }

    // Handle response after requesting to join party
    public void OnJoinedParty(string channelName, Status partyStatus)
    {
        Debug.Log("OnJoinedParty()");
        if (gameObject.GetComponent<Party>() == null)
        {
            gameObject.AddComponent<Party>();
            _partyInstance = GetComponent<Party>();
            _partyInstance.InitParty(
                partyStatus.Joined,
                partyStatus.Invited,
                partyStatus.LeaderId,
                partyStatus.Target,
                partyStatus.InviteOnly,
                GetComponent<NetworkedClient>(),
                false,
                partyStatus.Room
                );
        }
        else
        {
            Debug.LogError("There is already a Party instance attached to this gameobject");
        }

        object[] partyJoinSuccess = new object[] { (string)PhotonNetwork.player.CustomProperties["UniqueId"], channelName, UserStatusCode.PartyJoinSuccess };

        this._client.Subscribe(channelName);
        this._client.Channels.Add(channelName);

        this._client.chatClient.PublishMessage(channelName, partyJoinSuccess);
    }

    public bool SaveFriendsList(string fileName)
    {
        if (_client.FriendsStatus == null || _client.FriendsStatus.Count == 0)
            return false;

        foreach (Friend friend in _client.FriendsStatus)
            friend.playerStatus = 0;

        // Path.Combine combines strings into a file path
        // Application.StreamingAssets points to Assets/StreamingAssets in the Editor, and the StreamingAssets folder in a build
        DataUtilities.SaveData(_client.FriendsStatus, fileName);
        Debug.Log(string.Format("Successfully saved friends to {0}.bin", (string)PhotonNetwork.player.CustomProperties["UniqueId"]));

        return true;
    }

    public void LoadFriendsList(string fileName)
    {
        if (_client.FriendsStatus == null)
            _client.FriendsStatus = (List<Friend>)DataUtilities.LoadData(fileName);
    }

    public string GetPlayerName(string id)
    {
        if (hasJoinedRoom == true)
        {
            List<string> ignoredViews = new List<string> { "Head", "Body", "Inventory" };
            foreach (PhotonView player in GameObject.Find("Player").transform.GetComponentsInChildren<PhotonView>())
            {
                string transformName = player.transform.name;
                if (!ignoredViews.Contains(transformName))
                {
                    string uniqueId = (string)player.transform.name;
                    Debug.Log(id + "\n" + uniqueId + "\n" + (id == uniqueId));
                    if (uniqueId == id)
                    {
                        return player.owner.NickName;
                    }
                }
            }
        }
        return null;
    }

    private void IsSuitableRoom(string name)
    {
        // Handled by chat client
    }

    private void LogFriends()
    {
        if (_client.FriendsStatus != null)
        {
            string friendList = null;

            foreach (Friend friend in _client.FriendsStatus)
            {
                friendList = friendList + friend.playerName + "\t" + friend.playerId + "\n";
            }
        }
    }
}
