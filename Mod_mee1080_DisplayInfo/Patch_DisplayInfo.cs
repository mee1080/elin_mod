namespace Mee1080_DisplayInfo;

using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;

internal static class Colors
{
    public readonly static Color UniqueColor = new(1.0f, 0.2f, 0.2f);
    private readonly static Dictionary<Affinity.Stage, Color> affinityColor = new()
    {
        {Affinity.Stage.Foe, new Color(0.1f, 0.1f, 0.1f)},
        {Affinity.Stage.Hate, new(0.0f, 0.2f, 0.8f)},
        {Affinity.Stage.Annoying, new(0.6f, 0.2f, 0.8f)},
        {Affinity.Stage.Normal, new(0.8f, 0.8f, 0.8f)},
        {Affinity.Stage.Approved, new(0.6f, 0.8f, 0.6f)},
        {Affinity.Stage.Friendly, new(0.2f, 0.8f, 0.2f)},
        {Affinity.Stage.Respected, new(0.6f, 0.8f, 0.2f)},
        {Affinity.Stage.Intimate, new(0.8f, 0.6f, 0.2f)},
        {Affinity.Stage.Fond, new(0.9f, 0.5f, 0.2f)},
        {Affinity.Stage.Love, new(0.8f, 0.2f, 0.2f)},
        {Affinity.Stage.LoveLove, new(0.9f, 0.1f, 0.1f)},
        {Affinity.Stage.LoveLoveLove, new(1.0f, 0.0f, 0.0f)}
    };

    public static Color AffinityColor(Affinity.Stage stage) => GetColor(affinityColor, stage);

    public readonly static Color HpLabelColor = new(0.8f, 0.2f, 0.2f);
    public readonly static Color SpLabelColor = new(0.2f, 0.8f, 0.2f);
    public readonly static Color MpLabelColor = new(0.4f, 0.4f, 0.8f);
    public readonly static Color ValueHighColor = new(0.7f, 1.0f, 0.7f);
    public readonly static Color ValueMiddleColor = new(0.8f, 0.8f, 0.7f);
    public readonly static Color ValueLowColor = new(1.0f, 0.6f, 0.6f);

    private static Color GetColor<S>(Dictionary<S, Color> dictionary, S key)
    {
        return dictionary.ContainsKey(key) ? dictionary[key] : Color.white;
    }
}

internal static class Patch_DisplayInfo_Data
{
    public static bool inGetHoverText2 = false;
}

[HarmonyPatch(typeof(Chara), nameof(Chara.GetHoverText))]
class Patch_DisplayInfo_GetHoverText
{
    public static void Postfix(Chara __instance, ref string __result)
    {
        string lv = $"Lv.{__instance.LV}";
        string unique = __instance.IsUnique ? "★".TagColor(Colors.UniqueColor) : "";
        string affinity = "♥".TagColor(Colors.AffinityColor(__instance.affinity.CurrentStage));
        __result = $"{lv}{unique} {affinity} {__result}";
    }
}

[HarmonyPatch(typeof(Chara), nameof(Chara.GetHoverText2))]
class Patch_DisplayInfo_GetHoverText2
{
    public static void Prefix()
    {
        Patch_DisplayInfo_Data.inGetHoverText2 = true;
    }

