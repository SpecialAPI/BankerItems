using Alexandria.ItemAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using HarmonyLib;

namespace BankerItems
{
    [HarmonyPatch]
    public class BankerItem : PassiveItem
    {
        public static void Init()
        {
            string itemName = "Golden Heart";
            string resourceName = "BankerItems/Resources/heart_of_gold_002";
            GameObject obj = new GameObject(itemName);
            var item = obj.AddComponent<BankerItem>();
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);
            string shortDesc = "A bit too literal";
            string longDesc = "A heart made of pure gold, animated by some unseen force. Few know of its actual origin, yet many are the myths and legends written after it. Some believe it to be the result of a supernatural curse, " +
                "replacing the live hearts of those who accumulate too much wealth; some say it is the result of a cruel operation by the Hegemony of Man's banking conglomerates, replacing the hearts of their most generous bankers so " +
                "that they become physically unable to do unprofitable deeds of kindness during their work.\n\nOne thing, however, is certain: to all those whose chests beat with these hearts, money truly becomes their life.";
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "spapi");
            item.quality = ItemQuality.SPECIAL;
            item.CanBeDropped = false;
        }

        public override void Pickup(PlayerController player)
        {
            if(mod == null)
            {
                mod = StatModifier.Create(PlayerStats.StatType.Health, StatModifier.ModifyMethod.ADDITIVE, 0f);
            }
            mod.statToBoost = PlayerStats.StatType.Health;
            player.ownerlessStatModifiers.Add(mod);
            base.Pickup(player);
            player.OnReceivedDamage += LoseMoney;
            player.LostArmor += DoIgnoreNextShot;
            player.healthHaver.ModifyHealing += CoinsForHealth;
            player.AllowZeroHealthState = true;
            IncrementFlag(player, typeof(BankerItem));
        }

        public void CoinsForHealth(HealthHaver hh, HealthHaver.ModifyHealingEventArgs args)
        {
            if(args == EventArgs.Empty)
            {
                return;
            }
            Owner.carriedConsumables.Currency += Mathf.FloorToInt(args.InitialHealing * MONEY_PER_HALF_HEART * 2);
        }

        public void DoIgnoreNextShot()
        {
            ignoreNextShot = true;
        }

        public void LoseMoney(PlayerController player)
        {
            if (ignoreNextShot)
            {
                ignoreNextShot = false;
                return;
            }
            var previousMoney = player.carriedConsumables.Currency;
            player.carriedConsumables.Currency = Mathf.Max(0, previousMoney - MONEY_PER_HALF_HEART);
            var moneyLost = previousMoney - player.carriedConsumables.Currency;
            if(moneyLost * MONEY_DROPPED_ON_HIT >= 1)
            {
                var moneyToDrop = Mathf.FloorToInt(moneyLost * MONEY_DROPPED_ON_HIT);
                LootEngine.SpawnCurrencyManual(player.CenterPosition, moneyToDrop);
            }
            if(Owner.carriedConsumables.Currency > 0)
            {
                Owner.healthHaver.currentHealth = Owner.healthHaver.AdjustedMaxHealth;
            }
            lastCoins = Owner.carriedConsumables.Currency;
        }

        public void LateUpdate()
        {
            ignoreNextShot = false;
            if (PickedUp && Owner != null && Owner.carriedConsumables != null)
            {
                if (!gaveMoney)
                {
                    Owner.carriedConsumables.Currency += MONEY_GIVEN_ON_START;
                    gaveMoney = true;
                }
                if (lastCoins != Owner.carriedConsumables.Currency || Owner.carriedConsumables.Currency > 0)
                {
                    Owner.healthHaver.currentHealth = Owner.healthHaver.AdjustedMaxHealth;
                }
                if (lastCoins != Owner.carriedConsumables.Currency && Owner.carriedConsumables.Currency <= 0)
                {
                    Owner.healthHaver.Armor = 0;
                    Owner.healthHaver.minimumHealth = 0f;
                    Owner.healthHaver.IsVulnerable = true;
                    Owner.healthHaver.ApplyDamage(0.5f, Vector2.zero, "Bankruptcy", CoreDamageTypes.None, DamageCategory.Normal, true, null, true);
                }
                lastCoins = Owner.carriedConsumables.Currency;
            }
        }

        public override void DisableEffect(PlayerController player)
        {
            DecrementFlag(player, typeof(BankerItem));
            player.ownerlessStatModifiers.Remove(mod);
            player.OnReceivedDamage -= LoseMoney;
            player.LostArmor -= DoIgnoreNextShot;
            player.healthHaver.ModifyHealing -= CoinsForHealth;
            player.AllowZeroHealthState = false;
            base.DisableEffect(player);
        }

        [HarmonyPatch(typeof(HealthHaver), nameof(HealthHaver.FullHeal))]
        [HarmonyPostfix]
        public static void CoinsForFullHeal(HealthHaver __instance)
        {
            if(__instance.isPlayerCharacter && IsFlagSetForCharacter(__instance.m_player, typeof(BankerItem)))
            {
                __instance.m_player.carriedConsumables.Currency += MONEY_PER_HALF_HEART * 4;
            }
        }

        [HarmonyPatch(typeof(HealthHaver), nameof(HealthHaver.SetHealthMaximum))]
        [HarmonyPostfix]
        public static void CoinsForMaxHealth(HealthHaver __instance, float? amountOfHealthToGain)
        {
            if (amountOfHealthToGain != null && __instance.m_player && IsFlagSetForCharacter(__instance.m_player, typeof(BankerItem)))
            {
                __instance.m_player.carriedConsumables.Currency += Mathf.FloorToInt(amountOfHealthToGain.GetValueOrDefault() * MONEY_PER_HALF_HEART * 2);
            }
        }

        [HarmonyPatch(typeof(HealthHaver), nameof(HealthHaver.GetMaxHealth))]
        [HarmonyPrefix]
        public static bool MaxHealth(HealthHaver __instance, ref float __result)
        {
            if(__instance && __instance.m_player && IsFlagSetForCharacter(__instance.m_player, typeof(BankerItem)))
            {
                __result = Mathf.Max(Mathf.Floor(__instance.m_player.carriedConsumables.Currency / MONEY_PER_HALF_HEART) / 2f, __instance.m_player.carriedConsumables.m_currency > MONEY_PER_HALF_HEART ? 1f : 0.5f);

                return false;
            }

            return true;
        }

        public const int MONEY_PER_HALF_HEART = 40;
        public const float MONEY_DROPPED_ON_HIT = 0.3f;
        public const int MONEY_GIVEN_ON_START = 120;
        public StatModifier mod;
        private bool ignoreNextShot;
        private int lastCoins = -1;
        public bool gaveMoney;
    }
}
