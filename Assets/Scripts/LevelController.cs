using System.Linq;
using UnityEngine;
using Visartech.Levels;

public class LevelController : MonoBehaviour
{
    [SerializeField] private int _testLevel;
    [SerializeField] private LevelsConfig _levels;
    [SerializeField] private PlayerController _controller;

    [SerializeField] private TextAsset _startPattern;
    [SerializeField] private TextAsset _endPattern;

    private ObjectsPool _pool;
    private LevelSpawner _spawner;

    void Start()
    {
        _pool = new ObjectsPool(transform);
        _spawner = new LevelSpawner(new GameObject("_LevelSpawnerTarget").transform, _pool);
        
        PlayLevel(_levels.LevelConfigs[_testLevel]);
    }

    public void PlayLevel(LevelConfig config)
    {
        var list = config.TextAssets.ToList();
        list.Insert(0, _startPattern);
        list.Add(_endPattern);
        
        _spawner.LoadLevel(list);
        _controller.Play();
    }

    void Update()
    {
        _spawner.OnDistanceChanged(_controller.transform.position);
    }
}
