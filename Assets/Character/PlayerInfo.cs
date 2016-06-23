using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public enum PlayerState
{
    Waiting,
    Playing
}

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(WeaponScript))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerInfo : NetworkBehaviour {
    [SyncVar]
    public string playerName;

    [SyncVar, HideInInspector]
    public int team = 0;
    //0: green
    //1: brown

    public int playerHealth;

    public GameObject playerHead;
    public GameObject playerBody;


    public Material greenHeadMaterial;
    public Material brownHeadMaterial;

    public Material greenBodyMaterial;
    public Material brownBodyMaterial;

    public GameObject[] playerMeshes;

    public float timeToRespawn = 5.0f;
    private float _respawnTimer = 0.0f;

    private PlayerMovement _playerMovement;
    private WeaponScript _weapon;

    [SyncVar]
    private int _syncedHealth;
    private uint _lastPlayerHitId;

    [SyncVar]
    private bool _dead = false;

    private bool _weaponChanged = false;

    //private int _actualHealth;

    private Vector3 _playerSpawn;

    private GameManager _gameManager;


    private PlayerState _state = PlayerState.Waiting;

    public override void OnStartLocalPlayer()
    {
        _gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        if (_gameManager)
        {
            _gameManager.StartConnection();
            /*if(isServer)
                _gameManager.Match.IncrementScore(0, 1);*/
            if(!isServer)
                CmdRequestMatchState(netId.Value);


        }
        _syncedHealth = playerHealth;
        _playerSpawn = transform.position;
        
        _playerMovement = GetComponent<PlayerMovement>();
        _weapon = GetComponent<WeaponScript>();
        
        CmdUpdatePlayerPool();
        CmdRestartPlayer();

        SetName();
        CmdRequestTeam();
    }

    //Sync local variables here
    public override void OnStartClient()
    {
        base.OnStartClient();
        SetPlayerMaterials(team);
        

    }

    public void SetName()
    {
        CmdSetNamePlayer(ClientInfo.GetClientInfo().ClientName);
    }

    [Command]
    private void CmdSetNamePlayer(string name)
    {
        RpcSetNamePlayer(name);
        playerName = name;
    }

    [ClientRpc]
    private void RpcSetNamePlayer(string name)
    {
        playerName = name;
    }

    [Command]
    private void CmdSetPlayerHealth(int health)
    {
        RpcSetPlayerHealth(health);
        _syncedHealth = health;
    }

    [ClientRpc]
    private void RpcSetPlayerHealth(int health)
    {
        _syncedHealth = health;
    }

    /*private void ForceClientsUpdate()
    {
        ApplyZeroDamage();
    }

    private void ApplyZeroDamage()
    {
        List<PlayerInfo> players = _gameManager.Players;
        foreach(PlayerInfo player in players)
        {
            player.ApplyDamage(this, 0);
        }
    }*/

    [Command]
    public void CmdForceNetworkUpdate()
    {
        GetGameManager();

        _gameManager.GetLocalPlayer().CmdValidateHealth(_gameManager.GetLocalPlayer().ActualHealth);
        RpcForceNetworkUpdate();
    }
    
    [ClientRpc]
    public void RpcForceNetworkUpdate()
    {
        GetGameManager();

        _gameManager.GetLocalPlayer().CmdValidateHealth(_gameManager.GetLocalPlayer().ActualHealth);
    }

    [Command]
    public void CmdUpdatePlayerPool()
    {
        GetGameManager();

        _gameManager.AddAllScenePlayers();
        RpcUpdatePlayerPool();
    }

    [ClientRpc]
    public void RpcUpdatePlayerPool()
    {
        GetGameManager();

        _gameManager.AddAllScenePlayers();
    }
	
	// Update is called once per frame
	void Update () {

        if (!isLocalPlayer)
        {
            Debug.Log(playerName + " health: " + _syncedHealth);
            return;

        }

        if(Input.GetKeyDown(KeyCode.L) && Ammo <= 0 && MaxAmmo <= 0)
        {
            _weapon.RestartWeapon();
        }

        if (_state == PlayerState.Waiting && _playerMovement && _playerMovement.CanMove)
        {
            DisablePlayer();
        }

        if (_dead)
        {
            UpdateDeathTimer();
        }
        else
        {
            UpdatePlayerState();
        }
        
        //_syncedHealth = _actualHealth;

    }

    private void UpdateDeathTimer()
    {
        if (_state == PlayerState.Waiting)
            return;

        _respawnTimer -= Time.deltaTime;
        if(_weapon && _weapon.weaponName != ClientInfo.GetClientInfo().WeaponEquipped)
        {
            _weapon.weaponName = ClientInfo.GetClientInfo().WeaponEquipped;
            _weaponChanged = true;
        }

        if(_respawnTimer <= 0.0f)
        {
            CmdRestartPlayer();
        }
    }
    
    private void HideMeshes()
    {
        foreach(GameObject mesh in playerMeshes)
        {
            mesh.SetActive(false);
        }
    }

    private void ShowMeshes()
    {
        foreach (GameObject mesh in playerMeshes)
        {
            mesh.SetActive(true);
        }
    }

    public void RestartPlayer()
    {
        GetGameManager();

        GetComponent<Rigidbody>().useGravity = true;

        _dead = false;

        if(isLocalPlayer || hasAuthority)
            CmdSetPlayerHealth(playerHealth);

        _playerMovement = GetComponent<PlayerMovement>();
        _playerMovement.MoveToPosition(_gameManager.GetRandomSpawn());
        _playerMovement.InitPlayerMovement();

        ShowMeshes();

        if (isLocalPlayer)
        {
            if (_weaponChanged)
            {
                _weapon.CmdInitWeapon(_weapon.weaponName);
                _weaponChanged = false;
            }

            _gameManager.camera.RenderObjects(false);
            if(!_weapon)
                _weapon = GetComponent<WeaponScript>();
            if(_weapon)
                _weapon.RestartWeapon();
        }

        EnablePlayer();

    }

    [Command]
    public void CmdRestartPlayer()
    {
        
        RestartPlayer();

        RpcRestartPlayer();
    }

    [ClientRpc]
    public void RpcRestartPlayer()
    {

        RestartPlayer();
    }

    public void DisablePlayer()
    {
        if(_playerMovement)
            _playerMovement.CanMove = false;
        if(_weapon)
            _weapon.CanShootWeapon = false;
    }

    public void EnablePlayer()
    {
        if (_playerMovement)
            _playerMovement.CanMove = true;
        if (_weapon)
            _weapon.CanShootWeapon = true;
    }

    public void SetPlayerMaterials(int team)
    {
        Renderer head = playerHead.GetComponent<Renderer>();

        Renderer body = playerBody.GetComponent<Renderer>();

        if (!head || !body)
            return;


        if(team == 0)
        {
            head.material = greenHeadMaterial;
            body.material = greenBodyMaterial;
        }
        else
        {
            head.material = brownHeadMaterial;
            body.material = brownBodyMaterial;
        }
    }

    public void ApplyDamage(PlayerInfo killerPlayer, int damage)
    {
        if (_dead)
            return;

        if(!killerPlayer)
        {
            Debug.Log("Killer player not found");
            return;
        }

        if (!killerPlayer.isLocalPlayer)
            return;

        if (killerPlayer.team == team)
            return;

        GetGameManager();


        PlayerInfo localPlayer = _gameManager.GetLocalPlayer();
        if(!localPlayer)
        {
            Debug.Log("Local player not found");
            return;
        }

        if (netId.Value == localPlayer.netId.Value)
            return;

        localPlayer.CmdSendDamage(netId.Value, localPlayer.netId.Value, damage);

        //_actualHealth -= damage;
    }


    [Command]
    public void CmdKillPlayer()
    {
        if(!isLocalPlayer)
        {
            KillPlayer();
        }
        RpcKillPlayer();
    }

    [ClientRpc]
    public void RpcKillPlayer()
    {
        if (!isLocalPlayer)
        {
            KillPlayer();
        }
    }

    public void KillPlayer()
    {
        _dead = true;
        HidePlayer();
        DisablePlayer();
        _respawnTimer = timeToRespawn;

        if(isLocalPlayer || hasAuthority)
            CmdSetPlayerHealth(playerHealth);
        //_syncedHealth = playerHealth;

        //_actualHealth = playerHealth;
    }

    private void HidePlayer()
    {
        gameObject.transform.position = new Vector3(0, -100000, 0);
        GetComponent<Rigidbody>().useGravity = false;
    }
    
    private void DebugPlayerInfo()
    {
        Debug.Log("Player name: " + playerName + "\nTeam: " + team);
    }

    //uint id: player who receives damage
    [Command]
    public void CmdSendDamage(uint id, uint killerId, int damage)
    {
        RpcSendDamage(id, killerId, damage);
    } 

    [ClientRpc]
    public void RpcSendDamage(uint id, uint killerId, int damage)
    {
        GetGameManager();

        PlayerInfo player = _gameManager.GetLocalPlayer();

        PlayerInfo playerDamaged = _gameManager.GetPlayerByID(id);
        if (playerDamaged.isLocalPlayer)
            playerDamaged.DecreaseHealth(damage, killerId);
    }

    public void DecreaseHealth(int amount, uint killerId)
    {
        //_actualHealth -= amount;
        //_syncedHealth -= amount;
        CmdSetPlayerHealth(_syncedHealth - amount);
        _lastPlayerHitId = killerId;
        UpdatePlayerState();
    }

    public void UpdatePlayerState()
    {
        if (isLocalPlayer)
        {
            if (/*_actualHealth <= 0 || */_syncedHealth <= 0)
            {
                CmdKillMessage(_lastPlayerHitId, netId.Value);
                KillPlayer();
                CmdKillPlayer();
            }
        }
    }

    [Command]
    private void CmdKillMessage(uint killer, uint victim)
    {

        GetGameManager();
        PlayerInfo killerInfo = _gameManager.GetPlayerByID(killer);
        PlayerInfo victimInfo = _gameManager.GetPlayerByID(victim);

        string killerName = _gameManager.GetPlayerByID(killer).playerName;
        string victimName = _gameManager.GetPlayerByID(victim).playerName;


        SetKillMessage(killerName, victimName);
        RpcKillMessage(killerName, victimName);
        RpcAddKillScore(killerInfo.team, 1);
    }


    [Command]
    public void CmdAddKillScore(int team, int increment)
    {
        RpcAddKillScore(team, increment);
    }
    [ClientRpc]
    public void RpcAddKillScore(int team, int increment)
    {
        GetGameManager();
        if (_gameManager.Match)
            _gameManager.Match.IncrementScore(team, increment);
        else
            Debug.Log("Couldn't find Match.");
    }


    [ClientRpc]
    private void RpcKillMessage(string killerName, string victimName)
    {
        SetKillMessage(killerName, victimName);
    }


    private void SetKillMessage(string killerName, string victimName)
    {
        GetGameManager();
        _gameManager.SetCanvasMessage(killerName + " eliminated " + victimName, 1.0f);
    }

    [Command]
    public void CmdValidateHealth(int health)
    {
        //_actualHealth = health;
        RpcValidateHealth(health);
    }

    [ClientRpc]
    public void RpcValidateHealth(int health)
    {
        //_actualHealth = health;
    }

    [Command]
    public void CmdSendServerDebugMessage(string message)
    {
        Debug.Log(message);
        RpcReceiveServerDebugMessage(message);
    }

    [ClientRpc]
    public void RpcReceiveServerDebugMessage(string message)
    {
        Debug.Log(message);
    }
    
    [Command]
    public void CmdRequestTeam()
    {
        GetGameManager();

        int newTeam = _gameManager.GetBestTeam();
        
        team = newTeam;
        SetPlayerMaterials(newTeam);
        RpcSetTeam(newTeam);
    }

    [ClientRpc]
    public void RpcSetTeam(int newTeam)
    {
        SetPlayerMaterials(newTeam);

        if (!isLocalPlayer)
            return;

        team = newTeam;
    }

    [Command]
    public void CmdSendMessage(string msg)
    {
        Debug.Log(msg);
        RpcSendMessage(msg);
    }

    [ClientRpc]
    private void RpcSendMessage(string msg)
    {
        Debug.Log(msg);
    }

    private void GetGameManager()
    {
        if (!_gameManager)
            _gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
    }

    public int ActualHealth
    {
        get { return _syncedHealth; }
    }

    public bool Dead
    {
        get { return _dead; }
    }
    
    public int Ammo
    {
        get {
            if (_weapon)
                return _weapon.Ammo;
            else
                return 0;
        }
    }

    public int MaxAmmo
    {
        get
        {
            if (_weapon)
                return _weapon.MaxAmmo;
            else
                return 0;
        }
    }

    public Vector3 BulletImpactPredicted
    {
        get
        {
            return _weapon.BulletImpactPredicted;
        }
    }


    //Match CMDs and RPCs
    [Command]
    public void CmdRequestMatchState(uint playerId)
    {
        if (!_gameManager)
            _gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();

        if (!_gameManager || !_gameManager.Match)
            return;

        int state = _gameManager.Match.MatchState;

        float warmT = _gameManager.Match.WarmUpTimeLeft;
        float roundT = _gameManager.Match.RoundTimeLeft;

        int s1 = _gameManager.Match.Team1Score;
        int s2 = _gameManager.Match.Team2Score;

        RpcRetrieveMatchState(playerId, state, warmT, roundT, s1, s2);
    }

    [ClientRpc]
    public void RpcRetrieveMatchState(uint playerId, int state, float warmT, float roundT, int score1, int score2)
    {
        GetGameManager();

        if(playerId == netId.Value && _gameManager.Match)
        {
            _gameManager.SetMatchState(state, warmT, roundT, score1, score2);
            SetPlayerState(state);
        }

    }

    public PlayerState PlayerState
    {
        get { return _state; }
    }
    public void SetPlayerState(int state)
    {

        if (state == 0)
        {
            _state = PlayerState.Waiting;
            DisablePlayer();
            HidePlayer();
        }
        else
        {
            _state = PlayerState.Playing;
            EnablePlayer();
            if(isLocalPlayer)
                CmdRestartPlayer();
        }
    }

    public GameObject GameCamera
    {
        get {
            GetGameManager();
            return _gameManager.camera.Camera;
        }
    }
}
