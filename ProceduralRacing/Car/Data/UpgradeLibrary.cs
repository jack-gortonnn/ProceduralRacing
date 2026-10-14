// Each car part is a simple set of multipliers applied to the base stats in CarPhysics.
// 1.0 = baseline. Every part has a trade-off so there is no single "best" build.

public class Engine
{
    public string DisplayName;
    public string Description;
    public float TopSpeed;      // multiplies the car's top speed
    public float Acceleration;  // multiplies how quickly the car reaches top speed
    public int GearCount;

    public static readonly Engine Balanced = new Engine { DisplayName = "Balanced V6", Description = "All-rounder",          TopSpeed = 1.00f, Acceleration = 1.00f, GearCount = 5 };
    public static readonly Engine Sprint   = new Engine { DisplayName = "Sprint I4",   Description = "Quick off the line",   TopSpeed = 0.85f, Acceleration = 1.30f, GearCount = 4 };
    public static readonly Engine Power    = new Engine { DisplayName = "Power V8",    Description = "Huge top speed",       TopSpeed = 1.20f, Acceleration = 0.80f, GearCount = 6 };

    public static readonly Engine[] All = { Balanced, Sprint, Power };
}

public class Tyres
{
    public string DisplayName;
    public string Description;
    public float Grip;      // multiplies sideways grip (higher = less sliding)
    public float Braking;   // multiplies braking strength
    public float Turning;   // multiplies steering speed

    public static readonly Tyres Medium = new Tyres { DisplayName = "Medium", Description = "All-rounder",              Grip = 1.00f, Braking = 1.00f, Turning = 1.00f };
    public static readonly Tyres Soft   = new Tyres { DisplayName = "Soft",   Description = "Sticky, but turns slowly", Grip = 1.40f, Braking = 1.20f, Turning = 0.85f };
    public static readonly Tyres Hard   = new Tyres { DisplayName = "Hard",   Description = "Sharp turn-in, slides",    Grip = 0.70f, Braking = 0.85f, Turning = 1.25f };

    public static readonly Tyres[] All = { Medium, Soft, Hard };
}

public class Chassis
{
    public string DisplayName;
    public string Description;
    public float Size;   // multiplies the sprite and collision box size
    public float Grip;   // multiplies sideways grip
    public float Speed;  // multiplies top speed (lighter = faster)

    public static readonly Chassis Standard = new Chassis { DisplayName = "Standard", Description = "All-rounder",          Size = 1.00f, Grip = 1.00f, Speed = 1.00f };
    public static readonly Chassis Light    = new Chassis { DisplayName = "Light",    Description = "Small and fast",       Size = 0.85f, Grip = 0.85f, Speed = 1.10f };
    public static readonly Chassis Heavy    = new Chassis { DisplayName = "Heavy",    Description = "Big and planted",      Size = 1.15f, Grip = 1.20f, Speed = 0.92f };

    public static readonly Chassis[] All = { Standard, Light, Heavy };
}

public struct CarLoadout
{
    public Chassis Chassis;
    public Engine Engine;
    public Tyres Tyres;
}
