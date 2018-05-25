﻿using GTA;
using GTA.Math;
using GTA.Native;
using System.Collections.Generic;

namespace AdvancedWorld
{
    public class EmergencyHeli : Emergency
    {
        private bool rappeling;

        public EmergencyHeli(string name, Entity target, string emergencyType) : base(name, target, emergencyType) { this.rappeling = false; }

        public override bool IsCreatedIn(Vector3 safePosition, List<string> models)
        {
            spawnedVehicle = Util.Create(name, new Vector3(safePosition.X, safePosition.Y, safePosition.Z + 50.0f), (target.Position - safePosition).ToHeading(), false);

            if (!Util.ThereIs(spawnedVehicle)) return false;
            if (emergencyType == "LSPD")
            {
                for (int i = -1; i < spawnedVehicle.PassengerSeats && i < 3; i++)
                {
                    if (spawnedVehicle.IsSeatFree((VehicleSeat)i))
                    {
                        members.Add(spawnedVehicle.CreatePedOnSeat((VehicleSeat)i, models[Util.GetRandomInt(models.Count)]));
                        Script.Wait(50);
                    }
                }
            }
            else
            {
                string selectedModel = models[Util.GetRandomInt(models.Count)];

                if (selectedModel == null)
                {
                    Restore(true);
                    return false;
                }

                for (int i = -1; i < spawnedVehicle.PassengerSeats; i++)
                {
                    if (spawnedVehicle.IsSeatFree((VehicleSeat)i))
                    {
                        members.Add(spawnedVehicle.CreatePedOnSeat((VehicleSeat)i, selectedModel));
                        Script.Wait(50);
                    }
                }
            }

            foreach (Ped p in members)
            {
                if (!Util.ThereIs(p))
                {
                    Restore(true);
                    return false;
                }

                switch (emergencyType)
                {
                    case "ARMY":
                        {
                            p.Weapons.Give(WeaponHash.MachinePistol, 300, true, true);
                            p.Weapons.Give(WeaponHash.CombatMG, 500, false, false);
                            p.ShootRate = 1000;
                            p.Armor = 100;

                            break;
                        }

                    case "LSPD":
                        {
                            p.Weapons.Give(WeaponHash.Pistol, 100, true, true);
                            p.ShootRate = 500;
                            p.Armor = 30;

                            break;
                        }

                    case "SWAT":
                        {
                            p.Weapons.Give(WeaponHash.MachinePistol, 300, true, true);
                            p.Weapons.Give(WeaponHash.SMG, 300, false, false);
                            p.ShootRate = 700;
                            p.Armor = 70;

                            break;
                        }
                }

                p.Weapons.Current.InfiniteAmmo = true;
                p.CanSwitchWeapons = true;

                Function.Call(Hash.SET_PED_FLEE_ATTRIBUTES, p, 0, false);
                Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, p, 46, true);
                Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, p, 5, true);

                Function.Call(Hash.SET_PED_AS_COP, p, false);
                p.AlwaysKeepTask = true;
                p.BlockPermanentEvents = true;

                p.RelationshipGroup = relationship;
                p.NeverLeavesGroup = true;
            }

            spawnedVehicle.EngineRunning = true;
            spawnedVehicle.Livery = 0;
            Function.Call(Hash.SET_HELI_BLADES_FULL_SPEED, spawnedVehicle);
            SetPedsOnDuty();

            return true;
        }

        private new void SetPedsOnDuty()
        {
            if (onVehicleDuty)
            {
                foreach (Ped p in members)
                {
                    if (Util.ThereIs(spawnedVehicle) && p.Equals(spawnedVehicle.Driver)) Function.Call(Hash.TASK_VEHICLE_HELI_PROTECT, p, spawnedVehicle, target, 50.0f, 32, 25.0f, 35, 1);
                    else if (!p.IsInCombat) p.Task.FightAgainstHatedTargets(400.0f);
                }
            }
            else
            {
                if (Util.ThereIs(spawnedVehicle) && spawnedVehicle.IsDriveable)
                {
                    foreach (Ped p in members)
                    {
                        if (p.Equals(spawnedVehicle.Driver))
                        {
                            if (rappeling) Function.Call(Hash.TASK_HELI_MISSION, p, spawnedVehicle, 0, 0, spawnedVehicle.Position.X, spawnedVehicle.Position.Y, spawnedVehicle.Position.Z, 4, 40.0f, 1.0f, spawnedVehicle.Heading, -1, 5, -1.0f, 0);
                            else Function.Call(Hash.TASK_VEHICLE_HELI_PROTECT, p, spawnedVehicle, target, 50.0f, 32, 25.0f, 35, 1);
                        }
                        else if (p.Equals(spawnedVehicle.GetPedOnSeat(VehicleSeat.Passenger)))
                        {
                            if (!p.IsInCombat) p.Task.FightAgainstHatedTargets(400.0f);
                        }
                        else
                        {
                            if (p.IsSittingInVehicle(spawnedVehicle))
                            {
                                Function.Call(Hash.TASK_RAPPEL_FROM_HELI, p, 0x41200000);
                                rappeling = true;
                            }
                            else if (!Function.Call<bool>(Hash.GET_IS_TASK_ACTIVE, p, 67) && !p.IsInCombat)
                            {
                                p.Task.FightAgainstHatedTargets(400.0f);
                                rappeling = false;
                            }
                        }
                    }
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

        private new void SetPedsOffDuty()
        {
            foreach (Ped p in members)
            {
                if (Util.ThereIs(p) && p.IsPersistent)
                {
                    p.AlwaysKeepTask = false;
                    p.BlockPermanentEvents = false;
                    Function.Call(Hash.SET_PED_AS_COP, p, true);
                    p.MarkAsNoLongerNeeded();
                }
            }
        }

        public override bool ShouldBeRemoved()
        {
            int alive = 0;

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
                else if (!members[i].Equals(spawnedVehicle.Driver)) alive++;
            }

            if (members.Count < 1 || !spawnedVehicle.IsInRangeOf(Game.Player.Character.Position, 500.0f))
            {
                Restore(false);
                return true;
            }

            if (!TargetIsFound() || alive < 1) SetPedsOffDuty();
            else
            {
                if (!Util.ThereIs(spawnedVehicle) || !spawnedVehicle.IsDriveable || (spawnedVehicle.IsInRangeOf(target.Position, 100.0f) && target.Model.IsPed && !((Ped)target).IsInVehicle())) onVehicleDuty = false;
                else onVehicleDuty = true;

                SetPedsOnDuty();
            }

            return false;
        }
    }
}