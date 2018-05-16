using GTA;

namespace AdvancedWorld
{
    public class Firefighter : EmergencyFire
    {
        public Firefighter(string name, Entity target) : base(name, target, "FIREMAN") { }
        
        protected override void SetPedsOnDuty()
        {
            Entity[] nearbyEntities = World.GetNearbyEntities(spawnedVehicle.Position, 100.0f);

            if (nearbyEntities.Length < 1) return;

            foreach (Entity en in nearbyEntities)
            {
                if (Util.ThereIs(en) && en.IsOnFire)
                {
                    target = en;

                    break;
                }
                else target = null;
            }

            if (Util.ThereIs(target))
            {
                foreach (Ped p in members)
                {
                    if (p.TaskSequenceProgress < 0)
                    {
                        TaskSequence ts = new TaskSequence();
                        ts.AddTask.RunTo(target.Position.Around(3.0f));
                        ts.AddTask.ShootAt(target.Position, 10000, FiringPattern.FullAuto);
                        ts.Close();

                        p.Task.PerformSequence(ts);
                        ts.Dispose();
                    }
                }
            }
        }
    }
}