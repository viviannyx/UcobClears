﻿using Dalamud.Plugin;
using ECommons.Automation;
using ECommons.Commands;
using ECommons.Configuration;
using ECommons.DalamudServices;
using ECommons.Events;
using ECommons.EzContextMenu;
using ECommons.EzDTR;
using ECommons.EzEventManager;
using ECommons.EzHookManager;
using ECommons.EzIpcManager;
using ECommons.EzSharedDataManager;
using ECommons.GameFunctions;
using ECommons.Hooks;
using ECommons.ImGuiMethods;
using ECommons.LazyDataHelpers;
using ECommons.Loader;
using ECommons.Logging;
using ECommons.ObjectLifeTracker;
using ECommons.Reflection;
using ECommons.SimpleGui;
using ECommons.Singletons;
using ECommons.SplatoonAPI;
using ECommons.StringHelpers;
using ECommons.Throttlers;
using Serilog.Events;
using System;
using System.Linq;
using System.Reflection;
using Callback = ECommons.Automation.Callback;


#nullable disable

namespace ECommons;

public static class ECommonsMain
{
    public static IDalamudPlugin Instance = null;
    public static bool Disposed { get; private set; } = false;

    /// <summary>
    /// Set this to true to significantly reduce amount of logging ECommons will do. You can change it any time. 
    /// </summary>
    public static bool ReducedLogging = false;

    public static void Init(IDalamudPluginInterface pluginInterface, IDalamudPlugin instance, params Module[] modules)
    {
        Instance = instance;
        GenericHelpers.Safe(() => Svc.Init(pluginInterface));
#if DEBUG
var type = "debug build";
#elif RELEASE
        var type = "release build";
#else
var type = "unknown build";
#endif
        if(!ReducedLogging) PluginLog.Information($"This is ECommons v{typeof(ECommonsMain).Assembly.GetName().Version} ({type}) and {Svc.PluginInterface.InternalName} v{instance.GetType().Assembly.GetName().Version}. Hello!");
        Svc.Log.MinimumLogLevel = LogEventLevel.Verbose;
        GenericHelpers.Safe(CmdManager.Init);
        if(modules.ContainsAny(Module.All, Module.ObjectFunctions))
        {
            if(!ReducedLogging) PluginLog.Information("Object functions module has been requested");
            GenericHelpers.Safe(ObjectFunctions.Init);
        }
        if(modules.ContainsAny(Module.All, Module.DalamudReflector, Module.SplatoonAPI))
        {
            if(!ReducedLogging) PluginLog.Information("Advanced Dalamud reflection module has been requested");
            GenericHelpers.Safe(() => DalamudReflector.Init());
        }
        if(modules.ContainsAny(Module.All, Module.ObjectLife))
        {
            if(!ReducedLogging) PluginLog.Information("Object life module has been requested");
            GenericHelpers.Safe(ObjectLife.Init);
        }
        if(modules.ContainsAny(Module.All, Module.SplatoonAPI))
        {
            if(!ReducedLogging) PluginLog.Information("Splatoon API module has been requested");
            GenericHelpers.Safe(Splatoon.Init);
        }
    }

    public static void CheckForObfuscation()
    {
        if(Assembly.GetCallingAssembly().GetTypes().FirstOrDefault(x => x.IsAssignableTo(typeof(IDalamudPlugin))).Name == Svc.PluginInterface.InternalName)
        {
            DuoLog.Error($"{Svc.PluginInterface.InternalName} name match error!");
        }
    }

    public static void Dispose()
    {
        Disposed = true;
        GenericHelpers.Safe(SingletonServiceManager.DisposeAll);
        GenericHelpers.Safe(PluginLoader.Dispose);
        GenericHelpers.Safe(CmdManager.Dispose);
        if(EzConfig.Config != null)
        {
            GenericHelpers.Safe(EzConfig.Save);
        }
        GenericHelpers.Safe(EzConfig.Dispose);
        GenericHelpers.Safe(ThreadLoadImageHandler.ClearAll);
        GenericHelpers.Safe(ObjectLife.Dispose);
        GenericHelpers.Safe(DalamudReflector.Dispose);
        if(EzConfigGui.WindowSystem != null)
        {
            if(EzConfigGui.Type is EzConfigGui.WindowType.Main or EzConfigGui.WindowType.Both)
                Svc.PluginInterface.UiBuilder.OpenMainUi -= EzConfigGui.Open;
            if(EzConfigGui.Type is EzConfigGui.WindowType.Config or EzConfigGui.WindowType.Both)
                Svc.PluginInterface.UiBuilder.OpenConfigUi -= EzConfigGui.Open;
            Svc.PluginInterface.UiBuilder.Draw -= EzConfigGui.Draw;
            if(EzConfigGui.Config != null)
            {
                Svc.PluginInterface.SavePluginConfig(EzConfigGui.Config);
                Notify.Info("Configuration saved");
            }
            EzConfigGui.WindowSystem.RemoveAllWindows();
            EzConfigGui.WindowSystem = null;
        }
        foreach(var x in EzCmd.RegisteredCommands)
        {
            Svc.Commands.RemoveHandler(x);
        }
        if(Splatoon.Instance != null)
        {
            GenericHelpers.Safe(Splatoon.Reset);
        }
        GenericHelpers.Safe(Splatoon.Shutdown);
        GenericHelpers.Safe(ProperOnLogin.Dispose);
        GenericHelpers.Safe(DirectorUpdate.Dispose);
        GenericHelpers.Safe(ActionEffect.Dispose);
        GenericHelpers.Safe(MapEffect.Dispose);
        GenericHelpers.Safe(SendAction.Dispose);
        GenericHelpers.Safe(Automation.LegacyTaskManager.TaskManager.DisposeAll);
        GenericHelpers.Safe(Automation.NeoTaskManager.TaskManager.DisposeAll);
#pragma warning disable CS0618 // Type or member is obsolete
        GenericHelpers.Safe(EqualStrings.Dispose);
#pragma warning restore CS0618 // Type or member is obsolete
        GenericHelpers.Safe(AutoCutsceneSkipper.Dispose);
        GenericHelpers.Safe(() => ThreadLoadImageHandler.httpClient?.Dispose());
        EzThrottler.Throttler = null;
        FrameThrottler.Throttler = null;
        GenericHelpers.Safe(Callback.Dispose);
        GenericHelpers.Safe(EzEvent.DisposeAll);
        GenericHelpers.Safe(EzHookCommon.DisposeAll);
        GenericHelpers.Safe(EzSharedData.Dispose);
        GenericHelpers.Safe(EzIPC.Dispose);
        GenericHelpers.Safe(ContextMenuPrefixRemover.Dispose);
        GenericHelpers.Safe(Purgatory.Purge);
        GenericHelpers.Safe(ExternalWriter.Dispose);
        GenericHelpers.Safe(EzDtr.DisposeAll);
        //SingletonManager.Dispose();
        Instance = null;
    }
}
