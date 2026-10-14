public static class Settings
{
    public static class Generation
    {
        public const int TileSize = 64;
        public const int TrackOriginX = 10;
        public const int TrackOriginY = 14;
        public const int GridSize = 25; // the grid covers tiles 0..GridSize inclusive - also used as the map boundary
    }

    public static class Camera
    {
        public const float PositionSmoothing = 5f;    // how quickly the camera catches up to the car
        public const float RotationSmoothing = 5f;    // how quickly the camera turns to face the car's heading
        public const float LeadStrength = 0.15f;      // how far the camera shifts against acceleration (lags when speeding up, leads when braking)
        public const float ZoomMin = 4f;
        public const float ZoomMax = 7f;
        public const float ZoomSmoothing = 2f;
        public const float MaxSpeedReference = 150f;  // speed at which zoom is fully out
        public const float ShakeStrength = 1.5f;      // max shake offset (world units) when off-track at full speed
    }

    public static class Race
    {
        public const float CountdownDuration = 3f;
        public const float OffTrackDrag = 0.97f;      // per-frame (at 60fps) speed multiplier while off-track
        public const double TrackLimitsPenalty = 5.0; // seconds added each time a car fully leaves the track
        public const double CutPenalty = 5.0;         // seconds added when a lap is invalidated by skipping pieces
        public const int SkipGrace = 2;               // how many pieces can be skipped before a lap is invalid
        public const float SlipstreamRange = 200f;    // how close behind another car you need to be to get a tow
        public const float SlipstreamBoost = 1.2f;    // top speed multiplier while in the slipstream (+20%)
        public const float Restitution = 0.3f;        // how bouncy car-vs-car impacts are (0 = dead stop, 1 = perfect bounce)
    }
}
