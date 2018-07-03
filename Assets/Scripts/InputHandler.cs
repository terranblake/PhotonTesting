using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputHandler : MonoBehaviour
{
    public InputField SaveDirectory;


    public NetworkedSocial socialActions;
    public NetworkedClient client;


    public InputField friendTextId;
    public InputField setName;


    public Text partyStatus;
    public Text id;
    public InputField idInputField;
    public Text name;
    public Text room;


    public GameObject friendsList;
    public GameObject friendPrefab;
    public List<GameObject> friends = new List<GameObject>();


    public GameObject inviteePrefab;
    public GameObject inviterPrefab;
    public List<GameObject> invites = new List<GameObject>();

    public GameObject partyInfo;
    public GameObject partiersList;
    public GameObject partierPrefab;
    public List<GameObject> partiers = new List<GameObject>();

    public Button joinPartyLobby;
    public Button leavyParty;


    public void InitInput()
    {
        SaveDirectory.text = Application.persistentDataPath + "/" + (string)PhotonNetwork.player.CustomProperties["UniqueId"];
        room.text = FindObjectOfType<NetworkedManager>().Room;
    }

    public void OnPartyKick(string memberId)
    {
        if (socialActions._partyInstance != null)
        {
            Debug.Log("KickFromParty()");
            socialActions._partyInstance.KickFromParty(memberId);
        }
        else
        {
            Debug.Log("No party associated with this player");
        }
    }

    public void OnPartyLeave()
    {
        if (socialActions._partyInstance != null)
        {
            Debug.Log("LeaveParty()");
            socialActions._partyInstance.LeaveParty();
        }
        else
        {
            Debug.Log("No party associated with this player");
            OnUpdatePartyList();
        }
    }

    public void OnPartyJoinLobby()
    {
        if (FindObjectOfType<Party>() != null)
        {
            Debug.Log("OnPartyJoinLobby()");
            socialActions.GetComponent<Party>().OnJoinPartyLobby();
        }
        else
        {
            Debug.Log("No party associated with this player");
        }
    }

    public void OnMakeLeader(string memberId)
    {
        if (FindObjectOfType<Party>() != null)
        {
            Debug.Log("OnMakeLeader()");
            socialActions.gameObject.GetComponent<Party>().OnMakeLeader(memberId);
        }
        else
        {
            Debug.Log("No party associated with this player");
        }
    }

    public void OnPartyJoin(string player)
    {
        socialActions.JoinParty(player);
        Debug.Log(string.Format("JoinParty()\nJoining {0}'s Party...", player));
    }

    public void OnPartyInvite(string player)
    {
        socialActions.OnInviteToParty(player);
        Debug.Log(string.Format("InviteToParty()\nInviting {0} to Party...", player));
    }

    public void OnAddFriend()
    {
        string friendId = friendTextId.text;

        if (string.IsNullOrEmpty(friendId))
        {
            Debug.LogError("Please provide a non-null value");
            return;
        }

        string friendName = socialActions.GetPlayerName(friendId);
        bool result = socialActions.AddFriend(friendId, friendName);

        if (result)
            OnUpdateFriendsList();
        else
            Debug.Log("Encountered a problem while adding that player");
    }

    public void OnRemoveFriend()
    {
        string friendId = friendTextId.text;

        if (string.IsNullOrEmpty(friendId))
        {
            Debug.LogError("Please provide a non-null value");
            return;
        }

        string friendName = socialActions.GetPlayerName(friendId);
        bool result = socialActions.RemoveFriend(friendId, friendName);

        if (result)
            OnUpdateFriendsList();
        else
            Debug.Log("Encountered a problem while removing that player");
    }

    public void OnSetName()
    {
        string newName = setName.text;
        bool result = socialActions.SetName(newName);

        if (result)
            name.text = newName;
    }

    public void OnUpdatePartyList()
    {
        Debug.Log("OnUpdatePartyList()");

        string localId = (string)PhotonNetwork.player.CustomProperties["UniqueId"];
        Party currentParty = socialActions._partyInstance;

        UpdatePartyInfo(currentParty, currentParty != null);
        UpdatePartiersList(currentParty, localId);
    }

    public void OnUpdateInvitesList()
    {
        int x = 0;
        foreach (GameObject obj in invites)
            Destroy(obj);

        if (client.Invites == null || client.Invites.Count == 0)
        {
            Debug.Log("Invites is null or empty");
            return;
        }

        foreach (Status invite in client.Invites)
        {
            GameObject InviteItem;
            string isInviter = invite.Owner == (string)PhotonNetwork.player.CustomProperties["UniqueId"] ? "Inviter" : "Invitee";

            if (isInviter == "Inviter")
                InviteItem = (GameObject)GameObject.Instantiate(inviterPrefab, Vector3.zero, Quaternion.identity);
            else
                InviteItem = (GameObject)GameObject.Instantiate(inviteePrefab, Vector3.zero, Quaternion.identity);

            InviteItem.gameObject.SetActive(true);

            InviteItem.transform.SetParent(GameObject.Find("Invites").transform, false);
            InviteItem.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(256.7f, -230.1f - (30f * x), 0);

            GameObject id = InviteItem.transform.Find("Id").gameObject;
            id.GetComponent<Text>().text = invite.Owner;
            id.SetActive(false);

            Button canJoin;
            if (isInviter == "Invitee")
            {
                Debug.Log("OnUpdateInvitesList()\nFrom:\t" + invite.Owner);

                canJoin = InviteItem.transform.Find("Join").GetComponent<Button>();
                canJoin.onClick.AddListener(delegate () { OnPartyJoin(invite.Owner); });
            }

            string friendName = null;
            foreach (Friend friend in client.FriendsStatus)
            {
                if (isInviter == "Invitee")
                {
                    if (friend.playerId == invite.Target)
                    {
                        friendName = friend.playerName;
                    }
                }
                else if (isInviter == "Inviter")
                {
                    friendName = friend.playerName;
                }
            }
            InviteItem.transform.Find("Name").gameObject.GetComponent<Text>().text = friendName;

            invites.Add(InviteItem);
            x++;
        }
    }

    public void OnUpdateFriendsList()
    {
        int x = 0;
        foreach (GameObject obj in friends)
            Destroy(obj);

        if (client.FriendsStatus == null || client.FriendsStatus.Count == 0)
        {
            Debug.Log("FriendsStatus is null or empty");
            return;
        }

        foreach (Friend friend in client.FriendsStatus)
        {
            GameObject FriendItem = (GameObject)GameObject.Instantiate(friendsList, Vector3.zero, Quaternion.identity);
            FriendItem.gameObject.SetActive(true);

            FriendItem.transform.SetParent(GameObject.Find("Friends").transform, false);
            FriendItem.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(0f, (-30f * x), 0);

            GameObject id = FriendItem.transform.Find("Id").gameObject;
            id.GetComponent<Text>().text = friend.playerId;
            id.SetActive(false);

            Button canJoin = FriendItem.transform.Find("Join").GetComponent<Button>();
            Button canInvite = FriendItem.transform.Find("Invite").GetComponent<Button>();

            string onlineStatus = friend.ConvertStatusCode(friend.playerStatus);

            Text friendsStatus = FriendItem.transform.Find("Status").gameObject.GetComponent<Text>();
            Text friendsPartyStatus = FriendItem.transform.Find("PartyStatus").gameObject.GetComponent<Text>();
            Text friendsName = FriendItem.transform.Find("Name").gameObject.GetComponent<Text>();
            Text friendsRoom = FriendItem.transform.Find("Room").gameObject.GetComponent<Text>();

            friendsName.text = friend.playerName;
            Debug.Log("OnUpdateFriendsList()\n" + friend.playerName + " :: " + onlineStatus);

            if (onlineStatus != "Offline")
            {
                friendsRoom.text = friend.roomId;
                friendsStatus.text = onlineStatus;
                friendsName.color = new Color(1f, 1f, 1f);

                if (friend.isInParty && friend.isInviteOnly)
                {
                    friendsPartyStatus.text = "In Party";
                    canJoin.interactable = false;

                    Party party = FindObjectOfType<Party>();
                    bool isInvitable = false;
                    if (party != null)
                    {
                        foreach (string member in FindObjectOfType<Party>()._joined)
                        {
                            if (member != friend.playerId)
                            {
                                isInvitable = true;
                                break;
                            }
                        }
                    }

                    canInvite.interactable = isInvitable;
                }
                else
                {
                    friendsPartyStatus.text = "Lobby";
                    canJoin.interactable = true;
                    canInvite.interactable = true;
                }
                canInvite.onClick.AddListener(delegate () { OnPartyInvite(friend.playerId); });
                canJoin.onClick.AddListener(delegate () { OnPartyJoin(friend.playerId); });
            }
            else
            {
                friendsRoom.text = "-";
                friendsStatus.text = "Offline";
                friendsPartyStatus.text = "-";
                friendsName.color = new Color(0.5f, 0.5f, 0.5f);

                canJoin.interactable = false;
                canInvite.interactable = false;
            }

            friends.Add(FriendItem);
            x++;
        }
    }

    GameObject thisPlayer;
    // Update is called once per frame
    void Update()
    {
        if (thisPlayer == null && FindObjectOfType<NetworkedManager>().inRoomAndInitialized == true)
            thisPlayer = GameObject.Find((string)PhotonNetwork.player.CustomProperties["UniqueId"]);

        if (socialActions == null && PhotonNetwork.inRoom == true && FindObjectOfType<NetworkedManager>().inRoomAndInitialized == true)
        {
            socialActions = thisPlayer.GetComponent<NetworkedSocial>();
            client = thisPlayer.GetComponent<NetworkedClient>();
        }

        if (client != null && client.FriendsStatus != null)
        {
            if (id.text == "Id" && name.text == "Name")
            {
                OnUpdateFriendsList();

                idInputField.text = (string)PhotonNetwork.player.CustomProperties["UniqueId"];
                name.text = PhotonNetwork.player.NickName;
                room.text = PhotonNetwork.room.Name;
            }
        }
    }

    private void UpdatePartiersList(Party currentParty, string localId)
    {
        foreach (GameObject obj in partiers)
            Destroy(obj);
        partiers = new List<GameObject>();

        if (currentParty == null || currentParty._joined.Count == 0)
        {
            Debug.Log("Party is null or no members have joined yet");
            return;
        }

        int x = 0;
        foreach (string member in currentParty._joined)
        {
            GameObject MemberItem;

            MemberItem = (GameObject)GameObject.Instantiate(partierPrefab, Vector3.zero, Quaternion.identity);
            MemberItem.gameObject.SetActive(true);

            MemberItem.transform.SetParent(partiersList.transform, false);
            MemberItem.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(-161f, -129f - (30f * x), 0);

            GameObject name = MemberItem.transform.Find("Name").gameObject;

            Button kickButton = MemberItem.transform.Find("Kick").gameObject.GetComponent<Button>();
            if (currentParty._leaderId == localId && member != localId)
            {
                kickButton.interactable = true;
                kickButton.onClick.AddListener(delegate () { OnPartyKick(member); });
            }
            else
                kickButton.interactable = false;

            Button makeLeaderButton = MemberItem.transform.Find("MakeLeader").gameObject.GetComponent<Button>();
            if (currentParty._leaderId == localId && member != localId)
            {
                makeLeaderButton.interactable = true;
                makeLeaderButton.onClick.AddListener(delegate () { OnMakeLeader(member); });
            }
            else
                makeLeaderButton.interactable = false;

            name.GetComponent<Text>().text = member.Substring(40);

            // Member names
            // Members current instance

            partiers.Add(MemberItem);
            x++;
        }
    }

    private void UpdatePartyInfo(Party currentParty, bool isInParty)
    {
        Text partyName = partyInfo.transform.Find("Name").GetComponent<Text>();
        Text partyLeader = partyInfo.transform.Find("LeaderId").GetComponent<Text>();
        Text partyRoom = partyInfo.transform.Find("Room").GetComponent<Text>();

        Button joinPartyLobby = partyInfo.transform.Find("JoinPartyLobby").GetComponent<Button>();
        Button leaveParty = partyInfo.transform.Find("LeaveParty").GetComponent<Button>();

        if (currentParty != null && currentParty._room != PhotonNetwork.room.Name)
            joinPartyLobby.interactable = true;
        else
            joinPartyLobby.interactable = false;

        leaveParty.interactable = isInParty;
        joinPartyLobby.interactable = isInParty;

        partyName.text = isInParty ? currentParty._name.Substring(0, 50) : "";
        partyLeader.text = isInParty ? currentParty._leaderId.Substring(0, 50) : "";
        partyRoom.text = isInParty ? currentParty._room : "";

        if (socialActions._partyInstance != null && socialActions._partyInstance._room == PhotonNetwork.room.Name)
            joinPartyLobby.interactable = false;
        else
            joinPartyLobby.interactable = true;
    }
}
