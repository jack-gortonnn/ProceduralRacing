using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ProceduralRacing;

// The Car ties together its three subsystems: physics (movement), collision (track limits) and audio.

public class Car
{
    private const float BaseScale = 0.35f;

    public CarLoadout Loadout { get; }
    public CarPhysics Physics { get; }
    private readonly CarAudio audio = new CarAudio();
    private readonly Keys keyForward, keyBack, keyLeft, keyRight;

    private Texture2D sprite;
    private Vector2 origin;
    private float scale;

    public Vector2 Position => Physics.Position;
    public float Rotation => Physics.Rotation;
    public Vector2 Velocity => Physics.Velocity;
    public float Speed => Physics.Velocity.Length();
    public float RPM => Physics.RPM;
    public int Gear => Physics.Gear;

    public float CollisionWidth { get; private set; }
    public float CollisionHeight { get; private set; }
    public float CollisionRadius { get; private set; }

    public bool IsOnTrack { get; private set; } = true;
    public Color Tint = Color.White;

    public Car(Vector2 startPos, CarLoadout loadout, Keys forward, Keys back, Keys left, Keys right)
    {
        Loadout = loadout;
        Physics = new CarPhysics(startPos, loadout);
        keyForward = forward; keyBack = back; keyLeft = left; keyRight = right;
    }

    public void LoadContent(ContentManager content)
    { // Loads the sprite, then sizes the collision box from the sprite and the chassis size
        sprite = content.Load<Texture2D>("textures/cars/car");
        origin = new Vector2(sprite.Width / 2f, sprite.Height / 2f);
        scale = BaseScale * Loadout.Chassis.Size;

        CollisionWidth = sprite.Width * scale;
        CollisionHeight = sprite.Height * scale;
        CollisionRadius = Math.Min(CollisionWidth, CollisionHeight) / 2f;

        audio.LoadContent(content);
    }

    public void Update(float dt, List<PlacedPiece> track, Car opponent = null)
    {
        int oldGear = Physics.Gear;
        Physics.InSlipstream = IsBehind(opponent);

        Physics.Update(dt, Keyboard.GetState(), keyForward, keyBack, keyLeft, keyRight);

        IsOnTrack = CarCollision.IsOnTrack(this, track);
        if (!IsOnTrack)
            Physics.Velocity *= MathF.Pow(Settings.Race.OffTrackDrag, dt * 60f);

        CarCollision.KeepInsideMap(this);

        audio.Update(dt, Physics, oldGear, IsOnTrack, Speed);
    }

    private bool IsBehind(Car other)
    { // Slipstream: the other car is close, and roughly straight ahead of us (within a narrow cone)
        if (other == null) return false;
        Vector2 toOther = other.Position - Position;
        float distance = toOther.Length();
        Vector2 forward = new Vector2(MathF.Cos(Rotation), MathF.Sin(Rotation));
        return distance < Settings.Race.SlipstreamRange && Vector2.Dot(forward, toOther / distance) > 0.95f;
    }

    public void Draw(SpriteBatch sb) =>
        sb.Draw(sprite, Position, null, Tint, Rotation, origin, scale, SpriteEffects.None, 0);

    public void StopAudio() => audio.Stop();
    public void SetAudioPan(float pan) => audio.SetPan(pan);
}
