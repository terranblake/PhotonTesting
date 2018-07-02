using System;
using System.Collections.Generic;
using ExitGames.Client.Photon.Chat;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This simple Chat UI demonstrate basics usages of the Chat Api
/// </summary>
/// <remarks>
/// The ChatClient basically lets you create any number of channels.
///
/// some friends are already set in the Chat demo "DemoChat-Scene", 'Joe', 'Jane' and 'Bob', simply log with them so that you can see the status changes in the Interface
///
/// Workflow:
/// Create ChatClient, Connect to a server with your AppID, Authenticate the user (apply a unique name,)
/// and subscribe to some channels.
/// Subscribe a channel before you publish to that channel!
///
///
/// Note:
/// Don't forget to call ChatClient.Service() on Update to keep the Chatclient operational.
/// </remarks>
public class NetworkedClient : MonoBehaviour, IChatClientListener
{
    public List<string> Channels; // set in inspector. Demo channels to join automatically.
    public string[] FriendsList;
    public int HistoryLengthToFetch; // set in inspector. Up to a certain degree, previously sent messages can be fetched for context
    public string UserName;
    public ChatClient chatClient;
    public NetworkedSocial socialActions;


    public List<Status> Invites = new List<Status>();
    public List<Friend> FriendsStatus;


    public void Awake()
    {
    }

    public void Start()
    {
    }

    public void InitClient()
    {
        socialActions = GetComponent<NetworkedSocial>();
        //DontDestroyOnLoad(gameObject);

        if (string.IsNullOrEmpty(UserName))
        {
            UserName = (string)PhotonNetwork.player.CustomProperties["UniqueId"];
        }

        bool _AppIdPresent = string.IsNullOrEmpty(PhotonNetwork.PhotonServerSettings.ChatAppID);

        if (string.IsNullOrEmpty(PhotonNetwork.PhotonServerSettings.ChatAppID))
        {
            Debug.LogError("You need to set the chat app ID in the PhotonServerSettings file in order to continue.");
            return;
        }

        if (PhotonNetwork.connectedAndReady)
            Connect();
    }

    public void Connect()
    {
        this.chatClient = new ChatClient(this);
        this.chatClient.UseBackgroundWorkerForSending = true;
        this.chatClient.Connect(PhotonNetwork.PhotonServerSettings.ChatAppID, "1.0", new ExitGames.Client.Photon.Chat.AuthenticationValues(UserName));

        Debug.Log("Connecting as: " + UserName);
    }

    /// <summary>To avoid that the Editor becomes unresponsive, disconnect all Photon connections in OnDestroy.</summary>
    public void OnDestroy()
    {
        if (this.chatClient != null)
        {
            //this.chatClient.SetOnlineStatus(ChatUserStatus.Offline, new object[] { this.UserName, "OnDestroy()", 0, PhotonNetwork.room.Name, false, false });
            this.chatClient.Disconnect();
        }
    }

    /// <summary>To avoid that the Editor becomes unresponsive, disconnect all Photon connections in OnApplicationQuit.</summary>
    public void OnApplicationQuit()
    {
        if (this.chatClient != null)
        {
            socialActions.SaveFriendsList((string)PhotonNetwork.player.CustomProperties["UniqueId"]);
            Debug.Log("OnApplicationQuit()\n" + this.UserName + " :: " + PhotonNetwork.room.Name + " :: " + socialActions);
            //this.chatClient.SetOnlineStatus(ChatUserStatus.Offline, new object[] { this.UserName, "OnApplicationQuit()", 0, PhotonNetwork.room.Name, false, false });
            this.chatClient.Disconnect();
        }
    }

    public void Update()
    {
        if (this.chatClient != null)
        {
            this.chatClient.Service(); // make sure to call this regularly! it limits effort internally, so calling often is ok!
        }
    }

    public int TestLength = 2048;
    private byte[] testBytes = new byte[2048];

