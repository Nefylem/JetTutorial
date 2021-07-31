using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;
    public float distance;
    public float heightOffset;

    private Vector2 position;

    internal void updatePosition(float x, float y) 
    {
        position.x += y;
        position.y += x;
    }

    void LateUpdate()
    {
        transform.rotation = Quaternion.Euler(position.x, position.y, 0);
        transform.position = transform.rotation * new Vector3(0, heightOffset, -distance) + target.position;
    }
}
