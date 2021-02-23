public static class Constants  {

    internal static class LayerNames {        
        public const string PlayerLayer = "Player";
        public const string ObstacleLayer = "Obstacle";
        public const string BorderLayer = "Border";
        public const string TrapLayer = "Trap";
        public const string InvincibleLayer = "Invincible";
        public const string DisjointedPartLayer = "DisjointedPart";
    }

    internal static class Resources {
        public const string CharacterPrefabPath = "CharacterBody";
    }

    internal static class Level {
        public const float LevelWidth = 6;
    }
    
    internal static class Logic {
        public const int NonPausableSequenceId = 0;
        public const int PausableSequenceId = -999;
    }
    
}
