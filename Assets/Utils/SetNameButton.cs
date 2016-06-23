using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SetNameButton : MonoBehaviour {

    public InputField nameField;
    public Button nameButton;

	// Use this for initialization
	void Start () {
        if (!nameField || !nameButton)
            return;

        nameButton.onClick.AddListener(() => {
            SetClientName();
        });
	}
	
    private void SetClientName()
    {
        ClientInfo.GetClientInfo().ClientName = nameField.text;
        Debug.Log("Client name: " + ClientInfo.GetClientInfo().ClientName);
    }

}
