using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Mee1080_DisplayInfo;

internal static class ModInfo
{
    internal const string Guid = "mee1080.display_info";
    internal const string Name = "DisplayInfo";
    internal const string Version = "1.0.0.0";
}

[BepInPlugin(ModInfo.Guid, ModInfo.Name, ModInfo.Version)]
public class Plugin : BaseUnityPlugin
{

    private const bool Debug = true;

    private static ManualLogSource StaticLogger;

    private void Start()
    {
        StaticLogger = Logger;
        var harmony = new Harmony(ModInfo.Guid);
        harmony.PatchAll();
    }

    public static void Log(object data)
    {
        if (Debug)
        {
            StaticLogger?.LogInfo(data);
        }
    }
}
