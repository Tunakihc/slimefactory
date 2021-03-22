using DG.Tweening;
using UnityEngine;

public class BottleObject : PoolObject
{
    [Header("Components")]
    [SerializeField] private MeshRenderer _water;

    [SerializeField] private Rigidbody _body;

    [Header("Settings")]
    [SerializeField] private int _slimesTargetMass = 1;
    [SerializeField] private float _mimHeigh;
    [SerializeField] private float _maxHeigh;
    [SerializeField] private float _fillTIme = 0.1f;
    [SerializeField] private bool _isDisapearing;
    [SerializeField] private bool _isDinamic;
    [SerializeField] private float _activationDistance = 5;
    [SerializeField] private int _rewardCoins = 1;
    [SerializeField] private float _revardCoinSpawnHeigh;

    Material _bottleMaterial;
    
    bool _isFull;
    private int _currentMass = 0;

    private Transform _target;
    
    private void Awake()
    {
        _bottleMaterial = Instantiate<Material>(_water.material);
        _water.material = _bottleMaterial;
        _target = LevelController.Instance._controller.transform;
    }

    void Update()
    {
        if (_isDinamic && Vector3.Distance(transform.position, _target.position) <= _activationDistance)
            _body.isKinematic = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(_isFull) return;
        
        OnObject(other.gameObject);
    }

    private void OnCollisionEnter(Collision other)
    {
        if(_isFull) return;
        
        OnObject(other.gameObject);
    }

    void OnObject(GameObject obj)
    {
        var slime = obj.GetComponent<SlimeController>();
        
        if(slime == null) return;

        var mass = slime.SlimeMass;

        slime.Destroy();
        
        _currentMass += mass;

        UpdateBottleView();
    }

    private Sequence _fillAnim;
    
    void UpdateBottleView()
    {
        var progress = _currentMass / _slimesTargetMass;
        progress = Mathf.Clamp(progress, 0, 1);

        if (progress >= 1)
            _isFull = true;

        var targetHeigh = Mathf.Lerp(_mimHeigh, _maxHeigh, progress);
        var currentHeigh = _bottleMaterial.GetFloat("Vector1_8d1c1cd0d86844fe92c5faf600825729");
        
        _fillAnim?.Kill();
        _fillAnim = DOTween.Sequence();

        var fillTween = DOTween.To(() => currentHeigh, (x) => currentHeigh = x, targetHeigh, _fillTIme);
        fillTween.onUpdate = () =>
        {
            _bottleMaterial.SetFloat("Vector1_8d1c1cd0d86844fe92c5faf600825729", currentHeigh);
        };
        
        _fillAnim.Append(fillTween);
        _fillAnim.AppendInterval(0.1f);

        _fillAnim.onComplete = () =>
        {
            if (progress >= 1)
            {
                for (int i = 0; i < _rewardCoins; i++)
                    SpawnCoin();

                var destroyFX = LevelController.Instance.Pool.GetObjectOfType<ParticlePoolObject>("BottleDestroy");
                destroyFX.transform.position = transform.position;

                if (_isDisapearing)
                    Destroy();
            }
        };
    }


    void SpawnCoin()
    {
        var coin = LevelController.Instance.Pool.GetObjectOfType<Coin>("Coin");
        // var startScale = coin.transform.localScale;
        coin.transform.localScale = Vector3.zero;
        coin.transform.position = transform.position;

        Vector3 targetPos = transform.position;
        targetPos.x = 0;
        targetPos.y = _revardCoinSpawnHeigh;
        targetPos += Vector3.forward * Random.Range(15,18);
        targetPos += Vector3.Lerp(Vector3.right * Random.Range(2f, 4f), Vector3.left * Random.Range(2f, 4f),
            Random.Range(0f, 1f));
        
        coin.transform.DOJump(targetPos, 3, 1, 1f);
        coin.transform.DOScale(Vector3.one, 1f);
    }

    public override void ResetState()
    {
        _isFull = false;
        _currentMass = 0;
        _bottleMaterial.SetFloat("Vector1_8d1c1cd0d86844fe92c5faf600825729", 0);
        _body.isKinematic = true;
    }
}
