using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class BulletMovement : MonoBehaviour {
    
    public float spawnForce = 0.0f;

    public LayerMask rayMask;

    private Rigidbody _body;
    private Vector3 _initialDirection = Vector3.zero;

    private PlayerInfo _player;

    private bool _setInitialDirection = false;

    private float _startChecking = 0.2f;
    private float _checkTimer = 0.0f;

    private bool _destroyBullet = false;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update ()
    {
        

        /*
        if (_checkTimer < _startChecking)
            _checkTimer += Time.deltaTime;
        else
            CheckNextPosition();*/

    }

    void FixedUpdate()
    {
        if(_destroyBullet)
            Destroy(gameObject);


        if (_player && _player.isLocalPlayer)
            CheckNextPosition();
        else if (_player)
            CheckOtherClientNextPositon();
        else
            Destroy(gameObject);


    }

    private void CheckNextPosition()
    {
        if (!_player)
            return;

        if (!_player.isLocalPlayer)
            return;

        if (_destroyBullet)
            return;

        Vector3 actualPosition = transform.position;
        Vector3 direction = _body.velocity.normalized;
        float magnitude = _body.velocity.magnitude * Time.fixedDeltaTime;

        Ray ray = new Ray(actualPosition, direction);
        RaycastHit hit = new RaycastHit();

        if(Physics.Raycast(ray, out hit, magnitude, rayMask))
        {
            if(hit.collider.gameObject.tag.Equals("BodyPart"))
            {
                BodyPartCollider bodyPart = hit.collider.gameObject.GetComponent<BodyPartCollider>();

                if (bodyPart)
                    bodyPart.ApplyDamage(_player);
            }
            //Debug.Log(hit.collider.gameObject.name);
            _destroyBullet = true;
        }
        
    }

    private void CheckOtherClientNextPositon()
    {

        Vector3 actualPosition = transform.position;
        Vector3 direction = _body.velocity.normalized;
        float magnitude = _body.velocity.magnitude * Time.fixedDeltaTime;

        Ray ray = new Ray(actualPosition, direction);
        RaycastHit hit = new RaycastHit();

        if (Physics.Raycast(ray, out hit, magnitude, rayMask))
        {
            _destroyBullet = true;
        }
    }

    private void Move()
    {
        _body.AddForce(transform.forward * spawnForce);
    }

    private void PrintInfo(string info)
    {
        Debug.Log(info);
    }

    public void InitBullet(PlayerInfo player, Vector3 initialDirection)
    {
        _player = player;
        _initialDirection = initialDirection;

        _body = GetComponent<Rigidbody>();
        Destroy(gameObject, 2.0f);

        _body.velocity = _initialDirection.normalized * spawnForce;
        

    }

    public string GetPlayerName()
    {
        if (_player)
            return _player.playerName;
        else
            return "Bullet has no player info";
    }
    public PlayerInfo PlayerInfo
    {
        get { return _player; }
    }
    
}
