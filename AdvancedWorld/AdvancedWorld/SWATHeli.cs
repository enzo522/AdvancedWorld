using GTA;
using GTA.Math;
using GTA.Native;
using System.Collections.Generic;

namespace AdvancedWorld
{
    public class SWATHeli : Emergency
    {
        public SWATHeli(string name, Entity target) : base(name, target) { }

        public override bool IsCreatedIn(Vector3 position, List<string> models)
        {
            spawnedVehicle = Util.Create(name, new Vector3(position.X, position.Y, position.Z + 50.0f), target.Heading, false);

            if (!Util.ThereIs(spawnedVehicle)) return false;

            for (int i = -1; i < spawnedVehicle.PassengerSeats; i++)
            {
                if (spawnedVehicle.IsSeatFree((VehicleSeat)i)) members.Add(spawnedVehicle.CreatePedOnSeat((VehicleSeat)i, models[Util.GetRandomInt(models.Count)]));
            }

            foreach (Ped p in members)
            {
                if (!Util.ThereIs(p))
                {
                    Restore();
                    return false;
                }

                p.Weapons.Give(WeaponHash.SMG, 300, true, true);
                p.Weapons.Give(WeaponHash.Pistol, 100, false, false);
                p.Weapons.Current.InfiniteAmmo = true;
                p.ShootRate = 1000;

                p.Armor = 50;
                p.CanSwitchWeapons = true;
                Function.Call(Hash.SET_PED_AS_COP, p, true);
            }
            
            spawnedVehicle.EngineRunning = true;
            Function.Call(Hash.SET_HELI_BLADES_FULL_SPEED, spawnedVehicle);

            foreach (Ped p in members)
            {
                if (p.Equals(spawnedVehicle.Driver)) Function.Call(Hash.TASK_HELI_MISSION, p, spawnedVehicle, 0, target, 0.0f, 0.0f, 0.0f, 9, 50.0f, 10.0f, (target.Position - spawnedVehicle.Position).ToHeading(), 15, 15, -1.0f, 4096);
                else p.Task.FightAgainstHatedTargets(100.0f);
            }

            return true;
        }
    }
}
