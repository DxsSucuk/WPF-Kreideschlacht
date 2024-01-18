using System;
using System.Linq;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviourPunCallbacks, IPunOwnershipCallbacks
{
    public Slider _healthSlider;
    public TextMeshProUGUI _speed;
    public TextMeshProUGUI _mode;
    public GameObject crosshair, crosshairInactive;
    public PlayerController _playerController;
    public PlayerCam _playerCameraController;
    public PlayerMovementAdvanced _playerMovementController;

    public GameObject playerUI, gameUI;
    public GameObject preGameCamera;

    public GameObject playerList;
    public GameObject defaultEntry;

    public TMP_Text status;
    public GameObject startButton;

    private void Awake()
    {
        PhotonNetwork.NickName = PlayerPrefs.GetString("username",RandomString(8));
        if (!PhotonNetwork.IsConnected || !PhotonNetwork.InRoom)
        {
            if (!PhotonNetwork.IsConnected)
                PhotonNetwork.ConnectUsingSettings();

            if (!PhotonNetwork.InRoom && PhotonNetwork.IsConnectedAndReady)
                PhotonNetwork.JoinRandomOrCreateRoom(null, 0, Photon.Realtime.MatchmakingMode.RandomMatching, null,
                    null, RandomString(4));
        }
        
        if (PhotonNetwork.InRoom)
        {
            updatePlayerList();
        }

        Pickupable[] list = FindObjectsOfType<Pickupable>();

        if (list != null && list.Length > 0)
        {
            foreach (var pickupable in list)
            {
                pickupable.crosshair1 = crosshairInactive;
                pickupable.crosshair2 = crosshair;
            }
        }
    }

    public override void OnEnable()
    {
        base.OnEnable();
        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
    }

    private void Start()
    {
        if (_playerController != null)
        {
            _healthSlider.maxValue = _playerController.maxHealth;
        }
    }

    private void Update()
    {
        if (_playerController == null) return;

        Check_Health();
    }

    void Check_Health()
    {
        _healthSlider.value = _playerController.health;
        if (_healthSlider.value == _healthSlider.maxValue)
        {
            _healthSlider.gameObject.SetActive(false);
        }
        else
        {
            _healthSlider.gameObject.SetActive(true);
        }
    }

    public void damage(PlayerController playerController, float damage)
    {
        playerController.photonView.RPC("Player_Damage", RpcTarget.All, damage);
        Debug.Log("Damage dealt to " + playerController.photonView.Owner.NickName + "! " + damage);
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master!");
        
        if (!PhotonNetwork.InRoom)
            PhotonNetwork.JoinRandomOrCreateRoom(null, 0, Photon.Realtime.MatchmakingMode.FillRoom, null, null,
                RandomString(4));
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined Room!");
        updatePlayerList();
    }

    public void SpawnPlayer()
    {
        if (PhotonNetwork.GetPhotonView(PhotonNetwork.SyncViewId) == null)
        {
            gameUI.SetActive(false);
            playerUI.SetActive(true);
            preGameCamera.SetActive(false);
            GameObject player = PhotonNetwork
                .Instantiate("Prefabs/Player/Teacher", GameObject.FindWithTag("Respawn").transform.position,
                    Quaternion.identity);

            _playerController = player.GetComponent<PlayerController>();
            _playerMovementController = player.GetComponent<PlayerMovementAdvanced>();
            _playerCameraController = player.GetComponent<PlayerCam>();

            //_playerCameraController.orientation = _playerController.orientation;
            _playerController.PlayerCameraController = _playerCameraController;

            _healthSlider.maxValue = _playerController.maxHealth;

            _playerMovementController.text_mode = _mode;
            _playerMovementController.text_speed = _speed;
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log(newPlayer.NickName + " joined!");
        base.OnPlayerEnteredRoom(newPlayer);
        updatePlayerList();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log(otherPlayer.NickName + " left!");
        base.OnPlayerLeftRoom(otherPlayer);
        updatePlayerList();
    }

    public void updatePlayerList()
    {
        foreach (LayoutElement layoutElement in playerList.GetComponentsInChildren<LayoutElement>())
        {
            if (layoutElement.isActiveAndEnabled)
            {
                Destroy(layoutElement.gameObject);
            }
        }

        foreach (var player in PhotonNetwork.PlayerList)
        {
            GameObject newEntry = Instantiate(defaultEntry, playerList.transform);
            newEntry.GetComponentInChildren<TMP_Text>().text = player.NickName;
            newEntry.SetActive(true);
        }

        if (PhotonNetwork.IsMasterClient && PhotonNetwork.PlayerList.Length >= 2)
        {
            startButton.SetActive(true);
            UpdateState("Ready");
        }
    }

    string RandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[new System.Random().Next(s.Length)]).ToArray());
    }

    void IPunOwnershipCallbacks.OnOwnershipRequest(PhotonView targetView, Player requestingPlayer)
    {
        Debug.Log("Transfer Ownership GM.");
        if (targetView.TryGetComponent(out Pickupable pickupable))
        {
            pickupable.OnOwnershipRequest(targetView, requestingPlayer);
        }
    }

    void IPunOwnershipCallbacks.OnOwnershipTransfered(PhotonView targetView, Player previousOwner)
    {
        Debug.Log("Accept Transfered GM.");
        if (targetView.TryGetComponent(out Pickupable pickupable))
        {
            pickupable.OnOwnershipTransfered(targetView, previousOwner);
        }
    }

    void IPunOwnershipCallbacks.OnOwnershipTransferFailed(PhotonView targetView, Player senderOfFailedRequest)
    {
        Debug.Log("Failed Takeover GM.");
        if (targetView.TryGetComponent(out Pickupable pickupable))
        {
            pickupable.OnOwnershipTransferFailed(targetView, senderOfFailedRequest);
        }
    }

    public void StartMatch()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        
        PhotonNetwork.RaiseEvent(EventList.START_EVENT, null, new RaiseEventOptions
        {
            Receivers = ReceiverGroup.All
        }, SendOptions.SendReliable);
    }

    void OnEvent(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code;

        if (eventCode == EventList.START_EVENT)
        {
            SpawnPlayer();
        } else if (eventCode == EventList.STATE_UPDATE_EVENT)
        {
            status.text = photonEvent.CustomData as string;
        }
    }
    
    void UpdateState(string state)
    {
        PhotonNetwork.RaiseEvent(EventList.STATE_UPDATE_EVENT, state, new RaiseEventOptions
        {
            Receivers = ReceiverGroup.All
        }, SendOptions.SendReliable);
    }
}