using GTA;
using System.Collections.Generic;

namespace AdvancedWorld
{
    public class Paramedic : EmergencyFire
    {
        private List<Entity> checkedPeds;

        public Paramedic(string name, Entity target) : base(name, target, "MEDIC") { this.checkedPeds = new List<Entity>(); }

        private new void SetPedsOnDuty()
        {
            if (TargetIsFound() && target.Model.IsPed)
            {
                foreach (Ped p in members)
                {
                    if (p.TaskSequenceProgress < 0)
                    {
                        TaskSequence ts = new TaskSequence();
                        ts.AddTask.RunTo(target.Position.Around(1.0f));
                        ts.AddTask.LookAt(target.Position, 1000);
                        ts.AddTask.PlayAnimation("amb@medic@standing@kneel@enter", "enter");
                        ts.AddTask.PlayAnimation("amb@medic@standing@tendtodead@idle_a", "idle_c");
                        ts.AddTask.PlayAnimation("amb@medic@standing@tendtodead@exit", "exit");
                        ts.AddTask.PlayAnimation("amb@medic@standing@timeofdeath@exit", "exit");
                        ts.AddTask.Wait(1000);
                        ts.Close();

                        p.Task.PerformSequence(ts);
                        ts.Dispose();
                    }
                    else if (p.TaskSequenceProgress == 6 && !checkedPeds.Contains(target)) checkedPeds.Add(target);
                }
            }
        }

        private new bool TargetIsFound()
        {
            Ped[] nearbyPeds = World.GetNearbyPeds(spawnedVehicle.Position, 100.0f);

            if (nearbyPeds.Length < 1) return false;

            foreach (Ped selectedPed in nearbyPeds)
            {
                if (Util.ThereIs(selectedPed) && (selectedPed.IsDead || selectedPed.IsInjured))
                {
                    if (!checkedPeds.Contains(selectedPed))
                    {
                        target = selectedPed;
                        return true;
                    }
                }
                else target = null;
            }

            return false;
        }
    }
}