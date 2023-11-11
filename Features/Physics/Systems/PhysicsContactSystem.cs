using System;
using Arch.Core;
using Arch.Core.Extensions;
using CommunityToolkit.HighPerformance;
using Exanite.WarGames.Features.Players.Components;
using Exanite.WarGames.Features.Tiles.Systems;
using Exanite.WarGames.Systems;
using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Collision;
using nkast.Aether.Physics2D.Dynamics.Contacts;
using World = nkast.Aether.Physics2D.Dynamics.World;

namespace Exanite.WarGames.Features.Physics.Systems;

public class PhysicsContactSystem : ICallbackSystem
{
    private readonly World physicsWorld;

    public PhysicsContactSystem(World physicsWorld)
    {
        this.physicsWorld = physicsWorld;
    }

    public void RegisterCallbacks()
    {
        // physicsWorld.ContactManager.BeginContact = (contact) =>
        // {
        //     var bodyA = contact.FixtureA.Body;
        //     var bodyB = contact.FixtureB.Body;
        //
        //     if (bodyA.Tag is not Box<EntityReference> refA || bodyB.Tag is not Box<EntityReference> refB)
        //     {
        //         return contact.Enabled;
        //     }
        //
        //     if (!((EntityReference)refA).Entity.IsAlive() || !((EntityReference)refB).Entity.IsAlive())
        //     {
        //         return contact.Enabled;
        //     }
        //
        //     if (((EntityReference)refA).Entity.Has<PlayerComponent>() || !((EntityReference)refB).Entity.Has<TetrisBlockComponent>())
        //     {
        //         if (Math.Abs(Vector2.Dot(contact.Manifold.LocalNormal, Vector2.UnitY)) > 0.9f)
        //         {
        //         }
        //
        //         return contact.Enabled;
        //     }
        //
        //     return contact.Enabled;
        // };
    }
}
