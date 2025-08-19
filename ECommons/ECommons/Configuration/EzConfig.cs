﻿using Dalamud.Utility;
using ECommons.DalamudServices;
using ECommons.ImGuiMethods;
using ECommons.Logging;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ECommons.Configuration;

/// <summary>
/// A class that aims to significantly simplify working with Dalamud configuration.
/// 1. Does not includes type definitions, which allows changing underlying type if it can be deserialized from existing data (list into array...)
/// 2. Provides anti-corruption mechanism, reducing chance of data loss if game crashes or power goes off during configuration writing
/// 3. Allows to very easily load default configuration as well as additional configuration, taking path to config folder into account.
/// 4. Allows you to redefine serializer with your own implementation upon serializing or in general for the whole EzConfig module.
/// 5. Solves the issues with default Dalamud serialization settings where default values of collection will stay in addition to ones that were deserialized.
/// </summary>
public static class EzConfig
{
    public static string? PluginConfigDirectoryOverride { get; set; } = null;
    public static bool UseExternalWriter
    {
        get
        {
            return field;
        }
        set
        {
            if(Util.GetHostPlatform() != OSPlatform.Windows)
            {
                field = false;
                PluginLog.Warning($"External file writer is only supported on Windows. OS detected: {Util.GetHostPlatform()}. External file writer is disabled.");
            }
            else
            {
                field = value;
            }
        }
    } = false;

    public static Action<string, string>? SaveFileActionOverride
    {
        get => field;
        set
        {
            if(field != null) throw new InvalidOperationException("Can not change override once it has been set");
        }
    }

    public static string GetPluginConfigDirectory()
    {
        if(PluginConfigDirectoryOverride == null) return Svc.PluginInterface.GetPluginConfigDirectory();
        var d = new DirectoryInfo(Svc.PluginInterface.GetPluginConfigDirectory());
        var path = Path.Combine(d.Parent!.FullName, PluginConfigDirectoryOverride);
        Directory.CreateDirectory(path);
        return path;
    }
    /// <summary>
    /// Full path to default configuration file.
    /// </summary>
    public static string DefaultConfigurationFileName => Path.Combine(EzConfig.GetPluginConfigDirectory(), DefaultSerializationFactory.DefaultConfigFileName);
    /// <summary>
    /// Default configuration reference
    /// </summary>
    public static object? Config { get; private set; }

    private static bool WasCalled = false;

    public static event Action? OnSave;

    /// <summary>
    /// Default serialization factory. Create a class that extends SerializationFactory, implement your own serializer and deserializer and assign DefaultSerializationFactory to it before loading any configurations to change serializer to your own liking.
    /// </summary>
    public static ISerializationFactory DefaultSerializationFactory
    {
        get
        {
            return EzConfigValueStorage.DefaultSerializationFactory;
        }
        set
        {
            if(WasCalled) throw new InvalidOperationException("Can not change DefaultSerializationFactory after any configurations has been loaded or saved");
            EzConfigValueStorage.DefaultSerializationFactory = value;
        }
    }

    /// <summary>
    /// Loads and returns default configuration file
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T Init<T>() where T : new()
    {
        Config = LoadConfiguration<T>(DefaultSerializationFactory.DefaultConfigFileName);
        return (T)Config;
    }

    /// <summary>
    /// Migrates old default configuration to EzConfig, if applicable. Must be called before Init.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <exception cref="NullReferenceException"></exception>
    public static void Migrate<T>() where T : new()
    {
        if(Config != null)
        {
            throw new NullReferenceException("Migrate must be called before initialization");
        }
        WasCalled = true;
        var path = DefaultConfigurationFileName;
        var configFile = PluginConfigDirectoryOverride == null ? Svc.PluginInterface.ConfigFile : new FileInfo(Path.Combine(new DirectoryInfo(Svc.PluginInterface.GetPluginConfigDirectory()).Parent!.FullName, PluginConfigDirectoryOverride + ".json"));
        if(!DefaultSerializationFactory.FileExists(path) && configFile.Exists)
        {
            PluginLog.Warning($"Migrating {configFile} into EzConfig system");
            Config = LoadConfiguration<T>(configFile.FullName, false);
            Save();
            Config = null;
            File.Move(configFile.FullName, $"{configFile}.old");
        }
        else
        {
            PluginLog.Information($"Migrating conditions are not met, skipping...");
        }
    }

