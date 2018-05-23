using GTA;
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

            if (this.emergencyType == "ARMY") this.relationship = Util.NewRelationship(ListManager.EventType.Army);
            else this.relationship = Util.NewRelationship(ListManager.EventType.Cop);
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
                    if (Util.ThereIs(p)) p.MarkAsNoLongerNeeded();
                }

                if (Util.ThereIs(spawnedVehicle)) spawnedVehicle.MarkAsNoLongerNeeded();
            }
            
            if (relationship != 0)
            {
                if (emergencyType == "ARMY") Util.CleanUpRelationship(relationship, ListManager.EventType.Army);
                else Util.CleanUpRelationship(relationship, ListManager.EventType.Cop);
            }

            members.Clear();
        }

        protected void SetPedsOnDuty()
        {
            if (onVehicleDuty)
            {
                if (EveryoneIsSitting())
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
                    int startingSeat = 0;

                    if (Util.ThereIs(spawnedVehicle.Driver)) Function.Call(Hash.TASK_VEHICLE_TEMP_ACTION, spawnedVehicle.Driver, spawnedVehicle, 1, 1000);
                    else startingSeat = -1;

                    for (int i = startingSeat, j = 0; j < members.Count; j++)
                    {
                        if (Util.ThereIs(members[j]) && !members[j].IsSittingInVehicle(spawnedVehicle))
                        {
                            if (Function.Call<bool>(Hash.GET_IS_TASK_ACTIVE, members[j], 160))
                            {
                                if (members[j].IsStopped && !members[j].IsGettingIntoAVehicle)
                                    members[j].SetIntoVehicle(spawnedVehicle, (VehicleSeat)Function.Call<int>(Hash.GET_SEAT_PED_IS_TRYING_TO_ENTER, members[j]));
                            }
                            else
                            {
                                while (!spawnedVehicle.IsSeatFree((VehicleSeat)i) || !spawnedVehicle.GetPedOnSeat((VehicleSeat)i).IsDead)
                                {
                                    if (++i >= spawnedVehicle.PassengerSeats)
                                    {
                                        Restore(false);
                                        return;
                                    }
                                }

                                members[j].Task.EnterVehicle(spawnedVehicle, (VehicleSeat)i++, -1, 2.0f, 1);
                            }
                        }
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
            if (Util.ThereIs(spawnedVehicle) && spawnedVehicle.HasSiren && spawnedVehicle.SirenActive) spawnedVehicle.SirenActive = false;
            foreach (Ped p in members)
            {
                if (Util.ThereIs(p))
                {
                    if (Util.ThereIs(spawnedVehicle) && p.Equals(spawnedVehicle.Driver)) p.Task.CruiseWithVehicle(spawnedVehicle, 20.0f, (int)DrivingStyle.Normal);

                    p.AlwaysKeepTask = false;
                    p.BlockPermanentEvents = false;

                    if (emergencyType == "ARMY") p.RelationshipGroup = Function.Call<int>(Hash.GET_HASH_KEY, emergencyType);
                    else p.RelationshipGroup = Function.Call<int>(Hash.GET_HASH_KEY, "COP");

                    Function.Call(Hash.SET_PED_AS_COP, p, true);
                    p.MarkAsNoLongerNeeded();
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

        protected bool EveryoneIsSitting()
        {
            foreach (Ped p in members)
            {
                if (!p.IsDead && !p.IsSittingInVehicle(spawnedVehicle)) return false;
            }

            return true;
        }

        protected void AddVarietyTo(Ped p)
        {
            if (Function.Call<int>(Hash.GET_NUMBER_OF_PED_PROP_TEXTURE_VARIATIONS, p, 0, 0) > 0
                && Util.GetRandomInt(2) == 1)
                Function.Call(Hash.SET_PED_PROP_INDEX, p, 0, Util.GetRandomInt(Function.Call<int>(Hash.GET_NUMBER_OF_PED_PROP_TEXTURE_VARIATIONS, p, 0, 0)), 0, false);

            if (Function.Call<int>(Hash.GET_NUMBER_OF_PED_PROP_TEXTURE_VARIATIONS, p, 1, 0) > 0
                && Util.GetRandomInt(2) == 1)
                Function.Call(Hash.SET_PED_PROP_INDEX, p, 1, Util.GetRandomInt(Function.Call<int>(Hash.GET_NUMBER_OF_PED_PROP_TEXTURE_VARIATIONS, p, 1, 0)), 0, false);
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
                    members[i].MarkAsNoLongerNeeded();
                    members.RemoveAt(i);
                }
            }

            if (!Util.ThereIs(spawnedVehicle) || !spawnedVehicle.IsDriveable || !TargetIsFound() || !spawnedVehicle.IsInRangeOf(target.Position, 300.0f) || members.Count < 1 || !spawnedVehicle.IsInRangeOf(Game.Player.Character.Position, 500.0f))
            {
                SetPedsOffDuty();
                Restore(false);
                return true;
            }

            if (spawnedVehicle.IsInRangeOf(target.Position, 30.0f) && target.Model.IsPed && (!((Ped)target).IsInVehicle() || ((Ped)target).CurrentVehicle.Speed < 10.0f))
                onVehicleDuty = false;
            else onVehicleDuty = true;

            SetPedsOnDuty();
            return false;
        }
    }
}