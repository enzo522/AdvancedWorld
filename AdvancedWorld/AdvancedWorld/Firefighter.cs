using GTA;
using GTA.Math;
using GTA.Native;

namespace YouAreNotAlone
{
    public class Firefighter : EmergencyFire
    {
        public Firefighter(string name, Entity target) : base(name, target, "FIREMAN") { Logger.Write("Firefighter: Time to put off fires.", emergencyType + " " + name); }

        protected override void SetPedsOnDuty(bool onVehicleDuty)
        {
            if (onVehicleDuty)
            {
                if (ReadyToGoWith(members))
                {
                    if (Util.ThereIs(spawnedVehicle.Driver) && Util.WeCanGiveTaskTo(spawnedVehicle.Driver))
                    {
                        Logger.Write("Firefighter: Time to go with vehicle.", name);

                        if (spawnedVehicle.HasSiren && !spawnedVehicle.SirenActive) spawnedVehicle.SirenActive = true;
                        if (!Main.NoBlipOnDispatch) AddEmergencyBlip(true);

                        spawnedVehicle.Driver.Task.DriveTo(spawnedVehicle, targetPosition, 10.0f, 100.0f, 262708); // 4 + 16 + 32 + 512 + 262144
                    }
                    else
                    {
                        Logger.Write("Firefighter: There is no driver when on duty. Re-enter everyone.", emergencyType + " " + name);

                        foreach (Ped p in members)
                        {
                            if (Util.WeCanGiveTaskTo(p)) p.Task.LeaveVehicle(spawnedVehicle, false);
                        }
                    }
                }
                else
                {
                    if (!VehicleSeatsCanBeSeatedBy(members))
                    {
                        Logger.Write("Firefighter: Something wrong with assigning seats when on duty. Re-enter everyone.", emergencyType + " " + name);

                        foreach (Ped p in members)
                        {
                            if (Util.WeCanGiveTaskTo(p)) p.Task.LeaveVehicle(spawnedVehicle, false);
                        }
                    }
                    else Logger.Write("Firefighter: Assigned seats successfully when on duty.", emergencyType + " " + name);
                }
            }
            else
            {
                Logger.Write("Firefighter: Time to put off fires.", emergencyType + " " + name);

                if (!Main.NoBlipOnDispatch) AddEmergencyBlip(false);

                foreach (Ped p in members)
                {
                    if (p.TaskSequenceProgress < 0 && Util.WeCanGiveTaskTo(p))
                    {
                        TaskSequence ts = new TaskSequence();
                        ts.AddTask.RunTo(targetPosition.Around(3.0f));
                        ts.AddTask.ShootAt(targetPosition, 10000, FiringPattern.FullAuto);
                        ts.Close();

                        p.Task.PerformSequence(ts);
                        ts.Dispose();
                    }
                }
            }
        }

        protected override bool TargetIsFound()
        {
            target = null;
            targetPosition = Vector3.Zero;
            OutputArgument outPos = new OutputArgument();

            if (Function.Call<bool>(Hash.GET_CLOSEST_FIRE_POS, outPos, spawnedVehicle.Position.X, spawnedVehicle.Position.Y, spawnedVehicle.Position.Z))
            {
                Vector3 position = outPos.GetResult<Vector3>();

                if (!position.Equals(Vector3.Zero) && spawnedVehicle.IsInRangeOf(position, 200.0f))
                {
                    Logger.Write("Firefighter: Found fire position.", emergencyType + " " + name);
                    targetPosition = position;

                    return true;
                }
            }

            Logger.Write("Firefighter: Couldn't find fire position. Try to find entity on fire.", emergencyType + " " + name);
            Entity[] nearbyEntities = World.GetNearbyEntities(spawnedVehicle.Position, 200.0f);

            if (nearbyEntities.Length < 1)
            {
                Logger.Write("Firefighter: There is no fire near.", emergencyType + " " + name);

                return false;
            }

            foreach (Entity en in nearbyEntities)
            {
                if (Util.ThereIs(en) && en.IsOnFire)
                {
                    target = en;
                    targetPosition = target.Position;
                    Logger.Write("Firefighter: Found entity on fire.", emergencyType + " " + name);

                    return true;
                }
            }

            Logger.Write("Firefighter: There is no fire near.", emergencyType + " " + name);

            return false;
        }
    }
}