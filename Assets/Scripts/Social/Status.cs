using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

/// <summary>
/// Friend UI item used to represent the friend status as well as message.
/// It aims at showing how to share health for a friend that plays on a different room than you for example.
/// But of course the message can be anything and a lot more complex.
/// </summary>
public class Status
{
    private string _owner = "None";
    private string _target = "None";
    private string _state = "Offline";
    private int _code = 0;
    private string _room = "None";
    private bool _inParty = false;
    private bool _inviteOnly = false;
    private bool _actionable = false;

    private string _partyChannelName = "";
    private List<string> _invited;
    private List<string> _joined;
    private string _leaderId;

    // General status update
    public Status(string playerId, string target, int code, string room, bool inParty, bool inviteOnly)
    {
        Debug.Log("Status()\nGeneral Update");
        this._owner = playerId;
        this._target = target;

        this._code = code;

        this._room = room;
        this._inParty = inParty;
        this._inviteOnly = inviteOnly;

        this._actionable = false;
    }

    // Party Status Update
    public Status(string playerId, string target, int code, string partyChannelName, List<string> invited, List<string> joined, string leaderId)
    {
        Debug.Log("Status()\nParty Update");
        this._owner = playerId;
        this._target = target;

        this._code = code;

        this._partyChannelName = partyChannelName;
        this._state = this.ConvertStatusCode(code);

        this._invited = invited;
        this._joined = joined;
        this._leaderId = leaderId;
    }


    public Status(string playerId, string target, int code)
    {
        Debug.Log("Status()\nRequest Update");
        this._owner = playerId;
        this._target = target;

        this._code = code;
        this._state = this.ConvertStatusCode(code);

        this._actionable = (this._state == "Friend Request" || this._state == "Party Invite") ? true : false;
    }

    public Status(object parameters)
    {
        object[] expanded = ((IEnumerable)parameters).Cast<object>()
                                   .Select(x => x == null ? x : x.ToString())
                                   .ToArray();

        Debug.Log("Status()\n" + expanded.Length + " Parameter Update");

        if (expanded.Length == 8)
        {
            Debug.Log("Status()\n8 Parameter Party Update");
            this._owner = (string)expanded[0];
            this._code = int.Parse((string)expanded[1]);
            this._state = this.ConvertStatusCode(this._code);
            this._target = (string)expanded[2];

            string toInvited = (string)(expanded[3]);
            string[] invited = toInvited.Split('.');

            foreach (string item in invited)
                Debug.Log("PartyUpdate Status()\t" + item);

            string toJoined = (string)(expanded[4]);
            string[] joined = toInvited.Split('.');

            foreach (string item in joined)
                Debug.Log("PartyUpdate Status()\t" + item);

            this._invited = invited.ToList();
            this._joined = joined.ToList();
            this._leaderId = (string)expanded[5];
            this._room = (string)expanded[6];
            this._inviteOnly = (string)expanded[7] == "False" ? false : true;

            this._actionable = false;
        }
        if (expanded.Length == 6)
        {
            Debug.Log("Status()\n6 Parameter Status Update");
            this._owner = (string)expanded[0];
            this._target = (string)expanded[1];
            this._code = int.Parse((string)expanded[2]);
            this._state = this.ConvertStatusCode(this._code);

            this._room = (string)expanded[3];
            this._inParty = (string)expanded[4] == "False" ? false : true;
            this._inviteOnly = (string)expanded[5] == "False" ? false : true;

            this._actionable = false;

            // Specific update
        }
        else if (expanded.Length == 5)
        {
            Debug.Log("Status()\n5 Parameter Status Update");
            this._owner = (string)expanded[0];
            this._target = (string)expanded[1];
            this._code = int.Parse((string)expanded[2]);
            this._state = this.ConvertStatusCode(int.Parse((string)expanded[2]));

            this._room = (string)expanded[3];
            this._inParty = (string)expanded[4] == "False" ? false : true;

            this._actionable = false;

            // Specific update
        }
        else if (expanded.Length == 3)
        {
            Debug.Log("Status()\n3 Parameter Request Update");
            this._owner = (string)expanded[0];
            this._target = (string)expanded[1];

            this._code = int.Parse((string)expanded[2]);
            this._state = this.ConvertStatusCode(int.Parse((string)expanded[2]));

            this._actionable = (this._state == "FriendRequest" || this._state == "PartyInvitationRequest") ? true : false;
        }
    }

    public string Info
    {
        get
        {
            return this._state;
        }
    }

    public string Room
    {
        get
        {
            return this._room;
        }
    }

    public bool InParty
    {
        get
        {
            return this._inParty;
        }
    }

    public string Owner
    {
        get
        {
            return this._owner;
        }
    }

    public string Target
    {
        get
        {
            return this._target;
        }
    }

    public List<string> Invited
    {
        get
        {
            return this._invited;
        }
    }

    public List<string> Joined
    {
        get
        {
            return this._joined;
        }
    }

    public int StatusCode
    {
        get
        {
            return this._code;
        }
    }

    public string LeaderId
    {
        get
        {
            return this._leaderId;
        }
    }

    public bool InviteOnly
    {
        get
        {
            return this._inviteOnly;
        }
    }

    public string ConvertStatusCode(int stateCode)
    {
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

    public void Print()
    {
        Debug.LogWarning(
            "Owner:\t\t" + this.Owner +
            "Target:\t\t" + this.Target +
            "Info:\t\t" + this.Info +
            "Code:\t\t" + this.StatusCode +
            "Actionable:\t" + this._actionable
        );
    }

    public string Printable
    {
        get
        {
            return (
                "Owner:\t\t" + this.Owner +
                "\nTarget:\t\t" + this.Target +
                "\nInfo:\t\t" + this.Info +
                "\nCode:\t\t" + this.StatusCode +
                "\nActionable:\t" + this._actionable
                );
        }
    }
}
