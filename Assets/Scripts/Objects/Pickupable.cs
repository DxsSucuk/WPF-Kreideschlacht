using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PhotonTransformView))]
[RequireComponent(typeof(PhotonView))]
[RequireComponent(typeof(Collider))]
public class Pickupable : MonoBehaviourPun
{
    public GameObject crosshair1, crosshair2;
    public Transform cameraTrans;
    public bool interactable, pickedup;
    private Rigidbody objRigidbody;
    public float throwAmount = 500;
    public PlayerController controller;
    private ObjectAction lastAction;

    private void Awake()
    {
        objRigidbody = GetComponent<Rigidbody>();
        photonView.OwnershipTransfer = OwnershipOption.Request;
    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("MainCamera"))
        {
            crosshair1.SetActive(false);
            crosshair2.SetActive(true);
            interactable = true;
            cameraTrans = other.transform;
            controller = cameraTrans.parent.GetComponent<PlayerController>();
            controller.canShoot = false;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("MainCamera"))
        {
            if (pickedup == false)
            {
                crosshair1.SetActive(true);
                crosshair2.SetActive(false);
                interactable = false;
                controller.canShoot = true;
            }
            else
            {
                crosshair1.SetActive(true);
                crosshair2.SetActive(false);
                controller.canShoot = true;
            }
        }
    }

    void Update()
    {
        if (interactable)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                lastAction = ObjectAction.PICKUP;
                Pickup();
            }

            if (Input.GetKeyUp(KeyCode.E))
            {
                lastAction = ObjectAction.DROP;
                Drop();
            }
        }
        
        if (pickedup)
        {
            if (Input.GetMouseButtonDown(1))
            {
                lastAction = ObjectAction.THROW;
                Throw();
            }
        }
    }

    void Pickup()
    {
        if (photonView.IsMine)
        {
            Debug.Log("Sending Pickup.");
            photonView.RPC(nameof(PickupRPC), RpcTarget.All, controller.photonView.ViewID);
        }
        else
        {
            Debug.Log("Requesting Ownership.");
            photonView.RequestOwnership();
        }
    }
    
    [PunRPC]
    void PickupRPC(int viewId)
    {
        transform.parent = PhotonNetwork.GetPhotonView(viewId).transform.GetChild(0).transform;
        objRigidbody.useGravity = false;
        pickedup = true;
        interactable = false;
        objRigidbody.constraints = RigidbodyConstraints.FreezeAll;
    }
    
    void Drop()
    {
        if (photonView.IsMine)
        {
            Debug.Log("Sending Drop.");
            photonView.RPC(nameof(ResetInfo), RpcTarget.All);
        }
        else
        {
            Debug.Log("Requesting Ownership Drop.");
            photonView.RequestOwnership();
        }
        
    }

    void Throw()
    {
        if (photonView.IsMine)
        {
            Debug.Log("Sending Throw.");
            ThrowObject();
        }
        else
        {
            Debug.Log("Requesting Ownership Throw.");
            photonView.RequestOwnership();
        }
    }

    public void OnOwnershipRequest(PhotonView targetView, Player requestingPlayer)
    {
        if (!pickedup)
        {
            if (photonView.IsMine)
            {
                Debug.Log("Transfer Ownership.");
                targetView.TransferOwnership(requestingPlayer);
            }
        }
        else
        {
            Debug.Log("Is pickedup.");
        }
    }

    public void OnOwnershipTransfered(PhotonView targetView, Player previousOwner)
    {
        Debug.Log("Accept Transfered.");
        switch (lastAction)
        {
            case ObjectAction.THROW:
            {
                ThrowObject();
                break;
            }

            case ObjectAction.PICKUP:
            {
                photonView.RPC(nameof(PickupRPC), RpcTarget.All, controller.photonView.ViewID);
                break;
            }

            case ObjectAction.DROP:
            {
                photonView.RPC(nameof(ResetInfo), RpcTarget.All);
                break;
            }
        }
    }

    public void OnOwnershipTransferFailed(PhotonView targetView, Player senderOfFailedRequest)
    {
        Debug.Log("Failed Takeover");
    }

    void ThrowObject()
    {
        photonView.RPC(nameof(ResetInfo), RpcTarget.All);
        objRigidbody.velocity = controller.orientation.forward * throwAmount * Time.deltaTime;
    }
    
    [PunRPC]
    void ResetInfo()
    {
        transform.parent = null;
        objRigidbody.useGravity = true;
        objRigidbody.constraints = RigidbodyConstraints.None;
        pickedup = false;
    }
}