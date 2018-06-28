using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputHandler : MonoBehaviour
{
    public NetworkedSocial socialActions;

    public InputField addFriendId;

    public InputField setName;

    public Text id;
    public Text name;
    public Text room;
    public GameObject friendsList;
    public GameObject friendPrefab;

    public void OnAddFriend()
    {
        if (socialActions == null)
            socialActions = GetComponent<NetworkedSocial>();

        string friendId = addFriendId.text;
        bool result = socialActions.AddFriend(friendId, "socialActions.GetPlayerName(friendId)");

        if (result)
        {
            Debug.Log(socialActions.photonFriendsList.Count);
            OnUpdateFriendsList();
        }
        else
            Debug.Log("There was an issue adding that player");
    }

    public void OnSetName()
    {
        if (socialActions == null)
            socialActions = GetComponent<NetworkedSocial>();

        string newName = setName.text;
        socialActions.SetName(newName);
        Debug.Log(PhotonNetwork.playerName);
    }

    public void OnUpdateFriendsList()
    {
        if (socialActions != null)
            socialActions = GetComponent<NetworkedSocial>();

        GameObject FriendItem = Instantiate(friendsList, friendsList.transform);

        Vector3 pos = friendsList.GetComponent<RectTransform>().anchoredPosition3D;

        FriendItem.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(-10f, -43f - 20f * x, 0);
        FriendItem.GetComponent<Text>().text = friend.playerId;
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (socialActions != null && id.text == "Id" && name.text == "Name")
        {
            Friend local = socialActions.GetIdentification();

            id.text = local.playerId;
            name.text = local.playerName;
            room.text = PhotonNetwork.room.Name;
        }
    }
}
