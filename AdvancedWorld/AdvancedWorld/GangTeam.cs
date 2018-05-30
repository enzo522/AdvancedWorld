﻿using GTA;
using GTA.Math;
using GTA.Native;
using System.Collections.Generic;

namespace AdvancedWorld
{
    public class GangTeam : Criminal
    {
        private List<Ped> members;
        private List<WeaponHash> closeWeapons;
        private List<WeaponHash> standoffWeapons;
        private TaskSequence ts;

        public GangTeam() : base(EventManager.EventType.GangTeam)
        {
            this.members = new List<Ped>();
            this.closeWeapons = new List<WeaponHash> { WeaponHash.Bat, WeaponHash.Hatchet, WeaponHash.Hammer, WeaponHash.Knife, WeaponHash.KnuckleDuster, WeaponHash.Machete, WeaponHash.Wrench, WeaponHash.BattleAxe, WeaponHash.Unarmed };
            this.standoffWeapons = new List<WeaponHash> { WeaponHash.MachinePistol, WeaponHash.SawnOffShotgun, WeaponHash.Pistol, WeaponHash.APPistol, WeaponHash.PumpShotgun, WeaponHash.Revolver };
            Util.CleanUp(this.relationship);

            ts = new TaskSequence();
            ts.AddTask.FightAgainstHatedTargets(200.0f);
            ts.AddTask.WanderAround();
            ts.Close();
        }

        public bool IsCreatedIn(float radius, Vector3 safePosition, List<string> selectedModels, int teamID, BlipColor teamColor, string teamName)
        {
            Vector3 position = World.GetNextPositionOnSidewalk(safePosition);

            if (position.Equals(Vector3.Zero) || selectedModels == null) return false;

            this.relationship = teamID;

            for (int i = 0; i < 6; i++)
            {
                Ped p = Util.Create(selectedModels[Util.GetRandomIntBelow(selectedModels.Count)], position);

                if (!Util.ThereIs(p)) continue;

                if (Util.GetRandomIntBelow(3) == 0) p.Weapons.Give(closeWeapons[Util.GetRandomIntBelow(closeWeapons.Count)], 1, true, true);
                else p.Weapons.Give(standoffWeapons[Util.GetRandomIntBelow(standoffWeapons.Count)], 300, true, true);

                Function.Call(Hash.SET_PED_FLEE_ATTRIBUTES, spawnedPed, 0, false);
                Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, p, 17, true);
                Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, p, 46, true);
                Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, p, 5, true);

                p.RelationshipGroup = relationship;
                p.IsPriorityTargetForEnemies = true;
                p.AlwaysKeepTask = true;
                p.BlockPermanentEvents = true;
                p.Armor = Util.GetRandomIntBelow(100);

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
                    Restore(true);
                    return false;
                }
            }

            return true;
        }

        public override void Restore(bool instantly)
        {
            if (instantly)
            {
                foreach (Ped p in members)
                {
                    if (Util.ThereIs(p)) p.Delete();
                }
            }
            else
            {
                foreach (Ped p in members) Util.NaturallyRemove(p);
            }

            if (relationship != 0) Util.CleanUp(relationship);

            members.Clear();
        }

        public void PerformTask()
        {
            foreach (Ped p in members) p.Task.PerformSequence(ts);
        }

        public override bool ShouldBeRemoved()
        {
            spawnedPed = null;

            for (int i = members.Count - 1; i >= 0; i--)
            {
                if (!Util.ThereIs(members[i]))
                {
                    members.RemoveAt(i);
                    continue;
                }
                
                if (members[i].IsDead || !members[i].IsInRangeOf(Game.Player.Character.Position, 500.0f))
                {
                    Util.NaturallyRemove(members[i]);
                    members.RemoveAt(i);
                    continue;
                }

                spawnedPed = members[i];

                if (!members[i].IsInCombat && Util.AnyEmergencyIsNear(members[i].Position, DispatchManager.DispatchType.Cop) && Util.WeCanGiveTaskTo(members[i])) members[i].Task.PerformSequence(ts);
            }

            if (members.Count < 1)
            {
                if (relationship != 0) Util.CleanUp(relationship);

                ts.Dispose();
                return true;
            }

            if (Util.ThereIs(spawnedPed)) CheckDispatch();

            return false;
        }
    }
}