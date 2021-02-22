using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerController : MonoBehaviour
{
    public class SlimeInfo
    {
        public SlimeInfo(SlimeController controller, Transform target)
        {
            Slime = controller;
            Target = target;
            
            Slime.SetTarget(Target);
        }

        public SlimeController Slime;
        public Transform Target;
    }

    [SerializeField] private SlimeController[] _startSlimes;
    [SerializeField] private float _speed;
    [SerializeField] private Vector3 _minBounds;
    [SerializeField] private Vector3 _maxBounds;
    [SerializeField] private Vector3 _inputBounds;
    
    List<SlimeInfo> _slimes = new List<SlimeInfo>();
    List<Transform> _transforms = new List<Transform>();

    private Transform _slimesControllerObject;

    void Awake()
    {
        Input.simulateMouseWithTouches = true;
        
        _slimesControllerObject = new GameObject("SlimesController").transform;
        _slimesControllerObject.SetParent(transform);
        _slimesControllerObject.localPosition = Vector3.zero;
        
        for (int i = 0; i < _startSlimes.Length; i++)
        {
            AddSlime(_startSlimes[i]);
        }
    }

    private void Update()
    {
        if(_slimesControllerObject == null) return;
        
        transform.position += transform.forward * Time.deltaTime * _speed;

        UpdateInput();
    }

    private bool isTouched = false;
    private bool prevInput = true;

    void UpdateInput()
    {
        isTouched = Input.touchCount > 0 || Input.GetMouseButton(0);

        var pos = transform.position;

        if (isTouched)
        {
            var touchPos = Input.touchCount > 0 ? Input.touches[0].position : ((Vector2)Input.mousePosition);
            
            var screenPos = new Vector3(touchPos.x, touchPos.y,Vector3.Distance(Camera.main.transform.position, transform.position));
            
            pos = Camera.main.ScreenToWorldPoint(screenPos);

            var inputBounds = new Bounds(transform.position, _inputBounds);

            pos = inputBounds.ClosestPoint(pos);
        }

        _slimesControllerObject.localPosition = transform.InverseTransformPoint(pos);

        if (prevInput == isTouched) return;
        
        for (int i = 0; i < _slimes.Count; i++)
            SetSlimePos(_slimes[i]);

        prevInput = isTouched;
    }

    void SetSlimePos(SlimeInfo info)
    {
        var currentBounds = isTouched ? _minBounds : _maxBounds;
        var bounds = new Bounds(_slimesControllerObject.position, currentBounds);

        info.Target.position = RandomPointInBounds(bounds);
    }

    public void AddSlime(SlimeController controller)
    {
        var newSlime = new SlimeInfo(controller, GetTransform());
        SetSlimePos(newSlime);
        
        _slimes.Add(newSlime);
    }

    public void RemoveSlime(SlimeController controller)
    {
        for (int i = 0; i < _slimes.Count; i++)
        {
            if (_slimes[i].Slime != controller) continue;
            
            _slimes[i].Target.gameObject.SetActive(false);
            
            _slimes.RemoveAt(i);
            
            break;
        }
    }

    Transform GetTransform()
    {
        for (int i = 0; i < _transforms.Count; i++)
        {
            if(_transforms[i].gameObject.activeSelf) continue;
            
            _transforms[i].gameObject.SetActive(true);

            return _transforms[i];
        }
        
        var newTransform = new GameObject("_SlimeTarget_"+_transforms.Count).transform;
        newTransform.SetParent(_slimesControllerObject);
        newTransform.localPosition = Vector3.zero;
        
        _transforms.Add(newTransform);
        
        return newTransform;
    }
    
    public Vector3 RandomPointInBounds(Bounds bounds) {
        return new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            Random.Range(bounds.min.y, bounds.max.y),
            Random.Range(bounds.min.z, bounds.max.z)
        );
    }
}
