using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using Visartech.Levels;
using Visartech.Progress;

public class LevelController : MonoBehaviour
{
    private static LevelController _instance;
    public static LevelController Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType<LevelController>();

            return _instance;
        }
    }

    [SerializeField] private int _testLevel;
    [SerializeField] private bool _isTest = false;
    [SerializeField] private LevelsConfig _levels;
    [SerializeField] private int _endLevelsCount = 8;
    public PlayerController _controller;

    [SerializeField] private TextAsset _startPattern;
    [SerializeField] private TextAsset _endPattern;

    public ObjectsPool Pool;
    private LevelSpawner _spawner;

    private int _currentLevel = 0;

    void Start()
    {
        Pool = new ObjectsPool(transform);
        _spawner = new LevelSpawner(new GameObject("_LevelSpawnerTarget").transform, Pool);

        PlayLevel();
        
        _controller.Init(OnGameLoose);
    }

    public void PlayLevel()
    {
        bool endGame = false;
        
        _currentLevel = _isTest ? _testLevel : Progress.Player.CurrentLevel;
        
        if (_currentLevel >= _levels.LevelConfigs.Count)
        {
            _currentLevel = _levels.LevelConfigs.Count - 1;
            endGame = true;
        }
        
        var list = _levels.LevelConfigs[_currentLevel].TextAssets.ToList();

        if (Progress.Player.CurrentSlimes > 0 || Progress.Player.PlayedLevel || endGame)
            list = Reshuffle(list);

        if (endGame && list.Count > _endLevelsCount)
            list = list.GetRange(0, _endLevelsCount);

        list.Insert(0, _startPattern);
        list.Add(_endPattern);
        
        _spawner.LoadLevel(list);
    }

    private bool _isPlaying = false;
    
    void Update()
    {
        _spawner.OnDistanceChanged(_controller.transform.position);

        if (!_isPlaying && (Input.GetMouseButtonDown(0) || Input.touchCount > 0))
        {
            _isPlaying = true;
            _controller.Play();
        }
    }

    private bool _isFinishState = false;
    
    void OnGameLoose()
    {
        if (_isFinishState) return;

        var sequence = DOTween.Sequence();

        sequence.AppendInterval(2);
        sequence.onComplete = () => { 
            ReloadLevel();
            Progress.Player.PlayedLevel = true; 
        };
        _controller._isEnabled = false;
    }

    public void InitFinishState()
    {
        _isFinishState = true;
    }

    public void FinishResults(int slimesCount)
    {
        if (slimesCount >= _levels.LevelConfigs[_currentLevel].SlimesCount)
        {
            Progress.Player.CurrentLevel += 1;
            Progress.Player.CurrentSlimes = 0;
            Progress.Player.PlayedLevel = false;
        }
        else
            Progress.Player.CurrentSlimes = slimesCount;

        ReloadLevel();
    }
    
    public int GetTargetSlimesCount()
    {
        return _levels.LevelConfigs[_currentLevel].SlimesCount;
    }

    void ReloadLevel()
    {
        SceneManager.LoadScene(0);
    }
    
    List<TextAsset> Reshuffle(List<TextAsset> texts)
    {
        for (int t = 0; t < texts.Count; t++ )
        {
            var tmp = texts[t];
            var r = Random.Range(t, texts.Count);
            texts[t] = texts[r];
            texts[r] = tmp;
        }

        return texts;
    }
}
