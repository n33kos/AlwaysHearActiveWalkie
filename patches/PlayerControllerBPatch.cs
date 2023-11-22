using System;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace LCAlwaysHearWalkieMod.Patches
{
  [HarmonyPatch(typeof(PlayerControllerB))]
  internal class PlayerControllerBPatch
  {
    [HarmonyPatch("Update")]
    [HarmonyPostfix]
    static void alwaysHearWalkieTalkiesPatch(ref bool ___holdingWalkieTalkie)
    {
      ___holdingWalkieTalkie = true;
    }
  }
}