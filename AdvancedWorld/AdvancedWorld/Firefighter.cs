using GTA;
using GTA.Math;
using GTA.Native;
using System.Collections.Generic;

namespace AdvancedWorld
{
    public class Firefighter : EmergencyFire
    {
        public Firefighter(string name, Entity target) : base(name, target) { }

        public override bool IsCreatedIn(Vector3 safePosition, List<string> models)
        {
            Vector3 position = World.GetNextPositionOnStreet(safePosition, true);

            if (position.Equals(Vector3.Zero)) return false;

            spawnedVehicle = Util.Create(name, position, (targetPosition - position).ToHeading(), false);

            if (!Util.ThereIs(spawnedVehicle)) return false;

            for (int i = -1; i < spawnedVehicle.PassengerSeats && i < 3; i++)
            {
                if (spawnedVehicle.IsSeatFree((VehicleSeat)i))
                {
                    members.Add(spawnedVehicle.CreatePedOnSeat((VehicleSeat)i, models[Util.GetRandomInt(models.Count)]));
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
                
                p.RelationshipGroup = Function.Call<int>(Hash.GET_HASH_KEY, "FIREMAN");
                p.AlwaysKeepTask = true;
                p.BlockPermanentEvents = true;

                p.Weapons.Give(WeaponHash.FireExtinguisher, 100, true, true);
                p.Weapons.Current.InfiniteAmmo = true;
                p.CanSwitchWeapons = true;
            }

            if (spawnedVehicle.HasSiren) spawnedVehicle.SirenActive = true;

            foreach (Ped p in members)
            {
                if (p.Equals(spawnedVehicle.Driver))
                {
                    Function.Call(Hash.SET_DRIVER_ABILITY, p, 1.0f);
                    Function.Call(Hash.SET_DRIVER_AGGRESSIVENESS, p, 1.0f);
                    p.Task.DriveTo(spawnedVehicle, targetPosition, 30.0f, 100.0f, (int)DrivingStyle.AvoidTrafficExtremely);
                }

                p.IsFireProof = true;
            }

            return true;
        }

        protected override void SetPedOnDuty(Ped p)
        {
            if (p.IsRunning || Function.Call<bool>(Hash.GET_IS_TASK_ACTIVE, p, 355)) return;

            Entity[] nearbyEntities = World.GetNearbyEntities(p.Position, 100.0f);

            if (nearbyEntities.Length < 1) return;

            foreach (Entity en in nearbyEntities)
            {
                if (en.IsOnFire)
                {
                    TaskSequence ts = new TaskSequence();

                    if (!p.IsInRangeOf(en.Position, 10.0f)) ts.AddTask.RunTo(en.Position.Around(5.0f));

                    ts.AddTask.ShootAt(en.Position, 10000, FiringPattern.FullAuto);
                    ts.Close();

                    p.Task.PerformSequence(ts);
                    ts.Dispose();

                    return;
                }
            }

            SetPedsOffDuty();
        }
    }
}