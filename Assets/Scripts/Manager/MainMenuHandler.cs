using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuHandler : MonoBehaviourPunCallbacks
{
    public GameObject DefaultGameListEntryObject;
    public GameObject GameListObject;

    public TMP_InputField UsernameInputField;

    public override void OnEnable()
    {
        base.OnEnable();
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
    }

    void Start()
    {
        connectToServer();
        if (PlayerPrefs.HasKey("username"))
        {
            string nickname = PlayerPrefs.GetString("username");
            
            if (nickname.Length > 21 || string.IsNullOrWhiteSpace(nickname))
            {
                return;
            }

            PhotonNetwork.NickName = PlayerPrefs.GetString("username");
            UsernameInputField.text = PlayerPrefs.GetString("username");
        }
    }

    void connectToServer()
    {
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        PhotonNetwork.JoinLobby();
        RefreshGames();
    }

    string RandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[new System.Random().Next(s.Length)]).ToArray());
    }
    
    public void CreateGame()
    {
        Debug.Log("Creating Game");
        connectToServer();
        JoinGame(RandomString(5));
    }

    public void JoinGame(string lobbyId)
    {
        Debug.Log("Joining Game");
        connectToServer();

        if (string.IsNullOrWhiteSpace(lobbyId)) return;

        PhotonNetwork.JoinOrCreateRoom(lobbyId, new RoomOptions(), TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined room " + PhotonNetwork.CurrentRoom.Name);
        Debug.Log("Users in this Lobby " + PhotonNetwork.CurrentRoom.PlayerCount);
        PhotonNetwork.LoadLevel(SceneManagerHelper.ActiveSceneBuildIndex + 1);
    }
    
    public void ChangeName()
    {
        if (UsernameInputField.text.Length > 21 || string.IsNullOrWhiteSpace(UsernameInputField.text))
        {
            return;
        }
        
        PhotonNetwork.NickName = UsernameInputField.text;
        PlayerPrefs.SetString("username", UsernameInputField.text);
        PlayerPrefs.Save();
    }

    public void RefreshGames()
    {
        if (!PhotonNetwork.IsConnected)
        {
            return;
        }

        if (GameListObject is null || !GameListObject.activeSelf)
        {
            return;
        }

        if (DefaultGameListEntryObject is null)
        {
            return;
        }

        foreach (LayoutElement layoutElement in GameListObject.GetComponentsInChildren<LayoutElement>())
        {
            if (layoutElement.isActiveAndEnabled)
            {
                Destroy(layoutElement.gameObject);
            }
        }
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        if (roomList is null)
        {
            return;
        }

        RefreshGames();
        
        if (GameListObject is null || !GameListObject.activeSelf)
        {
            return;
        }

        if (DefaultGameListEntryObject is null)
        {
            return;
        }

        foreach (RoomInfo roomInfo in roomList)
        {
            GameObject currentObject = Instantiate(DefaultGameListEntryObject, GameListObject.transform);
            TMP_Text contentButton = currentObject.GetComponentInChildren<TMP_Text>();
            contentButton.text = roomInfo.Name + " " + roomInfo.PlayerCount + "/" + roomInfo.MaxPlayers;
            Button button = currentObject.GetComponent<Button>();
            button.onClick.AddListener(() => JoinGame(roomInfo.Name));
            currentObject.SetActive(true);
        }
    }
}
