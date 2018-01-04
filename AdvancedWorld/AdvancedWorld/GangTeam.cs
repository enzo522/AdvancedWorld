using GTA;
using GTA.Math;
using GTA.Native;
using System.Collections.Generic;

namespace AdvancedWorld
{
    public class GangTeam : EntitySet
    {
        private List<Ped> members;

        public GangTeam() : base()
        {
            members = new List<Ped>();
        }

        public bool IsCreatedIn(float radius, Vector3 position, List<string> selectedModels, int teamID, BlipColor teamColor, string teamName)
        {
            if (selectedModels == null) return false;

            for (int i = 0; i < 6; i++)
            {
                Ped p = Util.Create(selectedModels[Util.GetRandomInt(selectedModels.Count)], position);

                if (!Util.ThereIs(p)) continue;

                List<WeaponHash> weaponList = new List<WeaponHash> { WeaponHash.MachinePistol, WeaponHash.Bat, WeaponHash.SawnOffShotgun, WeaponHash.Hatchet, WeaponHash.Hammer, WeaponHash.Knife, WeaponHash.KnuckleDuster, WeaponHash.Machete, WeaponHash.Pistol, WeaponHash.APPistol, WeaponHash.PumpShotgun, WeaponHash.Unarmed };
                p.Weapons.Give(weaponList[Util.GetRandomInt(weaponList.Count)], 1000, false, true);
                p.Armor = Util.GetRandomInt(100);

                Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, p, 46, true);
                Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, p, 5, true);

                p.RelationshipGroup = teamID;
                p.AlwaysKeepTask = true;

                if (!Util.BlipIsOn(p))
                {
                    Util.AddBlipOn(p, 0.7f, BlipSprite.Rampage, teamColor, teamName);
                    members.Add(p);
                }
                else p.Delete();
            }

            foreach (Ped p in members)
            {
                if (!Util.ThereIs(p))
                {
                    Restore();
                    return false;
                }
            }
            
            return true;
        }

        private void Restore()
        {
            foreach (Ped p in members)
            {
                if (Util.ThereIs(p)) p.Delete();
            }

            members.Clear();
        }

        public void PerformTask()
        {
            TaskSequence ts = new TaskSequence();
            ts.AddTask.FightAgainstHatedTargets(400.0f);
            ts.AddTask.WanderAround();
            ts.Close();

            foreach (Ped p in members) p.Task.PerformSequence(ts);

            ts.Dispose();
        }

        public override bool ShouldBeRemoved()
        {
            for (int i = members.Count - 1; i >= 0; i--)
            {
                if (!Util.ThereIs(members[i]))
                {
                    members.RemoveAt(i);
                    break;
                }

                if (members[i].IsDead && Util.BlipIsOn(members[i])) members[i].CurrentBlip.Remove();
                if (!members[i].IsInRangeOf(Game.Player.Character.Position, 500.0f))
                {
                    if (members[i].IsPersistent) members[i].MarkAsNoLongerNeeded();

                    members.RemoveAt(i);
                    break;
                }
            }

            if (members.Count == 0) return true;
            else return false;
        }
    }
}
