using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public enum PlayerMovementState
{
    Walking,
    Jumping,
    Falling
}

public class PlayerMovement : NetworkBehaviour
{
    public GameObject head;
    public GameObject headPivot;

    public bool followCameraXRotation = false;
    public bool followCameraYRotation = true;
    public bool followCameraZRotation = false;

    public float acceleration = 10.0f;
    public float maxPlayerSpeed = 50.0f;
    public float jumpForce = 100.0f;

    public float maxSlopeAngle = 45.0f; //Max angle allowed to walk
    public float fallDetectionDistance = 0.5f; //Max distance allowed before being jumping/falling

    public PlayerAnimations animations;

    private bool _canMove = true;
    private float _actualMaxSpeed = 0.0f;

    private PlayerInfo _playerInfo;
    private Rigidbody _body;
    private Collider _physCollider;
    private CameraScript _camera;

    private float _initialFrictionValue = 0.0f;
    private PlayerMovementState _movementState;
    private bool _jumpPressed;
    private Vector3 _previousPosition;
    private Vector3 _actualPosition;
    private Vector3 _direction;
    private float _slopeAngle;

    private float _jumpTimer = 0.0f;
    private float _jumptTimeBeforeWalking = 0.5f;
    
    private float _inputForwardAmount = 0.0f;
    private float _inputSidewaysAmount = 0.0f;

    public override void OnStartLocalPlayer()
    {
        _body = GetComponent<Rigidbody>();
        _actualMaxSpeed = maxPlayerSpeed;
        _playerInfo = GetComponent<PlayerInfo>();
        _physCollider = GetComponentInChildren<Collider>();

        InitPlayerMovement();

    }

    public void InitPlayerMovement()
    {
        if (_physCollider)
        {
            _initialFrictionValue = _physCollider.material.dynamicFriction;
        }

        _movementState = PlayerMovementState.Falling;
        _jumpPressed = false;
        _previousPosition = transform.position;
        _actualPosition = transform.position;
        _direction = _actualPosition - _previousPosition;
    }

    // Use this for initialization
    void Start () {

    }

	// Update is called once per frame
	void Update ()
    {
        if (!_body)
        {
            _body = GetComponent<Rigidbody>();
        }
        else
        {
            animations.UpdateRunAnimationWithVelocity(_body.velocity, transform.forward);
        }


        if (isLocalPlayer)
            UpdateClientMovement();

    }

    private void UpdateClientMovement()
    {


        if (!_canMove)
            return;

        bool isInFloor = CheckFloor(30, 0.1f);
        //Debug.Log(_movementState);
        switch (_movementState)
        {
            case PlayerMovementState.Walking:
                MovePlayer();
                RotatePlayer();
                JumpControl();
                
                if (_jumpPressed)
                {
                    _jumpTimer = 0.0f;
                    _movementState = PlayerMovementState.Jumping;
                    Jump();
                }
                if (!isInFloor && _body.velocity.y < 0.0f)
                {
                    _movementState = PlayerMovementState.Falling;
                }
                break;
            case PlayerMovementState.Jumping:
                RotatePlayer();
                _jumpTimer += Time.deltaTime;
                if (_jumpTimer >= _jumptTimeBeforeWalking && isInFloor)
                {
                    _movementState = PlayerMovementState.Walking;
                    EndJump();
                    
                }
                else if (_body.velocity.y < 0.0f)
                {
                    _movementState = PlayerMovementState.Falling;
                    _jumpPressed = false;
                }

                break;
            case PlayerMovementState.Falling:
                RotatePlayer();

                if (isInFloor)
                {
                    _movementState = PlayerMovementState.Walking;
                    EndJump();
                }
                break;
            default:
                UpdateFriction(isInFloor);
                MovePlayer();
                RotatePlayer();
                break;
        }


        //Debug.Log(_movementState);
        UpdateFriction(isInFloor);
    }

    private void Jump()
    {
        animations.Jump();
        CmdJump();
    }

    [Command]
    private void CmdJump()
    {

        if(animations)
        {
            if (!isLocalPlayer)
                animations.Jump();
        }
        RpcJump();
    }

    [ClientRpc]
    private void RpcJump()
    {
        if (animations)
        {
            if (!isLocalPlayer)
                animations.Jump();
        }
    }

    private void EndJump()
    {
        animations.EndJump();
        CmdEndJump();
    }

    [Command]
    private void CmdEndJump()
    {

        if (animations)
        {
            if (!isLocalPlayer)
                animations.EndJump();
        }
        RpcEndJump();
    }

    [ClientRpc]
    private void RpcEndJump()
    {
        if (animations)
        {
            if (!isLocalPlayer)
                animations.EndJump();
        }
    }


