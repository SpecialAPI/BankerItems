using Alexandria.ItemAPI;
using Alexandria.SoundAPI;
using BepInEx;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BankerItems
{
    [BepInPlugin(GUID, NAME, VERSION)]
    [BepInDependency(ETGModMainBehaviour.GUID)]
    [BepInDependency(Alexandria.Alexandria.GUID)]
    public class Plugin : BaseUnityPlugin
    {
        public const string GUID = "spapi.etg.bankeritems";
        public const string NAME = "Banker Items";
        public const string VERSION = "1.2.4";
        public static GungeonFlags interestRoundsUnlocked;

        public void Awake()
        {
            ETGModMainBehaviour.WaitForGameManagerStart(GMStart);
        }

        public void GMStart(GameManager man)
        {
            interestRoundsUnlocked = ETGModCompatibility.ExtendEnum<GungeonFlags>(GUID, "InterestRoundsUnlocked");

            new Harmony(GUID).PatchAll();

            BankerItem.Init();
            BankerGun.Init();
            InterestRounds.Init();

            EnemyDatabase.GetOrLoadByGuid("465da2bb086a4a88a803f79fe3a27677").gameObject.AddComponent<InterestRoundsUnlock>(); // dragun
            EnemyDatabase.GetOrLoadByGuid("05b8afe0b6cc4fffa9dc6036fa24c8ec").gameObject.AddComponent<InterestRoundsUnlock>(); // advanced dragun

            monocle = new GameObject("monocle vfx");
            FakePrefab.MarkAsFakePrefab(monocle);
            monocle.SetActive(false);
            DontDestroyOnLoad(monocle);
            var helmetShader = "tk2d/CutoutVertexColorTilted";
            var ids = new List<string>
            {
                "001",
                "002",
                "003",
                "004",
                "005",
                "006",
                "007",
                "008",
                "009",
                "010",
                "011",
                "012",
                "011",
                "blank",
                "011",
                "blank",
                "011",
                "blank",
            }.Select(x => SpriteBuilder.AddSpriteToCollection($"BankerItems/Resources/Monocle/banker_monocle_{x}", SpriteBuilder.itemCollection, helmetShader)).ToList();
            tk2dSprite.AddComponent(monocle, SpriteBuilder.itemCollection, ids[0]);
            var anim = monocle.AddComponent<tk2dSpriteAnimator>();
            anim.Library = monocle.AddComponent<tk2dSpriteAnimation>();
            anim.Library.clips = new tk2dSpriteAnimationClip[]
            {
                new tk2dSpriteAnimationClip()
                {
                    frames = ids.Select(x => new tk2dSpriteAnimationFrame()
                    {
                        spriteId = x,
                        spriteCollection = SpriteBuilder.itemCollection
                    }).ToArray(),
                    fps = 15,
                    loopStart = 0,
                    maxFidgetDuration = 0f,
                    minFidgetDuration = 0f,
                    name = "monocle",
                    wrapMode = tk2dSpriteAnimationClip.WrapMode.Once
                }
            };
            anim.playAutomatically = true;
            anim.DefaultClipId = 0;
            var deb = monocle.AddComponent<DebrisObject>();
            deb.ACCURATE_DEBRIS_THRESHOLD = 0.25f;
            deb.accurateDebris = false;
            deb.additionalBounceEnglish = 0f;
            deb.additionalHeightBoost = 0f;
            deb.angularVelocity = 0f;
            deb.angularVelocityVariance = 20f;
            deb.animatePitFall = false;
            deb.AssignedGoop = null;
            deb.audioEventName = "";
            deb.bounceCount = 1;
            deb.breakOnFallChance = 1f;
            deb.breaksOnFall = true;
            deb.canRotate = true;
            deb.changesCollisionLayer = false;
            deb.collisionStopsBullets = false;
            deb.decayOnBounce = 0.5f;
            deb.detachedParticleSystems = new List<ParticleSystem>();
            deb.directionalAnimationData = new DebrisDirectionalAnimationInfo()
            {
                fallDown = "",
                fallLeft = "",
                fallRight = "",
                fallUp = ""
            };
            deb.doesDecay = false;
            deb.DoesGoopOnRest = false;
            deb.followupBehavior = DebrisObject.DebrisFollowupAction.None;
            deb.followupIdentifier = "";
            deb.ForceUpdateIfDisabled = false;
            deb.GoopRadius = 1f;
            deb.groundedCollisionLayer = CollisionLayer.LowObstacle;
            deb.groupManager = null;
            deb.inertialMass = 1f;
            deb.IsCorpse = false;
            deb.isFalling = false;
            deb.isPitFalling = false;
            deb.isStatic = true;
            deb.killTranslationOnBounce = false;
            deb.lifespanMax = 1f;
            deb.lifespanMin = 1f;
            deb.motionMultiplier = 1f;
            deb.onGround = false;
            deb.pitFallSplash = false;
            deb.placementOptions = new DebrisObject.DebrisPlacementOptions()
            {
                canBeFlippedHorizontally = false,
                canBeFlippedVertically = false,
                canBeRotated = false
            };
            deb.playAnimationOnTrigger = false;
            deb.PreventAbsorption = false;
            deb.PreventFallingInPits = false;
            deb.removeSRBOnGrounded = false;
            deb.shadowSprite = null;
            deb.shouldUseSRBMotion = false;
            deb.usesDirectionalFallAnimations = false;
            deb.usesLifespan = false;
            deb.Priority = EphemeralObject.EphemeralPriority.Middling;

            StartCoroutine(DelayedBankerEdit());
        }

        public static GameObject banker;
        public static GameObject monocle;

        public IEnumerator DelayedBankerEdit()
        {
            yield return null;
            var bank = (banker = (GameObject)BraveResources.Load("PlayerBanker", ".prefab")).GetComponent<PlayerController>();
            bank.lostAllArmorVFX = monocle;
            yield break;
        }
    }
}
