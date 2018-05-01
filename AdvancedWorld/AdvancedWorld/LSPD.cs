using GTA;
using GTA.Math;
using GTA.Native;
using System.Collections.Generic;

namespace AdvancedWorld
{
    public class LSPD : Emergency
    {
        public LSPD(string name) : base(name) { }

        public override bool IsCreatedIn(Vector3 position, Entity target, List<string> models)
        {
            spawnedVehicle = Util.Create(name, World.GetNextPositionOnStreet(position, true), target.Heading, false);

            if (!Util.ThereIs(spawnedVehicle)) return false;
            
            for (int i = -1; i < 1; i++)
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

                p.Weapons.Give(WeaponHash.Pistol, 100, true, true);
                p.Weapons.Give(WeaponHash.PumpShotgun, 30, false, false);
                p.Weapons.Current.InfiniteAmmo = true;
                p.ShootRate = 1000;

                p.Armor = 30;
                p.CanSwitchWeapons = true;
                Function.Call(Hash.SET_PED_AS_COP, p, true);
            }

            if (spawnedVehicle.HasSiren) spawnedVehicle.SirenActive = true;

            foreach (Ped p in members)
            {
                if (p.Equals(spawnedVehicle.Driver)) Function.Call(Hash.TASK_VEHICLE_CHASE, p, target);
                else p.Task.FightAgainstHatedTargets(100.0f);
            }
            
            return true;
        }
    }
}