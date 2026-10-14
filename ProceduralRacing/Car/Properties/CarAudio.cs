using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

// Plays four looping sounds whose volume/pitch follow the physics each frame:
//   engine (on/off throttle) -> pitch follows RPM
//   tyre squeal              -> fades in while the car is skidding
//   gravel rumble            -> fades in while the car is off-track
// plus one-shot up/down shift sounds whenever the gear changes.
// The squeal and rumble are generated in code from noise, so no extra sound files are needed.

public class CarAudio
{
    private SoundEffectInstance enginePower, engineCoast, squeal, rumble;
    private SoundEffect shiftUp, shiftDown;
    private float pan;
    private static readonly Random rng = new Random();

    public void LoadContent(ContentManager content)
    {
        enginePower = Loop(content.Load<SoundEffect>("audio/engine_power"));
        engineCoast = Loop(content.Load<SoundEffect>("audio/engine_coast"));
        squeal = Loop(GenerateSqueal());
        rumble = Loop(GenerateRumble());
        shiftUp = content.Load<SoundEffect>("audio/shift_up");
        shiftDown = content.Load<SoundEffect>("audio/shift_down");
    }

    public void Update(float dt, CarPhysics physics, int oldGear, bool onTrack, float speed)
    {
        float fade = 1f - MathF.Exp(-10f * dt); // smooths volume changes so nothing clicks on/off

        // Engine: pitch rises with RPM, power loop on throttle, coast loop off throttle
        float rpmFraction = physics.RPM / physics.RPMLimit;
        enginePower.Pitch = engineCoast.Pitch = MathHelper.Clamp(MathHelper.Lerp(-0.5f, 0.6f, rpmFraction), -1f, 1f);
        enginePower.Volume = MathHelper.Lerp(enginePower.Volume, physics.IsThrottle ? 0.2f : 0f, fade);
        engineCoast.Volume = MathHelper.Lerp(engineCoast.Volume, physics.IsThrottle ? 0f : 0.1f, fade);

        // Gear change one-shots
        if (physics.Gear > oldGear) PlayOnce(shiftUp, 0.3f);
        if (physics.Gear < oldGear) PlayOnce(shiftDown, 0.3f);

        // Tyre squeal while skidding on track
        float squealTarget = physics.IsSkidding && onTrack ? 0.25f : 0f;
        squeal.Volume = MathHelper.Lerp(squeal.Volume, squealTarget, fade);

        // Gravel rumble while off-track and moving
        float rumbleTarget = !onTrack ? MathHelper.Clamp(speed / 60f, 0f, 1f) * 0.5f : 0f;
        rumble.Volume = MathHelper.Lerp(rumble.Volume, rumbleTarget, fade);
    }

    public void SetPan(float newPan)
    { // -1 = left speaker, 1 = right - used in split screen so each player hears their own car
        pan = MathHelper.Clamp(newPan, -1f, 1f);
        foreach (var s in new[] { enginePower, engineCoast, squeal, rumble })
            if (s != null) s.Pan = pan;
    }

    public void Stop()
    {
        foreach (var s in new[] { enginePower, engineCoast, squeal, rumble })
            s?.Stop();
    }

    // --- Helpers ---

    private SoundEffectInstance Loop(SoundEffect sfx)
    {
        var instance = sfx.CreateInstance();
        instance.IsLooped = true;
        instance.Volume = 0f;
        instance.Pan = pan;
        instance.Play();
        return instance;
    }

    private void PlayOnce(SoundEffect sfx, float volume)
    {
        var instance = sfx.CreateInstance();
        instance.Volume = volume;
        instance.Pitch = (float)(rng.NextDouble() * 0.4 - 0.2); // slight random pitch so repeats don't sound robotic
        instance.Pan = pan;
        instance.Play();
    }

    private const int SampleRate = 22050;

    private static SoundEffect GenerateSqueal()
    { // Band-passed noise (roughly 900-2500Hz) with an uneven, scrubbing volume - real tyre squeal is
      // rubber juddering against tarmac, so it's a noisy hiss in a narrow band, not a clean tone.
      // Band-pass = a fast low-pass minus a slow low-pass, which keeps only the frequencies in between.
        var r = new Random(1);
        float fast = 0f, slow = 0f, judder = 0f;
        return FromSamples(t =>
        {
            float noise = (float)(r.NextDouble() * 2 - 1);
            fast += (noise - fast) * 0.35f;
            slow += (noise - slow) * 0.12f;
            judder += ((float)r.NextDouble() - judder) * 0.003f; // slowly wandering 0-1 value
            return (fast - slow) * 3f * (0.4f + judder);
        });
    }

    private static SoundEffect GenerateRumble()
    { // Low-pass filtered noise (each sample drifts towards a random value) - sounds like gravel
        var r = new Random(2);
        float value = 0f;
        return FromSamples(t => (value += ((float)(r.NextDouble() * 2 - 1) - value) * 0.05f) * 3f);
    }

    private static SoundEffect FromSamples(Func<float, float> sample)
    { // Builds one second of 16-bit mono audio from a function of time
        byte[] buffer = new byte[SampleRate * 2];
        for (int i = 0; i < SampleRate; i++)
        {
            short s = (short)(MathHelper.Clamp(sample(i / (float)SampleRate), -1f, 1f) * short.MaxValue * 0.8f);
            buffer[i * 2] = (byte)(s & 0xFF);
            buffer[i * 2 + 1] = (byte)((s >> 8) & 0xFF);
        }
        return new SoundEffect(buffer, SampleRate, AudioChannels.Mono);
    }
}
