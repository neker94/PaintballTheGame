using UnityEngine;
using System.Collections;
using UnityEngine.UI;


[RequireComponent(typeof(Button))]
public class SetWeponButton : MonoBehaviour {

    public string weaponName = "";

    private Button _button;

    // Use this for initialization
    void Start()
    {

        _button = GetComponent<Button>();

        if (!_button)
        {
            Debug.Log("Error, requires button");
            Destroy(this);
        }
        

        _button.onClick.AddListener(() =>
        {
            ClientInfo.GetClientInfo().WeaponEquipped = weaponName;
        });
    }
}
