using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerController : MonoBehaviour
{
    public class SlimeInfo
    {
        public SlimeInfo(SlimeController controller, Transform target)
        {
            Slime = controller;
            Slime.IsActivated = true;
            
            Target = target;
            
            Slime.SetTarget(Target);
        }

        public SlimeController Slime;
        public Transform Target;
    }

    [SerializeField] private SlimeController[] _startSlimes;
    [SerializeField] private float _speed;
    [SerializeField] private Vector3 _inputBounds;
    [SerializeField] private float _curve;
    
    List<SlimeInfo> _slimes = new List<SlimeInfo>();
    List<Transform> _transforms = new List<Transform>();

    public bool _isEnabled;

    private Transform _slimesControllerObject;

    private Action _onDeath;

    void Start()
    {
        Input.simulateMouseWithTouches = true;
        
        _slimesControllerObject = new GameObject("SlimesController").transform;
        _slimesControllerObject.SetParent(transform);
        _slimesControllerObject.localPosition = Vector3.zero;
    }

    public void Init(Action onDeath)
    {
        _onDeath = onDeath;
    }

    public void Play()
    {
        _isEnabled = true;
        
        for (int i = 0; i < _startSlimes.Length; i++)
        {
            _startSlimes[i]._controller = this;
            AddSlime(_startSlimes[i]);
            _startSlimes[i].GetComponent<Rigidbody>().isKinematic = false;
        }
    }

    private void Update()
    {
        if(_slimesControllerObject == null || !_isEnabled) return;
        
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
        
        UpdateSlimePositions();
    }

    void UpdateSlimePositions()
    {
        var inputBounds = new Bounds(transform.position, _inputBounds);

        if (isTouched)
        {

            float radius = 0;
            float angleStep = 0;
            var prevSlimesCount = 0;
            var slimesCount = 0f;

            var _positions = new List<Vector3>();

            for (int i = 0; i < _slimes.Count; i++)
            {
                if (slimesCount + prevSlimesCount < i)
                {
                    radius += _slimes[prevSlimesCount].Slime.Size;
                    slimesCount = ((2 * Mathf.PI * radius) / _slimes[i].Slime.Size);
                    angleStep = 360f / slimesCount;

                    angleStep *= Mathf.Deg2Rad;
                    prevSlimesCount = i;
                }

                var targetPos = _slimesControllerObject.position  + new Vector3(
                                    radius * Mathf.Cos(angleStep * (i - prevSlimesCount)), 0,
                                    radius * Mathf.Sin(angleStep * (i - prevSlimesCount)));

                targetPos = inputBounds.ClosestPoint(targetPos);
                
                _positions.Add(targetPos);
            }
            
            _positions.Sort((p1, p2) => p1.z > p2.z ? 1 : (p1.z == p2.z ? 0 : -1));

            for (var i = 0; i < _slimes.Count; i++)
                _slimes[i].Target.position = _positions[i];
        }
        else
        {
            var a = 0f;
            var row = 0;
            for (int i = 0; i < _slimes.Count; i++)
            {
                var targetPos = _slimesControllerObject.position + Vector3.right * (a * (i % 2 == 0 ? 1 : -1)) + Vector3.back * row;

                targetPos = inputBounds.ClosestPoint(targetPos);
                
                _slimes[i].Target.position = targetPos;

                if (a >= _inputBounds.x)
                {
                    row -= 1;
                    a = 0;
                }

                if (i % 2 == 0)
                    a += _slimes[i].Slime.Size;
            }
        }
    }

    public void AddSlime(SlimeController controller)
    {
        var newSlime = new SlimeInfo(controller, GetTransform());
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

        if (_slimes.Count <= 0)
            _onDeath?.Invoke();
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

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        
        for (int i = 0; i < _slimes.Count; i++)
        {
            if(i == 0) continue;
            
            Gizmos.DrawWireSphere(_slimes[i].Target.position,_slimes[i].Slime.Size/2);
        
        }
    }
}
