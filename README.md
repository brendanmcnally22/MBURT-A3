# Cat AI System Documentation
Author: Brendan McNally

---

## Demo Video

Click below to watch the Cat AI behaviour demonstration:

[Cat AI Behaviour Demonstration Video](https://youtu.be/Mv-4XNsc9DA)

---

## Overview

For this assignment I designed a predator-style cat AI using a finite state machine in Unity. The goal of this system was to make the cat feel reactive and believable instead of behaving like a simple guard that immediately chases the player when detected.

Instead of instantly chasing the player, the cat transitions through multiple behaviour states such as sneaking, investigating sounds, and searching last known positions. These behaviours encourage the player to think about movement timing, visibility, and positioning instead of relying on predictable AI behaviour.

The system combines:

- vision detection
- sound-based investigation
- last known position memory
- structured search behaviour after losing the player

Together these create the feeling that the cat is actively hunting instead of simply reacting.

---

## AI Architecture

The cat behaviour is controlled using a finite state machine.

The states included in the system are:

- Route
- Sneak
- Chase
- Search
- Investigate
- Meow

Each state controls its own movement behaviour and transition logic. Only one state can run at a time and all transitions occur through a single `SwitchState()` function inside the CatAI script.

Because the previous state always exits before the next begins execution, this guarantees that behaviours never overlap and prevents leaky state execution.

The FSM diagram used for development is included with the project files.

---

## Design Goals

My goals when designing this system were:

- avoid instant chase behaviour
- allow the cat to react to sound as well as vision
- make the cat remember where the player was last seen
- create behaviour that feels intentional instead of scripted

I wanted the cat to feel like it was hunting rather than simply reacting.

---

## Vision System

The cat detects the player using three checks:

- distance check
- viewing angle check
- raycast obstruction check

Detection works in this order:

1. the player enters detection range
2. the player falls inside the viewing angle
3. a raycast confirms line of sight is not blocked
4. the last seen position is stored
5. the cat transitions into Sneak behaviour

Instead of immediately chasing, the cat first transitions into Sneak behaviour so encounters feel more natural and readable.

---

## Hearing System

The player generates sound while moving using the PlayerNoiseEmitter script.

Originally I planned to include landing noise for jumping, but later removed this because the player controller does not include jumping. This kept the detection system focused only on behaviours that actually occur during gameplay.

Sound detection transitions the cat into Investigate behaviour so the AI can respond even without direct visibility.

---

## Route Behaviour

Route is the default behaviour when the cat has no player information.

During this state the cat:

- moves between route points
- listens for sound input
- checks for visibility continuously
- occasionally performs idle meow behaviour

This keeps the cat active instead of leaving it stationary when the player is not nearby.

---

## Sneak Behaviour

Sneak behaviour begins when the cat first detects the player visually.

Instead of immediately chasing, the cat:

- approaches from angled positions
- keeps some distance temporarily
- waits a short amount of time before escalating behaviour

This prevents instant Route-to-Chase transitions and makes encounters easier for the player to read.

---

## Chase Behaviour

Chase begins once the cat moves close enough to the player.

During chase:

- the cat directly pursues the player
- last known player position updates continuously
- visibility memory refreshes
- catch detection becomes active

If the player escapes line of sight, the cat transitions into Search behaviour instead of immediately returning to Route behaviour.

---

## Search Behaviour

Search begins after the cat loses visual contact with the player.

During search the cat:

- moves to the last seen position
- explores nearby positions
- checks for sound input
- checks for player visibility again

If the player is not found, the cat eventually returns to Route behaviour.

This prevents unrealistic instant forgetting behaviour.

---

## Investigate Behaviour

Investigate behaviour is triggered by sound detection instead of vision.

The cat moves toward:

- the last heard position
- nearby investigation points around that location

This allows the AI to react even when the player remains outside the viewing angle.

---

## Meow Behaviour

The meow state provides audio feedback representing behaviour transitions.

Different meow types communicate:

- idle behaviour
- alert detection
- frustration after losing the player

This helps make behaviour easier to understand without needing additional UI indicators.

---

## Memory System

The AI stores two types of memory.

Visual memory stores:

- last seen player position
- short timer before forgetting

Hearing memory stores:

- last heard player position
- investigation duration timer

Together these systems prevent the cat from instantly forgetting the player after losing detection.

---

## Player Interaction Design

The AI influences player decision making by encouraging:

- movement timing awareness
- line-of-sight management
- sound discipline
- route prediction

Because the cat reacts to both sound and vision instead of relying on only one detection method, the player has to think about positioning and movement timing instead of simply staying outside detection range.

This makes the AI behaviour influence how the player moves through the environment rather than acting as a passive obstacle.

---

## Technical Structure

The AI system uses:

- NavMeshAgent movement
- layered sensing systems
- finite state machine transitions
- timed behaviour logic

Each behaviour state operates independently and transitions only through the `SwitchState()` function.

Because states never run simultaneously and always exit before another begins execution, this guarantees that the FSM does not contain leaky states.

---

## Reflection

While building this system I focused on making the cat feel like a hunter instead of a guard-style AI.

I avoided instant chase behaviour by introducing a Sneak state before escalation. I also implemented sound-based investigation so the player could not rely entirely on staying outside visual detection range.

During development I simplified the noise system by removing landing noise once jumping was removed from the player controller so the detection logic only included behaviours that actually occur during gameplay.
