using GTA;
using GTA.Math;
using GTA.Native;

namespace AdvancedWorld
{
    public class AggressiveDriver : EntitySet
    {
        private string name;

        public AggressiveDriver(string name) : base()
        {
            this.name = name;
        }

        public bool IsCreatedIn(float radius)
        {
            Vector3 safePosition = Util.GetSafePositionIn(radius);

            if (safePosition.Equals(Vector3.Zero)) return false;

            spawnedVehicle = Util.Create(name, World.GetNextPositionOnStreet(safePosition.Around(10.0f), true), Util.GetRandomInt(360));

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
            spawnedPed.Task.CruiseWithVehicle(spawnedVehicle, 100.0f, (int)DrivingStyle.AvoidTrafficExtremely);

            if (!Util.BlipIsOn(spawnedPed))
            {
                Util.AddBlipOn(spawnedPed, 0.7f, BlipSprite.PersonalVehicleCar, BlipColor.Green, "Aggressive " + spawnedVehicle.FriendlyName);
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
