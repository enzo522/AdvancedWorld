﻿using GTA;
using GTA.Math;
using GTA.Native;
using System.Collections.Generic;

namespace AdvancedWorld
{
    public abstract class Emergency : EntitySet
    {
        protected List<Ped> members;
        protected string name;
        protected Entity target;
        protected string emergencyType;
        protected bool onVehicleDuty;
        protected int relationship;

        public Emergency(string name, Entity target, string emergencyType) : base()
        {
            this.members = new List<Ped>();
            this.name = name;
            this.target = target;
            this.emergencyType = emergencyType;
            this.onVehicleDuty = true;

            if (this.emergencyType == "ARMY") this.relationship = Util.NewRelationshipOf(DispatchManager.DispatchType.Army);
            else this.relationship = Util.NewRelationshipOf(DispatchManager.DispatchType.Cop);
        }

        public abstract bool IsCreatedIn(Vector3 safePosition, List<string> models);
        public override void Restore(bool instantly)
        {
            if (instantly)
            {
                foreach (Ped p in members)
                {
                    if (Util.ThereIs(p)) p.Delete();
                }

                if (Util.ThereIs(spawnedVehicle)) spawnedVehicle.Delete();
            }
            else
            {
                foreach (Ped p in members)
                {
                    if (Util.ThereIs(p))
                    {
                        p.AlwaysKeepTask = false;
                        p.BlockPermanentEvents = false;
                        Function.Call(Hash.SET_PED_AS_COP, p, true);
                        Util.NaturallyRemove(p);
                    }
                }

                if (Util.ThereIs(spawnedVehicle))
                {
                    if (spawnedVehicle.HasSiren && spawnedVehicle.SirenActive) spawnedVehicle.SirenActive = false;

                    Util.NaturallyRemove(spawnedVehicle);
                }
            }
            
            if (relationship != 0)
            {
                if (emergencyType == "ARMY") Util.CleanUp(relationship, DispatchManager.DispatchType.Army);
                else Util.CleanUp(relationship, DispatchManager.DispatchType.Cop);
            }

            members.Clear();
        }

        protected void SetPedsOnDuty()
        {
            if (spawnedVehicle.HasSiren && !spawnedVehicle.SirenActive) spawnedVehicle.SirenActive = true;
            if (onVehicleDuty)
            {
                if (ReadyToGoWith(members))
                {
                    if (Util.ThereIs(spawnedVehicle.Driver))
                    {
                        foreach (Ped p in members)
                        {
                            if (p.Equals(spawnedVehicle.Driver))
                            {
                                if (target.Model.IsPed && ((Ped)target).IsInVehicle()) p.Task.VehicleChase((Ped)target);
                                else p.Task.DriveTo(spawnedVehicle, target.Position, 30.0f, 100.0f, (int)DrivingStyle.AvoidTrafficExtremely);
                            }
                            else if (!p.IsInCombat) p.Task.FightAgainstHatedTargets(400.0f);
                        }
                    }
                    else
                    {
                        foreach (Ped p in members) p.Task.LeaveVehicle(spawnedVehicle, false);
                    }
                }
                else
                {
                    if (!VehicleSeatsCanBeSeatedBy(members))
                    {
                        Restore(false);
                        return;
                    }
                }
            }
            else
            {
                if (Util.ThereIs(spawnedVehicle.Driver))
                {
                    TaskSequence ts = new TaskSequence();
                    Function.Call(Hash.TASK_VEHICLE_TEMP_ACTION, 0, spawnedVehicle, 1, 1000);
                    ts.AddTask.LeaveVehicle(spawnedVehicle, false);
                    ts.AddTask.FightAgainstHatedTargets(400.0f);
                    ts.Close();

                    spawnedVehicle.Driver.Task.PerformSequence(ts);
                    ts.Dispose();
                }
                else
                {
                    foreach (Ped p in members)
                    {
                        if (!p.IsInCombat) p.Task.FightAgainstHatedTargets(400.0f);
                    }
                }
            }
        }

