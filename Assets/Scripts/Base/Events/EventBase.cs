using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class EventBase
{
    public const int kApproxNumberOfDerivedTypes = 200;

    protected EventBase()
    {
    }

    public virtual int GetChecksum()
    {
        return 0;
    }
}