using GTA;
using GTA.Math;
using GTA.Native;
using System.Collections.Generic;

namespace YouAreNotAlone
{
    public class EmergencyBlock : Emergency
    {
        public EmergencyBlock(string name, Entity target, string emergencyType) : base(name, target, emergencyType)
        {
            this.blipName += emergencyType + " Road Block";
            Logger.Write("EmergencyBlock: Time to block road.", emergencyType + " " + name);
        }

        public override bool IsCreatedIn(Vector3 safePosition, List<string> models)
        {
            Road road = new Road(Vector3.Zero, 0.0f);

            for (int cnt = 0; cnt < 5; cnt++)
            {
                road = Util.GetNextPositionOnStreetWithHeading(safePosition.Around(50.0f));

                if (!road.Position.Equals(Vector3.Zero))
                {
                    Logger.Write("EmergencyBlock: Found proper road.", emergencyType + " " + name);

                    break;
                }
            }

            if (road.Position.Equals(Vector3.Zero))
            {
                Logger.Error("EmergencyBlock: Couldn't find proper road. Abort.", emergencyType + " " + name);

                return false;
            }

            spawnedVehicle = Util.Create(name, road.Position, road.Heading + 90, false);

            if (!Util.ThereIs(spawnedVehicle))
            {
                Logger.Error("EmergencyBlock: Couldn't create vehicle. Abort.", emergencyType + " " + name);

                return false;
            }

            Stinger s = new Stinger(spawnedVehicle);

            if (s.IsCreatedIn(spawnedVehicle.Position - spawnedVehicle.ForwardVector * spawnedVehicle.Model.GetDimensions().Y)) DispatchManager.Add(s, DispatchManager.DispatchType.Stinger);
            else s.Restore(true);

            if (emergencyType == "LSPD")
            {
                for (int i = -1; i < spawnedVehicle.PassengerSeats && i < 1; i++)
                {
                    if (spawnedVehicle.IsSeatFree((VehicleSeat)i))
                    {
                        members.Add(spawnedVehicle.CreatePedOnSeat((VehicleSeat)i, models[Util.GetRandomIntBelow(models.Count)]));
                        Script.Wait(50);
                    }
                }
            }
            else
            {
                string selectedModel = models[Util.GetRandomIntBelow(models.Count)];

                if (selectedModel == null)
                {
                    Logger.Error("EmergencyBlock: Couldn't find model. Abort.", emergencyType + " " + name);
                    Restore(true);

                    return false;
                }

                for (int i = -1; i < spawnedVehicle.PassengerSeats && i < 1; i++)
                {
                    if (spawnedVehicle.IsSeatFree((VehicleSeat)i))
                    {
                        members.Add(spawnedVehicle.CreatePedOnSeat((VehicleSeat)i, selectedModel));
                        Script.Wait(50);
                    }
                }
            }

            Logger.Write("EmergencyBlock: Tried to create stinger and created members.", emergencyType + " " + name);

            foreach (Ped p in members)
            {
                if (!Util.ThereIs(p))
                {
                    Logger.Error("EmergencyBlock: There is a member who doesn't exist. Abort.", emergencyType + " " + name);
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
                            p.Weapons.Give(WeaponHash.PumpShotgun, 30, false, false);
                            p.ShootRate = 500;
                            p.Armor = 30;

                            break;
                        }

                    case "SWAT":
                        {
                            p.Weapons.Give(WeaponHash.SMG, 300, true, true);
                            p.Weapons.Give(WeaponHash.Pistol, 100, false, false);
                            p.ShootRate = 700;
                            p.Armor = 70;

                            break;
                        }
                }

                if (p.IsInVehicle(spawnedVehicle)) p.Task.LeaveVehicle(spawnedVehicle, false);
                
                p.Weapons.Current.InfiniteAmmo = true;
                p.CanSwitchWeapons = true;
                AddVarietyTo(p);

                Function.Call(Hash.SET_PED_FLEE_ATTRIBUTES, p, 0, false);
                Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, p, 17, true);
                Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, p, 52, true);
                Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, p, 46, true);
                Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, p, 5, true);

                Function.Call(Hash.SET_PED_AS_COP, p, false);
                p.AlwaysKeepTask = true;
                p.BlockPermanentEvents = true;

                p.RelationshipGroup = relationship;
                p.NeverLeavesGroup = true;
                Logger.Write("EmergencyBlock: Characteristics are set.", emergencyType + " " + name);
            }

            spawnedVehicle.EngineRunning = true;
            SetPedsOnDuty(false);
            Logger.Write("EmergencyBlock: Ready to dispatch.", emergencyType + " " + name);

            return true;
        }
    }
}