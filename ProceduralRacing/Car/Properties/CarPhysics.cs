using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

public class CarPhysics
{
    public CarConfig config { get; private set; }
    public Vector2 Velocity;
    public float RPM { get; private set; }
    public int Gear { get; private set; } = 1;
    public bool IsThrottle { get; private set; }

    private float[] ratios = { 0f, 3.8f, 2.9f, 2.2f, 1.7f, 1.3f };
    private float shiftTimer = 0f;
    private Random rng = new Random();

    public CarPhysics(CarPreset p) => config = CarConfig.FromPreset(p);

    public void Update(float dt, KeyboardState kb, ref Vector2 pos, ref float rot)
    {
        Vector2 fwd = new Vector2((float)Math.Cos(rot), (float)Math.Sin(rot));
        IsThrottle = kb.IsKeyDown(Keys.W);
        float fwdSpeed = Math.Max(0, Vector2.Dot(Velocity, fwd));

        // 1. Steering: Zero at stop, snappy at low speed, heavy at high speed
        float turnInput = (kb.IsKeyDown(Keys.D) ? 1 : 0) - (kb.IsKeyDown(Keys.A) ? 1 : 0);
        float turnUnlock = MathHelper.Clamp(fwdSpeed / 50f, 0f, 1f); // Adjust 50f to change 'unlock' speed
        float speedHeavy = MathHelper.Lerp(1.0f, 0.4f, fwdSpeed / config.maxSpeed);
        rot += turnInput * config.maxTurnSpeed * turnUnlock * speedHeavy * dt;

        // 2. RPM & Gear Logic
        if (shiftTimer > 0) shiftTimer -= dt;
        else
        {
            float targetRPM = (fwdSpeed / config.maxSpeed * 7500f * ratios[Gear]) + 1000f;
            RPM = MathHelper.Lerp(RPM, targetRPM + rng.Next(-50, 50), 0.2f);
            if (RPM > 6200 && Gear < 5) Shift(1);
            else if (RPM < 3200 && Gear > 1) Shift(-1);
        }

        // 3. Movement & Top Speed Gating
        if (IsThrottle && shiftTimer <= 0)
        {
            Velocity += fwd * (config.acceleration * ratios[Gear] * 0.5f) * dt;
            float gearMax = config.maxSpeed * (0.3f + (Gear * 0.15f)); // Gear 1 = 45%, Gear 5 = 105%
            if (Velocity.Length() > gearMax) Velocity = Vector2.Normalize(Velocity) * gearMax;
        }
        else Velocity *= kb.IsKeyDown(Keys.S) ? config.brakingPower : (float)Math.Pow(config.friction, dt * 60f);

        // 4. Lateral Grip (Drift logic)
        Vector2 side = new Vector2(-fwd.Y, fwd.X);
        // While turning at low speeds, we reduce grip slightly to allow the "tank spin"
        float dynamicGrip = config.gripFactor * MathHelper.Lerp(1.0f, 0.8f, turnUnlock);
        Velocity -= side * Vector2.Dot(Velocity, side) * dynamicGrip;

        pos += Velocity * dt;
        RPM = MathHelper.Clamp(RPM, 1000, 7700);
    }

    private void Shift(int dir)
    {
        Gear += dir;
        shiftTimer = 0.15f;
        Velocity *= 0.97f;
    }
}