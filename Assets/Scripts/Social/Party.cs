using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Party : MonoBehaviour
{
    public string _name = "";
    public string _leaderId = "";
    public string _room = "";
    public bool _isInviteOnly = false;
    public List<string> _invited = new List<string>(); // Red
    public List<string> _joined = new List<string>(); // Green
    private NetworkedClient _client = null;

    void Awake()
    {
        //Instance = this;
    }

    public void InitParty(List<string> joined, List<string> invited, string leaderId, string customName, bool inviteOnly, NetworkedClient client, bool doInvite, string room)
    {
        if (customName != null && doInvite == false)
            this._name = customName;

        if (doInvite == true)
        {
            this._name = PhotonNetwork.playerName + "__" + customName + "__" + DataUtilities.GetUniqueId();
            this._joined = joined;

            foreach (string invitee in invited)
            {
                if (
                    invitee != PhotonNetwork.player.UserId &&
                    !_joined.Contains(invitee)
                    )
                    this.Invite(invitee, client);
            }
        }
        else
        {
            this._joined = joined;
            this._invited = invited;
        }

        this._room = room;
        this._leaderId = leaderId;
        this._isInviteOnly = inviteOnly;
        this._client = client;

        FindObjectOfType<InputHandler>().OnUpdatePartyList();
        FindObjectOfType<InputHandler>().OnUpdateInvitesList();
    }

    public void Invite(string inviteeId, NetworkedClient client)
    {
        if (
            string.IsNullOrEmpty(this._name) ||
            !_joined.Contains((string)PhotonNetwork.player.CustomProperties["UniqueId"]) ||
            client.Channels.Contains(_name) ||
            (this._isInviteOnly && (this._leaderId != (string)PhotonNetwork.player.CustomProperties["UniqueId"]))
        )
        {
            Debug.LogError("There was a problem while sending the party invitation");
            return;
        }

        object[] partyInvitation = new object[] { client.UserName, inviteeId, UserStatusCode.PartyInvitationRequest };

        // Send invite to inviteeId, and update channel invitedIds lists
        client.chatClient.SendPrivateMessage(inviteeId, partyInvitation);

        client.Subscribe(_name);
        client.Channels.Add(_name);

        client.chatClient.PublishMessage(_name, partyInvitation);
    }

    public void KickFromParty(string playerId)
    {
        Debug.Log(string.Format("KickFromParty()\t{0}", playerId));
        string localId = (string)PhotonNetwork.player.CustomProperties["UniqueId"];

        if (localId == _leaderId && localId != playerId)
            Debug.Log(string.Format("Kicking {0} from the party.", playerId));
        else
        {
            Debug.Log(string.Format("Unable to kick {0} from the party.", playerId));
            return;
        }

        object[] onKickFromPartyUpdate = new object[] { playerId, _name, UserStatusCode.PartyKickResponse };
        this._client.chatClient.PublishMessage(_name, onKickFromPartyUpdate);
    }

    public void LeaveParty()
    {
        Debug.Log("LeaveParty()");
        string localId = (string)PhotonNetwork.player.CustomProperties["UniqueId"];

        object[] onLeaveUpdate = new object[] { localId, _name, UserStatusCode.PartyLeaveResponse };
        _client.chatClient.PublishMessage(_name, onLeaveUpdate);
        _client.Channels.Remove(_name);

        FindObjectOfType<NetworkedSocial>()._partyInstance = null;
        FindObjectOfType<InputHandler>().OnUpdatePartyList();

        Destroy(this);
    }

    public void OnMakeLeader(string playerId)
    {
        string localId = (string)PhotonNetwork.player.CustomProperties["UniqueId"];

        object[] onMakeLeaderUpdate = new object[] { playerId, _name, UserStatusCode.PartyLeaderUpdateResponse };
        this._client.chatClient.PublishMessage(_name, onMakeLeaderUpdate);
    }

    public void MakeLeader(string playerId)
    {
        Debug.Log("MakeLeader()\t" + playerId);
        this._leaderId = playerId;
    }

    public void OnRemoveFromParty(string playerId)
    {
        Debug.Log(string.Format("OnRemoveFromParty()\t{0}", playerId));
        string localId = (string)PhotonNetwork.player.CustomProperties["UniqueId"];

        if (localId == playerId)
        {
            _client.Channels.Remove(_name);
            Destroy(this);
        }
        _joined.Remove(playerId);

        if (_leaderId == playerId)
        {
            _leaderId = _joined[0];

            object[] leaderIdUpdate = new object[] { _leaderId, _name, UserStatusCode.PartyLeaderUpdateResponse };
            this._client.chatClient.PublishMessage(_name, leaderIdUpdate);
        }
    }

    public void OnJoinPartyLobby()
    {
        if (FindObjectOfType<NetworkedManager>().Room != _room)
        {
            Debug.Log(string.Format("OnJoinPartyLobby()\t{0}", _room));
            PhotonNetwork.JoinRoom(_room);
        }
        else
            Debug.LogWarning("You are already in the same room as your party.");
    }

    public void OnJoin(string inviteeId)
    {
        NetworkedClient client = GetComponent<NetworkedClient>();

        Debug.Log("name:\t" + this._name);
        Debug.Log("client:\t" + client);
        Debug.Log("joined:\t" + this._joined.Contains((string)PhotonNetwork.player.CustomProperties["UniqueId"]));
        Debug.Log("invited:\t" + _invited[0] + " :: " + inviteeId);

        if (
            this._name == null ||
            client == null ||
            !this._joined.Contains((string)PhotonNetwork.player.CustomProperties["UniqueId"]) ||
            this._invited.Contains(inviteeId) == false
            )
        {
            Debug.Log("There was an authentication issue while adding a player to the party");
            return;
        }

        string toJoined = "";
        foreach (string id in this._joined)
        {
            toJoined += id + "%";
        }
        toJoined = toJoined.Remove(toJoined.Length - 1);

        string toInvited = "";
        foreach (string id in this._invited)
        {
            toInvited += id + "%";
        }
        toInvited = toInvited.Remove(toInvited.Length - 1);

        Debug.Log("PARTY INVITE & JOIN UPDATE\n" + toJoined + "\n" + toInvited);

        object[] onJoinUpdate = new object[] { this._name, UserStatusCode.PartyJoinResponse, this._name, toInvited, toJoined, this._leaderId, this._room, this._isInviteOnly };
        client.chatClient.SendPrivateMessage(inviteeId, onJoinUpdate);
    }

    public void OnJoinSuccess(string partyMember)
    {
        Debug.Log("OnJoinSuccess()");
        if (_invited.Contains(partyMember))
        {
            Debug.Log(partyMember + " has joined the party");
            _invited.Remove(partyMember);
            _joined.Add(partyMember);
        }
    }
}
