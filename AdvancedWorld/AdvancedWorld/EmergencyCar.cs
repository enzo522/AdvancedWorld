using GTA;
using GTA.Math;
using GTA.Native;
using System.Collections.Generic;

namespace AdvancedWorld
{
    public class EmergencyCar : Emergency
    {
        public EmergencyCar(string name, Entity target, string emergencyType) : base(name, target, emergencyType) { }

        public override bool IsCreatedIn(Vector3 safePosition, List<string> models)
        {
            Road road = Util.GetNextPositionOnStreetWithHeading(safePosition);

            if (road.Position.Equals(Vector3.Zero)) return false;

            spawnedVehicle = Util.Create(name, road.Position, road.Heading, false);

            if (!Util.ThereIs(spawnedVehicle)) return false;
            if (emergencyType == "LSPD")
            {
                for (int i = -1; i < spawnedVehicle.PassengerSeats && i < 1; i++)
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
                    Restore();
                    return false;
                }

                for (int i = -1; i < spawnedVehicle.PassengerSeats && i < 5; i++)
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
                    Restore();
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

                    case "FIB":
                        {
                            p.Weapons.Give(WeaponHash.MachinePistol, 300, true, true);
                            p.Weapons.Give(WeaponHash.CarbineRifle, 300, false, false);
                            p.ShootRate = 900;
                            p.Armor = 50;

                            break;
                        }

                    case "LSPD":
                        {
                            p.Weapons.Give(WeaponHash.Pistol, 100, true, true);
                            p.Weapons.Give(WeaponHash.PumpShotgun, 30, false, false);
                            p.ShootRate = 500;
                            p.Armor = 30;

                            break;
                        }

                    case "SWAT":
                        {
                            if (Util.GetRandomInt(3) == 1)
                            {
                                Shield s = new Shield(p);

                                if (s.IsCreatedIn(p.Position.Around(5.0f)))
                                {
                                    ListManager.Add(s, ListManager.EventType.Shield);
                                    p.Weapons.Give(WeaponHash.Pistol, 100, true, true);
                                }
                                else s.Restore();
                            }
                            
                            if (!p.Weapons.HasWeapon(WeaponHash.Pistol))
                            {
                                p.Weapons.Give(WeaponHash.SMG, 300, true, true);
                                p.Weapons.Give(WeaponHash.Pistol, 100, false, false);
                            }
                            
                            p.ShootRate = 700;
                            p.Armor = 70;

                            break;
                        }
                }

                p.Weapons.Current.InfiniteAmmo = true;
                p.CanSwitchWeapons = true;

                Function.Call(Hash.SET_PED_ID_RANGE, p, 1000.0f);
                Function.Call(Hash.SET_PED_SEEING_RANGE, p, 1000.0f);
                Function.Call(Hash.SET_PED_HEARING_RANGE, p, 1000.0f);
                Function.Call(Hash.SET_PED_COMBAT_RANGE, p, 2);

                Function.Call(Hash.SET_PED_FLEE_ATTRIBUTES, p, 0, false);
                Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, p, 52, true);
                Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, p, 46, true);
                Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, p, 5, true);

                Function.Call(Hash.SET_PED_AS_COP, p, false);
                p.AlwaysKeepTask = true;
                p.BlockPermanentEvents = true;

                if (emergencyType == "ARMY") p.RelationshipGroup = Function.Call<int>(Hash.GET_HASH_KEY, emergencyType);
                else p.RelationshipGroup = Function.Call<int>(Hash.GET_HASH_KEY, "COP");

                p.NeverLeavesGroup = true;
            }

            if (spawnedVehicle.HasSiren) spawnedVehicle.SirenActive = true;

            spawnedVehicle.EngineRunning = true;

            foreach (Ped p in members)
            {
                if (p.Equals(spawnedVehicle.Driver))
                {
                    Function.Call(Hash.SET_DRIVER_ABILITY, p, 1.0f);
                    Function.Call(Hash.SET_DRIVER_AGGRESSIVENESS, p, 1.0f);
                    
                    if (target.Model.IsPed && ((Ped)target).IsInVehicle()) Function.Call(Hash.TASK_VEHICLE_CHASE, p, target);
                    else p.Task.DriveTo(spawnedVehicle, target.Position, 30.0f, 100.0f, (int)DrivingStyle.AvoidTrafficExtremely);
                }
                else p.Task.FightAgainstHatedTargets(400.0f);
            }

            return true;
        }

        protected override void SetPedsOnDuty()
        {
            foreach (Ped p in members)
            {
                if (Util.ThereIs(p))
                {
                    p.AlwaysKeepTask = false;
                    p.BlockPermanentEvents = false;
                    Function.Call(Hash.SET_PED_AS_COP, p, true);
                    p.MarkAsNoLongerNeeded();
                }
            }

            onDuty = true;
        }
    }
}