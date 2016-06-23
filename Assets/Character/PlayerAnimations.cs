using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
public class PlayerAnimations : MonoBehaviour {

    public float movementSpeedLerp;

    private Animator _animationController;

    private Vector2 _playerInputDirection = new Vector2();

    private GameObject _leftHand;
    private GameObject _rightHand;
    private Vector3 _lookPos = new Vector3();

    private Vector2 _previousDir = new Vector2();

	// Use this for initialization
	void Start () {
        _animationController = GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnAnimatorIK()
    {
        // Set the look target position, if one has been assigned
        _animationController.SetLookAtWeight(1);
        _animationController.SetLookAtPosition(_lookPos);

        if (_rightHand != null)
        {
            _animationController.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
            _animationController.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
            _animationController.SetIKPosition(AvatarIKGoal.RightHand, _rightHand.transform.position);
            _animationController.SetIKRotation(AvatarIKGoal.RightHand, _rightHand.transform.rotation);
        }

        if (_leftHand != null)
        {
            _animationController.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
            _animationController.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
            _animationController.SetIKPosition(AvatarIKGoal.LeftHand, _leftHand.transform.position);
            _animationController.SetIKRotation(AvatarIKGoal.LeftHand, _leftHand.transform.rotation);
        }
    }

    public void HeadLookAt(Vector3 position)
    {

    }

    public void UpdateRunAnimation(Vector2 inputDirection)
    {

        _playerInputDirection.x = Mathf.Lerp(_playerInputDirection.x, inputDirection.x, 0.2f);
        _playerInputDirection.y = Mathf.Lerp(_playerInputDirection.y, inputDirection.y, 0.2f);


        _playerInputDirection.x = Clamp(_playerInputDirection.x, -1.0f, 1.0f);
        _playerInputDirection.y = Clamp(_playerInputDirection.y, -1.0f, 1.0f);

        _animationController.SetFloat("MovementX", _playerInputDirection.x);
        _animationController.SetFloat("MovementY", _playerInputDirection.y);
    }

    public void UpdateRunAnimationWithVelocity(Vector3 velocity, Vector3 lookDir)
    {
        Vector2 dir = MathUtils.XYRotVecFromVectors(velocity, lookDir, Vector3.up);

        dir *= velocity.magnitude;

        _previousDir.x = ConstantLerp(_previousDir.x, dir.x, movementSpeedLerp* Time.deltaTime);//Mathf.Lerp(_previousDir.x, dir.x, movementSpeedLerp * Time.deltaTime);
        _previousDir.y = ConstantLerp(_previousDir.y, dir.y, movementSpeedLerp* Time.deltaTime);//Mathf.Lerp(_previousDir.y, dir.y, movementSpeedLerp * Time.deltaTime);

        _animationController.SetFloat("MovementX", _previousDir.x);
        _animationController.SetFloat("MovementY", _previousDir.y);
    }


    private float ConstantLerp(float initialValue, float finalValue, float speed)
    {
        float newValue = 0.0f;
        if(Mathf.Abs(initialValue - finalValue) < speed)
        {
            return finalValue;
        }

        if (initialValue > finalValue)
            newValue = initialValue - speed;
        else
            newValue = initialValue + speed;
        

        return newValue;
    }

    private float Clamp(float value, float min, float max)
    {
        if (value < min)
            return min;
        if (value > max)
            return max;

        return value;
    }

    public void SetHandsIKSockets(GameObject leftHand, GameObject rightHand)
    {
        _leftHand = leftHand;
        _rightHand = rightHand;
    }

    public void Jump()
    {
        Debug.Log("Jump start");
        _animationController.SetTrigger("Jump");
    }

    public void EndJump()
    {
        Debug.Log("Jump end");
        _animationController.SetTrigger("EndJump");
    }

    public Vector3 LookAt
    {
        set { _lookPos = value; }
    }
}
