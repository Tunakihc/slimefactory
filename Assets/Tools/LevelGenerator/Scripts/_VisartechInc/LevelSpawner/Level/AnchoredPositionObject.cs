using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class AnchoredPositionObject : MonoBehaviour
{
    [SerializeField] private GameObject _targetObject;
    [SerializeField] private Vector3 _anchorPosition;

    void Update()
    {
        if(_targetObject == null) return;

        var parent = _targetObject.GetMaxBounds();

        transform.position = _targetObject.transform.position + new Vector3((parent.size.x/2) * _anchorPosition.x,
            (parent.size.y/2) * _anchorPosition.y,
            (parent.size.z/2) * _anchorPosition.z);

    }
}
