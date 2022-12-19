using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    public Transform targetObject;
    public float distance = 1f;
    [SerializeField] private float distanceSmoothTime = 0.1f;
    public Vector2 angle;
    [SerializeField] private float angleSmoothTime = 0.1f;

    private float currDist;
    private float distVelocity;
    private Vector2 currAngle = Vector2.zero;
    private Vector2 angleVelocity = Vector2.zero;

    private void Start()
    {
        currDist = distance;
        currAngle = angle;
    }

    private void Update()
    {
        currDist = Mathf.SmoothDamp(currDist, distance, ref distVelocity, distanceSmoothTime);

        currAngle.x = Mathf.SmoothDampAngle(currAngle.x, angle.x, ref angleVelocity.x, angleSmoothTime);
        currAngle.y = Mathf.SmoothDampAngle(currAngle.y, angle.y, ref angleVelocity.y, angleSmoothTime);

        var offset = Vector3.zero;
        offset.x = Mathf.Sin(currAngle.x * Mathf.Deg2Rad);
        offset.y = Mathf.Tan(currAngle.y * Mathf.Deg2Rad);
        offset.z = Mathf.Cos(currAngle.x * Mathf.Deg2Rad);
        offset = -offset.normalized * currDist;

        if (targetObject == null)
        {
            transform.localPosition = offset;
        }
        else
        {
            transform.position = targetObject.position + offset;
        }

        transform.rotation = Quaternion.Euler(-currAngle.y, currAngle.x, 0f);
    }
}