﻿using GTA;
using GTA.Math;
using GTA.Native;
using System.Collections.Generic;

namespace AdvancedWorld
{
    public abstract class EmergencyFire : Emergency
    {
        public EmergencyFire(string name, Entity target, string emergencyType) : base(name, target, emergencyType)
        {
            Util.CleanUpRelationship(this.relationship, ListManager.EventType.Cop);
            this.relationship = 0;
        }

        public override bool IsCreatedIn(Vector3 safePosition, List<string> models)
        {
            Road road = Util.GetNextPositionOnStreetWithHeading(safePosition);

            if (road.Position.Equals(Vector3.Zero)) return false;

            spawnedVehicle = Util.Create(name, road.Position, road.Heading, false);

            if (!Util.ThereIs(spawnedVehicle)) return false;
            
            int max = emergencyType == "FIREMAN" ? 3 : 1;
            
            for (int i = -1; i < spawnedVehicle.PassengerSeats && i < max; i++)
            {
                if (spawnedVehicle.IsSeatFree((VehicleSeat)i))
                {
                    members.Add(spawnedVehicle.CreatePedOnSeat((VehicleSeat)i, models[Util.GetRandomInt(models.Count)]));
                    Script.Wait(50);
                }
            }

            foreach (Ped p in members)
            {
                if (!Util.ThereIs(p))
                {
                    Restore(true);
                    return false;
                }

                p.RelationshipGroup = Function.Call<int>(Hash.GET_HASH_KEY, emergencyType);
                p.AlwaysKeepTask = true;
                p.BlockPermanentEvents = true;

                if (emergencyType == "FIREMAN")
                {
                    p.Weapons.Give(WeaponHash.FireExtinguisher, 100, true, true);
                    p.Weapons.Current.InfiniteAmmo = true;
                    p.CanSwitchWeapons = true;
                    p.IsFireProof = true;
                }
            }

            if (spawnedVehicle.HasSiren) spawnedVehicle.SirenActive = true;
            if (Util.ThereIs(spawnedVehicle.Driver))
            {
                Function.Call(Hash.SET_DRIVER_ABILITY, spawnedVehicle.Driver, 1.0f);
                Function.Call(Hash.SET_DRIVER_AGGRESSIVENESS, spawnedVehicle.Driver, 1.0f);
                spawnedVehicle.Driver.Task.DriveTo(spawnedVehicle, target.Position, 10.0f, 100.0f, (int)DrivingStyle.SometimesOvertakeTraffic);
            }

            return true;
        }

        protected new void SetPedsOffDuty()
        {
            if (Util.ThereIs(spawnedVehicle) && spawnedVehicle.HasSiren && spawnedVehicle.SirenActive) spawnedVehicle.SirenActive = false;
            if (EveryoneIsSitting())
            {
                if (Util.ThereIs(spawnedVehicle.Driver))
                {
                    foreach (Ped p in members)
                    {
                        if (p.IsPersistent)
                        {
                            if (p.Equals(spawnedVehicle.Driver)) p.Task.CruiseWithVehicle(spawnedVehicle, 20.0f, (int)DrivingStyle.Normal);
                            else p.Task.Wait(1000);

                            p.RelationshipGroup = Function.Call<int>(Hash.GET_HASH_KEY, "CIVMALE");
                            p.AlwaysKeepTask = false;
                            p.BlockPermanentEvents = false;
                            p.MarkAsNoLongerNeeded();
                        }
                    }
                }
                else
                {
                    foreach (Ped p in members) p.Task.LeaveVehicle(spawnedVehicle, false);
                }
            }
            else
            {
                for (int i = -1, j = 0; j < members.Count; j++)
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
            
            if (!Util.ThereIs(spawnedVehicle) || members.Count < 1 || !spawnedVehicle.IsInRangeOf(Game.Player.Character.Position, 500.0f))
            {
                Restore(false);
                return true;
            }

            if (!Util.ThereIs(target)) SetPedsOffDuty();
            else if (spawnedVehicle.IsInRangeOf(target.Position, 30.0f) || !EveryoneIsSitting()) SetPedsOnDuty();

            return false;
        }
    }
}