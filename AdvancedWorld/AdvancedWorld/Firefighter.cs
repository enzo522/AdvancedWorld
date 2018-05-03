using GTA;
using GTA.Math;
using GTA.Native;
using System.Collections.Generic;

namespace AdvancedWorld
{
    public class Firefighter : Emergency
    {
        private Vector3 targetPosition;

        public Firefighter(string name, Entity target) : base(name, target)
        {
            targetPosition = target.Position;
        }

        public override bool IsCreatedIn(Vector3 safePosition, List<string> models)
        {
            Vector3 position = World.GetNextPositionOnStreet(safePosition, true);

            if (position.Equals(Vector3.Zero)) return false;

            spawnedVehicle = Util.Create(name, position, (position - targetPosition).ToHeading(), false);

            if (!Util.ThereIs(spawnedVehicle)) return false;

            for (int i = -1; i < spawnedVehicle.PassengerSeats; i++)
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
            }

            if (spawnedVehicle.HasSiren) spawnedVehicle.SirenActive = true;

            foreach (Ped p in members)
            {
                if (p.Equals(spawnedVehicle.Driver)) p.Task.DriveTo(spawnedVehicle, targetPosition, 10.0f, 100.0f, (int)DrivingStyle.AvoidTrafficExtremely);
            }

            return true;
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

                if (members[i].IsInRangeOf(targetPosition, 50.0f) && members[i].IsSittingInVehicle(spawnedVehicle))
                {
                    Vehicle[] nearbyVehicles = World.GetNearbyVehicles(members[i], 50.0f);

                    if (nearbyVehicles.Length < 1) continue;

                    foreach (Vehicle v in nearbyVehicles)
                    {
                        if (v.IsDead || v.IsOnFire)
                        {                            
                            TaskSequence ts = new TaskSequence();
                            ts.AddTask.RunTo(v.Position.Around(1.0f));
                            ts.AddTask.FightAgainstHatedTargets(100.0f);
                            ts.AddTask.WanderAround();
                            ts.Close();

                            members[i].Task.PerformSequence(ts);
                            ts.Dispose();
                        }
                    }
                }

                if (!members[i].IsInVehicle(spawnedVehicle))
                {
                    members[i].Weapons.Give(WeaponHash.FireExtinguisher, 1, true, true);
                    members[i].Weapons.Current.InfiniteAmmo = true;
                }

                if (members[i].IsDead)
                {
                    members[i].MarkAsNoLongerNeeded();
                    members.RemoveAt(i);
                }
                else spawnedPed = members[i];
            }

            if (!Util.ThereIs(spawnedVehicle) || !Util.ThereIs(target) || members.Count < 1 || !spawnedVehicle.IsInRangeOf(Game.Player.Character.Position, 500.0f))
            {
                foreach (Ped p in members)
                {
                    if (Util.ThereIs(p)) p.MarkAsNoLongerNeeded();
                }

                if (Util.ThereIs(spawnedVehicle)) spawnedVehicle.MarkAsNoLongerNeeded();

                members.Clear();
                return true;
            }

            return false;
        }
    }
}