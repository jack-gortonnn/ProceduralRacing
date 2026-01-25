using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace ProceduralRacing
{
    public class CarPhysics
    {
        public CarConfig config { get; private set; }

        public Vector2 Velocity;
        private float angularVelocity = 0f;

        public CarPhysics(CarPreset preset)
        { // Initialize config from preset
            config = CarConfig.FromPreset(preset);
            Velocity = Vector2.Zero;
        }

        public void Reset()
        { // Reset velocity
            Velocity = Vector2.Zero;
            angularVelocity = 0f;
        }

        public void Update(float dt, KeyboardState kb, ref Vector2 position, ref float rotation)
        {
            Vector2 forward = new Vector2((float)Math.Cos(rotation), (float)Math.Sin(rotation));
            Vector2 right = new Vector2(-forward.Y, forward.X);

            // Steering
            UpdateSteering(dt, kb, ref rotation);

            // Acceleration
            UpdateAcceleration(dt, kb, forward);

            // Lateral grip (drifting)
            ApplyLateralGrip(right);

            // Stop if moving very slowly
            if (Velocity.Length() < 0.5f)
                Velocity = Vector2.Zero;

            // Update position
            position += Velocity * dt;
        }

        private void UpdateSteering(float dt, KeyboardState kb, ref float rotation)
        { // Left and right
            float input = (kb.IsKeyDown(Keys.D) ? 1f : 0f) - (kb.IsKeyDown(Keys.A) ? 1f : 0f);
            float speedFactor = 1f / (1f + (Velocity.Length() / config.maxSpeed) * (Velocity.Length() / config.maxSpeed) * 2f);

            angularVelocity = MathHelper.Clamp(
                angularVelocity + (input * config.maxTurnSpeed * speedFactor - angularVelocity)
                * config.turnAcceleration * dt,
                -config.maxTurnSpeed * speedFactor,
                config.maxTurnSpeed * speedFactor
            );

            rotation += angularVelocity * dt;
        }

        private void UpdateAcceleration(float dt, KeyboardState kb, Vector2 forward)
        { // Forward and braking
            if (kb.IsKeyDown(Keys.W))
            {
                Velocity += forward * config.acceleration * dt;
                if (Velocity.Length() > config.maxSpeed)
                    Velocity = Vector2.Normalize(Velocity) * config.maxSpeed;
            }
            else if (kb.IsKeyDown(Keys.S))
            {
                Velocity *= config.brakingPower;
            }
            else
            {
                // Natural friction
                Velocity *= (float)Math.Pow(config.friction, dt * 60f);
            }
        }

        private void ApplyLateralGrip(Vector2 right)
        { // Reduce lateral velocity for grip
            float lateralVelocity = Vector2.Dot(Velocity, right);
            float gripStrength = MathHelper.Lerp(0f, 1.1f, config.gripFactor);
            Velocity -= right * lateralVelocity * gripStrength;
        }

        public void ApplyOutOfBoundsBraking()
        {
            Velocity *= config.oobBrakingPower;
        }
    }
}