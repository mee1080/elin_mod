using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Mee1080_DropWhenSlaughtering;

internal static class Patch_MeatToMarble_Data
{
    public static Card target = null;
}

[HarmonyPatch(typeof(Card), nameof(Card.SpawnLoot))]
class Patch_DropWhenSlaughtering
{

    public static void Prefix(Card __instance)
    {
        if (!AI_Slaughter.slaughtering || !__instance.isChara || __instance.IsPCFactionMinion || __instance.isCopy)
        {
            return;
        }
        Point nearestPoint = __instance.GetRootCard().pos;
        if (nearestPoint.IsBlocked)
        {
            nearestPoint = nearestPoint.GetNearestPoint();
        }
        List<Thing> list = [];
        foreach (Thing thing in __instance.things)
        {
            if (thing.HasTag(CTAG.gift) || thing.trait is TraitChestMerchant || thing.parent == EClass._zone)
            {
                continue;
            }
            list.Add(thing);
        }
        foreach (Thing thing in list)
        {
            thing.isHidden = false;
            thing.SetInt(116);
            EClass._zone.AddCard(thing, nearestPoint);
            if (!thing.IsEquipmentOrRanged || thing.rarity < Rarity.Artifact || thing.IsCursed)
            {
                continue;
            }
            foreach (Chara chara in EClass._map.charas)
            {
                if (chara.HasElement(1412) && chara.Dist(nearestPoint) < 3)
                {
                    thing.Thing.TryLickEnchant(chara);
                    break;
                }
            }
        }
    }
}
