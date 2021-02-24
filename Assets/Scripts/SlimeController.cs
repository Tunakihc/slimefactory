using System;
using Unity.Mathematics;
using UnityEngine;

public class SlimeController : PoolObject
{
    [HideInInspector]
    public bool IsActivated = false;

    public float Size = 0.5f;
    
    [Header("Movement")]
    [SerializeField] private Rigidbody _body;
    [SerializeField] private float _speed;
    [SerializeField] private float _maxAngularVelocity;
    [SerializeField] private float _maxTargetDistance = 10;
    [SerializeField] private LayerMask _deathObjects;
    [SerializeField] private LayerMask _slimeObjects;
    [SerializeField] private LayerMask _bonusObjects;
    [SerializeField] private GameObject _collisionEmmiter;
    [SerializeField] private Transform _view;

    [Header("Animation")]
    [SerializeField] private float _jumpHeight;
    [SerializeField] private AnimationCurve _jumpCurve;
    [SerializeField] private float _jumpTime;

    Transform _target;
    private Vector3 _prevPosition;
    private Vector3 _velocity;

    [HideInInspector]
    public PlayerController _controller;

    private void Awake()
    {
        _body.maxAngularVelocity = _maxAngularVelocity;
        _collisionEmmiter.SetActive(false);
        _body.isKinematic = true;
    }

    public void SetTarget(Transform target)
    {
        _target = target;
        IsActivated = true;
        _collisionEmmiter.SetActive(true);
    }

    void FixedUpdate()
    {
        if(_target == null) return;

        var dir = (_target.position - transform.position);
        dir.y = 0;
        dir = dir.normalized;

        var _power = Mathf.Clamp(Vector3.Distance(_body.position, _target.position) / 1, 0, 1);
        var targetVelocity = dir * (_speed * _power);

        targetVelocity.y = -9.8f;
        _body.velocity = Vector3.SmoothDamp(_body.velocity, targetVelocity, ref _velocity, 0.05f);

        if (Vector3.Distance(_body.position, _target.position) > _maxTargetDistance)
            Destroy();
    }

    private Vector3 _prevPos;
    private float _jumpProgress;
    void Update()
    {
        _view.rotation = quaternion.identity;

        var currentSpeed = Vector3.Distance(transform.position, _prevPos) / Time.deltaTime;

        if (currentSpeed > 0)
        {
            _jumpProgress += Time.deltaTime * (_jumpTime * currentSpeed);
            
            _view.position = Vector3.Lerp(transform.position, transform.position + Vector3.up * _jumpHeight,
                _jumpCurve.Evaluate(_jumpProgress));
        }

        _prevPos = transform.position;

        if (_jumpProgress > 1)
            _jumpProgress = 0;
    }

    private void OnCollisionEnter(Collision other)
    {
        var layer = other.gameObject.layer;

        if (_deathObjects == (_deathObjects | (1 << layer)))
            Destroy();

        if (_slimeObjects == (_slimeObjects | (1 << layer)) && !IsActivated)
            OnAddSlime(other.gameObject);
        
        if (_bonusObjects == (_bonusObjects | (1 << layer)))
            Debug.Log("Slime pick bonus");
    }

    void OnAddSlime(GameObject target)
    {
        var hitSlime = target.GetComponent<SlimeController>();

        if (hitSlime == null || hitSlime._controller == null) return;
        
        _controller = hitSlime._controller;
        _controller.AddSlime(this);
        _body.isKinematic = false;
    }

    public override void Destroy()
    {
        _controller?.RemoveSlime(this);
        
        base.Destroy();
    }

    public override void ResetState()
    {
        gameObject.SetActive(true);
        IsActivated = false;
        _body.isKinematic = true;
    }
}
