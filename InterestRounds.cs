using Alexandria.ItemAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BankerItems
{
    public class InterestRounds : PassiveItem
    {
        public static void Init()
        {
            string itemName = "Interest Bullets";
            string resourceName = "BankerItems/Resources/interest_rounds_idle_001";
            GameObject obj = new GameObject(itemName);
            var item = obj.AddComponent<InterestRounds>();
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);
            string shortDesc = "Loan and Gun";
            string longDesc = "A type of ammunition appreciated by usurers and economists alike, the Interest Bullets are the go-to tool for slowing down economies, diminishing inflation, and putting your enemies beneath the soil. " +
                "That is, of course, as long as you have the money to make use of the high interest rates.\n\nCollect casings before clearing a room to increase the stats of all your guns. Stats reset when entering a new room.";
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "spapi");
            item.quality = ItemQuality.B;
            item.damageMultPerCasing = 0.1f;
            item.SetupUnlockOnFlag(Plugin.interestRoundsUnlocked, true);
        }

        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
            player.PostProcessProjectile += DamageUp;
            player.OnEnteredCombat += ResetDamageMult;
        }

        public void ResetDamageMult()
        {
            damageMult = 1f;
        }

        public void DamageUp(Projectile proj, float f)
        {
            proj.baseData.damage *= damageMult;
        }

        public override void DisableEffect(PlayerController player)
        {
            player.PostProcessProjectile -= DamageUp;
            player.OnEnteredCombat -= ResetDamageMult;
            damageMult = 1f;
            base.DisableEffect(player);
        }

        public float damageMultPerCasing;
        [NonSerialized]
        public float damageMult = 1f;
    }
}
