using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Mee1080_Milk;

/**
 * Milk
 * original:
 *   https://steamcommunity.com/sharedfiles/filedetails/?id=3397427592
 *   https://github.com/air1068/MilkStats
 */
[HarmonyPatch(typeof(Thing), nameof(Thing.WriteNote))]
class Patch_Milk
{
    public const int SKILL_TAMING = 237;

    private static string FetchStats(Queue<string> s, int count)
    {
        string r = "";
        for (int i = 1; i < count && s.Count > 1; i++)
        {
            r += s.Dequeue() + ", ";
        }
        r += s.Dequeue();
        return ("<size=12>" + r + "</size>").TagColor(FontColor.Passive);
    }

    static void Postfix(Thing __instance, UINote n)
    {
        if (__instance.trait is TraitDrinkMilkMother && !__instance.trait.owner.c_idRefCard.IsNull())
        {
            int tmp_uidNext = EClass.game.cards.uidNext;
            EClass.game.cards.uidNext = 1;
            Rand.SetSeed(1);
            Chara chara = CharaGen.Create(__instance.trait.owner.c_idRefCard);
            int lv = 5 + __instance.trait.owner.encLV * 5;
            n.AddText(L("It requires training level ") + lv.ToString() + L(""));
            int limit = 20 + EClass.pc.Evalue(SKILL_TAMING);
            chara.SetLv(Mathf.Clamp(lv, 1, limit));
            Rand.SetSeed();
            EClass.game.cards.uidNext = tmp_uidNext;

            Queue<string> s = new Queue<string>();
            int num = 100;
            foreach (Element attribute in chara.elements.ListBestAttributes())
            {
                double value = attribute.ValueWithoutLink * 100.0 / num / 2.0;
                if (value >= 0.5)
                {
                    s.Enqueue(attribute.Name + " " + value.ToString("F2"));
                }
                num += 50;
            }
            if (s.Count > 0)
            {
                n.AddText(L("Milk bonus status: ") + FetchStats(s, 999), FontColor.Good);
            }

            s.Clear();
            num = 100;
            foreach (Element skill in chara.elements.ListBestSkills())
            {
                double value = skill.ValueWithoutLink * 100.0 / num / 2.0;
                if (value >= 0.5)
                {
                    s.Enqueue(skill.Name + " " + value.ToString("F2"));
                }
                num += 50;
            }

            if (s.Count > 0)
            {
                n.AddText(L("Milk bonus skills: ") + FetchStats(s, 999), FontColor.Good);
            }
        }
    }

    private static string L(string text)
    {
        if (Lang.isJP)
        {
            switch (text)
            {
                case "Milk bonus status: ": return "それは授乳により主能力を高める。";
                case "Milk bonus skills: ": return "それは授乳により技能を高める。";
                case "It requires training level ": return "それは調教Lv";
                case "": return "を必要とする。";
                default: return text;
            }
        }
        return text;
    }
}
