using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

using UnityEngine.UI;

public class NetworkedSocial : MonoBehaviour
{
    public static bool isJoiningParty = false;
    public static bool hasJoinedRoom = false;
    public static string partyLeaderId;

    private string photonNetworkId = null;
    private string photonNetworkName = null;

    public List<Friend> photonFriendsList = new List<Friend>();
    public static NetworkedParty LocalPartyInstance = null;
    private string playerDataPath = "PlayerData";

    private List<FriendInfo> currentFriendsStatus = null;
    private List<RoomInfo> currentRoomsStatus = null;
    private DataUtilities dataHandler = new DataUtilities();

    void Awake()
    {
        PlayerPrefs.SetString("PlayerId", "none");
        PlayerPrefs.SetString("PlayerName", "none");

        InitIdentification();
        LoadFriendsList(playerDataPath);
        InitFriendsStatus();
        currentFriendsStatus = PhotonNetwork.Friends;

        Debug.Log("PhotonId:\t" + PhotonNetwork.player.UserId);
        Debug.Log("PlayerName:\t" + PhotonNetwork.player.NickName);

        if (photonFriendsList != null) {
            string friendList = null;

            foreach (Friend friend in photonFriendsList) {
                friendList = friendList + friend.playerName + friend.playerId + "\n";
            }

            Debug.Log(friendList);
        }
    }

    void Update()
    {
    }

    void JoinParty(string leaderId)
    {
        FriendInfo partyLeader = GetFriendStatus(leaderId);

        if (partyLeader != null && partyLeader.IsOnline && partyLeader.IsInRoom)
        {
            if (IsSuitableRoom(partyLeader.Room))
            {
                partyLeaderId = leaderId;

                PhotonNetwork.JoinRoom(partyLeader.Room);
                isJoiningParty = true;
            }
        }
    }

    void CreateParty()
    {

    }

    public void SetName(string name)
    {
        if (photonNetworkName == photonNetworkId || photonNetworkName == "none")
        {
            photonNetworkName = name;
            PhotonNetwork.playerName = photonNetworkName;
            PlayerPrefs.SetString("PlayerName", photonNetworkName);
        }
        else
            Debug.Log("Player name has already been set. Not accepting name changes at this time.");
    }

    public bool AddFriend(string id, string name)
    {
        // null-check
        if (name == null || id == null)
            return false;

        if (photonFriendsList == null)
            photonFriendsList = new List<Friend>();

        // is friend already in list
        if (photonFriendsList.Contains(new Friend(id, name)) == true)
            return false;

        //is friend online
        //string[] friend = new string[] { id };
        //if (PhotonNetwork.FindFriends(friend))
        //    return false;

        // otherwise, add to friends list
        photonFriendsList.Add(new Friend(id, name));
        Debug.Log("Saved Friends:\t" + SaveFriendsList(playerDataPath));
        return true;
    }

    bool RemoveFriend(string id)
    {
        // null-check
        if (id == null)
            return false;

        // check if friend exists in list
        foreach (Friend friend in photonFriendsList)
        {
            if (friend.playerId == id)
            {
                photonFriendsList.Remove(new Friend(friend.playerId, friend.playerName));
                return true;
            }

        }

        return false;
    }

    bool SaveFriendsList(string fileName)
    {
        if (photonFriendsList == null || photonFriendsList.Count == 0)
            return false;

        // Path.Combine combines strings into a file path
        // Application.StreamingAssets points to Assets/StreamingAssets in the Editor, and the StreamingAssets folder in a build
        DataUtilities.SaveData(photonFriendsList, fileName);

        return true;
    }

    private void LoadFriendsList(string fileName)
    {
        photonFriendsList = (List<Friend>)DataUtilities.LoadData(fileName);
    }

    private void InitFriendsStatus()
    {
        if (photonFriendsList != null && photonFriendsList.Count != 0)
        {
            string[] friends = new string[photonFriendsList.Count];
            List<string> ids = new List<string>();

            foreach (Friend friend in photonFriendsList)
            {
                ids.Add(friend.playerId);
            }

            ids.CopyTo(friends);

            PhotonNetwork.FindFriends(friends);
        }
    }

    private FriendInfo GetFriendStatus(string id)
    {
        foreach (FriendInfo info in currentFriendsStatus)
        {
            if (info.UserId == id)
                return info;
        }

        return null;
    }

    public Friend GetIdentification() {
        return new Friend(photonNetworkId, photonNetworkName);
    }

    public string GetPlayerName(string id)
    {
        if (hasJoinedRoom == true)
        {
            foreach (PhotonPlayer player in PhotonNetwork.playerList)
            {
                if (player.UserId == id)
                    return player.NickName;
            }
        }

        return null;
    }

    private bool IsSuitableRoom(string name)
    {
        foreach (RoomInfo info in currentRoomsStatus)
        {
            if (info.Name == name)
            {
                // TODO :: Thorough check for room is instance
                if (info.IsOpen && info.MaxPlayers > info.PlayerCount && name.Contains("Instance") == isJoiningParty)
                    return true;

                return false;
            }
        }

        return false;
    }

    void InitIdentification()
    {
        photonNetworkId = PlayerPrefs.GetString("PlayerId", "none");
        photonNetworkName = PlayerPrefs.GetString("PlayerName", "none");

        if (photonNetworkId == "none")
        {
            photonNetworkId = DataUtilities.GetUniqueId(); // Unique ID class
            PlayerPrefs.SetString("PlayerId", photonNetworkId);
        }

        if (PhotonNetwork.player.UserId != photonNetworkId)
            PhotonNetwork.player.UserId = photonNetworkId;

        if (photonNetworkName == "none")
        {
            Debug.Log("No player name set. Defaulting to uniqueId");
            photonNetworkName = photonNetworkId;
        }
        else
        {
            PhotonNetwork.playerName = photonNetworkName;
        }
    }
}
