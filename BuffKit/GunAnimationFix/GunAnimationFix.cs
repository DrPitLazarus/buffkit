using HarmonyLib;
using System;
using UnityEngine;

namespace BuffKit.GunAnimationFix
{
    [HarmonyPatch]
    public class GunAnimationFix
    {
        private static bool _enabled = true;
        private static bool _firstMainMenuState = true;

        /// <summary>
        /// Feature initialization. Create settings.
        /// </summary>
        [HarmonyPatch(typeof(UIManager.UINewMainMenuState), nameof(UIManager.UINewMainMenuState.Enter))]
        [HarmonyPostfix]
        private static void Initialize()
        {
            if (!_firstMainMenuState) return;
            _firstMainMenuState = false;
            Settings.Settings.Instance.AddEntry("misc", "gun animation fix", v => _enabled = v, _enabled);
        }

        /// <summary>
        /// Patched method is called when firing guns with continuous fire (auto-fire) enabled. Original code plays continuous fire animation, but not all guns have that.
        /// This adds a fallback to play single fire animation.
        /// </summary>
        /// <param name="__instance"></param>
        /// <returns></returns>
        [HarmonyPatch(typeof(Turret), nameof(Turret.ContinuousFire))]
        [HarmonyPrefix]
        private static bool ContinuousFire_AddSingleFireFallback(Turret __instance)
        {
            if (!_enabled) return true; // Run original method if not enabled. Following is original game code.
            if (__instance.Ship != null && __instance.ReadyToContinuousFire)
            {
                if (!__instance.Ship.IsHumanControlled || __instance.Ship.IsLocalShip || NetworkedPlayer.Local != null && NetworkedPlayer.Local.IsSpectator)
                {
                    // MODIFIED SECTION.
                    // Following guns must play continuous fire animations. Detected if it has stopTriggerDownEffectSystem set.
                    // Gaze doesn't have it, so I added its type to the check.
                    // Single fire animation highly not recommended. If curious: Gaze and Coil have maximum screen shake.
                    // Gatling and Flamer play trigger down animation repeatedly. Hwacha and Laser trigger down animation doesn't play.
                    if (__instance.AirshipGunFX.stopTriggerDownEffectSystem != null ||
                        __instance.AirshipGunFX is ChargeReleaseFireGunEffectControls /* Gaze and Coil */ )
                    {
                        __instance.PlayContinousFireFX(__instance.localController != null, 1f); // Unmodified.
                    }
                    else
                    {
                        // Otherwise, play single fire animation.
                        __instance.Fire();
                        return false; // Skip original method. Early return.
                    }
                    // END MODIFIED SECTION.
                }
                if (__instance.shot != null)
                {
                    __instance.shot.transform.SetEffectParent(null);
                    __instance.shot.transform.position = __instance.barrels[__instance.currentBarrel].position;
                    __instance.shot.transform.rotation = __instance.barrels[__instance.currentBarrel].rotation;
                    float num = CameraControl.DistanceFromCamera(__instance.cachedTransform.position);
                    if (__instance.AmmoFX != null)
                    {
                        __instance.SetAmmoValues(__instance.AmmoFX);
                        __instance.AmmoFX.continuous = __instance.Continuous;
                        if (__instance.WeaponClass != WeaponClass.ProjectileGun)
                        {
                            __instance.AmmoFX.PlayInFlight(true);
                        }
                        __instance.AmmoFX.rateOfFire = 1f / __instance.FiringInterval;
                        __instance.AmmoFX.weaponClass = __instance.WeaponClass.ToString();
                        __instance.AmmoFX.muzzleVelocity = __instance.data.muzzleSpeed;
                        __instance.AmmoFX.burstSize = __instance.data.shellBurstSize;
                        __instance.AmmoFX.range = __instance.data.range;
                        __instance.AmmoFX.AmmoEquipmentID = __instance.AmmoEquipmentID;
                        __instance.AmmoFX.continuous = __instance.Continuous;
                        RayCastAmmoEffectControls rayCastAmmoEffectControls = __instance.AmmoFX as RayCastAmmoEffectControls;
                        if (rayCastAmmoEffectControls != null)
                        {
                            rayCastAmmoEffectControls.SetWorldVelocity(__instance.Ship.WorldVelocity);
                            rayCastAmmoEffectControls.TryPlayFlyByEffect(false);
                        }
                        if ((!__instance.Ship.IsHumanControlled ? num < 1000f : num < 300f) && __instance.ContinuousMuzzleFlash)
                        {
                            if (__instance.WeaponClass == WeaponClass.ProjectileGun)
                            {
                                __instance.AmmoFX.PlayMuzzleFlash(__instance.transform, __instance.barrels[__instance.currentBarrel]);
                            }
                            else
                            {
                                __instance.AmmoFX.PlayMuzzleFlash(__instance.barrels[__instance.currentBarrel], __instance.barrels[__instance.currentBarrel]);
                            }
                        }
                    }
                }
                __instance.recoilPitch += 2f * (UnityEngine.Random.value - 0.5f) * 2f;
                __instance.recoilYaw += 2f * (UnityEngine.Random.value - 0.5f) * 2f;
                __instance.ContinuousFireTriggered();
            }
            return false; // Skip original method.
        }

