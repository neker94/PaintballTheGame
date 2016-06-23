using UnityEngine;
using System.Collections;

public enum BodyPart
{
    Head,
    Torso,
    Arm,
    Leg
}

[RequireComponent(typeof(Rigidbody))]
public class BodyPartCollider : MonoBehaviour {

    public string bodyPartName = "Head";

    public BodyPart bodyPart = BodyPart.Head;

    public PlayerInfo playerInfo;
    
    private Rigidbody _body;

	// Use this for initialization
	void Start () {
        if (!playerInfo)
        {
            Debug.Log("PlayerInfo not assigned");
            Destroy(gameObject);
        }

        _body = GetComponent<Rigidbody>();
        _body.isKinematic = true;
        _body.mass = 0;

    }
	
	// Update is called once per frame
	void Update () {
	
	}

    /*void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Bullet")
        {
            BulletMovement bullet = other.GetComponent<BulletMovement>();
            if(bullet)
                ApplyDamage(bullet.PlayerInfo);
            Destroy(other.gameObject);
        }
    }*/

    public void ApplyDamage(PlayerInfo killerPlayer)
    {
        /*if (!playerInfo.isLocalPlayer)
            return;*/
        if (playerInfo.Dead)
            return;

        Debug.Log("Damaged");
        switch(bodyPart)
        {
            case BodyPart.Head:
                playerInfo.ApplyDamage(killerPlayer, 3);
                break;
            case BodyPart.Torso:
                playerInfo.ApplyDamage(killerPlayer, 2);
                break;
            case BodyPart.Arm:
                playerInfo.ApplyDamage(killerPlayer, 1);
                break;
            case BodyPart.Leg:
                playerInfo.ApplyDamage(killerPlayer, 1);
                break;
        }
        //Debug.Log(playerInfo.ActualHealth);
    }
}