    public static void Postfix(Chara __instance, ref string __result)
    {
        Chara chara = __instance;
        string text = "";

        string job = chara.job.GetText().ToTitleCase();
        string raceAgeGender = chara.bio.TextBio(chara);
        string birthday = chara.bio.TextBirthDate(chara);
        text += $"{Environment.NewLine}{job} {raceAgeGender} {birthday}";

        string hp = "HP:".TagColor(Colors.HpLabelColor) + ValueText(chara.hp, chara.MaxHP);
        string dvpv = $"DV:{chara.DV} PV:{chara.PV}";
        string speed = $"{Lang.Get("speed")}:{chara.Speed}";
        text += $"{Environment.NewLine}{hp} {dvpv} {speed}";

        if (EInput.isAltDown)
        {
            string sp = "SP:".TagColor(Colors.SpLabelColor) + ValueText(chara.stamina.value, chara.stamina.max);
            string hunger = $"{chara.hunger.name}:{chara.hunger.value}/{chara.hunger.max}";
            string armorStyle = chara.elements.GetOrCreateElement(chara.GetArmorSkill()).Name;
            string attackStyle = ("style" + chara.body.GetAttackStyle()).lang();
            text += $"{Environment.NewLine}{sp} {hunger} {armorStyle} {attackStyle}";

            string mp = "MP:".TagColor(Colors.MpLabelColor) + ValueText(chara.mana.value, chara.mana.max);
            string weight = $"{Lang.Get("headerWeight")}:{chara.ChildrenWeight / 1000}s/{chara.WeightLimit / 1000}s";
            string exp = $"EXP:{chara.exp}/{chara.ExpToNext}";
            text += $"{Environment.NewLine}{mp} {weight} {exp}";

            List<Element> mainStatusList = [];
            Dictionary<string, bool> mainStatusKeep = [];
            List<List<Element>> resistanceList = [];
            chara.elements.ListElements(comparison: (e1, e2) => e1.source.id.CompareTo(e2.source.id)).ForEach(e =>
            {
                //Plugin.Log($"{e.Name}:{e.Value} {e.source.type} {e.source.alias}");
                if (e.source.type == "AttbMain")
                {
                    if (e.source.lvFactor > 0)
                    {
                        mainStatusList.Add(e);
                    }
                }
                else if (e.source.type == "Resistance")
                {
                    Element parent = e.GetParent(chara);
                    if (parent != null)
                    {
                        if (resistanceList.Count == 0 || resistanceList.Last().Count >= 5)
                        {
                            resistanceList.Add([]);
                        }
                        resistanceList.Last().Add(e);
                    }
                }
                else if (e.source.alias.StartsWith("sustain_"))
                {
                    mainStatusKeep[e.source.alias.Substring(8)] = true;
                }
            });
            string mainStatus = mainStatusList.Join(e => MainStatus(e, mainStatusKeep), delimiter: " ");
            text += $"{Environment.NewLine}{mainStatus}".TagSize(14);

            if (resistanceList.Count > 0)
            {
                string resistance = resistanceList.Join(l => l.Join(e => $"{e.Name}:{e.Value}", delimiter: " "), delimiter: Environment.NewLine);
                text += $"{Environment.NewLine}{resistance}".TagSize(14);
            }

            List<List<string>> abilityList = [];
            chara.ability.list.items.ForEach(e =>
            {
                if (abilityList.Count == 0 || abilityList.Last().Count >= 4)
                {
                    abilityList.Add([]);
                }
                Element parent = e.act.GetParent(chara);
                string parentText = parent == null ? "" : $"({parent.Name})";
                abilityList.Last().Add($"{e.act.Name}{parentText}");
            });
            if (abilityList.Count > 0)
            {
                string ability = abilityList.Join(l => l.Join(delimiter: " "), delimiter: Environment.NewLine);
                text += $"{Environment.NewLine}{ability}".TagSize(14);
            }

            // Tactics tactics = chara.tactics;
            // text += $"{Environment.NewLine}{Lang.Get("tactics")}: {tactics.sourceChara.GetName()}({tactics.source.GetName()}) Range:{tactics.DestDist} MoveRate:{tactics.ChanceMove} MoveRate2:{tactics.ChanceSecondMove}".TagSize(14);
        }
        __result = text + __result;
        Patch_DisplayInfo_Data.inGetHoverText2 = false;
    }

    private static string ValueText(int current, int max)
    {
        float rate = (float)current / max;
        Color color;
        if (rate > 0.5f)
        {
            color = Colors.ValueHighColor;
        }
        else if (rate > 0.2f)
        {
            color = Colors.ValueMiddleColor;
        }
        else
        {
            color = Colors.ValueLowColor;
        }
        return $"{current}/{max}".TagColor(color);
    }

