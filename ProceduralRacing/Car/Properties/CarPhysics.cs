using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

public class CarPhysics
{
    public Vector2 Velocity;
    private float turnVelocity;
    public bool IsThrottle { get; private set; }

    public float RPM { get; private set; }
    public int Gear { get; private set; } = 1;

    private readonly record struct Stats(float MaxSpeed, float Acceleration, float Friction,
          float BrakingPower, float GripFactor, float MaxTurnSpeed, float TurnAcceleration);

    private static readonly Stats Base = new(100f, 50f, 0.985f, 0.94f, 0.75f, 4f, 5f);

    private readonly Stats s;
    private readonly Engine engine;

    public CarPhysics(Chassis chassis, Engine engine, Tyres tyres)
    {
        this.engine = engine;
        RPM = engine.rpmIdle;

        s = new Stats(
            Base.MaxSpeed * engine.maxSpeed * chassis.maxSpeed,
            Base.Acceleration * engine.acceleration,
            Base.Friction * tyres.friction,
            Base.BrakingPower * tyres.brakingPower,
            Base.GripFactor * tyres.gripFactor * chassis.gripFactor,
            Base.MaxTurnSpeed * tyres.maxTurnSpeed,
            Base.TurnAcceleration * tyres.turnAcceleration
        );
    }

    public void Update(float dt, KeyboardState kb, ref Vector2 pos, ref float rot)
    {
        Vector2 fwd = new Vector2((float)Math.Cos(rot), (float)Math.Sin(rot));
        float fwdSpeed = Vector2.Dot(Velocity, fwd);

        IsThrottle = kb.IsKeyDown(Keys.W);
        bool isBrake = kb.IsKeyDown(Keys.S);

        UpdateSteering(dt, kb, ref rot, fwdSpeed);
        UpdateVelocity(dt, fwd, fwdSpeed, isBrake);
        ApplyLateralGrip(dt, fwd, fwdSpeed);
        UpdateGearAndRPM(dt, fwdSpeed);

        pos += Velocity * dt;
    }

    private void UpdateSteering(float dt, KeyboardState kb, ref float rot, float fwdSpeed)
    {
        float turnInput = (kb.IsKeyDown(Keys.D) ? 1f : 0f) - (kb.IsKeyDown(Keys.A) ? 1f : 0f);

        if (fwdSpeed < -0.5f) turnInput *= -1f;

        float speedFactor = 1f - MathHelper.Clamp(Math.Abs(fwdSpeed) / s.MaxSpeed, 0f, 0.7f);
        float lerpFactor = 1f - MathF.Exp(-s.TurnAcceleration * dt);
        turnVelocity = MathHelper.Lerp(turnVelocity, turnInput * s.MaxTurnSpeed * speedFactor, lerpFactor);

        float speedEngagement = MathHelper.Clamp(Math.Abs(fwdSpeed) / 30f, 0f, 1f);
        rot += turnVelocity * speedEngagement * dt;
    }

    private void UpdateVelocity(float dt, Vector2 fwd, float fwdSpeed, bool isBrake)
    {
        if (IsThrottle)
        {
            float speedFraction = Velocity.Length() / s.MaxSpeed;
            float accelerationCurve = MathF.Pow(1f - speedFraction, 2f);
            Velocity += fwd * s.Acceleration * accelerationCurve * dt;
        }
        else if (isBrake && fwdSpeed < 0.5f)
        {
            Velocity -= fwd * (s.Acceleration * 0.35f) * dt;
            if (Velocity.Length() > s.MaxSpeed * 0.4f)
                Velocity = Vector2.Normalize(Velocity) * (s.MaxSpeed * 0.4f);
        }
        else
        {
            float factor = isBrake
                ? (float)Math.Pow(s.BrakingPower, dt * 60f)
                : (float)Math.Pow(s.Friction, dt * 60f);
            Velocity *= factor;
        }
    }

    private void ApplyLateralGrip(float dt, Vector2 fwd, float fwdSpeed)
    {
        Vector2 side = new Vector2(-fwd.Y, fwd.X);
        float sideVel = Vector2.Dot(Velocity, side);
        float speedBlend = MathHelper.Clamp(Math.Abs(fwdSpeed) / 5f, 0f, 1f);
        float gripReduction = 1f - (float)Math.Pow(1f - s.GripFactor, dt * 60f);
        Velocity -= side * sideVel * gripReduction * speedBlend;
    }

    private void UpdateGearAndRPM(float dt, float fwdSpeed)
    {
        float speed = Math.Max(0f, fwdSpeed);

        Gear = engine.gearRatios.Length - 1;
        for (int i = 1; i < engine.gearRatios.Length; i++)
        {
            if (speed < s.MaxSpeed * engine.gearRatios[i]) { Gear = i; break; }
        }

        float lo = s.MaxSpeed * engine.gearRatios[Gear - 1];
        float hi = s.MaxSpeed * engine.gearRatios[Gear];
        float t = MathHelper.Clamp((speed - lo) / (hi - lo), 0f, 1f);

        float targetRPM = IsThrottle
            ? MathHelper.Lerp(engine.rpmIdle, engine.rpmMax, t)
            : engine.rpmIdle;

        float rate = IsThrottle ? 8f : 8f;
        float lerpFactor = 1f - MathF.Exp(-rate * dt);
        RPM = MathHelper.Lerp(RPM, targetRPM, lerpFactor);
    }
}