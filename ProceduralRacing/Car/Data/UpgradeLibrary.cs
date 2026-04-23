public class Engine
{
    public string ID;
    public string DisplayName;
    public int Price;

    public float acceleration;
    public float maxSpeed;

    public float rpmIdle;
    public float rpmMax;
    public int gearCount;
    public float shiftDuration;
    public float[] gearRatios;

    public static readonly Engine Basic = new Engine
    {
        ID = "engine_basic",
        DisplayName = "Basic Engine",
        Price = 0,
        acceleration = 1.0f,
        maxSpeed = 1.0f,
        rpmIdle = 800f,
        rpmMax = 7000f,
        gearCount = 5,
        shiftDuration = 0.15f,
        gearRatios = new float[] { 0f, 0.38f, 0.60f, 0.75f, 0.87f, 1.0f },
    };
}

public class Tyres
{
    public string ID;
    public string DisplayName;
    public int Price;

    public float friction;
    public float brakingPower;
    public float oobBrakingPower;
    public float gripFactor;
    public float turnAcceleration;
    public float maxTurnSpeed;

    public static readonly Tyres Basic = new Tyres
    {
        ID = "tyres_basic",
        DisplayName = "Basic Tyres",
        Price = 0,
        friction = 1.0f,
        brakingPower = 1.0f,
        oobBrakingPower = 1.0f,
        gripFactor = 1.0f,
        turnAcceleration = 1.0f,
        maxTurnSpeed = 1.0f,
    };
}

public class Chassis
{
    public string ID;
    public string DisplayName;
    public int Price;

    public float collisionScale;
    public float gripFactor;
    public float maxSpeed;

    public static readonly Chassis Basic = new Chassis
    {
        ID = "chassis_basic",
        DisplayName = "Basic Chassis",
        Price = 0,
        collisionScale = 1.0f,
        gripFactor = 1.0f,
        maxSpeed = 1.0f,
    };
}