    private void MovePlayer()
    {
        //If the player can't move, return
        if (!_canMove)
            return;

        //Get the axis
        _inputForwardAmount = Input.GetAxis("Vertical");
        _inputSidewaysAmount = Input.GetAxis("Horizontal");

        //Get the direction vectors
        Vector3 forwardDirection = new Vector3(transform.forward.x, transform.forward.y, transform.forward.z);
        forwardDirection.Normalize();
        Vector3 sidewaysDirection = Vector3.Cross(Vector3.up, forwardDirection);
        sidewaysDirection.Normalize();

        //Create the ray
        RaycastHit hit;
        Ray ray = new Ray(transform.position, Vector3.down);



        if (Physics.Raycast(ray, out hit) && _body && _body.velocity.magnitude < _actualMaxSpeed/* && CheckFloor(10, 0.2f)*/)
        {
            //If the ray hits simething, calculate the movement
            Vector3 finalForwardDirection;
            Vector3 finalSidewaysDirection;
            Vector3 finalDirection;

            //Transform the normal to world space
            Vector3 normal = hit.point + hit.normal;
            normal = normal - hit.point;

            //Get the needed vectors in the plane surface
            finalForwardDirection = MathUtils.VectorOverPlane(forwardDirection, normal, hit.point);
            finalSidewaysDirection = MathUtils.VectorOverPlane(sidewaysDirection, normal, hit.point);

            //Multiply by the different axis
            finalForwardDirection *= _inputForwardAmount;
            finalSidewaysDirection *= _inputSidewaysAmount;

            //Add and normalize for the final direction
            finalDirection = finalForwardDirection + finalSidewaysDirection;
            finalDirection.Normalize();

            /*if (finalDirection.y > 0)
                finalDirection.y = finalDirection.y + finalDirection.y / 1.4f;*/

            //Debug rays
            Debug.DrawRay(hit.point, finalDirection, Color.blue);
            Debug.DrawRay(hit.point, normal, Color.red,0.00001f,false);

            _body.velocity = finalDirection * maxPlayerSpeed;

        }

        
    }

    private void JumpControl()
    {
        if (Input.GetButtonDown("Jump"))
        {
            _jumpPressed = true;
            
            _body.AddForce(new Vector3(0.0f, jumpForce, 0));
            _movementState = PlayerMovementState.Jumping;
        }
    }

    private void UpdateFriction(bool isInFloor)
    {
        if (!_physCollider)
            return;
        

        float forwardAmount = Input.GetAxisRaw("Vertical");
        float sidewaysAmount = Input.GetAxisRaw("Horizontal");

        if (forwardAmount == 0.0f && sidewaysAmount == 0.0f)
        {
            _physCollider.material.dynamicFriction = 0.8f;
            if (_movementState == PlayerMovementState.Walking && isInFloor)
                _body.velocity = Vector3.zero;
        }
        else
            _physCollider.material.dynamicFriction = 0;

        //float velocityMag = _body.velocity.magnitude;
        //_physCollider.material.dynamicFriction = /*_initialFrictionValue +*/ Mathf.Lerp(0.8f, 3, velocityMag / maxPlayerSpeed);
    }
    
    private bool CheckFloor(int numberOfPoints, float radius)
    {
        bool floorFound = false;

        Vector3[] circlePoints = MathUtils.GetCirclePoints(numberOfPoints, radius);

        foreach(Vector3 point in circlePoints)
        {

            Vector3 worldPoint = transform.position + point;

            Debug.DrawLine(worldPoint, worldPoint + (Vector3.down * fallDetectionDistance), Color.yellow);

            RaycastHit hit;
            Ray ray = new Ray(worldPoint, Vector3.down);
            if (Physics.Raycast(ray, out hit, fallDetectionDistance))
            {
                _slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
                if (_slopeAngle <= maxSlopeAngle)
                    floorFound = true;
            }

        }

        return floorFound;
    }

    private void RotatePlayer()
    {
        if (!_camera)
            return;

        Vector3 rotation = transform.rotation.eulerAngles;

        if (followCameraXRotation)
            rotation.x = _camera.Camera.transform.rotation.eulerAngles.x;

        if (followCameraYRotation)
            rotation.y = _camera.Camera.transform.rotation.eulerAngles.y;

        if (followCameraZRotation)
            rotation.z = _camera.Camera.transform.rotation.eulerAngles.z;

        transform.rotation = Quaternion.Euler(rotation);

    }

    public void MoveToPosition(Vector3 position)
    {
        transform.position = position;
    }

    public void AssignCamera(CameraScript camera)
    {
        _camera = camera;
    }
    
    public CameraScript GetCamera
    {
        get { return _camera; }
    }

    public bool CanMove
    {
        get { return _canMove; }
        set { _canMove = value; }
    }

    public GameObject Head
    {
        get
        {
            if (!headPivot)
                return head;
            else
                return headPivot;
        }
    }
}
