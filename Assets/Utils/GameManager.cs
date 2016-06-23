using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public enum ConnectionStatus
{
    Connected,
    Disconnected
}

[RequireComponent(typeof(NetworkManagerHUD))]
public class GameManager : NetworkBehaviour {


    public GameObject matchPrefab;
    private MatchManager _matchManager;
    private int _previousMatchState = -1;
    private GameObject _spawnedMatchManager;

    public CameraScript camera;
    private bool _cameraSetted = false;

    private PlayerInfo _localPlayer;

    public List<Transform> _spawnPositions = new List<Transform>();
    private List<PlayerInfo> _playerPool = new List<PlayerInfo>();
    private List<GameObject> _bulletsPool = new List<GameObject>();

    private NetworkManagerHUD _networkHud;

    private ConnectionStatus _connectionStatus = ConnectionStatus.Disconnected;

    void Start()
    {
        _networkHud = GetComponent<NetworkManagerHUD>();
    }

    void Update()
    {
        UpdateNetworkHUD();
        UpdateEscape();

        if(_connectionStatus == ConnectionStatus.Connected && _localPlayer)
        {

            UpdateMatchState();

            if (_previousMatchState != 0)
                UpdateLocalPlayerState();

            if (_localPlayer && _localPlayer.isServer)
            {
                if (Input.GetKeyDown(KeyCode.P))
                {
                    CanvasManager canvas = camera.GetComponent<CanvasManager>();
                    if (canvas)
                    {
                        canvas.HideUI();
                        camera.SwitchCameraMode(CameraMode.Free);
                        _localPlayer.DisablePlayer();
                    }
                }
                if (Input.GetKeyDown(KeyCode.O))
                {
                    CanvasManager canvas = camera.GetComponent<CanvasManager>();
                    if (canvas)
                    {
                        canvas.ShowUI();
                        AssignCameraToPlayer();
                        _localPlayer.EnablePlayer();
                    }
                }
            }

        }

        if (_connectionStatus == ConnectionStatus.Connected && _playerPool.Count > 0 && _playerPool[0] == null)
        {

            _connectionStatus = ConnectionStatus.Disconnected;

            if (_matchManager)
                Destroy(_matchManager.gameObject);

            RemoveAllPlayers();
            camera.SwitchCameraMode(CameraMode.Freeze);
            camera.ResetCameraTransform();
            Debug.Log(_connectionStatus);
        }

        if(!_localPlayer && _connectionStatus == ConnectionStatus.Connected)
        {
            _connectionStatus = ConnectionStatus.Disconnected;

            if (_matchManager)
                Destroy(_matchManager.gameObject);

            RemoveAllPlayers();
            camera.SwitchCameraMode(CameraMode.Freeze);
            camera.ResetCameraTransform();
            Debug.Log(_connectionStatus);
        }
        
        
    }

    private void UpdateNetworkHUD()
    {
        if(_connectionStatus == ConnectionStatus.Connected && _networkHud.enabled)
        {
            _networkHud.enabled = false;
        }
        if (_connectionStatus == ConnectionStatus.Disconnected && !_networkHud.enabled)
        {
            _networkHud.enabled = true;
        }
    }

    private void UpdateEscape()
    {
        if(Input.GetButtonDown("Escape"))
        {
            if (_connectionStatus == ConnectionStatus.Connected)
            {
                if (_localPlayer)
                {
                    if (_localPlayer.isServer)
                    {
                        NetworkManager.singleton.StopServer();
                        NetworkManager.singleton.StopClient();
                    }
                    else
                    {
                        NetworkManager.singleton.StopClient();
                    }
                }
                else
                {
                    Application.Quit();
                }
            }
            if (_connectionStatus == ConnectionStatus.Disconnected)
                Application.Quit();
        }
    }

    private void UpdateMatchState()
    {

        if(_localPlayer && _matchManager && _matchManager.MatchState != _previousMatchState)
        {
            if (_matchManager.MatchState == 0)
            {
                SwitchToWarmUpMode();
            }
            else
            {
                SwitchToRoundMode();
            }
            _previousMatchState = _matchManager.MatchState;
        }
    }

    private void UpdateLocalPlayerState()
    {
        if (_localPlayer.Dead)
        {
            if (_cameraSetted)
            {
                camera.SwitchCameraMode(CameraMode.Freeze);
                camera.ResetCameraTransform();
                _cameraSetted = false;
            }

        }
        if (!_localPlayer.Dead && !_cameraSetted)
        {
            AssignCameraToPlayer();
            _cameraSetted = true;
        }
    }

    /*[ClientRpc]
    public void RpcUpdatePlayers()
    {
        AddAllScenePlayers();
    }*/


    #region -----Player pool methods-----

