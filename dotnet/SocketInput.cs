using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Used to decide instruction type.
/// </summary>
public enum InstructionOpCode : byte
{
    Move = 0x01,
    Drop = 0x02,
    Ability = 0x03,
    Restart = 0x04
}

/// <summary>
/// This struct describes an instruction that can be given to the game.
/// </summary>
public readonly struct Instruction
{
    /// <summary>
    /// The operation code of the instruction.
    /// </summary>
    [JsonProperty(Required = Required.Always)]
    public readonly InstructionOpCode OpCode;
    /// <summary>
    /// The delay depending on the progress of the parent instruction in percent. This can be between 0f and 1f.
    /// </summary>
    public readonly float Delay;
    /// <summary>
    /// The arguments that should be passed to this instruction.
    /// Relevant for Move OpCode with target position between -3f and 3f, aswell as joystick strength between 0f and 1f.
    /// </summary>
    public readonly object[] Arguments;
    /// <summary>
    /// This is relevant if you want to chain instructions. Child Instructions with same delay, will be executed in array order.
    /// </summary>
    public readonly IEnumerable<Instruction> Children;

    [JsonConstructor]
    public Instruction(InstructionOpCode opcode, float delay = default, object[] arguments = default, IEnumerable<Instruction> children = default)
        => (OpCode, Delay, Arguments, Children) = (opcode, delay > 0f ? delay : 1.0f, arguments ?? Array.Empty<object>(), children ?? Array.Empty<Instruction>());
}

public abstract class InstructionBuilder
{
    protected InstructionOpCode OpCode;
    private float Delay;
    protected object[] Arguments = Array.Empty<object>();
    private Func<Instruction, IEnumerable<InstructionBuilder>> ChildrenAction;

    public virtual InstructionBuilder WithDelay(float delay)
    {
        Delay = delay;
        return this;
    }

    public virtual InstructionBuilder WithChildren(Func<Instruction, IEnumerable<InstructionBuilder>> action)
    {
        ChildrenAction = action;
        return this;
    }

    protected virtual void Validate()
    {
        if (Delay < 0f || Delay > 1f)
            throw new ArgumentException($"{nameof(Delay)} is not a float between 0 and 1.");
    }

    public Instruction Build()
    {
        Validate();
        if (ChildrenAction != null)
            return new(OpCode, Delay, Arguments, ChildrenAction(new(OpCode, Delay, Arguments)).Select(child => child.Build()).ToArray());
        else
            return new(OpCode, Delay, Arguments);
    }
}

public class MoveInstructionBuilder : InstructionBuilder
{
    public MoveInstructionBuilder()
        => OpCode = InstructionOpCode.Move;

    public InstructionBuilder WithArguments(float position, float strength)
    {
        Arguments = new object[] { position, strength };
        return this;
    }

    protected override void Validate()
    {
        base.Validate();
        if (Arguments.Length != 2)
            throw new ArgumentException($"{nameof(MoveInstructionBuilder)} has not exactly 2 arguments.");
        if (!(Arguments[0] is float position && position >= -3.0f && position <= 3.0f))
            throw new ArgumentException($"{nameof(Arguments)}[0] is not a float between -3.0 and 3.0.");
        if (!(Arguments[1] is float strength && strength >= 0f && strength <= 1f))
            throw new ArgumentException($"{nameof(Arguments)}[1] is not a float between 0 and 1.");
    }
}

public class DropInstructionBuilder : InstructionBuilder
{
    public DropInstructionBuilder()
        => OpCode = InstructionOpCode.Drop;

    protected override void Validate()
    {
        base.Validate();
        if (Arguments.Length != 0)
            throw new ArgumentException($"{nameof(DropInstructionBuilder)} should not have any arguments.");
    }
}

public class AbilityInstructionBuilder : InstructionBuilder
{
    public AbilityInstructionBuilder()
        => OpCode = InstructionOpCode.Ability;

    protected override void Validate()
    {
        base.Validate();
        if (Arguments.Length != 0)
            throw new ArgumentException($"{nameof(AbilityInstructionBuilder)} should not have any arguments.");
    }
}

public class RestartInstructionBuilder : InstructionBuilder
{
    public RestartInstructionBuilder()
        => OpCode = InstructionOpCode.Restart;

    public override InstructionBuilder WithChildren(Func<Instruction, IEnumerable<InstructionBuilder>> action)
        => throw new ArgumentException($"{nameof(RestartInstructionBuilder)} should not have any child instructions.");

    protected override void Validate()
    {
        base.Validate();
        if (Arguments.Length != 0)
            throw new ArgumentException($"{nameof(RestartInstructionBuilder)} should not have any arguments.");
    }
}

public static class ExampleClass
{
    public static void ExampleMethod()
    {
        Instruction instruction = new MoveInstructionBuilder()
            .WithArguments(2.3f, 1.0f)
            .WithChildren((parent) =>
            {
                var abilityInstruction = new AbilityInstructionBuilder()
                    .WithDelay(0.8f);
                var dropInstruction = new DropInstructionBuilder()
                    .WithDelay(0.8f);
                return new InstructionBuilder[] { abilityInstruction, dropInstruction };
            }).Build();
    }
}