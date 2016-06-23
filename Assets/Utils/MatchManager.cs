using UnityEngine;
using System.Collections;
using UnityEngine.Networking;


public class MatchManager : MonoBehaviour {

    public float warmUpTime = 20.0f;
    public float roundTime = 120.0f;

    private float _warmUpTimeLeft = 0.0f;
    private float _roundTimeLeft = 0.0f;

    //MATCH STATES:
    //0: warm up
    //1: playing
    
    private int _matchState = 0;
    
    private int _team1Score = 0;
    
    private int _team2Score = 0;

    private bool _playedMatch = false;


    // Use this for initialization
    void Start () {
	
	}
    
    public void RestartMatch()
    {
        _warmUpTimeLeft = warmUpTime;
        _roundTimeLeft = roundTime;
        _matchState = 0;
        _team1Score = 0;
        _team2Score = 0;
    }

    public void SetMatchState(int state, float warmUpT, float roundT, int team1S, int team2S)
    {
        _matchState = state;
        _warmUpTimeLeft = warmUpT;
        _roundTimeLeft = roundT;
        _team1Score = team1S;
        _team2Score = team2S;
        _playedMatch = false;
        if (state == 1)
            _playedMatch = true;
    }
    
    // Update is called once per frame
    void Update () {

        switch (_matchState)
        {
            case 0:
                UpdateWarmUp();
                break;
            case 1:
                UpdateRound();
                break;
            default:
                break;
        }
	}
    

    private void UpdateWarmUp()
    {
        _warmUpTimeLeft -= Time.deltaTime;

        if(_warmUpTimeLeft <= 0)
        {
            _roundTimeLeft = roundTime;
            _matchState = 1;
            _playedMatch = true;
        }
    }

    private void UpdateRound()
    {
        _roundTimeLeft -= Time.deltaTime;

        if(_roundTimeLeft <= 0)
        {
            _warmUpTimeLeft = warmUpTime;
            _matchState = 0;
        }
    }

    public void IncrementScore(int team, int increment)
    {
        if (team == 0)
            _team1Score += increment;
        else
            _team2Score += increment;
        
    }

    public float WarmUpTimeLeft
    {
        get { return _warmUpTimeLeft; }
    }

    public float RoundTimeLeft
    {
        get { return _roundTimeLeft; }
    }

    public int MatchState
    {
        get { return _matchState; }
    }

    public int Team1Score
    {
        get { return _team1Score; }
    }

    public int Team2Score
    {
        get { return _team2Score; }
    }

    public bool HasPlayedBefore
    {
        get { return _playedMatch; }
    }

}
