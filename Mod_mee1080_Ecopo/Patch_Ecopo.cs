using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Mee1080_Ecopo;

[HarmonyPatch(typeof(InvOwnerRecycle), nameof(InvOwnerRecycle._OnProcess))]
class Patch_Ecopo
{

    public static bool Prefix(InvOwnerRecycle __instance, Thing t)
    {
        if (t.id != "stone" || t.Num < 2)
        {
            return true;
        }
        Msg.Say("dump", t, __instance.Container.Name);
        EClass.pc.Pick(ThingGen.Create("ecopo").SetNum(t.Num / 2));
        t.Destroy();
        return false;
    }
}
