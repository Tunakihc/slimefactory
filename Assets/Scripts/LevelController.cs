using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Visartech.Levels;

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
    [SerializeField] private PlayerController _controller;

    [SerializeField] private TextAsset _startPattern;
    [SerializeField] private TextAsset _endPattern;

    public ObjectsPool Pool;
    private LevelSpawner _spawner;

    private int _currentLevel = 0;

    void Start()
    {
        Pool = new ObjectsPool(transform);
        _spawner = new LevelSpawner(new GameObject("_LevelSpawnerTarget").transform, Pool);

        _currentLevel = _isTest ? _testLevel : (PlayerPrefs.HasKey("CurrentLevel") ? PlayerPrefs.GetInt("CurrentLevel") : 0);

        PlayLevel(_levels.LevelConfigs[_currentLevel]);
        
        _controller.Init(OnGameLoose);
    }

    public void PlayLevel(LevelConfig config)
    {
        var list = config.TextAssets.ToList();

        if (GetCurrentSlimesCount() > 0 || (PlayerPrefs.HasKey("PlayedLevel") && PlayerPrefs.GetInt("PlayedLevel") > 0))
            list = Reshuffle(list);

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
        if (!_isFinishState)
        {
            ReloadLevel();
            PlayerPrefs.SetInt("PlayedLevel",1);
        }
    }
    
    public void InitFinishState()
    {
        _isFinishState = true;
    }

    public void FinishResults(int slimesCount)
    {
        if (slimesCount >= _levels.LevelConfigs[_currentLevel].SlimesCount)
        {
            PlayerPrefs.SetInt("CurrentLevel", _currentLevel++);
            PlayerPrefs.SetInt("CurrentSlimesCount", 0);
            PlayerPrefs.SetInt("PlayedLevel", 0);
        }
        else
        {
            PlayerPrefs.SetInt("CurrentSlimesCount", slimesCount);
        }

        ReloadLevel();
    }
    
    public int GetTargetSlimesCount()
    {
        return _levels.LevelConfigs[_currentLevel].SlimesCount;
    }
    
    public int GetCurrentSlimesCount()
    {
        return PlayerPrefs.HasKey("CurrentSlimesCount") ? PlayerPrefs.GetInt("CurrentSlimesCount") : 0;
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
