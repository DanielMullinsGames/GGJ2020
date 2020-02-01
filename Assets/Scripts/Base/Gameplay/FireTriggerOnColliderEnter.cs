using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

public class FireTriggerOnColliderEnter : MonoBehaviour
{
    public List<UnityEvent> TriggeredEvents;
    public List<UnityEvent> ExitEvents;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        foreach (var ev in TriggeredEvents)
            ev.Invoke();
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        foreach (var ev in ExitEvents)
            ev.Invoke();
    }

}
