using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Friend
{
    public string playerId;
    public string playerName;
    public int playerStatus;
    public bool isInParty;
    public bool isInviteOnly;
    public string roomId;

    public Friend(string id, string name, int status, bool inParty, bool inviteOnly, string roomId)
    {
        this.playerId = id;
        this.playerName = name;
        this.playerStatus = status;
        this.isInParty = inParty;
        this.isInviteOnly = inviteOnly;
        this.roomId = roomId;
    }

    public string ConvertStatusCode(int stateCode)
    {
		string _state;

        switch (stateCode)
        {
            case 1:
                _state = "Invisible";
                break;
            case 2:
                _state = "Online";
                break;
            case 3:
                _state = "Away";
                break;
            case 4:
                _state = "Do not disturb";
                break;
            case 5:
                _state = "Looking For Game/Group";
                break;
            case 6:
                _state = "Playing";
                break;
            case 7:
                _state = "FriendRequest";
                break;
            case 8:
                _state = "FriendRequestResponse";
                break;
            case 9:
                _state = "PartyInvitationRequest";
                break;
            case 10:
                _state = "PartyJoinRequest";
                break;
            case 11:
                _state = "PartyJoinResponse";
                break;
            case 12:
                _state = "PartyJoinSuccess";
                break;
            default:
                _state = "Offline";
                break;
        }
        return _state;
    }
}