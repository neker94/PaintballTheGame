using UnityEngine;

public class MathUtils {

    public static bool Between(float value, float min, float max)
    {
        return min <= value && value <= max;
    }

    public static Vector3 VectorOverPlane(Vector3 vector, Vector3 normal, Vector3 point)
    {
        vector.y = 0.0f;

        Vector4 ABCD = new Vector4();
        Vector3 finalVector = new Vector3();

        ABCD.x = normal.x;
        ABCD.y = normal.y;
        ABCD.z = normal.z;

        ABCD.w = -ABCD.x * point.x - ABCD.y * point.y - ABCD.z * point.z;

        if (Between(ABCD.x, -0.005f, 0.005f))
            ABCD.x = 0.0f;
        if (Between(ABCD.y, -0.005f, 0.005f))
            ABCD.y = 0.0f;
        if (Between(ABCD.z, -0.005f, 0.005f))
            ABCD.z = 0.0f;
        if (Between(ABCD.w, -0.005f, 0.005f))
            ABCD.w = 0.0f;


        Vector3 newPoint = new Vector3(point.x + vector.x, 0.0f, point.z + vector.z);

        newPoint.y = (float) - (newPoint.x * ABCD.x + newPoint.z * ABCD.z + ABCD.w) / ABCD.y;

        //Sometimes newPoint.y can be NaN
        if (float.IsNaN(newPoint.y))
            newPoint.y = 0;

        finalVector = newPoint - point;

        finalVector.Normalize();

        return finalVector;
    }

    public static Vector3[] GetCirclePoints(int numberOfPoints, float radius)
    {
        Vector3[] points = new Vector3[numberOfPoints];

        float increments = 360.0f / numberOfPoints;

        float actualIncrement = increments;

        for (int i = 0; i < numberOfPoints; i++)
        {
            float radians = actualIncrement * Mathf.Deg2Rad;
            float sin = Mathf.Sin(radians);
            float cos = Mathf.Cos(radians);
            Vector3 vector = new Vector3(sin, 0.0f, cos);
            vector.Normalize();
            vector *= radius;

            points[i] = vector;
            actualIncrement += increments;
        }

        return points;
    }

    public static Vector3 GetPointInDistanceFromPos(Vector3 position, Vector3 direction, float distance)
    {
        return position + direction.normalized * distance;
    }

    public static Vector2 XYRotVecFromVectors(Vector3 a, Vector3 b, Vector3 up)
    {
        Vector2 finalVector = new Vector2();

        a.Normalize();
        b.Normalize();
        //Debug.Log(a + " " + b);

        Vector3 right = Vector3.Cross(b, up);
        float angleAB = Vector3.Angle(a, b);
        float angleAr = Vector3.Angle(a, right);
        if (angleAr < 90)
            angleAB = 360f - angleAB;

        //Debug.Log(angleAB);

        finalVector.x = Mathf.Sin(angleAB * Mathf.Deg2Rad);
        finalVector.y = Mathf.Cos(angleAB * Mathf.Deg2Rad);

        return finalVector;
    }
}