    /*
        public void SendChatMessage(string inputLine)
        {
            if (string.IsNullOrEmpty(inputLine))
            {
                return;
            }
            if ("test".Equals(inputLine))
            {
                if (this.TestLength != this.testBytes.Length)
                {
                    this.testBytes = new byte[this.TestLength];
                }

                this.chatClient.SendPrivateMessage(this.chatClient.AuthValues.UserId, testBytes, true);
            }

            bool doingPrivateChat = this.chatClient.PrivateChannels.ContainsKey(this.selectedChannelName);
            string privateChatTarget = string.Empty;
            if (doingPrivateChat)
            {
                // the channel name for a private conversation is (on the client!!) always composed of both user's IDs: "this:remote"
                // so the remote ID is simple to figure out

                string[] splitNames = this.selectedChannelName.Split(new char[] { ':' });
                privateChatTarget = splitNames[1];
            }
            //UnityEngine.Debug.Log("selectedChannelName: " + selectedChannelName + " doingPrivateChat: " + doingPrivateChat + " privateChatTarget: " + privateChatTarget);

            if (inputLine[0].Equals('\\'))
            {
                string[] tokens = inputLine.Split(new char[] { ' ' }, 2);
                if (tokens[0].Equals("\\state"))
                {
                    int newState = 0;

                    List<string> messages = new List<string>();
                    messages.Add("i am state " + newState);
                    string[] subtokens = tokens[1].Split(new char[] { ' ', ',' });

                    if (subtokens.Length > 0)
                    {
                        newState = int.Parse(subtokens[0]);
                    }

                    if (subtokens.Length > 1)
                    {
                        messages.Add(subtokens[1]);
                    }

                    this.chatClient.SetOnlineStatus(newState, messages.ToArray()); // this is how you set your own state and (any) message
                }
                else if ((tokens[0].Equals("\\subscribe") || tokens[0].Equals("\\s")) && !string.IsNullOrEmpty(tokens[1]))
                {
                    this.chatClient.Subscribe(tokens[1].Split(new char[] { ' ', ',' }));
                }
                else if ((tokens[0].Equals("\\unsubscribe") || tokens[0].Equals("\\u")) && !string.IsNullOrEmpty(tokens[1]))
                {
                    this.chatClient.Unsubscribe(tokens[1].Split(new char[] { ' ', ',' }));
                }
                else if (tokens[0].Equals("\\clear"))
                {
                    if (doingPrivateChat)
                    {
                        this.chatClient.PrivateChannels.Remove(this.selectedChannelName);
                    }
                    else
                    {
                        ChatChannel channel;
                        if (this.chatClient.TryGetChannel(this.selectedChannelName, doingPrivateChat, out channel))
                        {
                            channel.ClearMessages();
                        }
                    }
                }
                else if (tokens[0].Equals("\\msg") && !string.IsNullOrEmpty(tokens[1]))
                {
                    string[] subtokens = tokens[1].Split(new char[] { ' ', ',' }, 2);
                    if (subtokens.Length < 2) return;

                    string targetUser = subtokens[0];
                    string message = subtokens[1];
                    this.chatClient.SendPrivateMessage(targetUser, message);
                }
                else if ((tokens[0].Equals("\\join") || tokens[0].Equals("\\j")) && !string.IsNullOrEmpty(tokens[1]))
                {
                    string[] subtokens = tokens[1].Split(new char[] { ' ', ',' }, 2);

                    // If we are already subscribed to the channel we directly switch to it, otherwise we subscribe to it first and then switch to it implicitly
                    // if (channelToggles.ContainsKey(subtokens[0]))
                    // {
                    //     ShowChannel(subtokens[0]);
                    // }
                    // else
                    // {
                    //     this.chatClient.Subscribe(new string[] { subtokens[0] });
                    // }
                }
                else
                {
                    Debug.Log("The command '" + tokens[0] + "' is invalid.");
                }
            }
            else
            {
                if (doingPrivateChat)
                {
                    this.chatClient.SendPrivateMessage(privateChatTarget, inputLine);
                }
                else
                {
                    this.chatClient.PublishMessage(this.selectedChannelName, inputLine);
                }
            }
        }
    */
    public void DebugReturn(ExitGames.Client.Photon.DebugLevel level, string message)
    {
        if (level == ExitGames.Client.Photon.DebugLevel.ERROR)
        {
            UnityEngine.Debug.LogError(message);
        }
        else if (level == ExitGames.Client.Photon.DebugLevel.WARNING)
        {
            UnityEngine.Debug.LogWarning(message);
        }
        else
        {
            UnityEngine.Debug.Log(message);
        }
    }

