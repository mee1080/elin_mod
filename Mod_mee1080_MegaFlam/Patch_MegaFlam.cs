using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Mee1080_MegaFlam;

internal static class Patch_MegaFlam_Data
{
    public static int overrideRadius = 0;
}

[HarmonyPatch(typeof(ActEffect), nameof(ActEffect.ProcAt))]
class Patch_MegaFlam1
{

    public static void Prefix(ActRef actRef)
    {
        Thing thing = actRef.refThing;
        if (thing != null && thing.trait is TraitThrownExplosive && thing.range >= 2)
        {
            Plugin.Log("override radius: " + thing.range);
            Patch_MegaFlam_Data.overrideRadius = thing.range;
        }
        else
        {
            Plugin.Log("ignore");
            Patch_MegaFlam_Data.overrideRadius = 0;
        }
    }

    public static void Postfix()
    {
        Patch_MegaFlam_Data.overrideRadius = 0;
    }
}

[HarmonyPatch(typeof(Map), nameof(Map.ListPointsInCircle))]
class Patch_MegaFlam2
{

    public static void Prefix(ref float radius)
    {
        int overrideRadius = Patch_MegaFlam_Data.overrideRadius;
        if (overrideRadius >= 2)
        {
            radius = overrideRadius;
        }
    }
}

[HarmonyPatch(typeof(RecipeCard), nameof(RecipeCard.Craft))]
class Patch_MegaFlam3
{

    public static void Postfix(Thing __result, bool model)
    {
        Plugin.Log("crafted " + __result?.trait);
        if (model || __result.trait is not TraitThrownExplosive) return;
        RecipeManager recipes = EClass.player.recipes;
        int handicraft = EClass.pc.Evalue(261);
        if (handicraft >= 15 && !recipes.knownRecipes.ContainsKey("mee1080_mega_flam"))
        {
            Msg.Say("learnRecipeIdea");
            recipes.Add("mee1080_mega_flam");
        }
        if (handicraft >= 25 && !recipes.knownRecipes.ContainsKey("mee1080_giga_flam"))
        {
            Msg.Say("learnRecipeIdea");
            recipes.Add("mee1080_giga_flam");
        }
        if (handicraft >= 35 && !recipes.knownRecipes.ContainsKey("mee1080_tera_flam"))
        {
            Msg.Say("learnRecipeIdea");
            recipes.Add("mee1080_tera_flam");
        }
    }
}
