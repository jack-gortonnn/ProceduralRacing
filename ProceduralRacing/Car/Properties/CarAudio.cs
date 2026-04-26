using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

public class CarAudio
{
    private SoundEffectInstance enginePower, engineCoast;
    private SoundEffect shiftDown;
    private float throttleVol = 0f;

    private static readonly Random rng = new Random();

    public void LoadContent(ContentManager content)
    {
        enginePower = content.Load<SoundEffect>("audio/engine_power").CreateInstance();
        engineCoast = content.Load<SoundEffect>("audio/engine_coast").CreateInstance();
        shiftDown = content.Load<SoundEffect>("audio/shift_down");

        enginePower.IsLooped = engineCoast.IsLooped = true;
        enginePower.Play(); engineCoast.Play();
    }

    public void Update(float dt, float rpm, float maxRpm, bool throttle, int gear, int prevGear)
    {
        float pitch = MathHelper.Lerp(-0.4f, 0.6f, rpm / maxRpm);

        float lerpFactor = 1.0f - MathF.Exp(-10f * dt);
        throttleVol = MathHelper.Lerp(throttleVol, throttle ? 0.6f : 0f, lerpFactor);

        bool shifting = gear != prevGear;
        enginePower.Volume = throttleVol * (shifting ? 0.1f : 0.2f);
        engineCoast.Volume = (0.6f - throttleVol) * (shifting ? 0.07f : 0.15f);
        enginePower.Pitch = engineCoast.Pitch = MathHelper.Clamp(pitch, -1f, 1f);

        if (shifting)
        { // Play downshift sound with a random pitch (-0.2 to +0.2)
            float randomPitch = (float)(rng.NextDouble() * 0.4f - 0.2f);
            if (gear < prevGear) PlaySound(shiftDown, 0.3f, randomPitch);
        }
    }

    private void PlaySound(SoundEffect sfx, float volume, float pitch)
    {
        var instance = sfx.CreateInstance();
        instance.Volume = volume;
        instance.Pitch = pitch;
        instance.Play();
    }
}