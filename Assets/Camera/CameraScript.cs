using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum CameraMode
{
    Free,
    Freeze,
    Gimbal,
    Follow
}

public class CameraScript : MonoBehaviour {

    #region ----------Public variables----------


    public CameraMode mode = CameraMode.Free;

    public GameObject objectToFollow;
    public GameObject objectToLook;
    public bool lookAtObject = false;
    public List<GameObject> objectsToHide = new List<GameObject>();

    public Vector3 followOffset = new Vector3(0, 0, 0);
    public Vector3 cameraOffset = new Vector3(0, 0, 0);

    public float horizontalMouseSensitivity = 0.0f;
    public bool invertVerticalAxis = false;
    public float verticalMouseSensitivity = 0.0f;
    public bool invertHorizontalAxis = false;

    public float verticalCameraAngle = 45.0f;

    public float freeCameraSpeed = 100.0f;

    //public bool moveCameraOnly = true;

    public bool lockXRotation = false;
    public bool lockYRotation = false;
    public bool lockZRotation = false;

    public bool followXRotation = false;
    public bool followYRotation = false;
    public bool followZRotation = false;

    #endregion

    #region ----------Private variables----------

    private Camera _camera;

    private Vector3 _initialPos;
    private Quaternion _initialRot;

    #endregion

    // Use this for initialization
    void Start ()
    {
        //Initialize variables.

        if (mode == CameraMode.Follow && !objectToFollow)
        {
            Debug.Log("<color=red>Object to follow not found, changing to Free mode</color>");
            mode = CameraMode.Free;
        }

        _camera = GetComponentInChildren<Camera>();

        if (!_camera)
            Debug.Log("Camera not found");
        else
            Debug.Log("Camera found");

        if (!objectToLook)
            objectToLook = objectToFollow;

        _initialPos = transform.position;
        _initialRot = transform.rotation;

        //Check sensitivities
        if (horizontalMouseSensitivity <= 0)
            horizontalMouseSensitivity = 1;

        if (verticalMouseSensitivity <= 0)
            verticalMouseSensitivity = 1;

        //Force the first update of the camera
        ForceUpdateCamera();

        //Hide GameObjects
        RenderObjects(false);

    }
	
	// Update is called once per frame
	void Update ()
    {
        //Debug.Log(mode);
        
        ForceUpdateCamera();
	}



    public void SwitchCameraMode(CameraMode newMode, GameObject newObjectToFollow = null)
    {
        if(newMode == CameraMode.Follow && !objectToFollow && !newObjectToFollow)
        {
            Debug.Log("Error: needs an object to follow.");
            return;
        }

        if(newMode == CameraMode.Follow)
        {
            objectToFollow = newObjectToFollow;
            objectToLook = objectToFollow;
        }

        if(newMode == CameraMode.Follow || newMode == CameraMode.Gimbal || newMode == CameraMode.Free)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        mode = newMode;
    }

    public void MoveToPosition(Vector3 newPosition, Quaternion newRotation)
    {
        transform.position = newPosition;
        transform.rotation = newRotation;
    }

    public void ForceUpdateCamera()
    {
        switch (mode)
        {
            case CameraMode.Free:
                FreeUpdate();
                break;

            case CameraMode.Freeze:
                FreezeUpdate();
                break;

            case CameraMode.Gimbal:
                GimbalUpdate();
                break;
            
            case CameraMode.Follow:
                FollowUpdate();
                break;

            default:
                break;
        }
    }

    public void RenderObjects(bool render)
    {
        foreach(GameObject obj in objectsToHide)
        {
            obj.SetActive(render);
        }
    }

    private void FreeUpdate()
    {
        //First the rotation
        RotateCamera();

        //Then the position
        ProcessMovement();


    }

    private void FreezeUpdate()
    {
        // Do nothing, camera is freezed
    }

    private void GimbalUpdate()
    {
        // Only rotate the camera
        ProcessRotation();
    }

    private void FollowUpdate()
    {
        //First follow the position
        if(objectToFollow)
            transform.position = objectToFollow.transform.position + followOffset;

        //Then set the rotation
        RotateCamera();
    }

    private void RotateCamera()
    {
        if (lookAtObject)
            _camera.transform.LookAt(objectToLook.transform.position);
        else
            ProcessRotation();
    }

    private void ProcessRotation()
    {
        Vector3 rotationValues;
        Vector3 objectRotation = new Vector3(0, 0, 0);
        
        rotationValues = transform.rotation.eulerAngles;


        if (followXRotation)
            objectRotation.x = objectToFollow.transform.rotation.eulerAngles.x;
        else
        {
            objectRotation.x = rotationValues.x;
            float previousAngle = rotationValues.x;
            objectRotation.x = objectRotation.x + Input.GetAxis("Mouse Y") * VerticalMouseSensitivity * GetInvertedAxis(invertVerticalAxis) * Time.deltaTime;
            objectRotation.x = (objectRotation.x < 0.0f) ? 360.0f + objectRotation.x : objectRotation.x;
            objectRotation.x = (objectRotation.x > 360.0f) ? objectRotation.x - 360.0f : objectRotation.x;
            objectRotation.x = (MathUtils.Between(objectRotation.x, 0, verticalCameraAngle) || MathUtils.Between(objectRotation.x, 360 - verticalCameraAngle, 360)) ? objectRotation.x : previousAngle;
        }

        if (followYRotation)
            objectRotation.y = objectToFollow.transform.rotation.eulerAngles.y;
        else
        {
            objectRotation.y = rotationValues.y;
            objectRotation.y += Input.GetAxis("Mouse X") * HorizontalMouseSensitivity * GetInvertedAxis(invertHorizontalAxis) * Time.deltaTime;
        }

        if (followZRotation)
            objectRotation.z = rotationValues.z;
        else
            objectRotation.z = transform.rotation.eulerAngles.z;
        
        transform.rotation = Quaternion.Euler(objectRotation);
    }

    private void ProcessMovement()
    {

        float forwardAmount = Input.GetAxis("Vertical");
        float sidewaysAmount = Input.GetAxis("Horizontal");

        Vector3 forwardVector = transform.forward * forwardAmount;
        Vector3 sidewaysVector = Vector3.Cross(transform.forward, Vector3.up) * -sidewaysAmount;
        Vector3 finalDirection = forwardVector + sidewaysVector;
        finalDirection.Normalize();
        finalDirection *= freeCameraSpeed * Time.deltaTime;

        transform.position += finalDirection;

    }

    private float GetInvertedAxis(bool inverted)
    {
        return inverted ? -1.0f : 1.0f;
    }

    public void ResetCameraTransform()
    {
        MoveToPosition(_initialPos, _initialRot);
    }

    /*-------------------------------
           GETTERS AND SETTERS    
    -------------------------------*/

    
    public float HorizontalMouseSensitivity
    {
        get { return horizontalMouseSensitivity; }
        set { horizontalMouseSensitivity = value * 1000; }
    }

    public float VerticalMouseSensitivity
    {
        get { return -verticalMouseSensitivity; }
        set { verticalMouseSensitivity = value * -1000; }
    }

    public GameObject Camera
    {
        get { return gameObject; }
    }
}
