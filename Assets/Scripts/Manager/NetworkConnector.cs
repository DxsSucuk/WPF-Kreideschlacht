using Photon.Pun;

namespace Network
{
    public class NetworkConnector : MonoBehaviourPunCallbacks
    {
        public void Start()
        {
            if (!PhotonNetwork.IsConnected)
                PhotonNetwork.ConnectUsingSettings();
        }
    }
}