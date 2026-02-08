using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Mee1080_SelectFeatOnClaim;

internal static class ModInfo
{
    internal const string Guid = "mee1080.select_feat_on_claim";
    internal const string Name = "Mod_mee1080_SelectFeatOnClaim";
    internal const string Version = "1.0.0.0";
}

[BepInPlugin(ModInfo.Guid, ModInfo.Name, ModInfo.Version)]
public class Plugin : BaseUnityPlugin
{

    private const bool Debug = false;

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
