using GTA;
using GTA.Math;
using GTA.Native;
using System.Collections.Generic;

namespace AdvancedWorld
{
    public class Driveby : Criminal
    {
        private List<Ped> members;
        private string name;

        public Driveby(string name) : base(EventManager.EventType.Driveby)
        {
            this.members = new List<Ped>();
            this.name = name;
        }

        public bool IsCreatedIn(float radius, List<string> selectedModels)
        {
            Vector3 safePosition = Util.GetSafePositionIn(radius);

            if (safePosition.Equals(Vector3.Zero) || selectedModels == null) return false;
            
            Road road = Util.GetNextPositionOnStreetWithHeading(safePosition);

            if (road.Position.Equals(Vector3.Zero)) return false;

            spawnedVehicle = Util.Create(name, road.Position, road.Heading, true);

            if (!Util.ThereIs(spawnedVehicle)) return false;
            
            List<WeaponHash> drivebyWeaponList = new List<WeaponHash> { WeaponHash.MicroSMG, WeaponHash.Pistol, WeaponHash.APPistol, WeaponHash.CombatPistol, WeaponHash.MachinePistol, WeaponHash.MiniSMG, WeaponHash.Revolver, WeaponHash.RevolverMk2, WeaponHash.DoubleActionRevolver };
            Util.Tune(spawnedVehicle, false, (Util.GetRandomInt(3) == 1));
            
            for (int i = -1; i < spawnedVehicle.PassengerSeats; i++)
            {
                if (spawnedVehicle.IsSeatFree((VehicleSeat)i))
                {
                    members.Add(spawnedVehicle.CreatePedOnSeat((VehicleSeat)i, selectedModels[Util.GetRandomInt(selectedModels.Count)]));
                    Script.Wait(50);
                }
            }

            foreach (Ped p in members)
            {
                if (!Util.ThereIs(p))
                {
                    Restore(true);
                    break;
                }

                Function.Call(Hash.SET_PED_FLEE_ATTRIBUTES, p, 0, false);
                Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, p, 17, true);
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
                p.IsPriorityTargetForEnemies = true;
                p.FiringPattern = FiringPattern.BurstFireDriveby;
            }

            if (SpawnedPedExists()) return true;
            else
            {
                Restore(true);
                return false;
            }
        }

        public override void Restore(bool instantly)
        {
            if (instantly)
            {
                foreach (Ped p in members)
                {
                    if (Util.ThereIs(p)) p.Delete();
                }

                if (Util.ThereIs(spawnedVehicle)) spawnedVehicle.Delete();
            }
            else
            {
                foreach (Ped p in members) Util.NaturallyRemove(p);

                Util.NaturallyRemove(spawnedVehicle);
            }

            if (relationship != 0) Util.CleanUpRelationship(relationship);

            members.Clear();
        }

        private bool SpawnedPedExists()
        {
            spawnedPed = null;

            foreach (Ped p in members)
            {
                if (Util.ThereIs(p) && !p.IsDead)
                {
                    if (!Util.BlipIsOn(p)) Util.AddBlipOn(p, 0.7f, BlipSprite.GunCar, BlipColor.White, "Driveby " + spawnedVehicle.FriendlyName);
                    else if (!p.CurrentBlip.Sprite.Equals(BlipSprite.GunCar)) p.CurrentBlip.Remove(); 

                    spawnedPed = p;
                    return true;
                }
            }

            return false;
        }

        public override bool ShouldBeRemoved()
        {
            for (int i = members.Count - 1; i >= 0; i--)
            {
                if (!Util.ThereIs(members[i]))
                {
                    members.RemoveAt(i);
                    continue;
                }
            }
            
            if (!Util.ThereIs(spawnedVehicle) || !SpawnedPedExists() || members.Count < 1 || !spawnedVehicle.IsInRangeOf(Game.Player.Character.Position, 500.0f))
            {
                Restore(false);
                return true;
            }

            if (spawnedVehicle.IsOnFire || !spawnedVehicle.IsDriveable || (spawnedVehicle.IsUpsideDown && spawnedVehicle.IsStopped))
            {
                foreach (Ped p in members)
                {
                    if (!p.IsInCombat) p.Task.FightAgainstHatedTargets(400.0f);
                }
            }
            else if (ReadyToGoWith(members))
            {
                if (Util.ThereIs(spawnedVehicle.Driver))
                {
                    foreach (Ped p in members)
                    {
                        if (p.Equals(spawnedVehicle.Driver))
                        {
                            if (!Function.Call<bool>(Hash.GET_IS_TASK_ACTIVE, p, 151)) p.Task.CruiseWithVehicle(spawnedVehicle, 20.0f, (int)DrivingStyle.AvoidTrafficExtremely);
                        }
                        else if (!p.IsInCombat) p.Task.FightAgainstHatedTargets(400.0f);
                    }
                }
                else
                {
                    foreach (Ped p in members) p.Task.LeaveVehicle(spawnedVehicle, false);
                }
            }
            else
            {
                if (!VehicleSeatsCanBeSeatedBy(members))
                {
                    Restore(false);
                    return true;
                }
            }

            if (Util.ThereIs(spawnedPed))
            {
                CheckDispatch();
                CheckBlockable();
            }

            return false;
        }
    }
}