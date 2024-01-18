using Photon.Pun;
using UnityEngine;

public class DisableIfNotMine : MonoBehaviourPun
{
    public bool disableSelf = true;
    public Behaviour[] disableComponents;
    private void Awake()
    {
        if (!photonView.IsMine)
        {
            if (disableComponents != null && disableComponents.Length > 0)
            {
                foreach (Behaviour disableComponent in disableComponents)
                {
                    disableComponent.enabled = false;
                }
            }
            
            if (disableSelf) gameObject.SetActive(false);
        }
    }
}