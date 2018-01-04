using GTA;
using GTA.Math;
using GTA.Native;
using System.Collections.Generic;

namespace AdvancedWorld
{
    public class Driveby : EntitySet
    {
        private List<Ped> members;
        private string name;

        public Driveby(string name) : base()
        {
            this.members = new List<Ped>();
            this.name = name;
        }

        public bool IsCreatedIn(float radius, List<string> selectedModels)
        {
            Vector3 safePosition = Util.GetSafePositionIn(radius);

            if (safePosition.Equals(Vector3.Zero)) return false;

            spawnedVehicle = Util.Create(name, World.GetNextPositionOnStreet(safePosition.Around(10.0f), true), Util.GetRandomInt(360));

            if (!Util.ThereIs(spawnedVehicle)) return false;

            List<VehicleSeat> seats = new List<VehicleSeat> { VehicleSeat.LeftFront, VehicleSeat.RightFront, VehicleSeat.LeftRear, VehicleSeat.RightRear };

            if (selectedModels == null)
            {
                spawnedVehicle.Delete();
                return false;
            }

            List<WeaponHash> drivebyWeaponList = new List<WeaponHash> { WeaponHash.MicroSMG, WeaponHash.Pistol, WeaponHash.APPistol, WeaponHash.MachinePistol };
            Util.Tune(spawnedVehicle, false);

            foreach (VehicleSeat vs in seats)
                members.Add(spawnedVehicle.CreatePedOnSeat(vs, selectedModels[Util.GetRandomInt(selectedModels.Count)]));

            foreach (Ped p in members)
            {
                if (!Util.ThereIs(p))
                {
                    Restore();
                    break;
                }

                Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, p, 46, true);
                Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, p, 5, true);
                p.AlwaysKeepTask = true;
                p.BlockPermanentEvents = true;
                p.Weapons.Give(drivebyWeaponList[Util.GetRandomInt(drivebyWeaponList.Count)], 100, true, true);
                p.Weapons.Current.InfiniteAmmo = true;
                p.RelationshipGroup = AdvancedWorld.cougarID;

                if (p.Equals(spawnedVehicle.Driver))
                {
                    p.Task.CruiseWithVehicle(spawnedVehicle, 20.0f, (int)DrivingStyle.AvoidTrafficExtremely);

                    if (!Util.BlipIsOn(p)) Util.AddBlipOn(p, 0.7f, BlipSprite.GunCar, BlipColor.White, "Driveby " + spawnedVehicle.FriendlyName);
                    else
                    {
                        p.Delete();
                        break;
                    }
                }
                else
                {
                    p.Task.FightAgainstHatedTargets(400.0f);
                    p.FiringPattern = FiringPattern.BurstFireDriveby;

                    if (Util.BlipIsOn(p)) p.Delete();
                }
            }

            if (!Util.ThereIs(spawnedVehicle) || !Util.ThereIs(spawnedVehicle.Driver))
            {
                Restore();
                return false;
            }
            else return true;
        }

        private void Restore()
        {
            foreach (Ped p in members)
            {
                if (Util.ThereIs(p)) p.Delete();
            }

            members.Clear();
            spawnedVehicle.Delete();
        }

        public override bool ShouldBeRemoved()
        {
            for (int i = members.Count - 1; i >= 0; i--)
            {
                if (!Util.ThereIs(members[i]))
                {
                    members.RemoveAt(i);
                    break;
                }

                if (members[i].IsDead || !spawnedVehicle.IsDriveable || !members[i].IsInRangeOf(Game.Player.Character.Position, 500.0f))
                {
                    if (Util.BlipIsOn(members[i])) members[i].CurrentBlip.Remove();
                    if (members[i].IsPersistent) members[i].MarkAsNoLongerNeeded();

                    members.RemoveAt(i);
                    break;
                }
            }

            if (members.Count == 0)
            {
                spawnedVehicle.MarkAsNoLongerNeeded();
                return true;
            }
            else return false;
        }
    }
}
