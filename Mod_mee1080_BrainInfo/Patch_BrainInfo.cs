using HarmonyLib;
using System;
using System.Linq;
using UnityEngine;

namespace Mee1080_BrainInfo;

internal static class Common
{
    public static string L(int dist, int chanceMove, int chanceSecondMove)
    {
        if (Lang.isJP)
        {
            return $"適正距離:{dist} 移動頻度:{chanceMove}%,{chanceSecondMove}%";
        }
        else
        {
            return $"Distance:{dist} MoveRate:{chanceMove}%,{chanceSecondMove}%";
        }
    }
}

[HarmonyPatch(typeof(DNA), nameof(DNA.WriteNote))]
class Patch_BrainInfo_WriteNote
{
    private static void Postfix(DNA __instance, UINote n)
    {
        if (__instance.type != DNA.Type.Brain)
        {
            return;
        }
        SourceChara.Row row = EClass.sources.charas.map[__instance.id];
        string key = row.tactics.IsEmpty(EClass.sources.tactics.map.TryGetValue(row.id)?.id ?? EClass.sources.tactics.map.TryGetValue(row.job)?.id ?? "predator");
        SourceTactics.Row source = EClass.sources.tactics.map[key];
        int distance = row.aiParam.Length < 1 ? source.dist : row.aiParam[0];
        int chanceMove = row.aiParam.Length < 2 ? source.move : row.aiParam[1];
        int chanceSecondMove = row.aiParam.Length < 3 ? 100 : row.aiParam[2];
        n.AddText(Common.L(distance, chanceMove, chanceSecondMove));
    }
}

[HarmonyPatch(typeof(Chara), nameof(Chara.GetHoverText2))]
class Patch_BrainInfo_GetHoverText2
{
    public static void Postfix(Chara __instance, ref string __result)
    {
        Tactics tactics = __instance.tactics;
        __result += $"{Environment.NewLine}{Lang.Get("tactics")}:{tactics.sourceChara.GetName()}({tactics.source.GetName()}) {Common.L(tactics.DestDist, tactics.ChanceMove, tactics.ChanceSecondMove)}".TagSize(14);
    }
}