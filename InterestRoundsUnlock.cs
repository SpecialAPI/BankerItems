using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BankerItems
{
    public class InterestRoundsUnlock : BraveBehaviour
    {
        public void Start()
        {
            if(healthHaver != null)
            {
                healthHaver.OnDeath += Unlock;
            }
        }

        public void Unlock(Vector2 v)
        {
            if(GameManager.HasInstance && GameManager.Instance.PrimaryPlayer != null && GameManager.Instance.PrimaryPlayer.name.ToLowerInvariant().Replace("(clone)", "").Trim() == "playerbanker")
            {
                GameStatsManager.Instance.SetFlag(Plugin.interestRoundsUnlocked, true);
            }
        }
    }
}