    /// <summary>
    /// Saves default configuration file, if applicable. 
    /// </summary>
    public static void Save()
    {
        if(Config != null)
        {
            SaveConfiguration(Config, DefaultSerializationFactory.DefaultConfigFileName, true);
            if(OnSave != null)
            {
                GenericHelpers.Safe(OnSave.Invoke);
            }
        }
    }

    /// <summary>
    /// Saves arbitrary configuration file.
    /// </summary>
    /// <param name="Configuration">Configuration instance</param>
    /// <param name="path">Path to save to</param>
    /// <param name="prettyPrint">Inform serializer that you want pretty-print your configuration</param>
    /// <param name="appendConfigDirectory">If true, plugin configuration directory will be added to path</param>
    /// <param name="serializationFactory">If null, then default factory will be used.</param>
    /// <param name="writeFileAsync">Whether to perform writing operation in a separate thread. Serialization is performed in current thread.</param>
    public static void SaveConfiguration(this object Configuration, string path, bool prettyPrint = false, bool appendConfigDirectory = true, ISerializationFactory? serializationFactory = null, bool writeFileAsync = false)
    {
        WasCalled = true;
        serializationFactory ??= DefaultSerializationFactory;
        string serializedString = null;
        byte[] serializedBinary = null;
        if(serializationFactory.IsBinary)
        {
            serializedBinary = serializationFactory.SerializeAsBin(Configuration) ?? throw new NullReferenceException();
        }
        else
        {
            serializedString = serializationFactory.Serialize(Configuration) ?? throw new NullReferenceException();
        }
        if(appendConfigDirectory) path = Path.Combine(EzConfig.GetPluginConfigDirectory(), path);
        if(UseExternalWriter)
        {
            if(serializationFactory.IsBinary)
            {
                ExternalWriter.PlaceWriteOrder(new(path, serializedBinary));
            }
            else
            {
                ExternalWriter.PlaceWriteOrder(new(path, serializedString));
            }
        }
        else
        {
            void Write()
            {
                try
                {
                    lock(Configuration)
                    {
                        if(serializationFactory.IsBinary)
                        {
                            serializationFactory.WriteFile(path, serializedBinary);
                        }
                        else
                        {
                            serializationFactory.WriteFile(path, serializedString);
                        }
                        
                    }
                }
                catch(Exception e)
                {
                    e.Log();
                }
            }
            if(writeFileAsync)
            {
                Task.Run(Write);
            }
            else
            {
                Write();
            }
        }
    }

    /// <summary>
    /// Loads arbitrary configuration file or creates an empty one.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path">Where to load it from.</param>
    /// <param name="appendConfigDirectory">If true, plugin configuration directory will be added to path</param>
    /// <param name="serializationFactory">If null, then default factory will be used.</param>
    /// <returns></returns>
    public static T LoadConfiguration<T>(string path, bool appendConfigDirectory = true, ISerializationFactory? serializationFactory = null) where T : new()
    {
        WasCalled = true;
        serializationFactory ??= DefaultSerializationFactory;
        if(appendConfigDirectory) path = Path.Combine(EzConfig.GetPluginConfigDirectory(), path);
        if(!serializationFactory.FileExists(path))
        {
            return new T();
        }
        if(serializationFactory.IsBinary)
        {
            return serializationFactory.Deserialize<T>(serializationFactory.ReadFileAsBin(path)) ?? new T();
        }
        else
        {
            return serializationFactory.Deserialize<T>(serializationFactory.ReadFileAsText(path)) ?? new T();
        }
    }

    internal static void Dispose()
    {
        OnSave = null;
    }
}
