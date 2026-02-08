using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Mee1080_SelectFeatOnClaim;

[HarmonyPatch(typeof(Zone), nameof(Zone.ClaimZone))]
class Patch_SelectFeatOnClaim
{

    private static List<SourceElement.Row> selectedFeats = [];

    private static string SelectedFeatNames => string.Join(", ", selectedFeats.Select(x => x.GetName()));

    public static bool Prefix(Zone __instance)
    {
        Plugin.Log("ClaimZone: selected feats [" + SelectedFeatNames + "]");
        if (selectedFeats.Count == 3)
        {
            __instance.landFeats = [];
            foreach (SourceElement.Row row in selectedFeats)
            {
                __instance.landFeats.Add(row.id);
            }
            selectedFeats = [];
            return true;
        }
        if (selectedFeats.Count == 0)
        {
            string[] listBase = __instance.IDBaseLandFeat.Split(',');
            if (listBase.Length != 1)
            {
                Plugin.Log("Ignore fixed feats");
                return true;
            }
            Plugin.Log("Base feat: " + listBase[0]);
            selectedFeats = [EClass.sources.elements.alias[listBase[0]]];
        }
        List<SourceElement.Row> availableFeats = AvailableFeats();
        if (availableFeats.Count == 0)
        {
            Plugin.Log("Feats not found");
            selectedFeats = [];
            return true;
        }
        ShowDialog(__instance, availableFeats);
        return false;
    }

    private static void ShowDialog(Zone zone, List<SourceElement.Row> availableFeats)
    {
        string title = L("フィート選択") + "(" + SelectedFeatNames + ", ?)";
        Dialog.List(title, availableFeats, (item) => item.GetName(), delegate (int index, string name)
        {
            selectedFeats.Add(availableFeats[index]);
            zone.ClaimZone();
            return true;
        });
    }

    /**
     * based on: Zone.ListLandFeats
     */
    private static List<SourceElement.Row> AvailableFeats()
    {
        return EClass.sources.elements.rows.Where(delegate (SourceElement.Row e)
        {
            if (e.category != "landfeat" || e.chance == 0)
            {
                return false;
            }
            if (selectedFeats.Contains(e))
            {
                return false;
            }
            bool flag = true;
            string[] tag = e.tag;
            foreach (string text2 in tag)
            {
                if (text2.StartsWith("bf"))
                {
                    flag = false;
                    if (selectedFeats[0].alias == text2)
                    {
                        flag = true;
                        break;
                    }
                }
            }
            return flag;
        }).ToList();
    }

    private static string L(string key)
    {
        if (Lang.isJP) return key;
        switch (key)
        {
            case "フィート選択": return "Select Feat";
            default: return key;
        }
    }
}
