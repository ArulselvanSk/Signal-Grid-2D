using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCameraController : MonoBehaviour
{
    [SerializeField] private Transform PlayerTransform;

    void FixedUpdate()
    {
        gameObject.transform.position = PlayerTransform.position;
        gameObject.transform.rotation = PlayerTransform.rotation;

    }
}
