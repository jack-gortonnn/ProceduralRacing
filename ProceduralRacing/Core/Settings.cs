public static class Settings
{
    public static class Generation
    {
        public const int TileSize = 64;
        public const int MaxTrackLength = 50;
        public const float SecondsPerStep = 0.00001f;
        public const int OptionPoolSize = 3;
        public const int TrackOriginX = 10;
        public const int TrackOriginY = 14;
    }

    public static class Camera
    {
        public const float OffsetStrength = 0.01f;
        public const float OffsetSmoothing = 10f;   // exp smoothing stiffness
        public const float PositionSmoothing = 5f;    // lerp rate toward desired pos
        public const float RotationOffsetStrength = 0.02f;
        public const float RotationOffsetSmoothing = 10f;
        public const float RotationSmoothing = 5f;
        public const float ZoomMin = 4f;
        public const float ZoomMax = 7f;
        public const float ZoomSmoothing = 2f;
        public const float MaxSpeedReference = 150f; // speed at which zoom is fully out
    }
}