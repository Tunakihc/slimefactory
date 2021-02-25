using System;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "Configs", menuName = "Configs/Configs")]
public class Configs : ScriptableObject {
    public LocationsConfig LocationsConfig;
}

public enum LevelPatternDifficulty {
    Easy,
    Medium,
    Hard,
    Insane
}

public enum LocationType
{
    First,
    Second,
    Third
}

[Serializable]
public struct DifficultyByDistanceConfig {
    public LevelPatternDifficulty Difficulty;
    public float Distance;
}

[Serializable]
public struct ConfigsByDifficultyConfig {
    public LevelPatternDifficulty Difficulty;
    public TextAsset[] TextAssets;
}

[Serializable]
public struct LevelConfig
{
    public int SlimesCount;
    public TextAsset[] TextAssets;
}

[Serializable]
public struct LocationConfig
{
    [FormerlySerializedAs("Location")] public LocationType LocationType;

    public float LocationLength;
    public LevelsConfig PatternsInfo;
}