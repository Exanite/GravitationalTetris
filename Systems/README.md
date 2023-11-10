# Shared Systems

## EcsSystem

Base class for any systems that interacts with the ECS World. Technically not needed, but provides a reference to the World object.

## ICallbackSystem

Allows event callbacks to be registered before the simulation begins running. This is to ensure that all callbacks are added before any events are raised. Mutations to the simulation should be avoided here.

## IStartSystem

Initializes the simulation. Use this to create entities, etc.

## IUpdateSystem

Advances the simulation. Use this to run simulation-related code.

## IDrawSystem

Draws the simulation. Mutations to the simulation should be avoided here.
