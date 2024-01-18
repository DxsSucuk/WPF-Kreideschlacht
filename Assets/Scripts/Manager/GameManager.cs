using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviourPunCallbacks, IPunObservable, IPunOwnershipCallbacks
{
    public Slider _healthSlider;
    public TextMeshProUGUI _speed;
    public TextMeshProUGUI _mode;
    public GameObject crosshair, crosshairInactive;
    public PlayerController _playerController;
    public PlayerCam _playerCameraController;
    public PlayerMovementAdvanced _playerMovementController;

    private void Awake()
    {
        PhotonNetwork.NickName = RandomString(8);
        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            if (PhotonNetwork.GetPhotonView(PhotonNetwork.SyncViewId) == null)
            {
                _playerController = PhotonNetwork
                    .Instantiate("Prefabs/Player/Teacher", GameObject.FindWithTag("Respawn").transform.position, Quaternion.identity)
                    .GetComponent<PlayerController>();
                _healthSlider.maxValue = _playerController.maxHealth;
                
                PlayerMovementAdvanced movement = _playerController.GetComponent<PlayerMovementAdvanced>();
                movement.text_mode = _mode;
                movement.text_speed = _speed;
            }
        }
        else
        {
            if (!PhotonNetwork.IsConnected) 
                PhotonNetwork.ConnectUsingSettings();
            
            if (!PhotonNetwork.InRoom && PhotonNetwork.IsConnectedAndReady)
                PhotonNetwork.JoinRandomOrCreateRoom(null, 0, Photon.Realtime.MatchmakingMode.RandomMatching, null, null, RandomString(4));
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

    public override void OnConnectedToMaster() {
        Debug.Log("Connected to Master!");
        if (!PhotonNetwork.InRoom)
            PhotonNetwork.JoinRandomOrCreateRoom(null, 0, Photon.Realtime.MatchmakingMode.FillRoom, null, null, RandomString(4));
    }
    
    public override void OnJoinedRoom()
    {
        Debug.Log("Joined Room!");
        if (PhotonNetwork.GetPhotonView(PhotonNetwork.SyncViewId) == null)
        {
            GameObject player = PhotonNetwork
                .Instantiate("Prefabs/Player/Teacher", GameObject.FindWithTag("Respawn").transform.position, Quaternion.identity);

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
        base.OnPlayerEnteredRoom(newPlayer);
        Debug.Log(newPlayer.NickName + " joined!");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        Debug.Log(otherPlayer.NickName + " left!");
    }

    
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
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
}