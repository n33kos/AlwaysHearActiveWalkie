using System.Collections.Generic;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace LCAlwaysHearWalkieMod.Patches;

[HarmonyPatch(typeof(PlayerControllerB))]
internal class PlayerControllerBPatch
{
    private static readonly float throttleInterval = 0.35f;
    private static float throttle = 0f;
    private static readonly float AverageDistanceToHeldWalkie = 2f;
    private static readonly float AudibleDistance = Plugin.AudibleDistance;
    private static readonly float WalkieRecordingRange = Plugin.WalkieRecordingRange;
    private static readonly float PlayerToPlayerSpatialHearingRange = Plugin.PlayerToPlayerSpatialHearingRange;

    [HarmonyPatch("Update")]
    [HarmonyPostfix]
    static void alwaysHearWalkieTalkiesPatch(ref bool ___holdingWalkieTalkie, ref PlayerControllerB __instance)
    {
        // Throttle calls to reduce performance impact
        throttle += Time.deltaTime;
        if (throttle < throttleInterval) {
            return;
        }
        throttle = 0f;

        // Early returns
        if (__instance == null) {
            return;
        }
        if (GameNetworkManager.Instance == null)
        {
            return;
        }
        if (__instance != GameNetworkManager.Instance.localPlayerController) {
            return;
        }
        if (GameNetworkManager.Instance.localPlayerController == null)
        {
            return;
        }

        if (!GameNetworkManager.Instance.localPlayerController.isPlayerDead) {
            List<WalkieTalkie> walkieTalkiesInRange = new List<WalkieTalkie>();
            List<WalkieTalkie> walkieTalkiesOutOfRange = new List<WalkieTalkie>();
            for (int i = 0; i < WalkieTalkie.allWalkieTalkies.Count; i++)
            {
                float distance = Vector3.Distance(WalkieTalkie.allWalkieTalkies[i].transform.position, __instance.transform.position);

                if (distance <= AudibleDistance)
                {
                    if (WalkieTalkie.allWalkieTalkies[i].isBeingUsed) {
                        walkieTalkiesInRange.Add(WalkieTalkie.allWalkieTalkies[i]);
                    }
                } else {
                    walkieTalkiesOutOfRange.Add(WalkieTalkie.allWalkieTalkies[i]);
                }
            }

            bool isAnyWalkieInRange = walkieTalkiesInRange.Count > 0;

            // If the player is going in or out of range of an active walkie
            if (isAnyWalkieInRange != __instance.holdingWalkieTalkie) {
                // Set the holdingWalkieTalkie bool to true if the player is within range of an active walkie
                ___holdingWalkieTalkie = isAnyWalkieInRange;
                
                // Immediately stop audio from walkies that are out of range
                for (int i = 0; i < walkieTalkiesOutOfRange.Count; i++)
                {
                    if (i < walkieTalkiesInRange.Count) {
                        walkieTalkiesOutOfRange[i].thisAudio.Stop();
                    }
                }
            }

            // Return early if we are out of range of all active walkies
            if (!isAnyWalkieInRange) {
                return;
            }
        }

        // Get the local player controller (or the spectated player controller) as the listener
        PlayerControllerB localOrSpectatedPlayerController = (
            !GameNetworkManager.Instance.localPlayerController.isPlayerDead
            || !(GameNetworkManager.Instance.localPlayerController.spectatedPlayerScript != null)
        )
            ? GameNetworkManager.Instance.localPlayerController
            : GameNetworkManager.Instance.localPlayerController.spectatedPlayerScript;

        for (int i = 0; i < StartOfRound.Instance.allPlayerScripts.Length; i++)
        {
            PlayerControllerB otherPlayerController = StartOfRound.Instance.allPlayerScripts[i];

            // Return if:
                // the player is not controlled by a player
                // The player is dead 
                // The other player controller is the local player controller
            if (
                (!otherPlayerController.isPlayerControlled && !otherPlayerController.isPlayerDead)
                || otherPlayerController == GameNetworkManager.Instance.localPlayerController
                || otherPlayerController.isPlayerDead
            )
            {
                continue;
            }

            // In PlayerControllerBPatch we set the holdingWalkieTalkie bool to true if the player is within range of an active walkie instead of actively holding one
            bool isOtherPlayerNearActiveWalkie = otherPlayerController.holdingWalkieTalkie;

            if (isOtherPlayerNearActiveWalkie)
            {
                float distanceLocalPlayerToOtherPlayer = Vector3.Distance(localOrSpectatedPlayerController.transform.position, otherPlayerController.transform.position);
                float distanceLocalPlayerToClosestWalkie = float.MaxValue;
                float distanceOtherPlayerToClosestWalkie = float.MaxValue;

                for (int j = 0; j < WalkieTalkie.allWalkieTalkies.Count; j++)
                {
                    // Get current walkie talkie
                    WalkieTalkie currentWalkieTalkie = WalkieTalkie.allWalkieTalkies[j];

                    // If walkie is not being used (Powered Off), skip it
                    if (!currentWalkieTalkie.isBeingUsed)
                    {
                        continue;
                    }

                    // Get the distance from the local player to the closest active walkie
                    float distanceLocalToWalkie = Vector3.Distance(currentWalkieTalkie.target.transform.position, localOrSpectatedPlayerController.transform.position);
                    if (distanceLocalToWalkie < distanceLocalPlayerToClosestWalkie)
                    {
                        distanceLocalPlayerToClosestWalkie = distanceLocalToWalkie;
                    }

                    // If walkie is not held or player holding walkie is not speaking into walkie talkie, skip it
                    if (
                        !currentWalkieTalkie.playerHeldBy
                        || (currentWalkieTalkie.playerHeldBy && !currentWalkieTalkie.playerHeldBy.speakingToWalkieTalkie)
                    )
                    {
                        continue;
                    }

                    // Get the distance from the other player to the closest active walkie
                    float distanceOtherToWalkie = Vector3.Distance(currentWalkieTalkie.transform.position, otherPlayerController.transform.position);
                    if (distanceOtherToWalkie < distanceOtherPlayerToClosestWalkie)
                    {
                        distanceOtherPlayerToClosestWalkie = distanceOtherToWalkie;
                    }
                }
                
                float normalizedOtherWalkieTalkieDistance = 1f - Mathf.InverseLerp(AverageDistanceToHeldWalkie, WalkieRecordingRange, distanceOtherPlayerToClosestWalkie);
                float normalizedLocalClosestValue =  1f - Mathf.InverseLerp(AverageDistanceToHeldWalkie, WalkieRecordingRange, distanceLocalPlayerToClosestWalkie);
                float playerVolumeByWalkieTalkieDistance = Mathf.Clamp01(1f + (normalizedOtherWalkieTalkieDistance - normalizedLocalClosestValue));

                // Derive the spatial volume for the other player based on the distance of the local player to the other player
                float playerVolumeBySpatialDistance = 1f - Mathf.InverseLerp(1f, PlayerToPlayerSpatialHearingRange, distanceLocalPlayerToOtherPlayer);

                // Set the volume of the other player to the louder of the two volumes
                otherPlayerController.voicePlayerState.Volume = Mathf.Max(playerVolumeByWalkieTalkieDistance, playerVolumeBySpatialDistance);

                // Set audio effect based on which volume "source" is louder
                if (playerVolumeByWalkieTalkieDistance > playerVolumeBySpatialDistance)
                {
                    makePlayerSoundWalkieTalkie(otherPlayerController);
                }
                else
                {
                    makePlayerSoundSpatial(otherPlayerController);
                }
            }
        }

        static void makePlayerSoundWalkieTalkie(PlayerControllerB playerController)
        {
            AudioSource currentVoiceChatAudioSource = playerController.currentVoiceChatAudioSource;
            AudioLowPassFilter lowPass = currentVoiceChatAudioSource.GetComponent<AudioLowPassFilter>();
            AudioHighPassFilter highPass = currentVoiceChatAudioSource.GetComponent<AudioHighPassFilter>();
            OccludeAudio occludeAudio = currentVoiceChatAudioSource.GetComponent<OccludeAudio>();

            highPass.enabled = true;
            lowPass.enabled = true;
            occludeAudio.overridingLowPass = true;

            currentVoiceChatAudioSource.spatialBlend = 0f;
            playerController.currentVoiceChatIngameSettings.set2D = true;
            currentVoiceChatAudioSource.outputAudioMixerGroup = SoundManager.Instance.playerVoiceMixers[playerController.playerClientId];
            currentVoiceChatAudioSource.bypassListenerEffects = false;
            currentVoiceChatAudioSource.bypassEffects = false;
            currentVoiceChatAudioSource.panStereo = GameNetworkManager.Instance.localPlayerController.isPlayerDead ? 0f : 0.4f;
            occludeAudio.lowPassOverride = 4000f;
            lowPass.lowpassResonanceQ = 3f;
        }

        static void makePlayerSoundSpatial(PlayerControllerB playerController)
        {
            AudioSource currentVoiceChatAudioSource = playerController.currentVoiceChatAudioSource;
            AudioLowPassFilter lowPass = currentVoiceChatAudioSource.GetComponent<AudioLowPassFilter>();
            AudioHighPassFilter highPass = currentVoiceChatAudioSource.GetComponent<AudioHighPassFilter>();
            OccludeAudio occludeAudio = currentVoiceChatAudioSource.GetComponent<OccludeAudio>();

            highPass.enabled = false;
            lowPass.enabled = true;
            occludeAudio.overridingLowPass = playerController.voiceMuffledByEnemy;

            currentVoiceChatAudioSource.spatialBlend = 1f;
            playerController.currentVoiceChatIngameSettings.set2D = false;
            currentVoiceChatAudioSource.bypassListenerEffects = false;
            currentVoiceChatAudioSource.bypassEffects = false;
            currentVoiceChatAudioSource.outputAudioMixerGroup = SoundManager.Instance.playerVoiceMixers[playerController.playerClientId];
            lowPass.lowpassResonanceQ = 1f;
        }
    }
}