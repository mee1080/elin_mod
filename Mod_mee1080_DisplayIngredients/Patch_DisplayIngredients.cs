using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mee1080_DisplayIngredients;

static internal class Data
{
    public static Dictionary<string, Dictionary<string, string>> data = null;
}

[HarmonyPatch(typeof(Thing), nameof(Thing.WriteNote))]
class Patch_DisplayIngredients
{

    static void Prefix(Thing __instance, ref Action<UINote> onWriteNote)
    {
        if (Data.data == null)
        {
            Data.data = new Dictionary<string, Dictionary<string, string>>()
            {
                {"sauce_soy", []},
                {"bonito", []},
                {"ration_basic", []}
            };
            Thing seaweed = ThingGen.Create("seaweed");
            Thing bark = ThingGen.Create("bark");
            EClass.sources.cards.rows.Where(r => r.origin?.id == "fish").Do(row =>
            {
                Thing fish = ThingGen.Create(row.id);
                for (int tier = 0; tier <= 3; tier++)
                {
                    fish.SetTier(tier);
                    Thing soySauce = ThingGen.Create("sauce_soy");
                    CraftUtil.MixIngredients(soySauce, [fish, seaweed], CraftUtil.MixType.General, 999);
                    Data.data["sauce_soy"][GetParams(soySauce)] = fish.Name;
                    Thing bonito = ThingGen.Create("bonito");
                    CraftUtil.MixIngredients(bonito, [fish], CraftUtil.MixType.General, 999);
                    Data.data["bonito"][GetParams(bonito)] = fish.Name;
                    Thing kibble = ThingGen.Create("ration_basic");
                    CraftUtil.MixIngredients(kibble, [fish, bark], CraftUtil.MixType.General, 999);
                    Data.data["ration_basic"][GetParams(kibble)] = fish.Name;
                }
            }
            );
        }
        if (!Data.data.ContainsKey(__instance.id)) return;
        // Data.data[__instance.id].Do(e => Plugin.Log(e.Key + ": " + e.Value));
        // Plugin.Log(GetParams(__instance));
        string text = Data.data[__instance.id].TryGetValue(GetParams(__instance));
        if (text != null)
        {
            onWriteNote = (Action<UINote>)Delegate.Combine(onWriteNote, (UINote n) => WriteNote(text, n));
        }
    }

    private static string GetParams(Thing thing)
    {
        return thing.elements.dict.Values
            .Where(e => e.IsFoodTraitMain && e.Value != 0)
            .OrderBy(e => e.id)
            .SelectMany<Element, int>(e => [e.id, e.Value])
            .Join();
    }

    private static void WriteNote(string name, UINote n)
    {
        n.AddText("NoteText_enc", L("madeFrom", name), FontColor.Myth);
    }

    private static string L(string id, params string[] args)
    {
        if (Lang.isJP)
        {
            return id switch
            {
                "madeFrom" => $"それは{args[0]}から作られた。",
                _ => id,
            };
        }
        else
        {
            return id switch
            {
                "madeFrom" => $"It is made from {args[0]}.",
                _ => id,
            };
        }
    }
}
