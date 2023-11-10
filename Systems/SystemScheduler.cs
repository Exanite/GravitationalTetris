using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;

namespace Exanite.Extraction.Systems;

public class SystemScheduler
{
    public SystemScheduler(ILifetimeScope container, Config config)
    {
        AllSystems = config.AllSystems.Select(system => (ISystem)container.Resolve(system)).ToList();
        CallbackSystems = config.CallbackSystems.Select(system => (ICallbackSystem)container.Resolve(system)).ToList();
        StartSystems = config.StartSystems.Select(system => (IStartSystem)container.Resolve(system)).ToList();
        UpdateSystems = config.UpdateSystems.Select(system => (IUpdateSystem)container.Resolve(system)).ToList();
        DrawSystems = config.DrawSystems.Select(system => (IDrawSystem)container.Resolve(system)).ToList();
    }

    public List<ISystem> AllSystems { get; }
    public List<ICallbackSystem> CallbackSystems { get; set; }
    public List<IStartSystem> StartSystems { get; }
    public List<IUpdateSystem> UpdateSystems { get; }
    public List<IDrawSystem> DrawSystems { get; }

    public void RegisterCallbacks()
    {
        foreach (var system in CallbackSystems)
        {
            system.RegisterCallbacks();
        }
    }

    public void Start()
    {
        foreach (var system in StartSystems)
        {
            system.Start();
        }
    }

    public void Update()
    {
        foreach (var system in UpdateSystems)
        {
            system.Update();
        }
    }

    public void Draw()
    {
        foreach (var system in DrawSystems)
        {
            system.Draw();
        }
    }

    public class Config : Module
    {
        private readonly HashSet<Type> registeredSystemTypeSet = new();

        public List<Type> AllSystems { get; } = new();
        public List<Type> CallbackSystems { get; } = new();
        public List<Type> StartSystems { get; } = new();
        public List<Type> UpdateSystems { get; } = new();
        public List<Type> DrawSystems { get; } = new();

        public void RegisterCallbackSystem<T>() where T : ICallbackSystem
        {
            RegisterSystem<T>();
            CallbackSystems.Add(typeof(T));
        }

        public void RegisterStartSystem<T>() where T : IStartSystem
        {
            RegisterSystem<T>();
            StartSystems.Add(typeof(T));
        }

        public void RegisterUpdateSystem<T>() where T : IUpdateSystem
        {
            RegisterSystem<T>();
            UpdateSystems.Add(typeof(T));
        }

        public void RegisterDrawSystem<T>() where T : IDrawSystem
        {
            RegisterSystem<T>();
            DrawSystems.Add(typeof(T));
        }

        protected override void Load(ContainerBuilder builder)
        {
            foreach (var systemType in registeredSystemTypeSet)
            {
                builder.RegisterType(systemType).AsSelf().AsImplementedInterfaces().SingleInstance();
            }
        }

        private void RegisterSystem<T>() where T : ISystem
        {
            if (registeredSystemTypeSet.Add(typeof(T)))
            {
                AllSystems.Add(typeof(T));
            }
        }
    }
}
