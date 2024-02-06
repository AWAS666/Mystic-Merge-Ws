# Meteora's Mystic Merge WebSocket Server Guide

## Overview
This explain and gives some samples how you can interact with the game over websocket. There are also json builder classes in C# included. Feel free to contribute your own solutions.

The Websocket Server runs on Port 3141 of localhost and it expects all objects as Json.

You can find builder classes to use in C# in "dotnet".

## Instruction Types
The server recognizes the following instruction types, each with its unique properties:

- **Move**
- **Drop**
- **Ability**
- **Restart**

### Move Instruction
- **OpCode**: `0x01`
- **Arguments**: Position (float, -3.0 to 3.0), Strength (float, 0.0 to 1.0)

### Drop Instruction
- **OpCode**: `0x02`
- **Arguments**: None

### Ability Instruction
- **OpCode**: `0x03`
- **Arguments**: None

### Restart Instruction
- **OpCode**: `0x04`
- **Arguments**: None

You can also chain instructions by providing child instructions.

## JSON Examples:

The delay is only ablied for children and in percentage. This means a delay of 0.8 will be executed once the parent reached 80% completion.

### Example: Move Instruction with drop at location
```json
{
  "OpCode": 1,
  "Delay": 0,
  "Arguments": [1.2, 0.8],
  "Children": [
    {
      "OpCode": "Drop",
      "Delay": 0.8,
      "Arguments": []
    }
  ]
}
```

### Example: Drop Instruction
```json
{
  "OpCode": 2,
  "Delay": 0,
  "Arguments": []
}
```

### Example: Ability Instruction
```json
{
  "OpCode": 3,
  "Delay": 0,
  "Arguments": []
}
```


### Example: Restart Instruction
```json
{
  "OpCode": 4,
  "Delay": 0,
  "Arguments": []
}
```

# Event Types
The server outputs the following event types:

- **Combination**: EventOpCode `0x01`
- **Preview**: EventOpCode `0x02`
- **Available**: EventOpCode `0x03`
- **Update**: EventOpCode `0x04`
- **Loose**: EventOpCode `0x05`

You will get the update event on the regular, all the others only happen once the event occurs.

Orb and Dropper objects are given below the events.
### Combination Event
```json
{
  "OpCode": 1,
  "Mergers": [
    // Array of Orb objects
  ],
  "Score": 0,
  "Charge": 0.0
}
```

### Preview Event
```json
{
  "OpCode": 2,
  "Orb": {
    // Orb object
  }
}
```

### Available Event
```json
{
  "OpCode": 3,
  "Player": {
    // Dropper object
  }
}
```

### Update Event
```json
{
  "OpCode": 4,
  "Preview": {
    // Orb object
  },
  "Player": {
    // Dropper object
  },
  "Orbs": [
    // Array of Orb objects
  ],
  "Score": 0,
  "Charge": 0.0
}
```

### Loose Event
```json
{
  "OpCode": 6,
  "Score": 0,
  "Highscore": 0
}
```

## Object Structures
### Orb
```json
{
  "Id": "GUID",
  "Index": 0,
  "State": "OrbState",
  "Location": {
    "X": 0.0,
    "Y": 0.0
  },
  "Velocity": {
    "X": 0.0,
    "Y": 0.0
  },
  "Radius": 0.0
}
```
### OrbStates can be:
- Preview -> 1
- Inactive -> 2
- Active -> 3
- Removed -> 4

### Index is the type of object:
0. None
1. Star
2. Sock
3. Aiko
4. Yui
5. Cabbage
6. Shiro
7. Hilda
8. Melba
9. Burnt Melba
10. Rune
11. Meteora


### Dropper
```json
{
  "Location": 0.0,
  "Orb": {
    // Orb object
  }
}
```
