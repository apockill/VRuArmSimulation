using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System;

class CollisionReporter : MonoBehaviour
{
    [Serializable] private class ColliderEvent : UnityEvent<Collision> { }
    [SerializeField] private ColliderEvent m_OnCollisionEnter = new ColliderEvent();
    [SerializeField] private ColliderEvent m_OnCollisionExit  = new ColliderEvent();

    void OnCollisionEnter(Collision other)
    {
        m_OnCollisionEnter.Invoke(other);
    }

    void OnCollisionExit(Collision other)
    {
        m_OnCollisionExit.Invoke(other);
    }
}

