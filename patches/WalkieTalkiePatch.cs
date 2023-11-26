using HarmonyLib;

namespace LCAlwaysHearWalkieMod.Patches
{
  [HarmonyPatch(typeof(WalkieTalkie))]
  internal class WalkieTalkiePatch
  {

    [HarmonyPatch("EnableWalkieTalkieListening")]
    [HarmonyPrefix]
    static bool alwaysHearWalkieTalkiesEnableWalkieTalkieListeningPatch(bool enable)
    {
      // If we are disabling Walkie talkie Listening, skip the original function entirely. We dont want to set holdingWalkieTalkie to 
      // false anymore fromt his location because that is controlled by distance to walkies on the playerControllerB patch.
      // This is potentially dangerous because we are skipping the original function entirely. If the original
      // function changes in the future, we may need to update this patch.
      if (enable == false) {
        return false;  
      }

      return true;

      // Original Function at time of authoring for reference:
      // if (this.playerHeldBy != null)
      // {
      //   this.playerHeldBy.holdingWalkieTalkie = enable;
      // }
      // if (!this.IsPlayerSpectatedOrLocal())
      // {
      //   return;
      // }
      // this.thisAudio.Stop();
      // StartOfRound.Instance.UpdatePlayerVoiceEffects();
    }
  }
}