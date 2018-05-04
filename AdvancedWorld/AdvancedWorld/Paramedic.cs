using GTA;
using GTA.Math;
using GTA.Native;
using System.Collections.Generic;

namespace AdvancedWorld
{
    public class Paramedic : Emergency
    {
        private Vector3 targetPosition;

        public Paramedic(string name, Entity target) : base(name, target)
        {
            targetPosition = target.Position;
        }

        public override bool IsCreatedIn(Vector3 safePosition, List<string> models)
        {
            Vector3 position = World.GetNextPositionOnStreet(safePosition, true);

            if (position.Equals(Vector3.Zero)) return false;

            spawnedVehicle = Util.Create(name, position, (targetPosition - position).ToHeading(), false);

            if (!Util.ThereIs(spawnedVehicle)) return false;

            for (int i = -1; i < 1; i++)
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

                p.RelationshipGroup = Function.Call<int>(Hash.GET_HASH_KEY, "MEDIC");
                p.AlwaysKeepTask = true;
                p.BlockPermanentEvents = true;
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
            }

            return true;
        }

        private void SetPedAsMedic(Ped p)
        {
            Ped[] nearbyPeds = World.GetNearbyPeds(p, 50.0f);

            if (nearbyPeds.Length < 1) return;

            foreach (Ped selectedPed in nearbyPeds)
            {
                if (selectedPed.IsDead || selectedPed.IsInjured)
                {
                    TaskSequence ts = new TaskSequence();
                    ts.AddTask.RunTo(selectedPed.Position.Around(0.5f));
                    ts.AddTask.LookAt(selectedPed, 2000);
                    ts.AddTask.PlayAnimation("amb@medic@standing@kneel@enter", "enter", 8.0f, -1, AnimationFlags.AllowRotation);
                    ts.AddTask.PlayAnimation("amb@medic@standing@tendtodead@idle_a", "idle_c", 8.0f, -1, AnimationFlags.AllowRotation);
                    ts.AddTask.PlayAnimation("amb@medic@standing@tendtodead@exit", "exit", 8.0f, -1, AnimationFlags.Loop);
                    ts.Close();

                    p.Task.PerformSequence(ts);
                    ts.Dispose();

                    break;
                }
            }
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

                if (members[i].IsInRangeOf(targetPosition, 50.0f)) SetPedAsMedic(members[i]);

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
                    if (Util.ThereIs(p))
                    {
                        p.AlwaysKeepTask = false;
                        p.BlockPermanentEvents = false;
                        p.MarkAsNoLongerNeeded();
                    }
                }

                if (Util.ThereIs(spawnedVehicle)) spawnedVehicle.MarkAsNoLongerNeeded();

                members.Clear();
                return true;
            }

            return false;
        }
    }
}