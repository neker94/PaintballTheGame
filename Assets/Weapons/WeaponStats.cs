using UnityEngine;
using System.Collections;

public class WeaponStats : MonoBehaviour {

    public string weaponDisplayName;
    public GameObject bulletObject;

    public float desviation = 5.0f;

    public int bulletsPerShot = 1;

    public int clipSize = 20;
    public int maxBullets = 100;

    public float reloadTime = 0.1f;

    public float fireRate = 0.2f;

    public float bestMinDistance = 20.0f;
    public float bestMaxDistance = 30.0f;

    public GameObject barrelMouth;
    public GameObject clipSocket;
    public Transform reloadingClipPosition;
    private Quaternion _idleClipRotation = Quaternion.identity;

    public GameObject leftHand;
    public GameObject rightHand;


    // Use this for initialization
    void Start () {
        _idleClipRotation = clipSocket.transform.localRotation;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public Quaternion ClipStartQuat
    {
        get { return _idleClipRotation; }
    }

}
