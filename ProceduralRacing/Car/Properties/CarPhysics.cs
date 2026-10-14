using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

// A simple point-based car model: the car is a single point with a position, heading and velocity.
// Each frame: steer -> accelerate/brake -> remove some sideways velocity (grip) -> work out gear/RPM -> move.

public class CarPhysics
{
    // --- Base stats (multiplied by the car's parts) ---

    private const float BaseTopSpeed = 100f;
    private const float BaseAcceleration = 50f;
    private const float BaseGrip = 8f;          // how much sideways velocity is removed per second
    private const float BaseTurnSpeed = 4f;     // radians per second at low speed
    private const float Friction = 0.985f;      // per-frame (60fps) speed kept when coasting
    private const float BrakeStrength = 0.94f;  // per-frame (60fps) speed kept when braking
    private const float SkidThreshold = 12f;    // sideways speed above which the tyres are "skidding"
    private const float IdleRPM = 800f;
    private const float MaxRPM = 7000f;

    // --- State ---

    public Vector2 Position;
    public float Rotation;
    public Vector2 Velocity;
    private float turnVelocity;

    public bool IsThrottle { get; private set; }
    public bool IsBrake { get; private set; }
    public bool IsSkidding { get; private set; }
    public float RPM { get; private set; } = IdleRPM;
    public int Gear { get; private set; } = 1;

    public float TopSpeed { get; }
    public float RPMLimit => MaxRPM;
    public float RPMIdle => IdleRPM;
    public bool InSlipstream;  // set by Car each frame - raises top speed while tucked in behind another car

    private readonly float acceleration, grip, brakeStrength, turnSpeed;
    private readonly int gearCount;

    public CarPhysics(Vector2 startPosition, CarLoadout loadout)
    { // Combines the base stats with the multipliers from each part
        Position = startPosition;

        TopSpeed = BaseTopSpeed * loadout.Engine.TopSpeed * loadout.Chassis.Speed;
        acceleration = BaseAcceleration * loadout.Engine.Acceleration;
        grip = BaseGrip * loadout.Tyres.Grip * loadout.Chassis.Grip;
        brakeStrength = MathF.Pow(BrakeStrength, loadout.Tyres.Braking);
        turnSpeed = BaseTurnSpeed * loadout.Tyres.Turning;
        gearCount = loadout.Engine.GearCount;
    }

    public void Update(float dt, KeyboardState kb, Keys forward, Keys back, Keys left, Keys right)
    {
        Vector2 fwd = new Vector2(MathF.Cos(Rotation), MathF.Sin(Rotation));
        float fwdSpeed = Vector2.Dot(Velocity, fwd);

        IsThrottle = kb.IsKeyDown(forward);
        IsBrake = kb.IsKeyDown(back);
        float turnInput = (kb.IsKeyDown(right) ? 1f : 0f) - (kb.IsKeyDown(left) ? 1f : 0f);

        UpdateSteering(dt, turnInput, fwdSpeed);
        UpdateVelocity(dt, fwd, fwdSpeed);
        ApplyGrip(dt, fwd);
        UpdateGearAndRPM(dt, fwdSpeed);

        Position += Velocity * dt;
    }

    private void UpdateSteering(float dt, float turnInput, float fwdSpeed)
    { // Steering builds up smoothly, is weaker at high speed, and does nothing when stationary
        if (fwdSpeed < 0f) turnInput = -turnInput; // reversing flips the steering, like a real car

        float highSpeedReduction = 1f - 0.6f * MathHelper.Clamp(Math.Abs(fwdSpeed) / TopSpeed, 0f, 1f);
        float targetTurn = turnInput * turnSpeed * highSpeedReduction;
        turnVelocity = MathHelper.Lerp(turnVelocity, targetTurn, 1f - MathF.Exp(-6f * dt));

        float lowSpeedReduction = MathHelper.Clamp(Math.Abs(fwdSpeed) / 30f, 0f, 1f);
        Rotation += turnVelocity * lowSpeedReduction * dt;
    }

    private void UpdateVelocity(float dt, Vector2 fwd, float fwdSpeed)
    { // Accelerate (fading out near top speed), brake/reverse, or coast
        if (IsThrottle)
        {
            float topSpeed = InSlipstream ? TopSpeed * Settings.Race.SlipstreamBoost : TopSpeed;
            float speedFraction = MathHelper.Clamp(Velocity.Length() / topSpeed, 0f, 1f);
            Velocity += fwd * acceleration * (1f - speedFraction) * dt;
        }
        else if (IsBrake && fwdSpeed < 1f)
        { // Stopped (or already reversing) - reverse slowly, capped at 30% of top speed
            Velocity -= fwd * acceleration * 0.35f * dt;
            if (Velocity.Length() > TopSpeed * 0.3f)
                Velocity = Vector2.Normalize(Velocity) * TopSpeed * 0.3f;
        }
        else
        {
            float keep = IsBrake ? brakeStrength : Friction;
            Velocity *= MathF.Pow(keep, dt * 60f);
        }
    }

    private void ApplyGrip(float dt, Vector2 fwd)
    { // Removes part of the sideways velocity each frame. Grip fades at high speed, so fast corners slide (understeer).
        Vector2 side = new Vector2(-fwd.Y, fwd.X);
        float sideSpeed = Vector2.Dot(Velocity, side);

        IsSkidding = Math.Abs(sideSpeed) > SkidThreshold;

        float speedFraction = MathHelper.Clamp(Velocity.Length() / TopSpeed, 0f, 1f);
        float effectiveGrip = grip * (1f - 0.5f * speedFraction);
        float removed = 1f - MathF.Exp(-effectiveGrip * dt);

        Velocity -= side * sideSpeed * removed;
    }

    private void UpdateGearAndRPM(float dt, float fwdSpeed)
    { // Splits the speed range into equal bands, one per gear. RPM climbs through each band then drops on the shift.
        float bandSize = TopSpeed * 0.95f / gearCount;
        float progress = Math.Abs(fwdSpeed) / bandSize;

        Gear = Math.Min((int)progress + 1, gearCount);
        float withinGear = Gear == gearCount ? Math.Min(progress - (gearCount - 1), 1f) : progress % 1f;

        float targetRPM = IsThrottle ? MathHelper.Lerp(IdleRPM, MaxRPM, withinGear) : IdleRPM + (MaxRPM - IdleRPM) * withinGear * 0.4f;
        RPM = MathHelper.Lerp(RPM, targetRPM, 1f - MathF.Exp(-8f * dt));
    }
}
