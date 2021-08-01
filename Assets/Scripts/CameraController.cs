using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;
    public float distance;
    public float heightOffset;

    public Vector2 sensitivity;
    public Vector2 smoothing;

    private Vector2 position;
    private Vector2 mousePosition;

    internal Vector3 rawPosition;

    private void Start()
    {
        if (sensitivity == Vector2.zero) sensitivity = new Vector2(1.5f, 1f);
        if (smoothing == Vector2.zero) smoothing = Vector2.one * 3f;
    }

    internal void updatePosition(float x, float y) 
    {
        Vector2 update = Vector2.zero;
        update.x = y * (sensitivity.x * smoothing.x);
        update.y = x * (sensitivity.y * smoothing.y);

        mousePosition.x = Mathf.Lerp(mousePosition.x, update.x, 1f / smoothing.x);
        mousePosition.y = Mathf.Lerp(mousePosition.y, update.y, 1f / smoothing.y);

        position.x += mousePosition.x;
        position.y += mousePosition.y;

        //Clamp the Y value for vertical rotation
        position.x = Mathf.Clamp(position.x, -85f, 85f);
    }

    void LateUpdate()
    {
        var newRotation = Quaternion.Euler(position.x, position.y, 0);
        var newPosition = newRotation * new Vector3(0, heightOffset, -distance) + target.position;
        var newDirection = target.position - newPosition;

        transform.rotation = Quaternion.LookRotation(newDirection, Vector3.up);
        transform.position = newPosition + new Vector3(0, heightOffset, 0);

        rawPosition = newPosition;
    }
}
