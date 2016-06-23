using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MovementCollider : MonoBehaviour {

    private List<ContactPoint> _contactPoints = new List<ContactPoint>();
    

    void OnCollisionEnter(Collision collision)
    {
        _contactPoints.Clear();

        foreach(ContactPoint point in collision.contacts)
        {
            Debug.Log(point.point);
            _contactPoints.Add(point);
        }
    }

    void OnCollisionExit()
    {
        _contactPoints.Clear();
    }

    public List<ContactPoint> ContactPoints
    {
        get { return _contactPoints; }
    }

}
