using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class Pickup : MonoBehaviour
{
    public Collider _Collider;
    public Rigidbody _Rigidbody;
    public Vector3 pPickupSize;

    /// <summary>
    /// _Collider and _Rigidbody should be set in the editor instead of awake
    /// But just in case they aren't, we'll assign them here and give a warning.
    /// </summary>
    private void Awake()
    {
        if (!_Collider)
        {
            Debug.LogWarning("_Collider not set for " + gameObject.name + "! Assigning now...");
            _Collider = GetComponent<BoxCollider>();
        }

        if (!_Rigidbody)
        {
            Debug.LogWarning("_Rigidbody not set for " + gameObject.name + "! Assigning now...");
            _Rigidbody = GetComponent<Rigidbody>();
        }
    }
}
