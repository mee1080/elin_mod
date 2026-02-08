using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Mee1080_DismantlePlankCutStone;

[HarmonyPatch(typeof(TaskHarvest), nameof(TaskHarvest.HarvestThing))]
class Patch_DismantlePlankCutStone
{

    public static bool Prefix(TaskHarvest __instance)
    {
        Thing target = __instance.target;
        Plugin.Log("HarvestThing: " + target.id);
        if (target.isCopy || target.Num != 1 || (target.id != "plank" && target.id != "cutstone"))
        //if (target.isCopy || target.Num != 1)
        {
            return true;
        }

        string destId = __instance.GetIdDismantled();
        Plugin.Log("To: " + destId);
        if (!__instance.ShouldGenerateDismantled(destId) || (destId != "log" && destId != "rock" && destId != "plank" && destId != "cutstone" && destId != "ingot" && destId != "scrap"))
        {
            return true;
        }
        int decay = target.decay;
        int lV = target.LV;
        target.Die(null, EClass.pc);

        CardBlueprint.Set(new CardBlueprint
        {
            fixedQuality = true
        });
        Thing thing = ThingGen.Create(destId, 1, Mathf.Max(1, lV * 2 / 3));
        if (thing != null)
        {
            thing.SetNum(1);
            thing.ChangeMaterial(target.material);
            thing.decay = decay;
            if (thing.IsDecayed && thing.IsFood)
            {
                thing.elements.SetBase(73, -10);
            }

            EClass._map.TrySmoothPick(__instance.pos.IsBlocked ? __instance.owner.pos : __instance.pos, thing, EClass.pc);
        }
        return false;
    }

}
