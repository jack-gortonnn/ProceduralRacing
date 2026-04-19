using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

public class CarAudio
{
    private SoundEffectInstance power, coast;
    private SoundEffect sUp, sDown;
    private float throttleVol = 0f;

    public void LoadContent(ContentManager content)
    {
        power = content.Load<SoundEffect>("audio/engine_power").CreateInstance();
        coast = content.Load<SoundEffect>("audio/engine_coast").CreateInstance();

        power.IsLooped = coast.IsLooped = true;
        power.Play(); coast.Play();
    }

    public void Update(float rpm, float maxRpm, bool shifting, bool throttle)
    {
        float pitch = MathHelper.Lerp(-0.4f, 0.6f, rpm / maxRpm);
        throttleVol = MathHelper.Lerp(throttleVol, throttle ? 0.6f : 0f, 0.1f);

        power.Volume = throttleVol * (shifting ? 0.1f : 0.2f);
        coast.Volume = (0.6f - throttleVol) * (shifting ? 0.07f : 0.15f);
        power.Pitch = coast.Pitch = MathHelper.Clamp(pitch, -1f, 1f);
    }
}