        /// <summary>
        /// Patched method is called when firing single (non-continuous) fire guns. Original code plays muzzle flash effect, but misbehaves if the effect is already playing.
        /// Issues are muzzle flash visual and gun firing sound not playing/cutting off.
        /// This makes sure the muzzle flash effect is stopped before playing a new one.
        /// </summary>
        /// <param name="__instance"></param>
        /// <returns></returns>
        [HarmonyPatch(typeof(Turret), nameof(Turret.Fire))]
        [HarmonyPrefix]
        private static bool Fire_AddStopMuzzleFlash(Turret __instance)
        {
            if (!_enabled) return true; // Run original method if not enabled. Following is original game code.
            if (__instance.Ship != null && __instance.TimeSinceLastShot >= __instance.FiringInterval)
            {
                if ((!__instance.Ship.IsHumanControlled || __instance.Ship.IsLocalShip || NetworkedPlayer.Local != null && NetworkedPlayer.Local.IsSpectator) && __instance.AirshipGunFX != null)
                {
                    __instance.AirshipGunFX.PlayTriggerDown(__instance.localController != null, __instance.currentBarrel);
                    __instance.AirshipGunFX.DecreaseWithClip(__instance.Ammunition, __instance.ammunitionClipSize);
                }
                if (__instance.shot != null)
                {
                    try
                    {
                        __instance.shot.transform.rotation = __instance.barrels[__instance.currentBarrel].rotation;
                    }
                    catch (ArgumentOutOfRangeException ex)
                    {
                        MuseLog.Error("Out of range happened on {0}. Too many barrels in the data__instance or not enough on the object (index:{1} >= count:{2})".F(new object[]
                        {
                    __instance.gameObject.name,
                    __instance.currentBarrel,
                    __instance.barrels != null ? __instance.barrels.Count : 0
                        }), __instance.gameObject);
                        if (__instance.barrels != null && __instance.barrels.Count != 0 && __instance.barrels[0] != null)
                        {
                            for (int i = 0; i < __instance.barrels.Count - (__instance.currentBarrel + 1); i++)
                            {
                                __instance.barrels.Add(__instance.barrels[0]);
                            }
                        }
                    }
                    if (__instance.AmmoFX != null)
                    {
                        __instance.SetAmmoValues(__instance.AmmoFX);
                        __instance.AmmoFX.continuous = __instance.Continuous;
                        float num = CameraControl.DistanceFromCamera(__instance.cachedTransform.position);
                        if (__instance.WeaponClass != WeaponClass.ProjectileGun)
                        {
                            __instance.AmmoFX.PlayInFlight(true);
                        }
                        if (__instance.WeaponClass == WeaponClass.RaycastGun)
                        {
                            RayCastAmmoEffectControls rayCastAmmoEffectControls = __instance.AmmoFX as RayCastAmmoEffectControls;
                            if (rayCastAmmoEffectControls != null)
                            {
                                Vector3 vector = new Vector3(__instance.cachedTransform.position.x - __instance.Ship.Position.x, 0f, __instance.cachedTransform.position.z - __instance.Ship.Position.z);
                                float num2 = __instance.Ship.AngularVelocity * vector.magnitude;
                                Vector3 vector2 = Vector3.Cross(Vector3.up, vector.normalized);
                                rayCastAmmoEffectControls.SetWorldVelocity(__instance.Ship.WorldVelocity + num2 * vector2);
                                rayCastAmmoEffectControls.TryPlayFlyByEffect(false);
                            }
                        }
                        if (!__instance.Ship.IsHumanControlled ? num < 1000f : num < 300f)
                        {
                            try
                            {
                                // MODIFIED SECTION.
                                __instance.AmmoFX.StopMuzzleFlash(); // Stop muzzle flash before playing new one.
                                // END MODIFIED SECTION.
                                __instance.AmmoFX.PlayMuzzleFlash(__instance.transform, __instance.barrels[__instance.currentBarrel]);
                            }
                            catch (ArgumentOutOfRangeException ex2)
                            {
                                MuseLog.Error("Out of range happened on {0}. Too many barrels in the data__instance or not enough on the object (index:{1} >= count:{2})".F(new object[]
                                {
                                    __instance.gameObject.name,
                                    __instance.currentBarrel,
                                    __instance.barrels != null ? __instance.barrels.Count : 0
                                }), __instance.gameObject);
                                if (__instance.barrels != null && __instance.barrels.Count != 0 && __instance.barrels[0] != null)
                                {
                                    for (int j = 0; j < __instance.barrels.Count - (__instance.currentBarrel + 1); j++)
                                    {
                                        __instance.barrels.Add(__instance.barrels[0]);
                                    }
                                }
                            }
                        }
                    }
                    __instance.currentBarrel = (__instance.currentBarrel + 1) % __instance.barrels.Count;
                }
                __instance.TimeSinceLastShot = 0f;
                __instance.hasContinuousFired = true;
                __instance.isChargeReleased = true;
            }
            return false; // Skip original method.
        }

