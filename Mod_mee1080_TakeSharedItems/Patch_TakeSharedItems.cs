using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Mee1080_TakeSharedItems;

[HarmonyPatch(typeof(Chara), nameof(Chara.TryTakeSharedItems), [typeof(IEnumerable<Thing>), typeof(bool), typeof(bool)])]
class Patch_TakeSharedItems_TryTakeSharedItems
{
    private static void Prefix(Chara __instance, IEnumerable<Thing> containers, bool msg, bool shouldEat)
    {
        // Plugin.Log($"TryTakeSharedItems {__instance.Name}");
        // 所持(0)or最後に食べた日(index+1)
        Dictionary<string, int> minHistory = [];
        for (int i = __instance._historyFood.Count - 1; i >= 0; i--)
        {
            foreach (string id in __instance._historyFood[i])
            {
                minHistory[id] = i + 1;
            }
        }

        int numFood = 2;
        int numJustCooked = 1;
        __instance.things.ForEach(t =>
        {
            if (__instance.CanEat(t))
            {
                numFood -= t.Num;
                minHistory[t.id] = 0;
                if (t.HasElement(757))
                {
                    numJustCooked -= t.Num;
                }
            }
        });
        // Plugin.Log($"minHistory={minHistory}");
        // Plugin.Log($"numFood={numFood}, numJustCooked={numJustCooked}");
        if (numFood <= 0) return;

        // 共有食料
        List<Thing> candidates = [];
        bool needBlood = __instance.HasElement(1250);
        foreach (Thing container in containers)
        {
            if (!container.IsSharedContainer) continue;
            container.things.ForEach(t =>
            {
                // 食べられないものは無視
                if (!__instance.CanEat(t, shouldEat)) return;

                // 吸血鬼以外は血の糧を無視
                if (!needBlood && t.HasElement(710)) return;

                // 追加済のものは無視
                if (candidates.Any(t2 => t2.id == t.id)) return;

                candidates.Add(t);
            });
        }
        // Plugin.Log($"candidates.Count={candidates.Count}");
        if (candidates.Count == 0) return;
        candidates.Shuffle();

        Thing first = null;
        Thing second = null;
        int firstScore = 0;
        int secondScore = 0;
        foreach (Thing t in candidates)
        {
            // 非所持かつ最近食べてない料理を優先
            // できたて料理非所持の場合、できたて料理優先
            int score = (numJustCooked > 0 && t.HasElement(757) ? 1 : 0)
                 + (minHistory.ContainsKey(t.id) ? minHistory[t.id] * 10 : 100);
            if (score > firstScore)
            {
                second = first;
                secondScore = firstScore;
                first = t;
                firstScore = score;
            }
            else if (score > secondScore)
            {
                second = t;
                secondScore = score;
            }
        }
        Add(__instance, first, msg);
        if (numFood >= 2) Add(__instance, second, msg);
    }

    private static void Add(Chara chara, Thing thing, bool msg)
    {
        if (thing == null) return;
        if (chara.things.IsFull(thing)) return;
        Thing thing2 = thing.parent as Thing;
        thing = thing.Split(1);
        if (msg)
        {
            chara.Say("takeSharedItem", chara, thing, thing2.GetName(NameStyle.Full));
        }
        chara.AddCard(thing);
    }
}
