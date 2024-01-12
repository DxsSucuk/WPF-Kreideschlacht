using System;
using Photon.Pun;
using UnityEngine;

public class Pickupable : MonoBehaviourPun
{
    public GameObject crosshair1, crosshair2;
    public Transform cameraTrans;
    public bool interactable, pickedup;
    private Rigidbody objRigidbody;
    public float throwAmount;

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
                transform.parent = null;
                objRigidbody.useGravity = true;
                crosshair1.SetActive(true);
                crosshair2.SetActive(false);
                interactable = false;
                pickedup = false;
            }
        }
    }

    void Update()
    {
        if (interactable)
        {
            if (Input.GetMouseButtonDown(0))
            {
                photonView.RPC(nameof(Pickup), RpcTarget.All);
            }

            if (Input.GetMouseButtonUp(0))
            {
                photonView.RPC(nameof(Drop), RpcTarget.All);
            }

            if (pickedup)
            {
                if (Input.GetMouseButtonDown(1))
                {
                    photonView.RPC(nameof(Throw), RpcTarget.All);
                }
            }
        }
    }

    [PunRPC]
    void Pickup()
    {
        transform.parent = cameraTrans;
        objRigidbody.useGravity = false;
        pickedup = true;
    }
    
    [PunRPC]
    void Drop()
    {
        transform.parent = null;
        objRigidbody.useGravity = true;
        pickedup = false;
    }

    [PunRPC]
    void Throw()
    {
        transform.parent = null;
        objRigidbody.useGravity = true;
        objRigidbody.velocity = cameraTrans.forward * throwAmount * Time.deltaTime;
        pickedup = false;
    }
}