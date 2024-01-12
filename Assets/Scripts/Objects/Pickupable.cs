using Photon.Pun;
using UnityEngine;

public class Pickupable : MonoBehaviourPun
{
    public GameObject crosshair1, crosshair2;
    public Transform cameraTrans;
    public bool interactable, pickedup;
    private Rigidbody objRigidbody;
    public float throwAmount;
    public PlayerController controller;

    private void Awake()
    {
        objRigidbody = GetComponent<Rigidbody>();
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
            }
            else
            {
                crosshair1.SetActive(true);
                crosshair2.SetActive(false);
            }
        }
    }

    void Update()
    {
        if (interactable)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                //Pickup();
                photonView.RPC(nameof(Pickup), RpcTarget.All, controller.photonView.ViewID);
            }

            if (Input.GetKeyUp(KeyCode.E))
            {
                //Drop();
                photonView.RPC(nameof(Drop), RpcTarget.All/*, controller.photonView.ViewID*/);
            }
        }
        
        if (pickedup)
        {
            if (Input.GetMouseButtonDown(1))
            {
                //Throw();
                photonView.RPC(nameof(Throw), RpcTarget.All, controller.photonView.ViewID);
            }
        }
    }
    
    [PunRPC]
    void Pickup(int viewId)
    {
        transform.parent = PhotonNetwork.GetPhotonView(viewId).transform.GetChild(0).transform;
        objRigidbody.useGravity = false;
        pickedup = true;
        interactable = false;
        objRigidbody.constraints = RigidbodyConstraints.FreezeAll;
    }
    
    [PunRPC]
    void Drop()
    {
        transform.parent = null;
        objRigidbody.useGravity = true;
        pickedup = false;
        objRigidbody.constraints = RigidbodyConstraints.None;
    }

    [PunRPC]
    void Throw(int viewId)
    {
        transform.parent = null;
        objRigidbody.useGravity = true;
        objRigidbody.constraints = RigidbodyConstraints.None;
        pickedup = false;
        objRigidbody.velocity = PhotonNetwork.GetPhotonView(viewId).transform.GetChild(0).transform.forward * throwAmount * Time.deltaTime;
    }
}