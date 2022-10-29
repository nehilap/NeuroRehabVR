using UnityEngine;
using Mirror;

public class JoinManager
{
    public string hostIP;
    public void Join()
    {
        string host = hostIP;
        if (host == null || host.Trim() == "")
        {
            host = "localhost";
        }
        else
        {
            if (!CheckIPValid(host))
            {
                return;
            }
        }
        NetworkManager manager = GameObject.Find("GameManager").GetComponent<NetworkManager>();
        manager.networkAddress = host;
        NetworkManager.singleton.StartClient();
    }

    public bool CheckIPValid(string strIP)
    {
        //  Split string by ".", check that array length is 4
        string[] arrOctets = strIP.Split('.');
        if (arrOctets.Length != 4)
            return false;

        //Check each substring checking that parses to byte
        byte obyte = 0;
        foreach (string strOctet in arrOctets)
            if (!byte.TryParse(strOctet, out obyte))
                return false;

        return true;
    }
}

