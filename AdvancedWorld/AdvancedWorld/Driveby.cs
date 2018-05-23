﻿using GTA;
using GTA.Math;
using GTA.Native;
using System.Collections.Generic;

namespace AdvancedWorld
{
    public class Driveby : Criminal
    {
        private List<Ped> members;
        private string name;

        public Driveby(string name) : base(ListManager.EventType.Driveby)
        {
            this.members = new List<Ped>();
            this.name = name;
        }

        public bool IsCreatedIn(float radius, List<string> selectedModels)
        {
            Vector3 safePosition = Util.GetSafePositionIn(radius);

            if (safePosition.Equals(Vector3.Zero) || selectedModels == null) return false;
            
            Road road = Util.GetNextPositionOnStreetWithHeading(safePosition);

            if (road.Position.Equals(Vector3.Zero)) return false;

            spawnedVehicle = Util.Create(name, road.Position, road.Heading, true);

            if (!Util.ThereIs(spawnedVehicle)) return false;
            
            List<WeaponHash> drivebyWeaponList = new List<WeaponHash> { WeaponHash.MicroSMG, WeaponHash.Pistol, WeaponHash.APPistol, WeaponHash.CombatPistol, WeaponHash.MachinePistol, WeaponHash.MiniSMG, WeaponHash.Revolver, WeaponHash.RevolverMk2, WeaponHash.DoubleActionRevolver };
            Util.Tune(spawnedVehicle, false, (Util.GetRandomInt(3) == 1));
            
            for (int i = -1; i < spawnedVehicle.PassengerSeats; i++)
            {
                if (spawnedVehicle.IsSeatFree((VehicleSeat)i))
                {
                    members.Add(spawnedVehicle.CreatePedOnSeat((VehicleSeat)i, selectedModels[Util.GetRandomInt(selectedModels.Count)]));
                    Script.Wait(50);
                }
            }

            foreach (Ped p in members)
            {
                if (!Util.ThereIs(p))
                {
                    Restore(true);
                    break;
                }

                Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, p, 46, true);
                Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, p, 5, true);
                Function.Call(Hash.SET_DRIVER_ABILITY, p, 1.0f);
                Function.Call(Hash.SET_DRIVER_AGGRESSIVENESS, p, 1.0f);

                p.AlwaysKeepTask = true;
                p.BlockPermanentEvents = true;
                p.Weapons.Give(drivebyWeaponList[Util.GetRandomInt(drivebyWeaponList.Count)], 100, true, true);
                p.Weapons.Current.InfiniteAmmo = true;

                p.ShootRate = 1000;
                p.RelationshipGroup = relationship;
                p.FiringPattern = FiringPattern.BurstFireDriveby;
            }

            if (DriverExists()) return true;
            else
            {
                Restore(true);
                return false;
            }
        }

        public override void Restore(bool instantly)
        {
            if (instantly)
            {
                foreach (Ped p in members)
                {
                    if (Util.ThereIs(p))
                    {
                        if (Util.BlipIsOn(p)) p.CurrentBlip.Remove();

                        p.Delete();
                    }
                }

                if (Util.ThereIs(spawnedVehicle)) spawnedVehicle.Delete();
            }
            else
            {
                foreach (Ped p in members)
                {
                    if (Util.ThereIs(p))
                    {
                        if (Util.BlipIsOn(p)) p.CurrentBlip.Remove();

                        p.MarkAsNoLongerNeeded();
                    }
                }

                if (Util.ThereIs(spawnedVehicle)) spawnedVehicle.MarkAsNoLongerNeeded();
            }

            if (relationship != 0) Util.CleanUpRelationship(relationship, ListManager.EventType.Driveby);

            members.Clear();
        }

        private bool DriverExists()
        {
            foreach (Ped p in members)
            {
                spawnedPed = null;

                if (!p.IsDead) spawnedPed = p;
                else
                {
                    if (Util.BlipIsOn(p)) p.CurrentBlip.Remove();
                    if (p.Equals(spawnedVehicle.Driver))
                    {
                        if (spawnedVehicle.Model.IsCar && p.IsSittingInVehicle(spawnedVehicle) && spawnedVehicle.IsStopped)
                        {
                            spawnedVehicle.OpenDoor(VehicleDoor.FrontLeftDoor, false, true);
                            Script.Wait(100);
                            Vector3 offset = p.Position + (p.RightVector * (-1.01f));
                            p.Position = new Vector3(offset.X, offset.Y, offset.Z - 0.5f);
                            p.MarkAsNoLongerNeeded();
                        }
                    }
                    else p.MarkAsNoLongerNeeded();
                }

                if (Util.ThereIs(spawnedPed))
                {
                    if (!Util.BlipIsOn(spawnedPed)) Util.AddBlipOn(spawnedPed, 0.7f, BlipSprite.GunCar, BlipColor.White, "Driveby " + spawnedVehicle.FriendlyName);

                    return true;
                }
            }

            return false;
        }

        private bool EveryoneIsSitting()
        {
            foreach (Ped p in members)
            {
                if (!p.Equals(spawnedPed) && !p.IsDead && !p.IsSittingInVehicle(spawnedVehicle)) return false;
            }

            return true;
        }

        public override bool ShouldBeRemoved()
        {
            for (int i = members.Count - 1; i >= 0; i--)
            {
                if (!Util.ThereIs(members[i])) members.RemoveAt(i);
            }
            
            if (!Util.ThereIs(spawnedVehicle) || !DriverExists() || members.Count < 1 || !spawnedPed.IsInRangeOf(Game.Player.Character.Position, 500.0f))
            {
                Restore(false);
                return true;
            }

            if (spawnedVehicle.IsOnFire || !spawnedVehicle.IsDriveable || (spawnedVehicle.IsUpsideDown && spawnedVehicle.IsStopped))
            {
                foreach (Ped p in members)
                {
                    if (!p.IsInCombat) p.Task.FightAgainstHatedTargets(400.0f);
                }
            }
            else if (EveryoneIsSitting())
            {
                if (Util.ThereIs(spawnedVehicle.Driver))
                {
                    foreach (Ped p in members)
                    {
                        if (p.Equals(spawnedPed))
                        {
                            if (!Function.Call<bool>(Hash.GET_IS_TASK_ACTIVE, p, 151)) p.Task.CruiseWithVehicle(spawnedVehicle, 20.0f, (int)DrivingStyle.AvoidTrafficExtremely);
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
                                    return true;
                                }
                            }

                            members[j].Task.EnterVehicle(spawnedVehicle, (VehicleSeat)i++, -1, 2.0f, 1);
                        }
                    }
                }
            }

            if (Util.ThereIs(spawnedPed))
            {
                CheckDispatch();
                CheckBlockable();
            }

            return false;
        }
    }
}