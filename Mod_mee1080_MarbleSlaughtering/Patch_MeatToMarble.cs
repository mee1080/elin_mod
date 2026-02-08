using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MyMod;

internal static class Patch_MeatToMarble_Data
{
    public static Card target = null;
}

[HarmonyPatch(typeof(Card), nameof(Card.SpawnLoot))]
class Patch_MeatToMarble1
{

    public static void Prefix(Card __instance)
    {
        Plugin.Log("Start Drop from " + __instance.Name);
        Patch_MeatToMarble_Data.target = __instance;
    }

    public static void Postfix()
    {
        Plugin.Log("End Drop from " + Patch_MeatToMarble_Data.target?.Name);
        Patch_MeatToMarble_Data.target = null;
    }
}

[HarmonyPatch(typeof(Zone), nameof(Zone.AddCard), [typeof(Card), typeof(int), typeof(int)])]
class Patch_MeatToMarble2
{

    public static void Prefix(ref Card t)
    {
        Card target = Patch_MeatToMarble_Data.target;
        if (target != null && AI_Slaughter.slaughtering && t.id == "_meat")
        {
            Plugin.Log("Dropped meat by slaughtering");
            if (EClass.rnd(2) == 0)
            {
                t = ThingGen.Create("meat_marble").SetNum(t.Num);
                t.MakeFoodFrom(target);
                Plugin.Log("Changed to meat_marble");
            }
        }
    }
}
