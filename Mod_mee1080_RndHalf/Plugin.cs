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

namespace Mee1080_RndHalf;

internal static class ModInfo
{
    internal const string Guid = "mee1080.rnd_half";
    internal const string Name = "RndHalf";
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
        // for (int i = 10; i <= 12; i++)
        // {
        //     int[] result = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
        //     for (int j = 0; j < 10000; j++)
        //     {
        //         result[EClass.rndHalf(i)]++;
        //     }
        //     Log($"{i},{result[4]},{result[5]},{result[6]},{result[7]},{result[8]},{result[9]},{result[10]},{result[11]},{result[12]}");
        // }
    }

    public static void Log(object data)
    {
        if (Debug)
        {
            StaticLogger?.LogInfo(data);
        }
    }
}