        /// <summary>
        /// Patched method is called when a gun is instructed to play the trigger down animation immediately. 
        /// Original code does not check if the animation is still playing, which results in the previous animation still playing and the new one does not start.
        /// This ensures the animation is reset before playing it, so it plays every time.
        /// </summary>
        /// <param name="state"></param>
        /// <param name="__instance"></param>
        /// <returns></returns>
        [HarmonyPatch(typeof(AnimatedEffectSystem), nameof(AnimatedEffectSystem.PlayEffectImediately))]
        [HarmonyPrefix]
        private static bool PlayEffectImmediately_AddRewind(AnimationState state, AnimatedEffectSystem __instance)
        {
            if (!_enabled) return true; // Run original method if not enabled.
            // Make sure animation is reset before playing.
            if (__instance.cachedAnimation.IsPlaying(state.name))
            {
                __instance.cachedAnimation.Rewind(state.name);
            }
            return true; // Run original method.
        }

        // Testing method. Do not use.
        private static void TestSetContinuousForCurrentGuns(bool enabled)
        {
            if (NetworkedPlayer.Local?.CurrentShip == null) return;
            var guns = NetworkedPlayer.Local.CurrentShip.GetComponentsInChildren<Turret>(true);
            foreach (var gun in guns)
            {
                if (gun == null) continue;
                if (gun.Continuous != enabled)
                {
                    gun.Continuous = enabled;
                }
            }
        }
    }
}