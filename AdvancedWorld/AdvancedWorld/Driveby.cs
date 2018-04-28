﻿using GTA;
using GTA.Math;
using GTA.Native;
using System.Collections.Generic;

namespace AdvancedWorld
{
    public class Driveby : EntitySet
    {
        private List<Ped> members;
        private string name;
        private int relationship;

        public Driveby(string name) : base()
        {
            this.members = new List<Ped>();
            this.name = name;
            this.relationship = 0;
        }

        public bool IsCreatedIn(float radius, List<string> selectedModels)
        {
            Vector3 safePosition = Util.GetSafePositionIn(radius);

            if (safePosition.Equals(Vector3.Zero)) return false;

            spawnedVehicle = Util.Create(name, World.GetNextPositionOnStreet(safePosition, true), Util.GetRandomInt(360));

            if (!Util.ThereIs(spawnedVehicle)) return false;
            if (selectedModels == null)
            {
                spawnedVehicle.Delete();
                return false;
            }

            List<WeaponHash> drivebyWeaponList = new List<WeaponHash> { WeaponHash.MicroSMG, WeaponHash.Pistol, WeaponHash.APPistol, WeaponHash.CombatPistol, WeaponHash.MachinePistol, WeaponHash.MiniSMG, WeaponHash.Revolver, WeaponHash.RevolverMk2, WeaponHash.DoubleActionRevolver };
            Util.Tune(spawnedVehicle, false, (Util.GetRandomInt(3) == 1));
            relationship = AdvancedWorld.NewRelationship(1);

            if (relationship == 0)
            {
                Restore();
                return false;
            }

            for (int i = -1; i < spawnedVehicle.PassengerSeats; i++)
            {
                if (spawnedVehicle.IsSeatFree((VehicleSeat)i)) members.Add(spawnedVehicle.CreatePedOnSeat((VehicleSeat)i, selectedModels[Util.GetRandomInt(selectedModels.Count)]));
            }

            foreach (Ped p in members)
            {
                if (!Util.ThereIs(p))
                {
                    Restore();
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
                Restore();
                return false;
            }
        }

        public override void Restore()
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
            if (relationship != 0) AdvancedWorld.CleanUpRelationship(relationship);

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
                    if (spawnedVehicle.Model.IsCar && p.IsSittingInVehicle(spawnedVehicle) && p.Equals(spawnedVehicle.Driver) && spawnedVehicle.IsStopped)
                    {
                        spawnedVehicle.OpenDoor(VehicleDoor.FrontLeftDoor, false, true);
                        Script.Wait(100);
                        Vector3 offset = p.Position + (p.RightVector * (-1.01f));
                        p.Position = new Vector3(offset.X, offset.Y, offset.Z - 0.5f);
                    }
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
                foreach (Ped p in members)
                {
                    if (Util.ThereIs(p))
                    {
                        if (Util.BlipIsOn(p)) p.CurrentBlip.Remove();
                        if (p.IsPersistent) p.MarkAsNoLongerNeeded();
                    }
                }

                if (Util.ThereIs(spawnedPed)) spawnedPed.MarkAsNoLongerNeeded();
                if (Util.ThereIs(spawnedVehicle)) spawnedVehicle.MarkAsNoLongerNeeded();

                members.Clear();
                AdvancedWorld.CleanUpRelationship(relationship);
                return true;
            }

            if (spawnedPed.IsSittingInVehicle(spawnedVehicle) && spawnedPed.Equals(spawnedVehicle.Driver))
            {
                if (EveryoneIsSitting())
                {
                    foreach (Ped p in members)
                    {
                        if (p.Equals(spawnedPed))
                        {
                            if (!Function.Call<bool>(Hash.GET_IS_TASK_ACTIVE, p, 151)) p.Task.CruiseWithVehicle(spawnedVehicle, 20.0f, (int)DrivingStyle.AvoidTrafficExtremely);
                        }
                        else if (!Function.Call<bool>(Hash.GET_IS_TASK_ACTIVE, p, 342)) p.Task.FightAgainstHatedTargets(400.0f);
                    }
                }
                else
                {
                    if (!spawnedVehicle.IsStopped) Function.Call(Hash.TASK_VEHICLE_TEMP_ACTION, spawnedPed, spawnedVehicle, 1, 1000);
                    else
                    {
                        foreach (Ped p in members)
                        {
                            if (p.Equals(spawnedPed)) p.Task.Wait(1000);
                            else if (!p.IsSittingInVehicle(spawnedVehicle) && !Function.Call<bool>(Hash.GET_IS_TASK_ACTIVE, p, 160))
                            {
                                for (int i = 0; i < spawnedVehicle.PassengerSeats; i++)
                                {
                                    if (spawnedVehicle.IsSeatFree((VehicleSeat)i) || spawnedVehicle.GetPedOnSeat((VehicleSeat)i).IsDead)
                                    {
                                        p.Task.EnterVehicle(spawnedVehicle, (VehicleSeat)i, -1, 2.0f, 1);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else if (spawnedVehicle.IsOnFire || !spawnedVehicle.IsDriveable)
            {
                foreach (Ped p in members)
                {
                    if (!Function.Call<bool>(Hash.GET_IS_TASK_ACTIVE, p, 342)) p.Task.FightAgainstHatedTargets(400.0f);
                }
            }
            else if (!Function.Call<bool>(Hash.GET_IS_TASK_ACTIVE, spawnedPed, 160)) spawnedPed.Task.EnterVehicle(spawnedVehicle, VehicleSeat.Driver, -1, 2.0f, 1);

            return false;
        }
    }
}