    public void AddAllScenePlayers()
    {
        _playerPool.Clear();
        GameObject[] findedPlayers = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in findedPlayers)
        {
            if (player.GetComponent<PlayerInfo>())
            {
                _playerPool.Add(player.GetComponent<PlayerInfo>());
            }
        }
        Debug.Log("Players added: " + _playerPool.Count);
    }

    public void AddPlayer(PlayerInfo newPlayer)
    {
        //Check if the player is in the pool.
        foreach (PlayerInfo player in _playerPool)
        {
            if (player == newPlayer)
            {
                Debug.Log("Player already added");
                return;
            }
        }

        //Add the player
        _playerPool.Add(newPlayer);

    }

    public PlayerInfo GetPlayerByID(uint id)
    {
        foreach (PlayerInfo player in _playerPool)
        {
            if (player.netId.Value == id)
                return player;
        }
        return null;

    }


    public void RemovePlayer(PlayerInfo playerToRemove)
    {
        if (_playerPool.Remove(playerToRemove))
            Debug.Log("Player removed from pool");
        else
            Debug.Log("Couldn't find the player");
    }

    public void RemoveAllPlayers()
    {
        Debug.Log("Players to remove: " + _playerPool.Count);
        _playerPool.Clear();
    }

    public PlayerInfo GetPlayer(int i)
    {
        if (MathUtils.Between(i, 0, _playerPool.Count - 1))
            return _playerPool[i];
        else
        {
            Debug.Log("Index out of length");
            return null;
        }
    }

    public PlayerInfo GetPlayerByName(string name)
    {

        foreach (PlayerInfo player in _playerPool)
        {
            if (player.name.Equals(name))
                return player;
        }

        return null;
    }

    public PlayerInfo GetLocalPlayer()
    {
        foreach (PlayerInfo player in _playerPool)
        {
            if  (player && player.isLocalPlayer)
                return player;
        }

        return null;
    }

    public bool AssignCameraToPlayer()
    {
        PlayerInfo playerObject = GetLocalPlayer();

        //if (!playerObject || (playerObject && !playerObject.IsLocalPlayer))
        //return false;

        if (!playerObject)
            return false;

        PlayerMovement player = playerObject.gameObject.GetComponent<PlayerMovement>();
        if (player)
        {
            player.AssignCamera(camera);

            camera.objectsToHide.Clear();
            camera.objectsToHide.Add(player.head);
            camera.SwitchCameraMode(CameraMode.Follow, player.Head);
            camera.RenderObjects(false);
            return true;
        }
        return false;
    }

    public void StartConnection()
    {
        AddAllScenePlayers();
        AssignCameraToPlayer();
        _localPlayer = GetLocalPlayer();
        _connectionStatus = ConnectionStatus.Connected;
        InitMatch();
    }

    public int GetBestTeam()
    {
        int greenPlayers = 0;
        int brownPlayers = 0;

        

        foreach(PlayerInfo player in _playerPool)
        {
            Debug.Log(player.team);

            if (player.team == 0)
                greenPlayers++;
            else
                brownPlayers++;
        }
        return (greenPlayers <= brownPlayers)? 0 : 1;
    }

    public Vector3 GetRandomSpawn()
    {
        return (_spawnPositions.Count > 0)? _spawnPositions[Random.Range(0, _spawnPositions.Count - 1)].position : new Vector3();
    }

    #endregion

    public void SetCanvasMessage(string msg, float lifeTime)
    {
        if (!camera)
            return;

        CanvasManager canvas = camera.GetComponent<CanvasManager>();
        if (!canvas)
            return;

        canvas.SetMessage(msg, lifeTime);
    }

    public List<PlayerInfo> Players
    {
        get { return _playerPool; }
    }

    public ConnectionStatus ConnectionStatus
    {
        get { return _connectionStatus; }
        set {
            _connectionStatus = value;
        }
    }

    public GameObject SpawnedMatch
    {
        get { return _spawnedMatchManager; }
    }

    //MATCH METHODS

    public MatchManager Match
    {
        get { return _matchManager; }
    }

    public void InitMatch()
    {
        _spawnedMatchManager = Instantiate(matchPrefab);

        _matchManager = _spawnedMatchManager.GetComponent<MatchManager>();
        if (!_matchManager)
        {
            Debug.Log("ERROR: couldn't find match manager");
            return;
        }
        _matchManager.RestartMatch();

        UpdateMatchState();

    }

    public void SetMatchState(int state, float warmUpT, float roundT, int team1S, int team2S)
    {
        if (_matchManager)
        {
            _matchManager.SetMatchState(state, warmUpT, roundT, team1S, team2S);
        }

        UpdateMatchState();

    }

    public Vector2 TeamScores
    {
        get {
            if (_matchManager)
                return new Vector2(_matchManager.Team1Score, _matchManager.Team2Score);
            else
                return new Vector2();
        }
    }

    public void SwitchToWarmUpMode()
    {
        camera.SwitchCameraMode(CameraMode.Freeze);
        camera.ResetCameraTransform();
        _cameraSetted = false;

        _localPlayer.SetPlayerState(_matchManager.MatchState);

        if(_matchManager.HasPlayedBefore)
        {
            SetCanvasMessage(GetWinnerString(), _matchManager.warmUpTime);
        }

        _matchManager.RestartMatch();
    }

    public void SwitchToRoundMode()
    {
        AssignCameraToPlayer();
        _cameraSetted = true;

        _localPlayer.SetPlayerState(_matchManager.MatchState);
    }

    public string GetWinnerString()
    {
        if(_matchManager.Team1Score > _matchManager.Team2Score)
        {
            return "<color=green>TEAM ONE WINS!</color>";
        }
        else if (_matchManager.Team2Score > _matchManager.Team1Score)
        {
            return "<color=brown>TEAM TWO WINS!</color>";
        }
        else
        {
            return "DRAW!";
        }
    }

}
