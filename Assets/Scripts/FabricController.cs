using System;
using System.Collections.Generic;
using Cinemachine;
using DG.Tweening;
using PathCreation;
using UnityEngine;
using Visartech.Progress;

public class FabricController : PoolObject
{
    [Header("Components")]
    [SerializeField] private PathCreator _path;
    [SerializeField] private Transform _sucker;
    [SerializeField] private Transform _suckerTransform;
    [SerializeField] GameObject[] _finishParticless;
    [SerializeField] private GameObject[] _workParticless;
    [SerializeField] private GameObject[] _steamParticless;
    [SerializeField] private GameObject _slimeParticle;
    [SerializeField] private Material _liquidMaterial;
    [SerializeField] private Transform _liquidTop;
    [SerializeField] private CinemachineVirtualCamera _camera;
    
    [Header("Settings")]
    [SerializeField] private float _idleScaleSpeed;
    [SerializeField] private Vector3 _idleScaleEnd;
    [SerializeField] private float _insertAnimTime;
    [SerializeField] private Vector3 _insertAnimScale;
    [SerializeField] private Ease _insertEasing;
    [SerializeField] private float _minHeigh;
    [SerializeField] private float _maxHeight;
    [SerializeField] private float _liquidTopOffset;

    private Sequence _animation;
    private Sequence _liquidAnimation;

    private Dictionary<SlimeController, Sequence> _slimesInside = new Dictionary<SlimeController,Sequence>();
    List<GameObject> _slimeParticles = new List<GameObject>();

    private int _currentSlimeMass = 0;
    private int _targetSlimeMass;
    private bool _isFinished = false;
    
    private Vector3 _startScale;
    private void Awake()
    {
        _startScale = _sucker.localScale;
    }

    void StartAnim()
    {
        _animation?.Kill();
        _animation = DOTween.Sequence();

        _sucker.localScale = _startScale;
        
        _animation.Insert(0, _sucker.DOScale(_idleScaleEnd, _idleScaleSpeed).SetEase(Ease.InBack));
        _animation.Insert(_idleScaleSpeed, _sucker.DOScale(_startScale, _idleScaleSpeed).SetEase(Ease.OutBack));

        _animation.SetLoops(-1);
        
        _animation.Play();
    }

    private void OnTriggerEnter(Collider other)
    {
        var slime = other.GetComponent<SlimeController>();
        
        if(slime == null) return;

        if (!_isFinished)
        {
            LevelController.Instance.InitFinishState();
            _camera.Priority = 11;
            _isFinished = true;
            
            for (int i = 0; i < _steamParticless.Length; i++)
                _steamParticless[i].SetActive(true);
        }

        InsertSlime(slime);
    }

    void InsertSlime(SlimeController slime)
    {
        if(_slimesInside.ContainsKey(slime)) return;
        
        slime.SetFinish();

        var sequence = DOTween.Sequence();
        
        sequence.Append(slime.transform.DOMove(_suckerTransform.position, _insertAnimTime + _slimesInside.Count * 0.1f).SetEase(_insertEasing));
        sequence.Insert(0,slime.transform.DOScale(_insertAnimScale, _insertAnimTime + _slimesInside.Count * 0.1f).SetEase(_insertEasing));
        
        sequence.AppendCallback(()=>
        {
            StartAnim();
            slime.Destroy();
        });
        
        sequence.AppendInterval(0.1f);

        var particle = GetParticle();
        particle.transform.position = _path.path.GetPointAtTime(0);

        var t = 0f;

        var movement = DOTween.To(() => t, (x) => t = x, 1, 1).SetEase(Ease.InSine);

        movement.onUpdate = () => { particle.transform.position = _path.path.GetPointAtTime(t, EndOfPathInstruction.Stop); };
        sequence.Append(movement);

        var duration = Vector3.Distance(_path.path.GetPointAtTime(1, EndOfPathInstruction.Stop), _liquidTop.position) / 25;

        sequence.Append(particle.transform.DOMove(_liquidTop.position, duration).SetEase(Ease.Linear));
        sequence.onComplete = () =>
        {
            _currentSlimeMass += slime.SlimeMass;
            var progress = (float) _currentSlimeMass / _targetSlimeMass;
            
            PlayLiquidAnim(progress);
            
            LevelController.Instance.Pool.GetObjectOfType<ParticlePoolObject>("SlimeSplash").transform.position = _liquidTop.position;
            
            particle.gameObject.SetActive(false);
        };
        
        _slimesInside.Add(slime, sequence);
    }

    void PlayLiquidAnim(float t)
    {
        _liquidAnimation?.Kill();
        _liquidAnimation = DOTween.Sequence();

        var currentProgress = _liquidMaterial.GetFloat("Vector1_8d1c1cd0d86844fe92c5faf600825729");
        currentProgress = (currentProgress - _minHeigh) / (_maxHeight - _minHeigh);
        
        var anim = DOTween
            .To(() => currentProgress, (x) => currentProgress = x,  t, 0.3f)
            .SetEase(Ease.Linear);
        
        anim.onUpdate = () =>
        {
            _liquidMaterial.SetFloat("Vector1_8d1c1cd0d86844fe92c5faf600825729", Mathf.Lerp(_minHeigh, _maxHeight,currentProgress));
            _liquidTop.position = transform.position + Vector3.up * (Mathf.Lerp(_minHeigh, _maxHeight,currentProgress) + _liquidTopOffset);
        };
        _liquidAnimation.Insert(0, anim);
        
        _liquidAnimation.AppendCallback(() =>
        {
            for (int i = 0; i < _workParticless.Length; i++)
                _workParticless[i].SetActive(false);

            if (!(t >= 1)) return;
            
            for (int i = 0; i < _finishParticless.Length; i++)
                _finishParticless[i].SetActive(true);
        });
        
        _liquidAnimation.AppendInterval(t >= 1 ? 3 : 1);
        _liquidAnimation.AppendCallback(() => { LevelController.Instance.FinishResults(_currentSlimeMass); });
    }

    GameObject GetParticle()
    {
        for (int i = 0; i < _slimeParticles.Count; i++)
        {
            if(_slimeParticles[i].activeSelf) continue;
            
            _slimeParticles[i].SetActive(true);
            
            return _slimeParticles[i];
        }

        var newParticle = Instantiate(_slimeParticle, transform);
        
        _slimeParticles.Add(newParticle);
        newParticle.SetActive(true);

        return newParticle;
    }

    public override void ResetState()
    {
        _animation?.Kill();
        _liquidAnimation?.Kill();
        
        _camera.Priority = 0;
        
        for (int i = 0; i < _finishParticless.Length; i++)
            _finishParticless[i].SetActive(false);
        
        for (int i = 0; i < _workParticless.Length; i++)
            _workParticless[i].SetActive(true);
        
        for (int i = 0; i < _steamParticless.Length; i++)
            _steamParticless[i].SetActive(false);

        _slimesInside.Clear();
        
        _currentSlimeMass = Progress.Player.CurrentSlimes;
        _targetSlimeMass = LevelController.Instance.GetTargetSlimesCount();
        var progress = (float) _currentSlimeMass / _targetSlimeMass;
        _liquidMaterial.SetFloat("Vector1_8d1c1cd0d86844fe92c5faf600825729",Mathf.Lerp(_minHeigh, _maxHeight, progress));
        
        _isFinished = false;
        
        StartAnim();
    }
}
