using HarmonyLib;
using Timberborn.ModManagerScene;
using UnityEngine;

namespace DirectionalPowerShafts;

public sealed class ModStarter : IModStarter
{
    public void StartMod(IModEnvironment modEnvironment)
    {
        new Harmony("directional-power-shafts").PatchAll();
        Debug.Log("Directional Power Shafts: construction direction enabled.");
    }
}