    private static string MainStatus(Element element, Dictionary<string, bool> mainStatusKeep)
    {
        bool keep = mainStatusKeep.ContainsKey(element.source.alias) && mainStatusKeep[element.source.alias];
        return $"{element.Name}:{element.Value}{(keep ? "+" : "")}";
    }
}

[HarmonyPatch(typeof(Card), nameof(Card.GetHoverText))]
class Patch_DisplayInfo_GetHoverText_Card
{
    public static void Postfix(Card __instance, ref string __result)
    {
        string lv = $"Lv.{__instance.LV}";
        string unique = __instance.IsUnique ? "★".TagColor(Colors.UniqueColor) : "";
        string material = __instance.material == null ? "" : $" [{__instance.material.GetName()}]";
        __result = $"{lv}{unique} {material} {__result}";
    }
}

internal abstract class Patch_DisplayInfo_GetPhaseStr
{
    protected static string Apply(BaseStats __instance, string __result)
    {
        //Plugin.Log($"GetPhaseStr {__instance.source.GetName()}({__instance.source.alias}) {__instance.GetValue()}");
        if (!Patch_DisplayInfo_Data.inGetHoverText2 || __result.IsEmpty() || __result == "#")
        {
            return __result;
        }
        __result = $"{__result}({__instance.GetValue()})";
        Chara owner = __instance.Owner;
        if (owner?.resistCon != null && owner.resistCon.ContainsKey(__instance.id))
        {
            __result += "{" + owner.resistCon[__instance.id] + "}";
        }
        return __result;
    }
}

[HarmonyPatch(typeof(BaseStats), nameof(BaseStats.GetPhaseStr))]
class Patch_DisplayInfo_GetPhaseStr_BaseStats : Patch_DisplayInfo_GetPhaseStr
{

    public static void Postfix(BaseStats __instance, ref string __result)
    {
        __result = Apply(__instance, __result);
    }
}

[HarmonyPatch(typeof(BaseCondition), nameof(BaseCondition.GetPhaseStr))]
class Patch_DisplayInfo_GetPhaseStr_BaseCondition : Patch_DisplayInfo_GetPhaseStr
{

    public static void Postfix(BaseCondition __instance, ref string __result)
    {
        __result = Apply(__instance, __result);
    }
}

[HarmonyPatch(typeof(ConBuffStats), nameof(ConBuffStats.GetPhaseStr))]
class Patch_DisplayInfo_GetPhaseStr_ConBuffStats : Patch_DisplayInfo_GetPhaseStr
{

    public static void Postfix(ConBuffStats __instance, ref string __result)
    {
        __result = Apply(__instance, __result);
    }
}

[HarmonyPatch(typeof(BaseTaskHarvest), nameof(BaseTaskHarvest.GetText))]
class Patch_DisplayInfo_GetText_BaseTaskHarvest
{
    public static void Postfix(BaseTaskHarvest __instance, ref string __result)
    {
        string tool = __instance.tool == null ? "" : $"{__instance.tool.Name} ";
        string toolLv = $"Lv.{__instance.toolLv}";
        string compare = __instance.IsTooHard ? "<" : ">=";
        string reqLv = $"{__instance.reqLv}";
        string text = $"{Environment.NewLine}{tool}{toolLv} {compare} {reqLv}";
        if (__instance.IsTooHard)
        {
            text = text.TagColor(Color.gray);
        }
        __result += text;
    }
}

[HarmonyPatch(typeof(NotificationStats), nameof(NotificationStats.OnRefresh))]
class Patch_DisplayInfo_NotificationStats
{
    private static bool showExtra = EClass.debug.showExtra;

    public static void Prefix(NotificationStats __instance)
    {
        showExtra = EClass.debug.showExtra;
        if (!__instance.stats().GetText().IsEmpty())
        {
            EClass.debug.showExtra = true;
        }
    }

    public static void Postfix()
    {
        EClass.debug.showExtra = showExtra;
    }
}
