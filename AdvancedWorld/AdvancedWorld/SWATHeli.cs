using GTA;
using GTA.Math;
using GTA.Native;
using System.Collections.Generic;

namespace AdvancedWorld
{
    public class SWATHeli : Emergency
    {
        public SWATHeli(string name, Entity target) : base(name, target) { }

        public override bool IsCreatedIn(Vector3 safePosition, List<string> models)
        {
            spawnedVehicle = Util.Create(name, new Vector3(safePosition.X, safePosition.Y, safePosition.Z + 50.0f), target.Heading, false);

            if (!Util.ThereIs(spawnedVehicle)) return false;

            string selectedModel = models[Util.GetRandomInt(models.Count)];

            if (selectedModel == null) return false;

            for (int i = -1; i < spawnedVehicle.PassengerSeats; i++)
            {
                if (spawnedVehicle.IsSeatFree((VehicleSeat)i))
                {
                    members.Add(spawnedVehicle.CreatePedOnSeat((VehicleSeat)i, selectedModel));
                    Script.Wait(50);
                }
            }

            foreach (Ped p in members)
            {
                if (!Util.ThereIs(p))
                {
                    Restore();
                    return false;
                }

                p.Weapons.Give(WeaponHash.SMG, 300, true, true);
                p.Weapons.Give(WeaponHash.HeavySniper, 30, false, false);
                p.Weapons.Give(WeaponHash.Pistol, 100, false, false);
                p.Weapons.Current.InfiniteAmmo = true;
                p.ShootRate = 1000;

                p.Armor = 50;
                p.CanSwitchWeapons = true;

                Function.Call(Hash.SET_PED_AS_COP, p, false);
                p.AlwaysKeepTask = true;
                p.BlockPermanentEvents = true;
            }
            
            spawnedVehicle.EngineRunning = true;
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

        private void SetPedAsCop(Ped p)
        {
            if (Util.ThereIs(p))
            {
                if (p.Equals(spawnedVehicle.Driver) && !target.IsDead) return;

                p.AlwaysKeepTask = false;
                p.BlockPermanentEvents = false;
                Function.Call(Hash.SET_PED_AS_COP, p, true);
            }
        }
    }
}