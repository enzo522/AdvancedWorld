using GTA;
using GTA.Math;
using GTA.Native;
using System.Collections.Generic;

namespace AdvancedWorld
{
    public class Paramedic : EmergencyFire
    {
        private List<int> checkedPeds;
        private List<string> scenarios;

        public Paramedic(string name, Entity target) : base(name, target, "MEDIC")
        {
            this.checkedPeds = new List<int>();
            this.scenarios = new List<string>
            {
                "CODE_HUMAN_MEDIC_KNEEL",
                "CODE_HUMAN_MEDIC_TEND_TO_DEAD",
                "CODE_HUMAN_MEDIC_TIME_OF_DEATH"
            };
        }

        protected override void SetPedsOnDuty()
        {
            if (spawnedVehicle.HasSiren && !spawnedVehicle.SirenActive) spawnedVehicle.SirenActive = true;
            if (onVehicleDuty)
            {
                if (ReadyToGoWith(members))
                {
                    if (Util.ThereIs(spawnedVehicle.Driver) && Util.NewTaskCanBeDoneBy(spawnedVehicle.Driver)) spawnedVehicle.Driver.Task.DriveTo(spawnedVehicle, targetPosition, 10.0f, 100.0f, (int)DrivingStyle.AvoidTrafficExtremely);
                    else
                    {
                        foreach (Ped p in members)
                        {
                            if (Util.NewTaskCanBeDoneBy(p)) p.Task.LeaveVehicle(spawnedVehicle, false);
                        }
                    }
                }
                else
                {
                    if (!VehicleSeatsCanBeSeatedBy(members))
                    {
                        Restore(false);
                        return;
                    }
                }
            }
            else if (target.Model.IsPed)
            {
                foreach (Ped p in members)
                {
                    Vector3 dest = targetPosition.Around(1.0f);

                    if (p.TaskSequenceProgress < 0 && Util.NewTaskCanBeDoneBy(p))
                    {
                        TaskSequence ts = new TaskSequence();
                        ts.AddTask.RunTo(dest);
                        Function.Call(Hash.TASK_START_SCENARIO_AT_POSITION, 0, scenarios[Util.GetRandomIntBelow(scenarios.Count)], dest.X, dest.Y, dest.Z, (dest - targetPosition).ToHeading(), 0, 0, 1);
                        ts.AddTask.Wait(1000);
                        ts.Close();

                        p.Task.PerformSequence(ts);
                        ts.Dispose();
                    }
                    else if (p.TaskSequenceProgress == 2 && !checkedPeds.Contains(target.Handle)) checkedPeds.Add(target.Handle);
                }
            }
        }

        protected override bool TargetIsFound()
        {
            target = null;
            targetPosition = Vector3.Zero;
            Ped[] nearbyPeds = World.GetNearbyPeds(spawnedVehicle.Position, 200.0f);

            if (nearbyPeds.Length < 1) return false;

            foreach (Ped selectedPed in nearbyPeds)
            {
                if (Util.ThereIs(selectedPed) && selectedPed.IsDead)
                {
                    if (!checkedPeds.Contains(selectedPed.Handle))
                    {
                        target = selectedPed;
                        targetPosition = Function.Call<Vector3>(Hash.GET_PED_BONE_COORDS, (Ped)target, 11816, 0.0f, 0.0f, 0.0f);
                        return true;
                    }
                }
            }
            
            return false;
        }
    }
}