using System.Collections.Generic;
using HarmonyLib;

namespace Mee1080_SearchRecipe;

internal class RecipeTarget(string name, Point pos, RecipeManager.LearnState recipeLearnState)
{
    public string Name => name;
    public Point Pos => pos;
    public RecipeManager.LearnState RecipeLearnState => recipeLearnState;
}

[HarmonyPatch(typeof(ActPlan), nameof(ActPlan._Update))]
public class Patch_SearchRecipe_ActPlanUpdate
{
    public static void Postfix(ActPlan __instance, PointTarget target)
    {
        // Plugin.Log($"ActPlan._Update input={__instance.input} pcPos={EClass.pc.pos} targetPos={target.pos} isPC={EClass.pc.pos.Equals(target.pos)}");
        if (__instance.input != ActInput.AllAction || !EClass.pc.pos.Equals(target.pos) || !EClass.pc.HasElement(1661))
        {
            return;
        }
        __instance.TrySetAct(L("action_name"), delegate
        {
            Dictionary<string, RecipeTarget> dict = [];
            foreach (Thing thing in EClass._map.things)
            {
                string recipeID = thing.source.RecipeID;
                string name = thing.source.GetName();
                CheckAndAdd(dict, thing.source.RecipeID, thing.source.GetName(), thing.pos);
            }
            EClass._map.ForeachCell(delegate (Cell c)
            {
                if (c.HasBridge)
                {
                    CheckAndAdd(dict, c.sourceBridge.RecipeID, c.sourceBridge.GetName(), c.GetPoint());
                }
                if (!c.sourceFloor.tileType.IsWater)
                {
                    CheckAndAdd(dict, c.sourceFloor.RecipeID, c.sourceFloor.GetName(), c.GetPoint());
                }
                CheckAndAdd(dict, c.sourceBlock.RecipeID, c.sourceBlock.GetName(), c.GetPoint());
            });
            bool found = false;
            dict.Values.Do(item =>
            {
                if (item.RecipeLearnState == RecipeManager.LearnState.Learnable)
                {
                    found = true;
                    EClass.pc.Say(L("found_message", $"{item.Name}({EClass.pc.pos.x - item.Pos.x},{EClass.pc.pos.z - item.Pos.z})"));
                }
            });
            if (!found)
            {
                EClass.pc.Say(L("no_recipe"));
            }
            return false;
        });
    }

    private static void CheckAndAdd(Dictionary<string, RecipeTarget> dict, string recipeID, string name, Point pos)
    {
        if (dict.ContainsKey(recipeID))
        {
            RecipeTarget current = dict[recipeID];
            if (EClass.pc.Dist(pos) < EClass.pc.Dist(current.Pos))
            {
                //Plugin.Log($"recipeID={recipeID} name={name} pos={pos} update");
                dict[recipeID] = new RecipeTarget(name, pos, current.RecipeLearnState);
            }
        }
        else
        {
            RecipeManager.LearnState recipeLearnState = EClass.player.recipes.GetRecipeLearnState(recipeID);
            dict[recipeID] = new RecipeTarget(name, pos, recipeLearnState);
            //Plugin.Log($"recipeID={recipeID} name={name} pos={pos} recipeLearnState={recipeLearnState}");
        }
    }

    private static string L(string id, params string[] args)
    {
        if (Lang.isJP)
        {
            return id switch
            {
                "action_name" => $"{"TaskDisassemble_newrecipe".lang()}サーチ",
                "no_recipe" => $"{"TaskDisassemble_newrecipe".lang()}なし。",
                "found_message" => $"{args[0]}に{"TaskDisassemble_newrecipe".lang()}。",
                _ => id,
            };
        }
        else
        {
            return id switch
            {
                "action_name" => $"Search {"TaskDisassemble_newrecipe".lang()}",
                "no_recipe" => $"No {"TaskDisassemble_newrecipe".lang()}.",
                "found_message" => $"{"TaskDisassemble_newrecipe".lang()} for {args[0]}.",
                _ => id,
            };
        }
    }
}
