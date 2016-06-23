using UnityEngine;
using System.Collections;

public class ClientInfo {

    private static ClientInfo _client;

    private string _clientName = "";
    private string _weaponEquipped = "";

    private ClientInfo()
    {

    }

    public static ClientInfo GetClientInfo()
    {
        if(_client == null)
        {
            _client = new ClientInfo();
        }

        return _client;

    }

    public string ClientName
    {
        get
        {
            if (_clientName.Equals(""))
                _clientName = "DefaultName";
            return _clientName;
        }

        set { _clientName = value; }
    }

    public string WeaponEquipped
    {
        get
        {
            if (_weaponEquipped.Equals(""))
                _weaponEquipped = "assault_rifle";
            return _weaponEquipped;
        }

        set { _weaponEquipped = value; }
    }

}
