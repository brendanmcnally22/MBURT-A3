# MBURT-A3

# Cat AI System Documentation
Author: Brendan McNally

---

## Demo Video

Click below to watch the Cat AI behaviour demonstration:

[![Cat AI Behaviour Demonstration](https://img.youtube.com/vi/Mv-4XNsc9DA/0.jpg)](https://youtu.be/Mv-4XNsc9DA)

---

## Overview

For this assignment, I designed a predator-style cat AI using a finite state machine (FSM) in Unity. The goal was to make the cat feel reactive and believable instead of behaving like a simple guard that immediately chases the player when detected.

Instead of instantly chasing on detection, the cat transitions through multiple behaviour states such as sneaking, investigating sounds, and searching last known positions. These behaviours encourage the player to think about movement timing, visibility, and positioning instead of relying on predictable AI patterns.

The AI system combines:

- vision detection
- sound-based investigation
- memory of last known positions
- structured search behaviour after losing the player

This creates an interaction loop where the cat appears to actively hunt rather than passively react.

---

## AI Architecture

The cat uses a finite state machine architecture where only one state can be active at a time.

States included in the system:

- Route
- Sneak
- Chase
- Search
- Investigate
- Meow

Each state controls its own behaviour logic and movement rules.

State transitions occur only through the `SwitchState()` function inside the CatAI script. Because each state exits before another begins execution, this guarantees that only one behaviour runs at a time and prevents overlapping execution. This ensures the system does not contain leaky states.

The FSM diagram used for development is included in this repository as:
# Cat AI System Documentation
Author: Brendan McNally

---

## Demo Video

Click below to watch the Cat AI behaviour demonstration:

[![Cat AI Behaviour Demonstration](https://img.youtube.com/vi/Mv-4XNsc9DA/0.jpg)](https://youtu.be/Mv-4XNsc9DA)

---

## Overview

For this assignment, I designed a predator-style cat AI using a finite state machine (FSM) in Unity. The goal was to make the cat feel reactive and believable instead of behaving like a simple guard that immediately chases the player when detected.

Instead of instantly chasing on detection, the cat transitions through multiple behaviour states such as sneaking, investigating sounds, and searching last known positions. These behaviours encourage the player to think about movement timing, visibility, and positioning instead of relying on predictable AI patterns.

The AI system combines:

- vision detection
- sound-based investigation
- memory of last known positions
- structured search behaviour after losing the player

This creates an interaction loop where the cat appears to actively hunt rather than passively react.

---

## AI Architecture

The cat uses a finite state machine architecture where only one state can be active at a time.

States included in the system:

- Route
- Sneak
- Chase
- Search
- Investigate
- Meow

Each state controls its own behaviour logic and movement rules.

State transitions occur only through the `SwitchState()` function inside the CatAI script. Because each state exits before another begins execution, this guarantees that only one behaviour runs at a time and prevents overlapping execution. This ensures the system does not contain leaky states.

The FSM diagram is included in the project files

---

## Design Goals

My goals when designing this AI system were:

- avoid instant chase behaviour
- allow the cat to react to sound as well as vision
- make the cat remember where the player was last seen
- create behaviour that feels intentional instead of scripted

I wanted the player to feel like the cat was actively hunting rather than simply reacting.

---

## Vision System

The cat detects the player using three checks:

- distance check
- viewing angle check
- raycast obstruction check

Detection works in this order:

1. the player enters the detection radius
2. the player falls inside the viewing angle range
3. a raycast confirms line of sight is not blocked
4. the last seen player position is stored
5. the cat transitions into Sneak state

Instead of immediately chasing, the cat enters Sneak state first so encounter escalation feels natural and readable.

---

## Hearing System

The player generates sound while moving using the PlayerNoiseEmitter script.

Originally I planned to include landing noise for jumping, but I later simplified the movement system so the player remains grounded. Because of that change, I removed the landing noise logic so the detection system only includes behaviours that actually occur during gameplay.

Sound detection transitions the cat into Investigate behaviour. This allows the AI to react even when the player remains outside direct visibility.

---

## Route Behaviour

Route state is the default behaviour when the cat has no player information.

During this state the cat:

- moves between route points
- listens for sound input
- checks for visibility continuously
- occasionally performs idle meow behaviour

This keeps the cat active instead of remaining stationary when the player is not nearby.

---

## Sneak Behaviour

Sneak behaviour begins when the cat first detects the player visually.

Instead of immediately chasing, the cat:

- approaches from angled positions
- keeps some distance temporarily
- waits a short minimum time before escalation

This prevents instant Route-to-Chase transitions and makes encounters easier for the player to read.

---

## Chase Behaviour

Chase begins once the cat moves close enough to the player.

During chase:

- the cat directly pursues the player
- last known player position updates continuously
- visibility memory refreshes
- catch detection becomes active

If the player escapes line of sight, the cat transitions into Search behaviour instead of immediately returning to Route.

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

This allows the AI to respond even when the player remains outside the viewing angle range and supports stealth-style movement decisions.

---

## Meow Behaviour

The meow state provides audio feedback representing behaviour transitions.

Different meow types communicate:

- idle behaviour
- alert detection
- frustration after losing the player

This improves behaviour readability without requiring UI indicators.

---

## Memory System

The AI stores two types of memory.

### Visual Memory

Stores:

- last seen player position
- short timer before forgetting

This allows the cat to continue searching after losing visibility.

### Hearing Memory

Stores:

- last heard player position
- investigation duration timer

This allows the cat to respond to sound even without visual confirmation.

Together these systems prevent unrealistic instant forgetting behaviour.

---

## Player Interaction Design

The AI influences player decision making by encouraging:

- movement timing awareness
- line-of-sight management
- sound discipline
- route prediction

Because the cat reacts to both sound and vision instead of relying on only one detection method, the player must think about positioning and movement timing instead of simply staying outside detection range.

This makes the AI behaviour influence how the player navigates the environment rather than acting as a passive obstacle.

---

## Technical Structure

The AI system uses:

- NavMeshAgent movement
- layered sensing systems
- finite state machine transitions
- timed behaviour logic

Each behaviour state operates independently and transitions only through the SwitchState() function.

Because states never run simultaneously and always exit before another begins execution, this guarantees the FSM does not contain leaky states.

---

## Reflection

While building this system, I focused on making the cat feel like a hunter instead of a guard-style AI.

I avoided instant chase behaviour by introducing a Sneak state before escalation. I also implemented sound-based investigation so the player could not rely entirely on staying outside visual detection range.

During development I simplified the movement noise system by removing landing noise once jumping was removed from the player controller. This kept the detection logic focused only on behaviours that actually occur during gameplay.
