using Mirror;

public class JoinManager {
    public string hostIP;
    public void Join(string ip) {
        if (ip == null || ip.Trim() == "") {
            hostIP = "localhost";
        }
        else {
            if (!CheckIPValid(hostIP)) {
                return;
            }
        }

        NetworkManager.singleton.networkAddress = hostIP;
        NetworkManager.singleton.StartClient();
    }

    public void Join() {
        if (!NetworkManager.singleton.networkAddress.Equals("localhost")) {
            if (!CheckIPValid(NetworkManager.singleton.networkAddress)) {
                return;
            }   
        }

        NetworkManager.singleton.StartClient();
    }

    public bool CheckIPValid(string strIP) {
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

