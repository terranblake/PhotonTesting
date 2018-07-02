using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UserStatusCode
{
    /// <summary>(0) Offline.</summary>
    public const int Offline = 0;
    /// <summary>(1) Be invisible to everyone. Sends no message.</summary>
    public const int Invisible = 1;
    /// <summary>(2) Online and available.</summary>
    public const int Online = 2;
    /// <summary>(3) Online but not available.</summary>
    public const int Away = 3;
    /// <summary>(4) Do not disturb.</summary>
    public const int DND = 4;
    /// <summary>(5) Looking For Game/Group. Could be used when you want to be invited or do matchmaking.</summary>
    public const int LFG = 5;
    /// <summary>(6) Could be used when in a room, playing.</summary>
    public const int Playing = 6;
    /// <summary>(6) Used when sending a friend request.</summary>
    public const int FriendRequest = 7;
    /// <summary>(6) Used when responding to a friend request.</summary>
    public const int FriendRequestResponse = 8;
    /// <summary>(6) Used when sending a party invite.</summary>
    public const int PartyInvitationRequest = 9;
    /// <summary>(6) Used when responding to a party invite.</summary>
    public const int PartyJoinRequest = 10;
    /// <summary>(6) Used when responding to a party join request.</summary>
    public const int PartyJoinResponse = 11;
    /// <summary>(6) Used when successfully joined a party.</summary>
    public const int PartyJoinSuccess = 12;
    /// <summary>(6) Used when leaving a party.</summary>
    public const int PartyLeaveResponse = 13;
    /// <summary>(6) Used when kicking a player from a party.</summary>
    public const int PartyKickResponse = 14;
    /// <summary>(6) Used when updating the party leader id.</summary>
    public const int PartyLeaderUpdateResponse = 15;
}
