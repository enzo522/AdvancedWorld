using GTA;
using GTA.Math;
using GTA.Native;
using System.Collections.Generic;

namespace AdvancedWorld
{
    public class EmergencyHeli : Emergency
    {
        public EmergencyHeli(string name, Entity target, string emergencyType) : base(name, target, emergencyType) { }

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
                    Restore();
                    return false;
                }

                for (int i = -1; i < spawnedVehicle.PassengerSeats && i < 3; i++)
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

                Function.Call(Hash.SET_PED_ID_RANGE, p, 2000.0f);
                Function.Call(Hash.SET_PED_SEEING_RANGE, p, 2000.0f);
                Function.Call(Hash.SET_PED_HEARING_RANGE, p, 2000.0f);
                Function.Call(Hash.SET_PED_COMBAT_RANGE, p, 2);

                Function.Call(Hash.SET_PED_FLEE_ATTRIBUTES, p, 0, false);
                Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, p, 46, true);
                Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, p, 5, true);

                Function.Call(Hash.SET_PED_AS_COP, p, false);
                p.AlwaysKeepTask = true;
                p.BlockPermanentEvents = true;
            }

            spawnedVehicle.EngineRunning = true;
            spawnedVehicle.Livery = 0;
            Function.Call(Hash.SET_HELI_BLADES_FULL_SPEED, spawnedVehicle);

            if (Util.ThereIs(spawnedVehicle.Driver))
            {
                foreach (Ped p in members)
                {
                    if (p.Equals(spawnedVehicle.Driver)) Function.Call(Hash.TASK_VEHICLE_HELI_PROTECT, p, spawnedVehicle, target, 50.0f, 32, 25.0f, 35, 1);
                    else p.Task.FightAgainstHatedTargets(100.0f);
                }
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

                    if (emergencyType == "ARMY") p.RelationshipGroup = Function.Call<int>(Hash.GET_HASH_KEY, emergencyType);
                    else p.RelationshipGroup = Function.Call<int>(Hash.GET_HASH_KEY, "COP");

                    p.NeverLeavesGroup = true;
                    onDuty = true;
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

            if (!Util.ThereIs(spawnedVehicle) || alive < 1 || members.Count < 1 || !spawnedVehicle.IsInRangeOf(Game.Player.Character.Position, 500.0f))
            {
                foreach (Ped p in members)
                {
                    if (Util.ThereIs(p)) p.MarkAsNoLongerNeeded();
                }

                if (Util.ThereIs(spawnedVehicle)) spawnedVehicle.MarkAsNoLongerNeeded();

                members.Clear();
                return true;
            }

            if (spawnedVehicle.IsInRangeOf(target.Position, 50.0f) || !Util.ThereIs(target) || target.IsDead) SetPedsOnDuty();

            return false;
        }
    }
}