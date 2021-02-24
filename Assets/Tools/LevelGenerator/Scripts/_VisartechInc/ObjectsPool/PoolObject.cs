﻿using UnityEngine;

public abstract class PoolObject : PoolObjectTransform
{
    public virtual string SerializeSettings()
    {
        var infoClass = new PoolObjectInfo();

        GetTransformInfo(infoClass);

        infoClass.SelfDestroy = SelfDestroy;

        return Helpers.XMLHelper.Serialize(infoClass);
    }

    public virtual void AcceptSettings(string info)
    {
        var infoClass = Helpers.XMLHelper.Deserialize<PoolObjectInfo>(info);

        AcceptTransformInfo(infoClass);

        SelfDestroy = infoClass.SelfDestroy;
    }

    public bool SelfDestroy = false;

    public abstract void ResetState();

    public virtual void Destroy() {
        ResetState();
    
        IsDestroying = false;
        IsActive = false;
    }

    [HideInInspector]
    public bool IsDestroying = false;

    public virtual bool IsActive {
        get { return gameObject.activeSelf; }
        set {gameObject.SetActive(value);}
    }
}
