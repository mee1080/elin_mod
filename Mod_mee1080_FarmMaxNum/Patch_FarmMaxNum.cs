using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Mee1080_FarmMaxNum;

internal class Patch_FarmMaxNum_Data
{
    internal static bool Active = false;
}

[HarmonyPatch(typeof(Map), nameof(Map.MineObj))]
class Patch_FarmMaxNum1
{

    public static void Prefix(Task task = null)
    {
        Patch_FarmMaxNum_Data.Active = task is TaskHarvest { IsReapSeed: not false };
    }

    public static void PostFix()
    {
        Patch_FarmMaxNum_Data.Active = false;
    }
}

[HarmonyPatch(typeof(Chara), nameof(Chara.PickOrDrop), [typeof(Point), typeof(Thing), typeof(bool)])]
class Patch_FarmMaxNum2
{

    public static void Prefix(Chara __instance, Thing t)
    {
        if (Patch_FarmMaxNum_Data.Active && __instance.IsPC && t.trait is TraitSeed)
        {
            t.SetNum(3);
        }
    }
}
