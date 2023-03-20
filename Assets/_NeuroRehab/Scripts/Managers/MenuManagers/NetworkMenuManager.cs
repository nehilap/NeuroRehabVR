using UnityEngine;
using Mirror;

public class NetworkMenuManager : MonoBehaviour
{
	public void disconnectClient() {
		NetworkManager.singleton.StopClient();
	}
}
