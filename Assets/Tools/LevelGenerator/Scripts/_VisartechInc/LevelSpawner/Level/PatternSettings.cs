#if UNITY_EDITOR
using System.Collections.Generic;
using Unity.Collections;
using UnityEditor;
using UnityEngine;

public class PatternSettings : MonoBehaviour
{
    public Vector3 MinSize;
    public Vector3 MaxSize;
    public bool FixCenter;
    public Vector3 Center;
    [ReadOnly]
    public Vector3 Size;

    public Transform _StartPoint;
    public Transform _EntPoint;

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        var rect = gameObject.GetMaxBounds();

        Vector3 center = FixCenter ? Center : rect.center;
        
        Gizmos.color = Color.red;
        
        rect.size = new Vector3(Mathf.Clamp(rect.size.x, MinSize.x, MaxSize.x),
            Mathf.Clamp(rect.size.y, MinSize.y, MaxSize.y),
            Mathf.Clamp(rect.size.z, MinSize.z, MaxSize.z));

        if (MaxSize.x < MinSize.x)
            MaxSize.x = MinSize.x;
        
        if (MaxSize.y < MinSize.y)
            MaxSize.y = MinSize.y;
        
        if (!FixCenter)
            Center = rect.center;

        Size = rect.size;

  

        Gizmos.DrawWireCube(center, rect.size);
    }
    
}
#endif
