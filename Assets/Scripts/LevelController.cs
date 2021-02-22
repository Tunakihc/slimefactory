using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelController : MonoBehaviour
{
    [SerializeField] private SlimeController[] _slimes;
    [SerializeField] private PlayerController _controller;
    [SerializeField] private float _levelDistance = 200;
    
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < _slimes.Length; i++)
        {
            if(_slimes[i] != null)
                _slimes[i].Init(_controller.AddSlime, _controller.RemoveSlime);       
        }
    }

    void Update()
    {
        if (_controller.transform.position.z > _levelDistance)
            SceneManager.LoadScene(0);
    }
}
