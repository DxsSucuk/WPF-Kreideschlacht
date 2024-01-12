using Photon.Pun;

public class DisableIfNotMine : MonoBehaviourPun
{
    private void Awake()
    {
        if (!photonView.IsMine)
        {
            gameObject.SetActive(false);
        }
    }
}