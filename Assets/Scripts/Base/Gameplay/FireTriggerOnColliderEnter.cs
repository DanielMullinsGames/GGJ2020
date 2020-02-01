using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

public class FireTriggerOnColliderEnter : MonoBehaviour
{
    public List<UnityEvent> TriggeredEvents;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        foreach (var ev in TriggeredEvents)
            ev.Invoke();
    }

}
