using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Numerics;

/// <summary>
/// Used to decide event type.
/// </summary>
public enum EventOpCode
{
    Combination = 0x01,
    Preview = 0x02,
    Available = 0x03,
    Update = 0x04,
    Loose = 0x05
}

/// <summary>
/// Current state of a orb.
/// </summary>
public enum OrbState
{
    Preview = 0x01,
    Inactive = 0x02,
    Active = 0x03,
    Removed = 0x04
}

/// <summary>
/// Definition of a orb in the game.
/// </summary>
public class Orb
{
    public Guid Id;
    public byte Index;
    public OrbState State;
    public Vector2 Location;
    public Vector2 Velocity;
    public float Radius;
}

/// <summary>
/// Definition of the dropper controlled by the player.
/// </summary>
public struct Dropper
{
    public float Location;
    public Orb Orb;
}

[JsonConverter(typeof(EventConverter))]
public abstract class BaseEvent
{
    public abstract EventOpCode OpCode { get; }
}

/// <summary>
/// Indicates that a combination of two objects happened.
/// </summary>
public class CombinationEvent : BaseEvent
{
    public override EventOpCode OpCode => EventOpCode.Combination;
    public Orb[] Mergers;
    public int Score;
    public float Charge;
}

/// <summary>
/// Indicates that a new object is shown in preview.
/// </summary>
public class PreviewEvent : BaseEvent
{
    public override EventOpCode OpCode => EventOpCode.Preview;
    public Orb Orb;
}

/// <summary>
/// Indicates that a new object to drop is available.
/// </summary>
public class AvailableEvent : BaseEvent
{
    public override EventOpCode OpCode => EventOpCode.Available;
    public Dropper Player;
}

/// <summary>
/// Informs about the general state of the game.
/// </summary>
public class UpdateEvent : BaseEvent
{
    public override EventOpCode OpCode => EventOpCode.Update;
    public Orb Preview;
    public Dropper Player;
    public Orb[] Orbs;
    public int Score;
    public float Charge;
}

/// <summary>
/// Indicates that the player lost.
/// </summary>
public class LooseEvent : BaseEvent
{
    public override EventOpCode OpCode => EventOpCode.Loose;
    public int Score;
    public int Highscore;
}

/// <summary>
/// Confirms that the opcode matches the event.
/// </summary>
/// <typeparam name="T"></typeparam>
public class EventConverter : JsonConverter<BaseEvent>
{
    public override BaseEvent ReadJson(JsonReader reader, Type objectType, BaseEvent existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var jsonObject = JObject.Load(reader);
        EventOpCode eventOpCode = jsonObject.GetValue("OpCode").ToObject<EventOpCode>();
        BaseEvent targetEvent = eventOpCode switch
        {
            EventOpCode.Combination => new CombinationEvent(),
            EventOpCode.Update => new UpdateEvent(),
            EventOpCode.Available => new AvailableEvent(),
            EventOpCode.Preview => new PreviewEvent(),
            EventOpCode.Loose => new LooseEvent(),
            _ => throw new NotSupportedException($"Unsupported event op code type: {eventOpCode}"),
        };

        serializer.Populate(jsonObject.CreateReader(), targetEvent);

        return targetEvent;
    }

    public override bool CanWrite => false;
    public override void WriteJson(JsonWriter writer, BaseEvent value, JsonSerializer serializer)
    {
        throw new NotImplementedException("This converter handles deserialization only.");
    }
}