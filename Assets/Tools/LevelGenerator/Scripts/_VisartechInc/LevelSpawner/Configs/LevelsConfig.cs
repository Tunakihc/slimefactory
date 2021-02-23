using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelsConfig", menuName = "Configs/LevelsConfig")]
public sealed class LevelsConfig : ScriptableObject
{
    public List<LevelConfig> LevelConfigs = new List<LevelConfig>();
}