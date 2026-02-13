namespace Mee1080_Poop;

using System.Linq;
using HarmonyLib;

[HarmonyPatch(typeof(FactionBranch), nameof(FactionBranch.TryTrash))]
public class Patch_Poop_TryTrash
{
    public static bool Prefix(Thing t)
    {
        if (t.id != "_poop" || (t.material.alias != "gold" && t.material.alias != "silver"))
        {
            //Plugin.Log($"Ignore {t.Name}");
            return true;
        }
        Thing container = EClass._map.props.installed.containers
            .FirstOrDefault(x => x.trait is TraitFridge && x.IsSharedContainer && !x.things.IsFull(t));
        if (container == null)
        {
            //Plugin.Log($"container not found");
            return true;
        }
        //Plugin.Log($"{t.Name} moved to {container.Name}");
        container.AddCard(t);
        return false;
    }
}
