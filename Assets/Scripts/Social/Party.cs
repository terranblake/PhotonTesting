using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Party : MonoBehaviour
{
    //public static Party Instance;

    public string _name = "";
    private string _leaderId = "";
    public bool _isInviteOnly = false;
    public List<string> _invited = new List<string>(); // Red
    public List<string> _joined = new List<string>(); // Green
    private NetworkedClient _client = null;
    private string _room = "";

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
            this._room = room;
        }

        this._leaderId = leaderId;
        this._isInviteOnly = inviteOnly;
        this._client = client;
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

    private string toJoined;
    private string toInvited;
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

        toJoined = "";
        foreach (string id in this._joined)
        {
            toJoined += id + ".";
        }

        toInvited = "";
        foreach (string id in this._invited)
        {
            toJoined += id + ".";
        }

        Debug.Log("PARTY INVITE & JOIN UPDATE\n" + toJoined + "\n" + toInvited);

        object[] partyJoinResponse = new object[] { this._name, UserStatusCode.PartyJoinResponse, this._name, this.toInvited, this.toJoined, this._leaderId, this._room, this._isInviteOnly };
        this._client.chatClient.SendPrivateMessage(inviteeId, partyJoinResponse);
    }

    public void OnJoinSuccess(string partyMemeber)
    {
        Debug.Log("OnJoinSuccess()");
        if (_invited.Contains(partyMemeber))
        {
            _invited.Remove(partyMemeber);
            _joined.Add(partyMemeber);
        }
    }
}
