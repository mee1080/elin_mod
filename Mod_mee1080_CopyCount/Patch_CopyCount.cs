using HarmonyLib;
using System;
using System.Linq;

namespace Mee1080_CopyCount;

[HarmonyPatch(typeof(Thing), nameof(Thing.WriteNote))]
class Patch_CopyCount
{

    static void Prefix(Thing __instance, ref Action<UINote> onWriteNote)
    {
        onWriteNote = (Action<UINote>)Delegate.Combine(onWriteNote, (UINote n) => WriteNote(__instance, n));
    }

    private static void WriteNote(Thing __instance, UINote n)
    {
        Chara kettle = null;
        foreach (var item in EClass.game.cards.globalCharas.Values)
        {
            if (item.id == "kettle" && item.trait?.CanJoinParty == true)
            {
                kettle = item;
                break;
            }
        }
        if (kettle == null || !kettle.trait.CanCopy(__instance))
        {
            return;
        }
        int num4 = 1;
        if (__instance.trait?.CanStack == true)
        {
            Thing thing3 = __instance.Duplicate(1);
            thing3.isStolen = false;
            thing3.isCopy = true;
            thing3.c_priceFix = 0;
            foreach (Element item in thing3.elements.dict.Values.Where((Element e) => e.HasTag("noInherit")).ToList())
            {
                thing3.elements.Remove(item.id);
            }
            num4 = (1000 + kettle.c_invest * 100) / (thing3.GetPrice(CurrencyType.Money, sell: false, PriceType.CopyShop) + 50);
            int[] array = new int[3] { 704, 703, 702 };
            foreach (int ele in array)
            {
                if (thing3.HasElement(ele))
                {
                    num4 = 1;
                }
            }
        }
        n.AddText(L("copy_count", num4.ToString()));
    }

    private static string L(string id, params string[] args)
    {
        if (Lang.isJP)
        {
            return id switch
            {
                "copy_count" => $"複製数: {args[0]}",
                _ => id,
            };
        }
        else
        {
            return id switch
            {
                "copy_count" => $"Copy count: {args[0]}",
                _ => id,
            };
        }
    }
}
