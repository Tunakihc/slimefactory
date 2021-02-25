using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class AnchoredPositionObject : MonoBehaviour
{
    [SerializeField] private GameObject _targetObject;
    [SerializeField] private Vector3 _anchorPosition = new Vector3(0.5f, 0.5f, 0.5f);

    void Update()
    {
        if(_targetObject == null) return;

        var parent = _targetObject.GetMaxBounds();

        transform.position =new Vector3(Mathf.Lerp(parent.min.x, parent.max.x, _anchorPosition.x),
            Mathf.Lerp(parent.min.y, parent.max.y, _anchorPosition.y),
            Mathf.Lerp(parent.min.z, parent.max.z, _anchorPosition.z));

    }
}
