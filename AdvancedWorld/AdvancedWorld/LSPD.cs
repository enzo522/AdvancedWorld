using GTA;
using GTA.Native;
using System.Collections.Generic;

namespace AdvancedWorld
{
    public class LSPD : EntitySet
    {
        private List<Ped> members;
        private Entity target;
        private string name;

        public LSPD(string name) : base()
        {
            this.members = new List<Ped>();
            this.name = name;
        }

        public bool IsCreatedNear(Entity target, List<string> copModels)
        {
            if (copModels == null) return false;
            
            spawnedVehicle = Util.Create(name, World.GetNextPositionOnStreet(Util.GetSafePositionNear(target), true), target.Heading);

            if (!Util.ThereIs(spawnedVehicle)) return false;
            
            for (int i = -1; i < 1; i++)
            {
                if (spawnedVehicle.IsSeatFree((VehicleSeat)i)) members.Add(spawnedVehicle.CreatePedOnSeat((VehicleSeat)i, copModels[Util.GetRandomInt(copModels.Count)]));
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
                
                p.CanSwitchWeapons = true;
                Function.Call(Hash.SET_PED_AS_COP, p, true);
            }

            if (spawnedVehicle.HasSiren) spawnedVehicle.SirenActive = true;

            Function.Call(Hash.TASK_VEHICLE_CHASE, spawnedVehicle.Driver, target);
            spawnedVehicle.GetPedOnSeat(VehicleSeat.Passenger).Task.FightAgainstHatedTargets(100.0f);
            this.target = target;

            return true;
        }

        public override void Restore()
        {
            foreach (Ped p in members)
            {
                if (Util.ThereIs(p)) p.Delete();
            }

            if (Util.ThereIs(spawnedVehicle)) spawnedVehicle.Delete();
        }

        public override bool ShouldBeRemoved()
        {
            if (spawnedVehicle.IsInRangeOf(target.Position, 70.0f) || !spawnedVehicle.IsInRangeOf(Game.Player.Character.Position, 500.0f))
            {
                foreach (Ped p in members)
                {
                    if (Util.ThereIs(p)) p.MarkAsNoLongerNeeded();
                }

                if (Util.ThereIs(spawnedVehicle)) spawnedVehicle.MarkAsNoLongerNeeded();

                return true;
            }
            else return false;
        }
    }
}
