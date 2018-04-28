using GTA;
using GTA.Native;

namespace AdvancedWorld
{
    public class Carjacker : EntitySet
    {
        private float radius;
        private int trycount;
        private int relationship;

        public Carjacker() : base()
        {
            this.radius = 0.0f;
            this.trycount = 0;
            this.relationship = 0;
        }

        public bool IsCreatedIn(float radius)
        {
            Ped[] nearbyPeds = World.GetNearbyPeds(Game.Player.Character.Position, radius);

            if (nearbyPeds.Length < 1) return false;

            spawnedPed = nearbyPeds[Util.GetRandomInt(nearbyPeds.Length)];

            if (!Util.ThereIs(spawnedPed) || spawnedPed.IsPersistent || spawnedPed.Equals(Game.Player.Character) || !spawnedPed.IsHuman || spawnedPed.IsDead) return false;

            this.radius = radius;
            spawnedPed.IsPersistent = true;
            relationship = AdvancedWorld.NewRelationship(0);

            if (relationship == 0)
            {
                Restore();
                return false;
            }

            spawnedPed.RelationshipGroup = relationship;
            spawnedPed.AlwaysKeepTask = true;
            spawnedPed.BlockPermanentEvents = true;

            if (!Util.BlipIsOn(spawnedPed))
            {
                Util.AddBlipOn(spawnedPed, 0.7f, BlipSprite.Masks, BlipColor.White, "Carjacker");
                FindNewVehicle();

                return true;
            }
            else
            {
                Restore();
                return false;
            }
        }

        private void FindNewVehicle()
        {
            if (Util.ThereIs(spawnedVehicle) && spawnedVehicle.IsPersistent) spawnedVehicle.MarkAsNoLongerNeeded();

            trycount++;

            Vehicle[] nearbyVehicles = World.GetNearbyVehicles(spawnedPed.Position, radius / 2);

            if (nearbyVehicles.Length < 1) return;

            spawnedVehicle = nearbyVehicles[Util.GetRandomInt(nearbyVehicles.Length)];

            if (!Util.ThereIs(spawnedVehicle) || !spawnedVehicle.IsDriveable || Game.Player.Character.IsInVehicle(spawnedVehicle) || spawnedPed.IsInVehicle(spawnedVehicle))
            {
                spawnedVehicle = null;
                return;
            }

            spawnedVehicle.IsPersistent = true;

            TaskSequence ts = new TaskSequence();
            ts.AddTask.ClearAll();
            ts.AddTask.EnterVehicle(spawnedVehicle, VehicleSeat.Driver, -1, 2.0f, 1);
            ts.AddTask.CruiseWithVehicle(spawnedVehicle, 100.0f, (int)DrivingStyle.AvoidTrafficExtremely);
            ts.Close();

            spawnedPed.Task.PerformSequence(ts);
            ts.Dispose();
        }

        public override void Restore()
        {
            if (Util.ThereIs(spawnedPed)) spawnedPed.MarkAsNoLongerNeeded();
            if (Util.ThereIs(spawnedVehicle)) spawnedVehicle.MarkAsNoLongerNeeded();
            if (relationship != 0) AdvancedWorld.CleanUpRelationship(spawnedPed.RelationshipGroup);
        }

        public override bool ShouldBeRemoved()
        {
            if (!Util.ThereIs(spawnedPed))
            {
                if (Util.ThereIs(spawnedVehicle) && spawnedVehicle.IsPersistent) spawnedVehicle.MarkAsNoLongerNeeded();

                AdvancedWorld.CleanUpRelationship(spawnedPed.RelationshipGroup);
                return true;
            }

            if (!Util.ThereIs(spawnedVehicle) || !spawnedVehicle.IsDriveable || (spawnedVehicle.IsUpsideDown && spawnedVehicle.IsStopped) || !spawnedVehicle.IsInRangeOf(spawnedPed.Position, 100.0f)) FindNewVehicle();
            if (!Function.Call<bool>(Hash.GET_IS_TASK_ACTIVE, spawnedPed, 160)) radius += 50.0f;
            if (trycount > 5 || spawnedPed.IsDead || !spawnedPed.IsInRangeOf(Game.Player.Character.Position, 500.0f))
            {
                if (Util.BlipIsOn(spawnedPed)) spawnedPed.CurrentBlip.Remove();
                if (spawnedPed.IsPersistent) spawnedPed.MarkAsNoLongerNeeded();
                if (Util.ThereIs(spawnedVehicle) && spawnedVehicle.IsPersistent) spawnedVehicle.MarkAsNoLongerNeeded();

                AdvancedWorld.CleanUpRelationship(relationship);
                return true;
            }

            return false;
        }
    }
}