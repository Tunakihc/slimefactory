using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Visartech.Levels
{
    public class LevelSpawner
    {
        public LevelSpawner(Transform targetingObject, ObjectsPool pool)
        {
            _pool = pool;
            _targetingObject = targetingObject;
        }

        const int SpawnRadius = 200;
        const int ClearRadius = 10;
        
        static readonly Vector3 StartPos = new Vector3(0, 0, -5);
        
        ObjectsPool _pool;

        readonly HashSet<PoolObject> _spawnedObjects = new HashSet<PoolObject>();
        readonly HashSet<PoolObject> _decorObjects = new HashSet<PoolObject>();
        readonly HashSet<PoolObject> _excludedObjects = new HashSet<PoolObject>();
        
        Transform _targetingObject;
        
        Vector3 _currentPoint;
        Vector3 _locationStartPoint;
        Vector3 _spawnPoint;
        Vector3 _currentDir;

        private List<TextAsset> _currentLevelInfo;
        PatternObjectInfo _currentPattern;

        public void LoadLevel(List<TextAsset> _patterns)
        {
            ResetLevel();

            _targetingObject.position = StartPos;
            _currentPattern = null;
            
            _currentLevelInfo = _patterns;
        }
        
        public void ResetLevel()
        {
            foreach (var decorObject in _decorObjects)
                decorObject.Destroy();
            
            _decorObjects.Clear();
            
            foreach (var decorObject in _decorObjects)
                decorObject.Destroy();
            
            _decorObjects.Clear();
        }

        public void OnDistanceChanged(Vector3 value)
        {
            _currentPoint = value;
            
            if(_currentLevelInfo.Count > 0)
                CheckPattern();

            UpdateLevel();
        }

        void CheckPattern()
        {
            if (_currentPattern == null)
            {
                GetNewPattern();

                _spawnPoint = StartPos;
                
                _currentDir = _currentPattern.Dir;
                _targetingObject.forward = _currentDir;
                _targetingObject.position = _spawnPoint;

                return;
            }

            if (_currentPattern.Childs != null && _currentPattern.Childs.Count != 0)
                return;

            var prevPatternEndPos = _targetingObject.TransformPoint(_currentPattern.EndPoint);

            GetNewPattern();

            _spawnPoint = prevPatternEndPos;
            
            _currentDir = _targetingObject.TransformDirection(_currentPattern.Dir);
            
            _targetingObject.forward = _currentDir;
            _targetingObject.position = _spawnPoint;
        }

        void UpdateLevel()
        {
            SpawnPattern();

            ClearUnusedObjects(_spawnedObjects, ClearRadius + SpawnRadius);
            ClearUnusedObjects(_decorObjects, ClearRadius + SpawnRadius);
        }

        void ClearUnusedObjects(HashSet<PoolObject> targetList, float checkDistance)
        {
            _excludedObjects.Clear();

            foreach (var obj in targetList)
            {
                if (!(Vector3.Distance(_currentPoint, obj.gameObject.GetMaxBounds().ClosestPoint(_currentPoint)) >
                      checkDistance)) continue;
                
                obj.Destroy();
                _excludedObjects.Add(obj);
            }

            targetList.ExceptWith(_excludedObjects);
        }

        void SpawnPattern()
        {
            if (_currentPattern == null)
                return;

            for (var i = 0; i < _currentPattern.Childs.Count; i++)
            {
                if (!(Vector3.Distance(_currentPoint, _currentPoint + (SpawnRadius * _currentDir)) >
                      Vector3.Distance(_currentPoint, _spawnPoint + _currentPattern.Childs[i].LocalPosition))) continue;
                
                var obj = SpawnPatternObject(_currentPattern.Childs[i]);

                if (obj != null && !obj.SelfDestroy)
                    _spawnedObjects.Add(obj);

                _currentPattern.Childs.RemoveAt(i);
            }
        }

        PoolObject SpawnPatternObject(ChildInfo info)
        {
            var obj = _pool.GetObjectOfType<PoolObject>(info.ObjectName);

            if (obj == null)
            {
                Debug.LogError("There is no " + info.ObjectName + " object in pool!");

                return null;
            }

            obj.transform.position = _targetingObject.TransformPoint(info.LocalPosition);
            obj.transform.localEulerAngles = info.LocalRotation + _targetingObject.eulerAngles;
            obj.transform.localScale = info.LocalScale;
            
            obj.AcceptSettings(info.Settings);

            return obj;
        }

        void GetNewPattern()
        {
            _currentPattern = ObjectSerializer.DeserializeObject(_currentLevelInfo[0]);
            
            _currentLevelInfo.RemoveAt(0);
        }
    }
}