    public void OnConnected()
    {
        if (this.Channels != null && this.Channels.Count > 0)
        {
            this.chatClient.Subscribe(this.Channels.ToArray(), this.HistoryLengthToFetch);
        }
        FriendsList = GetFriendsList();

        if (FriendsList != null && FriendsList.Length > 0)
        {
            this.chatClient.AddFriends(FriendsList); // Add some users to the server-list to get their status updates
        }

        if (this.UserName != null)
        {
            Debug.Log("OnConnected()\t" + PhotonNetwork.room.Name);
            this.chatClient.SetOnlineStatus(ChatUserStatus.Online, new object[] { this.UserName, "OnConnected()", 2, PhotonNetwork.room.Name, false, false });
            GameObject.Find("Player").GetComponent<InputHandler>().OnUpdateFriendsList();
        }
    }

    public void OnDisconnected()
    {
        Debug.Log("Disconnected");
    }

    public void OnChatStateChange(ChatState state)
    {
        // use OnConnected() and OnDisconnected()
        // this method might become more useful in the future, when more complex states are being used.

        Debug.Log("Local Client State:\t" + state.ToString());
    }

    public void Subscribe(string channel)
    {
        if (channel != null)
            this.chatClient.Subscribe(new string[] { channel });
    }

    public void Unsubscribe(string channel)
    {
        if (channel != null)
            this.chatClient.Subscribe(new string[] { channel });
    }

    public void OnSubscribed(string[] channels, bool[] results)
    {
        // in this demo, we simply send a message into each channel. This is NOT a must have!
        // foreach (string channel in channels)
        // {
        //     this.chatClient.PublishMessage(channel, string.Format("{0} has joined the channel", this.UserName)); // you don't HAVE to send a msg on join but you could.
        // }
    }

    private string[] GetFriendsList()
    {
        FriendsStatus = (List<Friend>)DataUtilities.LoadData((string)PhotonNetwork.player.CustomProperties["UniqueId"]);
        GameObject.Find("Player").GetComponent<InputHandler>().OnUpdateFriendsList();

        if (FriendsStatus != null)
        {
            string[] friends = new string[FriendsStatus.Count];
            List<string> ids = new List<string>();
            foreach (Friend item in FriendsStatus)
            {
                ids.Add(item.playerId);
            }

            ids.CopyTo(friends);
            return friends;
        }
        else
        {
            FriendsStatus = new List<Friend>();
            return null;
        }
    }

    public void OnUnsubscribed(string[] channels)
    {
        // foreach (string channelName in channels)
        // {
        //     if (this.channelToggles.ContainsKey(channelName))
        //     {
        //         Toggle t = this.channelToggles[channelName];
        //         Destroy(t.gameObject);

        //         this.channelToggles.Remove(channelName);

        //         Debug.Log("Unsubscribed from channel '" + channelName + "'.");

        //         // Showing another channel if the active channel is the one we unsubscribed from before
        //         if (channelName == selectedChannelName && channelToggles.Count > 0)
        //         {
        //             IEnumerator<KeyValuePair<string, Toggle>> firstEntry = channelToggles.GetEnumerator();
        //             firstEntry.MoveNext();

        //             ShowChannel(firstEntry.Current.Key);

        //             firstEntry.Current.Value.isOn = true;
        //         }
        //     }
        //     else
        //     {
        //         Debug.Log("Can't unsubscribe from channel '" + channelName + "' because you are currently not subscribed to it.");
        //     }
        // }
    }

