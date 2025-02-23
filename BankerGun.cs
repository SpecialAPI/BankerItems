using BankerItems.ItemAPI;
using BankerItems.SoundAPI;
using Gungeon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using HarmonyLib;

namespace BankerItems
{
    public class BankerGun : GunBehaviour
    {
        public static void Init()
        {
            Gun gun = ETGMod.Databases.Items.NewGun("Interest Gun", "interest_gun");
            Game.Items.Rename("outdated_gun_mods:interest_gun", "spapi:interest_gun");
            gun.gameObject.AddComponent<BankerGun>().damageMultPerCasing = 0.1f;
            gun.SetShortDescription("High-Interest Loan");
            gun.SetLongDescription("A gun designed to incentivise loan-sharks, they originally only featured a bright display to indicate how good their wielders were at their job. This gun, however, has itself run afoul of the banks " +
                "after it developed a gambling addiction, and will desperately reward the collection of casings to pay off its room-based debt.\n\nCollect casings before clearing a room to increase the gun's stats. " +
                "Stats reset when entering a new room.");
            gun.SetupSprite(null, "interest_gun_idle_001", 8);
            gun.SetAnimationFPS(gun.shootAnimation, 14);
            gun.SetAnimationFPS(gun.reloadAnimation, 8);
            gun.AddProjectileModuleFrom("ak-47", true, false);
            gun.DefaultModule.ammoCost = 1;
            gun.DefaultModule.shootStyle = ProjectileModule.ShootStyle.Automatic;
            gun.DefaultModule.sequenceStyle = ProjectileModule.ProjectileSequenceStyle.Random;
            gun.reloadTime = 1.5f;
            gun.DefaultModule.cooldownTime = 0.2f;
            gun.DefaultModule.numberOfShotsInClip = 8;
            gun.SetBaseMaxAmmo(250);
            gun.PreventStartingOwnerFromDropping = true;
            gun.StarterGunForAchievement = true;
            gun.InfiniteAmmo = true;
            gun.gunSwitchGroup = "Interest";
            var frames = gun.spriteAnimator.GetClipByName(gun.reloadAnimation).frames;
            MakeOffset(frames[1].spriteCollection.spriteDefinitions[frames[1].spriteId], new Vector3(0f, 0.125f), false);
            MakeOffset(frames[2].spriteCollection.spriteDefinitions[frames[2].spriteId], new Vector3(0f, -0.1875f), false);
            SoundManager.AddCustomSwitchData("WPN_Guns", "Interest", "Play_WPN_Gun_Shot_01", new SwitchedEvent("Play_WPN_Gun_Shot_01", "WPN_Guns", "Cash"));
            SoundManager.AddCustomSwitchData("WPN_Guns", "Interest", "Play_WPN_Gun_Reload_01", new SwitchedEvent("Play_WPN_Gun_Reload_01", "WPN_Guns", "Origami"));
            gun.barrelOffset.position = new Vector3(1.1875f, 0.6875f);
            gun.quality = PickupObject.ItemQuality.SPECIAL;
            gun.muzzleFlashEffects = PickupObjectDatabase.GetById(95).GetComponent<Gun>().muzzleFlashEffects;
            Projectile projectile = Instantiate(gun.DefaultModule.projectiles[0]);
            projectile.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(projectile.gameObject);
            DontDestroyOnLoad(projectile);
            projectile.shouldRotate = true;
            gun.DefaultModule.projectiles[0] = projectile;
            projectile.baseData.damage = 6f;
            projectile.baseData.speed = 23f;
            gun.DefaultModule.ammoType = GameUIAmmoType.AmmoType.CUSTOM;
            gun.DefaultModule.customAmmoType = CustomAmmoUtility.AddCustomAmmoType("interest", "InterestFG", "InterestBG", "BankerItems/Resources/interest_clip.png", "BankerItems/Resources/interest_clip_empty.png");
            var animator = projectile.GetAnySprite().gameObject.AddComponent<tk2dSpriteAnimator>();
            animator.Library = projectile.GetAnySprite().gameObject.AddComponent<tk2dSpriteAnimation>();
            animator.Library.clips = new tk2dSpriteAnimationClip[]
            {
                new tk2dSpriteAnimationClip()
                {
                    name = "coin_projectile",
                    fps = 10,
                    frames = new tk2dSpriteAnimationFrame[]
                    {
                        new tk2dSpriteAnimationFrame()
                        {
                            spriteCollection = projectile.GetAnySprite().Collection,
                            spriteId = projectile.GetAnySprite().Collection.GetSpriteIdByName("coin_projectile_001")
                        },
                        new tk2dSpriteAnimationFrame()
                        {
                            spriteCollection = projectile.GetAnySprite().Collection,
                            spriteId = projectile.GetAnySprite().Collection.GetSpriteIdByName("coin_projectile_002")
                        },
                        new tk2dSpriteAnimationFrame()
                        {
                            spriteCollection = projectile.GetAnySprite().Collection,
                            spriteId = projectile.GetAnySprite().Collection.GetSpriteIdByName("coin_projectile_003")
                        },
                        new tk2dSpriteAnimationFrame()
                        {
                            spriteCollection = projectile.GetAnySprite().Collection,
                            spriteId = projectile.GetAnySprite().Collection.GetSpriteIdByName("coin_projectile_004")
                        }
                    }
                }
            };
            animator.playAutomatically = true;
            animator.defaultClipId = 0;
            projectile.GetAnySprite().SetSprite(projectile.GetAnySprite().Collection.GetSpriteIdByName("coin_projectile_001"));
            ETGMod.Databases.Items.Add(gun, null, "ANY");
        }

