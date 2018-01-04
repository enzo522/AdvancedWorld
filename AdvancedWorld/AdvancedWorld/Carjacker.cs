using GTA;

namespace AdvancedWorld
{
    public class Carjacker : EntitySet
    {
        public Carjacker() : base() { }

        public bool IsCreatedIn(float radius)
        {
            Ped[] nearbyPeds = World.GetNearbyPeds(Game.Player.Character.Position, radius);

            if (nearbyPeds.Length <= 0) return false;

            Ped selectedPed = nearbyPeds[Util.GetRandomInt(nearbyPeds.Length)];

            if (!Util.ThereIs(selectedPed) || Util.BlipIsOn(selectedPed) || selectedPed.Equals(Game.Player.Character) || !selectedPed.IsHuman || selectedPed.IsDead) return false;

            Vehicle[] nearbyVehicles = World.GetNearbyVehicles(selectedPed.Position, radius / 5);

            if (nearbyVehicles.Length <= 0) return false;

            Vehicle selectedVehicle = nearbyVehicles[Util.GetRandomInt(nearbyVehicles.Length)];

            if (!Util.ThereIs(selectedVehicle) || !selectedVehicle.IsDriveable) return false;

            selectedPed.IsPersistent = true;
            selectedVehicle.IsPersistent = true;
            selectedVehicle.EngineRunning = true;

            selectedPed.RelationshipGroup = AdvancedWorld.racerID;
            selectedPed.AlwaysKeepTask = true;
            selectedPed.BlockPermanentEvents = true;

            TaskSequence ts = new TaskSequence();
            ts.AddTask.EnterVehicle(selectedVehicle, VehicleSeat.Driver);
            ts.AddTask.CruiseWithVehicle(selectedVehicle, 100.0f, (int)DrivingStyle.AvoidTrafficExtremely);
            ts.Close();

            selectedPed.Task.PerformSequence(ts);
            ts.Dispose();

            Util.AddBlipOn(selectedPed, 0.7f, BlipSprite.Masks, BlipColor.White, "Carjacker");
            spawnedPed = selectedPed;
            spawnedVehicle = selectedVehicle;
            return true;
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

            if (spawnedPed.IsDead || !spawnedVehicle.IsDriveable || !spawnedVehicle.IsInRangeOf(spawnedPed.Position, 200.0f) || !spawnedPed.IsInRangeOf(Game.Player.Character.Position, 500.0f))
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