        protected void SetPedsOffDuty()
        {
            if (ReadyToGoWith(members))
            {
                if (Util.ThereIs(spawnedVehicle.Driver))
                {
                    foreach (Ped p in members)
                    {
                        if (Util.ThereIs(p) && p.IsPersistent)
                        {
                            if (spawnedVehicle.HasSiren && spawnedVehicle.SirenActive) spawnedVehicle.SirenActive = false;
                            if (p.Equals(spawnedVehicle.Driver) && !Function.Call<bool>(Hash.GET_IS_TASK_ACTIVE, p, 151)) p.Task.CruiseWithVehicle(spawnedVehicle, 20.0f, (int)DrivingStyle.Normal);

                            p.AlwaysKeepTask = false;
                            p.BlockPermanentEvents = false;
                            Function.Call(Hash.SET_PED_AS_COP, p, true);
                            Util.NaturallyRemove(p);
                        }
                    }

                    if (spawnedVehicle.IsPersistent) Util.NaturallyRemove(spawnedVehicle);
                }
                else
                {
                    foreach (Ped p in members) p.Task.LeaveVehicle(spawnedVehicle, false);
                }
            }
            else
            {
                if (!VehicleSeatsCanBeSeatedBy(members))
                {
                    Restore(false);
                    return;
                }
            }
        }

        protected bool TargetIsFound()
        {
            target = null;
            Ped[] nearbyPeds = World.GetNearbyPeds(spawnedVehicle.Position, 300.0f);

            if (nearbyPeds.Length < 1) return false;

            foreach (Ped p in nearbyPeds)
            {
                if (Util.ThereIs(p) && !p.IsDead && World.GetRelationshipBetweenGroups(relationship, p.RelationshipGroup).Equals(Relationship.Hate))
                {
                    target = p;
                    return true;
                }
            }

            return false;
        }

        protected void AddVarietyTo(Ped p)
        {
            if (Function.Call<int>(Hash.GET_NUMBER_OF_PED_PROP_TEXTURE_VARIATIONS, p, 0, 0) > 0
                && Util.GetRandomIntBelow(2) == 1)
                Function.Call(Hash.SET_PED_PROP_INDEX, p, 0, Util.GetRandomIntBelow(Function.Call<int>(Hash.GET_NUMBER_OF_PED_PROP_TEXTURE_VARIATIONS, p, 0, 0)), 0, false);

            if (Function.Call<int>(Hash.GET_NUMBER_OF_PED_PROP_TEXTURE_VARIATIONS, p, 1, 0) > 0
                && Util.GetRandomIntBelow(2) == 1)
                Function.Call(Hash.SET_PED_PROP_INDEX, p, 1, Util.GetRandomIntBelow(Function.Call<int>(Hash.GET_NUMBER_OF_PED_PROP_TEXTURE_VARIATIONS, p, 1, 0)), 0, false);
        }

        public override bool ShouldBeRemoved()
        {
            for (int i = members.Count - 1; i >= 0; i--)
            {
                if (!Util.ThereIs(members[i]))
                {
                    members.RemoveAt(i);
                    continue;
                }

                if (members[i].IsDead)
                {
                    Util.NaturallyRemove(members[i]);
                    members.RemoveAt(i);
                }
            }

            if (!Util.ThereIs(spawnedVehicle) || members.Count < 1 || !spawnedVehicle.IsInRangeOf(Game.Player.Character.Position, 500.0f))
            {
                Restore(false);
                return true;
            }

            if (!TargetIsFound()) SetPedsOffDuty();
            else
            {
                if (!Util.WeCanEnter(spawnedVehicle) || (spawnedVehicle.IsInRangeOf(target.Position, 30.0f) && target.Model.IsPed && (!((Ped)target).IsInVehicle() || ((Ped)target).CurrentVehicle.Speed < 10.0f)))
                    onVehicleDuty = false;
                else onVehicleDuty = true;

                SetPedsOnDuty();
            }

            return false;
        }
    }
}