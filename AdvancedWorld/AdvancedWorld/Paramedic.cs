using GTA;
using GTA.Math;
using GTA.Native;
using System.Collections.Generic;

namespace AdvancedWorld
{
    public class Paramedic : EmergencyFire
    {
        private List<DeadPed> deadPeds;

        public Paramedic(string name, Entity target) : base(name, target) { this.deadPeds = new List<DeadPed>(); }

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

        protected override void SetPedOnDuty(Ped p)
        {
            if (p.IsRunning || Function.Call<bool>(Hash.GET_IS_TASK_ACTIVE, p, 134)) return;

            Ped[] nearbyPeds = World.GetNearbyPeds(p, 100.0f);

            if (nearbyPeds.Length < 1) return;

            foreach (Ped selectedPed in nearbyPeds)
            {
                if (Util.ThereIs(selectedPed) && (selectedPed.IsDead || selectedPed.IsInjured))
                {
                    bool thisIsNewPed = true;

                    foreach (DeadPed dp in deadPeds)
                    {
                        if (dp.Equals(selectedPed))
                        {
                            thisIsNewPed = false;
                            break;
                        }
                    }

                    if (thisIsNewPed) deadPeds.Add(new DeadPed(selectedPed));
                }
            }

            foreach (DeadPed dp in deadPeds)
            {
                if (!dp.IsChecked)
                {
                    TaskSequence ts = new TaskSequence();
                    ts.AddTask.RunTo(dp.Position.Around(0.5f));
                    ts.AddTask.LookAt(dp.Position, 1000);
                    ts.AddTask.PlayAnimation("amb@medic@standing@kneel@enter", "enter");
                    ts.AddTask.PlayAnimation("amb@medic@standing@tendtodead@idle_a", "idle_c");
                    ts.AddTask.PlayAnimation("amb@medic@standing@tendtodead@exit", "exit");
                    ts.AddTask.PlayAnimation("amb@medic@standing@timeofdeath@exit", "exit");
                    ts.Close();

                    p.Task.PerformSequence(ts);
                    ts.Dispose();

                    return;
                }

                dp.IsChecked = AnyParamedicIsNear(dp);
            }
            
            deadPeds.Clear();
            SetPedsOffDuty();
        }

        private bool AnyParamedicIsNear(DeadPed dp)
        {
            foreach (Ped p in members)
            {
                if (p.IsInRangeOf(dp.Position, 0.5f)) return true;
            }

            return false;
        }
    }
}