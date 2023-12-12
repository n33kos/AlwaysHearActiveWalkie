using System.Collections.Generic;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace LCAlwaysHearWalkieMod.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatch
    {
        private const float AudibleDistance = 20f;
        private const float ThrottleInterval = 0.4f;
        private const float AverageDistanceToHeldWalkie = 2f;
        private const float WalkieRecordingRange = 20f;
        private const float PlayerToPlayerSpatialHearingRange = 20f;
        private static float throttle = 0f;

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        /// <summary>
        /// Postfix for Update method of PlayerControllerB, which adjusts player's hearing ability based on walkie talkie range.
        /// </summary>
        static void AlwaysHearWalkieTalkiesPatch(ref bool ___holdingWalkieTalkie, ref PlayerControllerB __instance)
        {
            if (!ShouldProcessUpdate(__instance)) return;

            throttle = 0f;

            var walkieTalkiesInRange = new List<WalkieTalkie>();
            var walkieTalkiesOutOfRange = new List<WalkieTalkie>();

            foreach (var walkieTalkie in WalkieTalkie.allWalkieTalkies)
            {
                float distance = Vector3.Distance(walkieTalkie.transform.position, __instance.transform.position);
                if (distance <= AudibleDistance && walkieTalkie.isBeingUsed)
                {
                    walkieTalkiesInRange.Add(walkieTalkie);
                }
                else
                {
                    walkieTalkiesOutOfRange.Add(walkieTalkie);
                }
            }

            bool isAnyWalkieInRange = walkieTalkiesInRange.Count > 0;
            ___holdingWalkieTalkie = isAnyWalkieInRange;

            StopOutOfRangeWalkies(walkieTalkiesOutOfRange, walkieTalkiesInRange.Count);

            if (!isAnyWalkieInRange) return;

            var localOrSpectatedPlayerController = GetLocalOrSpectatedPlayerController();
            AdjustPlayerVolumes(localOrSpectatedPlayerController, walkieTalkiesInRange);
        }

        /// <summary>
        /// Determines if the Update method should proceed with its operations.
        /// </summary>
        private static bool ShouldProcessUpdate(PlayerControllerB __instance)
        {
            throttle += Time.deltaTime;
            if (throttle < ThrottleInterval || __instance == null) return false;

            var networkManager = GameNetworkManager.Instance;
            if (networkManager == null || networkManager.localPlayerController == null ||
                networkManager.localPlayerController.isPlayerDead ||
                __instance != networkManager.localPlayerController) return false;

            return true;
        }

        /// <summary>
        /// Stops audio from walkie talkies that are out of range.
        /// </summary>
        private static void StopOutOfRangeWalkies(List<WalkieTalkie> walkieTalkiesOutOfRange, int inRangeCount)
        {
            for (int i = 0; i < walkieTalkiesOutOfRange.Count; i++)
            {
                if (i < inRangeCount)
                {
                    walkieTalkiesOutOfRange[i].thisAudio.Stop();
                }
            }
        }

        /// <summary>
        /// Gets the local player controller or the spectated player controller.
        /// </summary>
        private static PlayerControllerB GetLocalOrSpectatedPlayerController()
        {
            var localPlayerController = GameNetworkManager.Instance.localPlayerController;
            return (!localPlayerController.isPlayerDead || localPlayerController.spectatedPlayerScript != null)
                ? localPlayerController
                : localPlayerController.spectatedPlayerScript;
        }

        /// <summary>
        /// Adjusts the volume of other players based on their distance to the local or spectated player controller and active walkie talkies.
        /// </summary>
        private static void AdjustPlayerVolumes(PlayerControllerB localOrSpectatedPlayerController, List<WalkieTalkie> walkieTalkiesInRange)
        {
            foreach (var playerScript in StartOfRound.Instance.allPlayerScripts)
            {
                if (playerScript == localOrSpectatedPlayerController || playerScript.isPlayerDead || !playerScript.isPlayerControlled) continue;

                if (playerScript.holdingWalkieTalkie)
                {
                    AdjustVolumeBasedOnDistance(playerScript, localOrSpectatedPlayerController, walkieTalkiesInRange);
                }
            }
        }

        /// <summary>
        /// Adjusts the volume of a player based on their distance to walkie talkies and other players.
        /// </summary>
        private static void AdjustVolumeBasedOnDistance(PlayerControllerB otherPlayerController, PlayerControllerB localOrSpectatedPlayerController, List<WalkieTalkie> walkieTalkiesInRange)
        {
            float distanceLocalPlayerToOtherPlayer = Vector3.Distance(localOrSpectatedPlayerController.transform.position, otherPlayerController.transform.position);
            float playerVolumeBySpatialDistance = 1f - Mathf.InverseLerp(0f, PlayerToPlayerSpatialHearingRange, distanceLocalPlayerToOtherPlayer);

            float minDistanceOtherPlayerToWalkie = float.MaxValue;
            float minDistanceLocalPlayerToWalkie = float.MaxValue;
            foreach (var walkieTalkie in walkieTalkiesInRange)
            {
                if (!walkieTalkie.isBeingUsed) continue;

                float distanceOtherToWalkie = Vector3.Distance(walkieTalkie.transform.position, otherPlayerController.transform.position);
                minDistanceOtherPlayerToWalkie = Mathf.Min(minDistanceOtherPlayerToWalkie, distanceOtherToWalkie);

                float distanceLocalToWalkie = Vector3.Distance(walkieTalkie.target.transform.position, localOrSpectatedPlayerController.transform.position);
                minDistanceLocalPlayerToWalkie = Mathf.Min(minDistanceLocalPlayerToWalkie, distanceLocalToWalkie);
            }

            float playerVolumeByWalkieTalkieDistance = Mathf.Min(
                1f - Mathf.InverseLerp(AverageDistanceToHeldWalkie, WalkieRecordingRange, minDistanceOtherPlayerToWalkie),
                1f - Mathf.InverseLerp(AverageDistanceToHeldWalkie, WalkieRecordingRange, minDistanceLocalPlayerToWalkie)
            );

            otherPlayerController.voicePlayerState.Volume = Mathf.Max(playerVolumeByWalkieTalkieDistance, playerVolumeBySpatialDistance);
        }
    }
}
