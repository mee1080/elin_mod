using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Mee1080_DNAInfo;

[HarmonyPatch(typeof(Chara), nameof(Chara.GetHoverText2))]
class Patch_DNAInfo_GetHoverText2
{
    public static void Postfix(Chara __instance, ref string __result)
    {
        Tactics tactics = __instance.tactics;
        List<string> items = [];
        __instance.elements.ListBestAttributes().Take(3).ToList().ForEach(attr =>
         {
             if (attr.ValueWithoutLink / 4 > 0)
             {
                 items.Add(attr.Name);
             }
         });
        __instance.elements.ListBestSkills().Take(6).ToList().ForEach(skill =>
        {
            if (skill.ValueWithoutLink / 4 > 0)
            {
                items.Add(skill.Name);
            }
        });
        __result += Environment.NewLine + ((Lang.isJP ? "遺伝子対象: " : "DNA targets: ") + string.Join(", ", items)).TagSize(14);
    }
}
