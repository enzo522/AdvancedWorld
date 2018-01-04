using GTA;
using GTA.Math;
using GTA.Native;

namespace AdvancedWorld
{
    public class Massacre : EntitySet
    {
        public Massacre() : base() { }

        public bool IsCreatedIn(float radius, Vector3 safePosition)
        {
            spawnedPed = Util.Create("hc_gunman", safePosition);

            if (!Util.ThereIs(spawnedPed)) return false;

            spawnedPed.Weapons.Give(WeaponHash.Minigun, 1000, true, true);
            spawnedPed.Weapons.Current.InfiniteAmmo = true;
            spawnedPed.Armor = 100;

            Function.Call(Hash.RESET_PED_MOVEMENT_CLIPSET, spawnedPed, 1.0f);
            Function.Call(Hash.RESET_PED_STRAFE_CLIPSET, spawnedPed);
            Function.Call(Hash.SET_PED_USING_ACTION_MODE, spawnedPed, true, -1, 0);

            Function.Call(Hash.SET_PED_MOVEMENT_CLIPSET, spawnedPed, "ANIM_GROUP_MOVE_BALLISTIC", 1.0f);
            Function.Call(Hash.SET_PED_STRAFE_CLIPSET, spawnedPed, "MOVE_STRAFE_BALLISTIC");

            Function.Call(Hash.SET_PED_COMPONENT_VARIATION, spawnedPed, 0, 6, 0, 0);
            Function.Call(Hash.SET_PED_COMPONENT_VARIATION, spawnedPed, 1, 1, 0, 0);
            Function.Call(Hash.SET_PED_COMPONENT_VARIATION, spawnedPed, 2, 1, 0, 0);
            Function.Call(Hash.SET_PED_COMPONENT_VARIATION, spawnedPed, 3, 1, 0, 0);
            Function.Call(Hash.SET_PED_COMPONENT_VARIATION, spawnedPed, 4, 1, 0, 0);
            Function.Call(Hash.SET_PED_COMPONENT_VARIATION, spawnedPed, 6, 1, 0, 0);
            Function.Call(Hash.SET_PED_COMPONENT_VARIATION, spawnedPed, 8, 2, 1, 0);
            Function.Call(Hash.SET_PED_COMPONENT_VARIATION, spawnedPed, 9, 1, 0, 0);
            Function.Call(Hash.SET_PED_COMPONENT_VARIATION, spawnedPed, 10, 9, 0, 0);
            Function.Call(Hash.SET_PED_PROP_INDEX, spawnedPed, 0, 5, 0, false);

            Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, spawnedPed, 46, true);
            Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, spawnedPed, 5, true);

            spawnedPed.RelationshipGroup = AdvancedWorld.cougarID;
            spawnedPed.AlwaysKeepTask = true;
            spawnedPed.Task.FightAgainstHatedTargets(400.0f);
            spawnedPed.FiringPattern = FiringPattern.FullAuto;

            if (!Util.BlipIsOn(spawnedPed))
            {
                Util.AddBlipOn(spawnedPed, 0.7f, BlipSprite.Rampage, BlipColor.White, "Massacre Squad");
                return true;
            }
            else
            {
                spawnedPed.MarkAsNoLongerNeeded();
                return false;
            }
        }

        public override bool ShouldBeRemoved()
        {
            if (!Util.ThereIs(spawnedPed)) return true;
            if (spawnedPed.IsDead && Util.BlipIsOn(spawnedPed)) spawnedPed.CurrentBlip.Remove();
            if (!spawnedPed.IsInRangeOf(Game.Player.Character.Position, 500.0f))
            {
                if (spawnedPed.IsPersistent) spawnedPed.MarkAsNoLongerNeeded();

                return true;
            }

            return false;
        }
    }
}
