using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Mee1080_RndHalf;

[HarmonyPatch(typeof(EClass), nameof(EClass.rndHalf))]
class Patch_RndHalf
{

    public static void Postfix(int a, ref int __result)
    {
        __result = (a + Rand.rnd(a)) / 2;
    }
}
