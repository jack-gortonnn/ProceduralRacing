using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

public class CarAudio
{
    private SoundEffectInstance power, coast;
    private SoundEffect sUp, sDown;
    private float throttleVol = 0f;

    private static readonly Random rng = new Random();

    public void LoadContent(ContentManager content)
    {
        power = content.Load<SoundEffect>("audio/engine_power").CreateInstance();
        coast = content.Load<SoundEffect>("audio/engine_coast").CreateInstance();
        sUp = content.Load<SoundEffect>("audio/shift_up");
        sDown = content.Load<SoundEffect>("audio/shift_down");

        power.IsLooped = coast.IsLooped = true;
        power.Play(); coast.Play();
    }

    public void Update(float dt, float rpm, float maxRpm, bool throttle, int gear, int prevGear)
    {
        float pitch = MathHelper.Lerp(-0.4f, 0.6f, rpm / maxRpm);

        float lerpFactor = 1.0f - MathF.Exp(-10f * dt);
        throttleVol = MathHelper.Lerp(throttleVol, throttle ? 0.6f : 0f, lerpFactor);

        bool shifting = gear != prevGear;
        power.Volume = throttleVol * (shifting ? 0.1f : 0.2f);
        coast.Volume = (0.6f - throttleVol) * (shifting ? 0.07f : 0.15f);
        power.Pitch = coast.Pitch = MathHelper.Clamp(pitch, -1f, 1f);

        if (shifting)
        {
            float randomPitch = (float)(rng.NextDouble() * 1 - 0.5f);
            if (gear > prevGear) PlayOneShot(sUp, 1f, randomPitch);
            else PlayOneShot(sDown, 1f, randomPitch);
        }
    }

    private void PlayOneShot(SoundEffect sfx, float volume, float pitch)
    {
        var instance = sfx.CreateInstance();
        instance.Volume = volume;
        instance.Pitch = pitch;
        instance.Play();
    }
}