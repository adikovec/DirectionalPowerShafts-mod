using System.Collections.Generic;
using HarmonyLib;
using Timberborn.AreaSelectionSystem;
using Timberborn.BlockSystem;
using Timberborn.ConstructionSites;
using Timberborn.Coordinates;
using Timberborn.TemplateSystem;

namespace DirectionalPowerShafts;

[HarmonyPatch(typeof(ConstructionFactory), nameof(ConstructionFactory.CreateAsUnfinished))]
internal static class NewShaftPatch
{
    private static void Postfix(BlockObject __result)
    {
        __result.GetComponent<DirectionalShaft>()?.EnableForNewSite();
    }
}

[HarmonyPatch(typeof(AreaPicker), "GetPlacements")]
internal static class ShaftLineOrientationPatch
{
    private static void Postfix(PlaceableBlockObjectSpec blockObjectSpec,
        Orientation orientation,
        ref IEnumerable<Placement> __result)
    {
        if (blockObjectSpec.Layout != BlockObjectLayout.TwoSegmentLine
            || !IsTargetShaft(blockObjectSpec.GetSpec<TemplateSpec>().TemplateName))
        {
            return;
        }

        __result = ShaftLineDirections.UseToolOrientation(__result, orientation);
    }

    private static bool IsTargetShaft(string name)
    {
        return name == "PowerShaft.Folktails" || name == "PowerShaft.IronTeeth"
            || name == "VerticalPowerShaft.Folktails"
            || name == "VerticalPowerShaft.IronTeeth";
    }

}
