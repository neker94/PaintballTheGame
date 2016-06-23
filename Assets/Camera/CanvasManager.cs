using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CanvasManager : MonoBehaviour {

    public Camera camera;
    public GameManager gameManager;

    public GameObject clientInfoObject;
    public GameObject[] weaponSelectionButtons;

    public GameObject playerHudObject;
    public RawImage crosshair;
    public Text ammoText;

    public GameObject matchInfo;
    public Text scoreText;

    public Text messageText;
    //public float messageLifeSpan;
    private float _messageTime;

    private PlayerInfo _localPlayer;

    public InputField clientName;
    public Button setNameButton;

    private bool _updateCanvas = true;

    // Use this for initialization
    void Start () {
	    if(!gameManager)
        {
            Debug.Log("ERROR: needs a game manager");
            Destroy(this);
        }

        if (!camera)
        {
            Debug.Log("ERROR: needs a camera");
            Destroy(this);
        }

    }
	
	// Update is called once per frame
	void LateUpdate () {
        if (!_updateCanvas)
            return;

        if (!_localPlayer)
            GetLocalPlayer();

        if (!_localPlayer)
        {
           // if (!clientInfoObject.activeInHierarchy)
            EnableClientInfo(true);
            EnableWeaponSelection(true);


            EnableHUD(false);
            EnableMatchInfo(false);
            return;
        }
        else
        {
            EnableMatchInfo(true);

            if (_localPlayer.Dead)
            {
                EnableHUD(false); //Sustituir aquí por pantalla de muerte, esconder vida restante, munición y mirilla
                EnableWeaponSelection(true);
            }
            else
            {
                EnableWeaponSelection(false);
                EnableHUD(true);
                UpdatePlayerInfo();
            }
            if (clientInfoObject.activeInHierarchy)
            {
                EnableClientInfo(false);
                EnableWeaponSelection(false);
            }

            if(gameManager.Match && gameManager.Match.MatchState == 0)
            {
                EnableHUD(false);
            }

            UpdateMatchInfo();
        }
	}
    
    private void EnableHUD(bool enable)
    {
        if(crosshair.enabled != enable)
        {
            crosshair.enabled = enable;
            ammoText.enabled = enable;
        }
    }

    private void EnableClientInfo(bool enable)
    {
        if(clientInfoObject.activeInHierarchy != enable)
            clientInfoObject.SetActive(enable);
    }

    private void EnableMatchInfo(bool enable)
    {
        if(scoreText.enabled != enable)
        {
            scoreText.enabled = enable;
            messageText.enabled = enable;
        }
    }

    private void EnableWeaponSelection(bool enable)
    {
        if(weaponSelectionButtons.Length > 0 && weaponSelectionButtons[0].activeInHierarchy != enable)
        {
            foreach(GameObject button in weaponSelectionButtons)
            {
                button.SetActive(enable);
            }
        }
    }

    private void UpdatePlayerInfo()
    {
        Vector3 crosshairScreenPos = camera.WorldToScreenPoint(_localPlayer.BulletImpactPredicted);
        if(crosshairScreenPos.x >= Mathf.Infinity || float.IsNaN(crosshairScreenPos.x) ||
            crosshairScreenPos.y >= Mathf.Infinity || float.IsNaN(crosshairScreenPos.y) ||
            crosshairScreenPos.z >= Mathf.Infinity || float.IsNaN(crosshairScreenPos.z))
        {
            crosshair.rectTransform.position = new Vector2(camera.pixelWidth/2, camera.pixelHeight/2);
        }
        else
        {
            crosshair.rectTransform.position = crosshairScreenPos;
            if (crosshairScreenPos == Vector3.zero)
                crosshair.rectTransform.position = new Vector2(camera.pixelWidth / 2, camera.pixelHeight / 2);
        }

        ammoText.text = "Ammo: " + _localPlayer.Ammo + "/" + _localPlayer.MaxAmmo;

    }

    public void UpdateMatchInfo()
    {
        Vector2 scores = gameManager.TeamScores;

        scoreText.text = "<color=#008000ff>" + gameManager.Match.Team1Score + "</color><color=#000000ff> : </color><color=#800000ff>" + gameManager.Match.Team2Score + "</color><color=#000000ff></color>";

        if (gameManager.Match.MatchState == 0)
            scoreText.text += "\nMatch State: Warm Up\n Time left: " + (int)gameManager.Match.WarmUpTimeLeft;
        else
            scoreText.text += "\nMatch State: Playing\n Time left: " + (int)gameManager.Match.RoundTimeLeft;




        if (_messageTime > 0.0f)
        {
            _messageTime -= Time.deltaTime;
        }

        if (_messageTime <= 0.0f)
        {
            messageText.text = "";
        }
    }

    public void HideUI()
    {
        _updateCanvas = false;
        EnableClientInfo(false);
        EnableHUD(false);
        EnableMatchInfo(false);
        EnableWeaponSelection(false);
    }
    public void ShowUI()
    {
        _updateCanvas = true;
    }
    public void SetMessage(string msg, float lifeTime)
    {
        _messageTime = lifeTime;
        messageText.text = msg;
    }

    private void GetLocalPlayer()
    {
        _localPlayer = gameManager.GetLocalPlayer();
    }
}