    public void OnGetMessages(string channelName, string[] senders, object[] messages)
    {
        if (Channels.Contains(channelName))
        {
            for (int x = 0; x < senders.Length; x++)
            {
                Status update = null;
                if (messages[0] != null)
                    update = new Status(messages[0]);
                Debug.Log(string.Format("OnGetMessages()\n{0} :: {1}\n{2}", channelName, senders[x], update.Printable));

                if (update.Info == "PartyInvitationRequest")
                {
                    socialActions._partyInstance._invited.Add(update.Target);
                    this.Invites.Add(update);
                }
                else if (update.Info == "PartyKickResponse" || update.Info == "PartyLeaveResponse")
                {
                    FindObjectOfType<Party>().OnRemoveFromParty(update.Owner);
                }
                else if (update.Info == "PartyLeaderUpdateResponse")
                {
                    FindObjectOfType<Party>().MakeLeader(update.Owner);
                }
                else if (update.Info == "PartyJoinSuccess")
                {
                    string photonId = (string)PhotonNetwork.player.CustomProperties["UniqueId"];
                    foreach (Status invite in Invites)
                    {
                        bool shouldRemove = invite.Target == photonId && update.Owner == photonId || invite.Owner == photonId;

                        if (shouldRemove == true)
                        {
                            Invites.Remove(invite);
                            break;
                        }
                    }
                    FindObjectOfType<Party>().OnJoinSuccess(update.Owner);

                }
                FindObjectOfType<InputHandler>().OnUpdateInvitesList();
                FindObjectOfType<InputHandler>().OnUpdatePartyList();

            }
        }
    }

    public void OnPrivateMessage(string sender, object message, string channelName)
    {
        if (sender == (string)PhotonNetwork.player.CustomProperties["UniqueId"] == true)
        {
            Debug.Log("THIS IS OUR PRIVATE MESSAGE");
            return;
        }

        Status privateUpdate = new Status(message);
        Debug.Log(privateUpdate.Printable);
        Debug.Log(string.Format("OnPrivateMessages()\n{0} :: {1}\n{2}", sender, privateUpdate.Info, privateUpdate.Printable));
        // byte[] msgBytes = message as byte[];
        if (privateUpdate != null)
        {
            Debug.Log(privateUpdate.Info);
            string code = privateUpdate.Info;

            if (code == "PartyInvitationRequest")
            {
                // Handle when a player receives a party invite
                Invites.Add(privateUpdate);
                GameObject.Find("Player").GetComponent<InputHandler>().OnUpdateInvitesList();
            }
            else if (code == "PartyJoinRequest")
            {
                socialActions = GetComponent<NetworkedSocial>();
                Debug.Log(string.Format("PartyJoinRequest()\n{0}\n{1}\n{2}", (string)PhotonNetwork.player.CustomProperties["UniqueId"], socialActions, privateUpdate.Target));
                // Handle when a player requests to join a party
                GetComponent<Party>().OnJoin(privateUpdate.Owner);
            }
            else if (code == "PartyJoinResponse")
            {
                // Handle when a player receives party join data
                socialActions.OnJoinedParty(privateUpdate.Owner, privateUpdate);
            }
        }
    }

    /// <summary>
    /// New status of another user (you get updates for users set in your friends list).
    /// </summary>
    /// <param name="user">Name of the user.</param>
    /// <param name="status">New status of that user.</param>
    /// <param name="gotMessage">True if the status contains a message you should cache locally. False: This status update does not include a
    /// message (keep any you have).</param>
    /// <param name="message">Message that user set.</param>
    public void OnStatusUpdate(string user, int status, bool gotMessage, object message)
    {
        Status update = null;

        if (message != null)
            update = new Status(message);
        Debug.Log("OnStatusUpdate()\n" + user + " : " + status);

        foreach (Friend friend in FriendsStatus)
        {
            if (update == null && friend.playerId == user)
                friend.playerStatus = status;
            else if (update != null && update.Owner == user)
            {
                friend.playerStatus = update.StatusCode;
                friend.roomId = update.Room;
                friend.isInParty = update.InParty;
                friend.isInviteOnly = update.InviteOnly;

                Debug.Log("OnStatusUpdate()\n" + string.Format("{0} is {1}", update.Owner, update.StatusCode));
            }
        }

        Debug.Log("OnStatusUpdate()\n" + string.Format("{0} is {1}", user, status));
        GameObject.Find("Player").GetComponent<InputHandler>().OnUpdateFriendsList();
    }

    public void AddMessageToSelectedChannel(ChatChannel channel, string msg)
    {
        if (channel != null)
        {
            channel.Add("Bot", msg);
        }
    }
}