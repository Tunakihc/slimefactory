using System;
using Unity.Mathematics;
using UnityEngine;

public class SlimeController : MonoBehaviour
{
    [HideInInspector]
    public bool IsActivated = false;

    [SerializeField] private Rigidbody _body;
    [SerializeField] private float _speed;
    [SerializeField] private float _maxAngularVelocity;
    [SerializeField] private float _maxTargetDistance = 10;
    [SerializeField] private LayerMask _deathObjects;
    [SerializeField] private LayerMask _slimeObjects;
    [SerializeField] private LayerMask _bonusObjects;
    [SerializeField] private GameObject _collisionEmmiter;
    [SerializeField] private Transform _view;

    private Action<SlimeController> _onAddSlime;
    private Action<SlimeController> _onDeath;

    Transform _target;
    private Vector3 _prevPosition;
    private Vector3 _velocity;

    private void Awake()
    {
        _body.maxAngularVelocity = _maxAngularVelocity;
        _collisionEmmiter.SetActive(false);
    }

    public void Init(Action<SlimeController> onAddSlime,  Action<SlimeController> onDeath)
    {
        _onAddSlime = onAddSlime;
        _onDeath = onDeath;
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

        var _power = Mathf.Clamp(Vector3.Distance(_body.position, _target.position) / _maxTargetDistance, 0, 1);

        var targetVelocity = dir * (_speed * (1 + _power));
        
        targetVelocity.y = -9.8f;
        _body.velocity = Vector3.SmoothDamp(_body.velocity, targetVelocity, ref _velocity, 0.1f);

        if (Vector3.Distance(_body.position, _target.position) > _maxTargetDistance) 
            OnTargetLoose();
    }

    void Update()
    {
        _view.rotation = quaternion.identity;
    }

    private void OnCollisionEnter(Collision other)
    {
        var layer = other.gameObject.layer;

        if (_deathObjects == (_deathObjects | (1 << layer)))
            OnDeath();

        if (_slimeObjects == (_slimeObjects | (1 << layer)) && !IsActivated)
            OnAddSlime();
        
        if (_bonusObjects == (_bonusObjects | (1 << layer)))
            Debug.Log("Slime pick bonus");
    }

    void OnAddSlime()
    {
        _onAddSlime?.Invoke(this);   
    }

    void OnTargetLoose()
    {
        _onDeath?.Invoke(this);
        _target = null;
    }

    void OnDeath()
    {
        _onDeath?.Invoke(this);
        gameObject.SetActive(false);
    }
}
