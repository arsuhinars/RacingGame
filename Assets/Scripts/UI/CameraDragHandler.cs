using UnityEngine;
using UnityEngine.EventSystems;

public class CameraDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private const float LastPositionUpdateTime = 0.1f;

    public LookAtCamera lookAtCamera;
    public Vector2 sensivity = new(1f, 1f);
    public Vector2 gravity = new(1f, 1f);
    public float maxInertia = 1f;
    public float maxPitch = 90f;
    public float minPitch = -90f;

    private Vector2 lastPos;
    private float lastPosUpdateTime;
    private Vector2 inertia;
    private bool isDragging = false;

    private void Update()
    {
        if (!isDragging)
        {
            lookAtCamera.angle += inertia * Time.unscaledDeltaTime;
            lookAtCamera.angle.y = Mathf.Clamp(lookAtCamera.angle.y, minPitch, maxPitch);

            var deltaInertia = Time.unscaledDeltaTime * gravity * inertia.normalized;
            if (Mathf.Abs(inertia.x) > Mathf.Abs(deltaInertia.x))
                inertia.x -= deltaInertia.x;
            else
                inertia.x = 0f;

            if (Mathf.Abs(inertia.y) > Mathf.Abs(deltaInertia.y))
                inertia.y -= deltaInertia.y;
            else
                inertia.y = 0f;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        lastPos = eventData.position;
        lastPosUpdateTime = Time.unscaledTime;
    }

    public void OnDrag(PointerEventData eventData)
    {
        lookAtCamera.angle += eventData.delta * sensivity;
        lookAtCamera.angle.y = Mathf.Clamp(lookAtCamera.angle.y, minPitch, maxPitch);

        if (Time.unscaledTime - lastPosUpdateTime > LastPositionUpdateTime)
        {
            lastPos = eventData.position;
            lastPosUpdateTime = Time.unscaledTime;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        inertia = (eventData.position - lastPos) * sensivity / (Time.unscaledTime - lastPosUpdateTime);

        if (inertia.sqrMagnitude > maxInertia * maxInertia)
        {
            inertia = inertia.normalized * maxInertia;
        }
    }
}
