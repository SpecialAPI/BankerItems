using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BankerItems
{
    public static class UnlockTools
    {
        /// <summary>
        /// Setups a <see cref="DungeonPrerequisite"/> with the type of <see cref="DungeonPrerequisite.PrerequisiteType.FLAG"/>, flag of <paramref name="flag"/> and requiredFlagValue of 
        /// <paramref name="requiredFlagValue"/> and adds it to <paramref name="self"/>'s list of <see cref="DungeonPrerequisite"/>s
        /// </summary>
        /// <param name="self">The <see cref="PickupObject"/> to add the <see cref="DungeonPrerequisite"/> to</param>
        /// <param name="flag">The <see cref="GungeonFlags"/> to get the value from</param>
        /// <param name="requiredFlagValue">Value to compare <paramref name="flag"/>'s value to</param>
        /// <returns>The <see cref="DungeonPrerequisite"/> that was added to the list of <see cref="DungeonPrerequisite"/>s</returns>
        public static DungeonPrerequisite SetupUnlockOnFlag(this PickupObject self, GungeonFlags flag, bool requiredFlagValue)
        {
            if (self.encounterTrackable == null)
            {
                return null;
            }
            return self.encounterTrackable.SetupUnlockOnFlag(flag, requiredFlagValue);
        }

        /// <summary>
        /// Setups a <see cref="DungeonPrerequisite"/> with the type of <see cref="DungeonPrerequisite.PrerequisiteType.FLAG"/>, flag of <paramref name="flag"/> and requiredFlagValue of 
        /// <paramref name="requiredFlagValue"/> and adds it to <paramref name="self"/>'s list of <see cref="DungeonPrerequisite"/>s
        /// </summary>
        /// <param name="self">The <see cref="EncounterTrackable"/> to add the <see cref="DungeonPrerequisite"/> to</param>
        /// <param name="flag">The <see cref="GungeonFlags"/> to get the value from</param>
        /// <param name="requiredFlagValue">Value to compare <paramref name="flag"/>'s value to</param>
        /// <returns>The <see cref="DungeonPrerequisite"/> that was added to the list of <see cref="DungeonPrerequisite"/>s</returns>
        public static DungeonPrerequisite SetupUnlockOnFlag(this EncounterTrackable self, GungeonFlags flag, bool requiredFlagValue)
        {
            return self.AddPrerequisite(new DungeonPrerequisite
            {
                prerequisiteType = DungeonPrerequisite.PrerequisiteType.FLAG,
                saveFlagToCheck = flag,
                requireFlag = requiredFlagValue
            });
        }

        /// <summary>
        /// Adds <paramref name="prereq"/> to <paramref name="self"/>'s list of <see cref="DungeonPrerequisite"/>s
        /// </summary>
        /// <param name="self">The <see cref="PickupObject"/> to add <paramref name="prereq"/> to</param>
        /// <param name="prereq"><see cref="DungeonPrerequisite"/> to add</param>
        /// <returns><paramref name="prereq"/></returns>
        public static DungeonPrerequisite AddPrerequisite(this PickupObject self, DungeonPrerequisite prereq)
        {
            return self.encounterTrackable.AddPrerequisite(prereq);
        }

        /// <summary>
        /// Adds <paramref name="prereq"/> to <paramref name="self"/>'s list of <see cref="DungeonPrerequisite"/>s
        /// </summary>
        /// <param name="self">The <see cref="EncounterTrackable"/> to add <paramref name="prereq"/> to</param>
        /// <param name="prereq"><see cref="DungeonPrerequisite"/> to add</param>
        /// <returns><paramref name="prereq"/></returns>
        public static DungeonPrerequisite AddPrerequisite(this EncounterTrackable self, DungeonPrerequisite prereq)
        {
            if (!string.IsNullOrEmpty(self.ProxyEncounterGuid))
            {
                self.ProxyEncounterGuid = "";
            }
            if (self.prerequisites == null)
            {
                self.prerequisites = new DungeonPrerequisite[] { prereq };
            }
            else
            {
                DungeonPrerequisite[] prereqs = self.prerequisites;
                prereqs = prereqs.AddToArray(prereq);
                self.prerequisites = prereqs;
            }
            EncounterDatabaseEntry databaseEntry = EncounterDatabase.GetEntry(self.EncounterGuid);
            if (!string.IsNullOrEmpty(databaseEntry.ProxyEncounterGuid))
            {
                databaseEntry.ProxyEncounterGuid = "";
            }
            if (databaseEntry.prerequisites == null)
            {
                databaseEntry.prerequisites = new DungeonPrerequisite[] { prereq };
            }
            else
            {
                DungeonPrerequisite[] prereqs = databaseEntry.prerequisites;
                prereqs = prereqs.AddToArray(prereq);
                databaseEntry.prerequisites = prereqs;
            }
            return prereq;
        }
    }
}
