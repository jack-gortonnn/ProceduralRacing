using System;

public enum CarPreset
{
    Drifty,
    Grippy
}

public class CarConfig
{
    public float acceleration;         // pixels per second^2
    public float maxSpeed;             // pixels per second
    public float friction;             // multiply velocity when not accelerating
    public float brakingPower;         // multiply velocity when braking
    public float turnAcceleration;     // how fast turning builds up (radians/sec^2)
    public float maxTurnSpeed;         // max turning speed (radians/sec)
    public float gripFactor;           // 0 = very slidy, 1 = very grippy

    public static CarConfig FromPreset(CarPreset preset)
    {
        switch (preset)
        {
            case CarPreset.Drifty:
                return new CarConfig
                {
                    acceleration = 100f,
                    maxSpeed = 230f,
                    friction = 0.985f,
                    brakingPower = 0.97f,
                    turnAcceleration = 15f,
                    maxTurnSpeed = 4f,
                    gripFactor = 0.04f
                };

            case CarPreset.Grippy:
                return new CarConfig
                {
                    acceleration = 200f,
                    maxSpeed = 190f,
                    friction = 0.995f,
                    brakingPower = 0.94f,
                    turnAcceleration = 6f,
                    maxTurnSpeed = 5f,
                    gripFactor = 0.5f
                };

            default:
                throw new ArgumentOutOfRangeException(nameof(CarPreset));
        }
    }
}