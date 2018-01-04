using GTA;
using GTA.Math;
using GTA.Native;

namespace AdvancedWorld
{
    public class Racer : EntitySet
    {
        private string name;
        private Vector3 goal;

        public Racer(string name, Vector3 goal) : base()
        {
            this.name = name;
            this.goal = goal;
        }

        public bool IsCreatedIn(float radius, Vector3 safePosition)
        {
            spawnedVehicle = Util.Create(name, safePosition, Util.GetRandomInt(360));

            if (!Util.ThereIs(spawnedVehicle)) return false;

            spawnedPed = spawnedVehicle.CreateRandomPedOnSeat(VehicleSeat.Driver);

            if (!Util.ThereIs(spawnedPed))
            {
                spawnedVehicle.Delete();
                return false;
            }

            Function.Call(Hash.SET_DRIVER_ABILITY, spawnedPed, 1.0f);
            Function.Call(Hash.SET_DRIVER_AGGRESSIVENESS, spawnedPed, 1.0f);
            Util.Tune(spawnedVehicle, true);
            
            spawnedPed.RelationshipGroup = AdvancedWorld.racerID;
            spawnedPed.AlwaysKeepTask = true;
            spawnedPed.BlockPermanentEvents = true;

            if (!Util.BlipIsOn(spawnedPed))
            {
                if (spawnedVehicle.Model.IsCar)
                    Util.AddBlipOn(spawnedPed, 0.7f, BlipSprite.PersonalVehicleCar, (BlipColor)17, "Racer " + spawnedVehicle.FriendlyName);
                else
                {
                    if (!spawnedPed.IsWearingHelmet) spawnedPed.GiveHelmet(false, HelmetType.RegularMotorcycleHelmet, 4096);

                    Util.AddBlipOn(spawnedPed, 1.0f, BlipSprite.PersonalVehicleBike, (BlipColor)17, "Racer " + spawnedVehicle.FriendlyName);
                }

                TaskSequence ts = new TaskSequence();
                ts.AddTask.DriveTo(spawnedVehicle, goal, 10.0f, 100.0f, (int)DrivingStyle.AvoidTrafficExtremely);
                ts.AddTask.Wait(10000);
                ts.AddTask.CruiseWithVehicle(spawnedVehicle, 100.0f, (int)DrivingStyle.AvoidTrafficExtremely);
                ts.Close();

                spawnedPed.Task.PerformSequence(ts);
                ts.Dispose();
                return true;
            }
            else
            {
                spawnedPed.Delete();
                spawnedVehicle.Delete();
                return false;
            }
        }

        public override bool ShouldBeRemoved()
        {
            if (!Util.ThereIs(spawnedPed))
            {
                if (spawnedVehicle.IsPersistent) spawnedVehicle.MarkAsNoLongerNeeded();
                return true;
            }

            if (!Util.ThereIs(spawnedVehicle))
            {
                if (spawnedPed.IsPersistent) spawnedPed.MarkAsNoLongerNeeded();
                return true;
            }

            if (spawnedVehicle.IsUpsideDown && spawnedVehicle.IsStopped) spawnedVehicle.PlaceOnGround();
            if (spawnedPed.IsDead || !spawnedVehicle.IsDriveable || !spawnedPed.IsInRangeOf(Game.Player.Character.Position, 500.0f))
            {
                if (Util.BlipIsOn(spawnedPed)) spawnedPed.CurrentBlip.Remove();
                if (spawnedPed.IsPersistent) spawnedPed.MarkAsNoLongerNeeded();
                if (spawnedVehicle.IsPersistent) spawnedVehicle.MarkAsNoLongerNeeded();

                return true;
            }

            return false;
        }
    }
}