        public static void MakeOffset(tk2dSpriteDefinition def, Vector3 offset, bool changesCollider = false)
        {
            float xOffset = offset.x;
            float yOffset = offset.y;
            def.position0 += new Vector3(xOffset, yOffset, 0);
            def.position1 += new Vector3(xOffset, yOffset, 0);
            def.position2 += new Vector3(xOffset, yOffset, 0);
            def.position3 += new Vector3(xOffset, yOffset, 0);
            def.boundsDataCenter += new Vector3(xOffset, yOffset, 0);
            def.boundsDataExtents += new Vector3(xOffset, yOffset, 0);
            def.untrimmedBoundsDataCenter += new Vector3(xOffset, yOffset, 0);
            def.untrimmedBoundsDataExtents += new Vector3(xOffset, yOffset, 0);
            if (def.colliderVertices != null && def.colliderVertices.Length > 0 && changesCollider)
            {
                def.colliderVertices[0] += new Vector3(xOffset, yOffset, 0);
            }
        }

        public override void OnInitializedWithOwner(GameActor actor)
        {
            base.OnInitializedWithOwner(actor);
            if(actor is PlayerController play)
            {
                owner = play;
                owner.OnEnteredCombat += ResetDamageMult;
            }
        }

        public override void PostProcessProjectile(Projectile projectile)
        {
            base.PostProcessProjectile(projectile);
            projectile.baseData.damage *= damageMult;
        }

        public void ResetDamageMult()
        {
            damageMult = 1f;
        }

        public override void OnDropped()
        {
            damageMult = 1f;
            base.OnDropped();
            if(owner != null)
            {
                owner.OnEnteredCombat -= ResetDamageMult;
                owner = null;
            }
        }

        [HarmonyPatch(typeof(CurrencyPickup), nameof(CurrencyPickup.Pickup))]
        [HarmonyPostfix]
        public static void DamageUp(CurrencyPickup __instance, PlayerController player)
        {
            if(!__instance.IsMetaCurrency && player != null && (__instance.GetComponent<PickupMover>() == null || !__instance.GetComponent<PickupMover>().m_shouldPath))
            {
                if(player.inventory != null && player.inventory.AllGuns != null)
                {
                    foreach (var gun in player.inventory.AllGuns)
                    {
                        if (gun != null && gun.GetComponent<BankerGun>() != null)
                        {
                            gun.GetComponent<BankerGun>().damageMult += gun.GetComponent<BankerGun>().damageMultPerCasing * __instance.currencyValue;
                        }
                    }
                }
                if(player.passiveItems != null)
                {
                    foreach (var passive in player.passiveItems)
                    {
                        if (passive != null && passive is InterestRounds interest)
                        {
                            interest.damageMult += interest.damageMultPerCasing * __instance.currencyValue;
                        }
                    }
                }
            }
        }

        public float damageMultPerCasing;
        private PlayerController owner;
        private float damageMult = 1f;
    }
}
