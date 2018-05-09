using GTA;
using GTA.Math;
using GTA.Native;

namespace AdvancedWorld
{
    public class AggressiveDriver : Nitroable, IBlockable
    {
        private string name;
        private int blockCooldown;

        public AggressiveDriver(string name) : base(AdvancedWorld.CrimeType.AggressiveDriver)
        {
            this.name = name;
            this.blockCooldown = 15;
        }

        public bool IsCreatedIn(float radius)
        {
            Vector3 safePosition = Util.GetSafePositionIn(radius);

            if (safePosition.Equals(Vector3.Zero)) return false;
            
            Road road = Util.GetNextPositionOnStreetWithHeading(safePosition);

            if (road.Position.Equals(Vector3.Zero)) return false;

            spawnedVehicle = Util.Create(name, road.Position, road.Heading, true);

            if (!Util.ThereIs(spawnedVehicle)) return false;
            
            spawnedPed = spawnedVehicle.CreateRandomPedOnSeat(VehicleSeat.Driver);

            if (!Util.ThereIs(spawnedPed))
            {
                spawnedVehicle.Delete();
                return false;
            }

            Script.Wait(50);
            Function.Call(Hash.SET_DRIVER_ABILITY, spawnedPed, 1.0f);
            Function.Call(Hash.SET_DRIVER_AGGRESSIVENESS, spawnedPed, 1.0f);
            Util.Tune(spawnedVehicle, true, true);
            relationship = Util.NewRelationship(AdvancedWorld.CrimeType.AggressiveDriver);

            if (relationship == 0)
            {
                Restore();
                return false;
            }

            spawnedPed.RelationshipGroup = relationship;
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
                Restore();
                return false;
            }
        }

        public override void Restore()
        {
            if (Util.ThereIs(spawnedPed)) spawnedPed.Delete();
            if (Util.ThereIs(spawnedVehicle)) spawnedVehicle.Delete();
            if (relationship != 0) Util.CleanUpRelationship(spawnedPed.RelationshipGroup);
        }

        public override bool ShouldBeRemoved()
        {
            if (!Util.ThereIs(spawnedPed))
            {
                if (Util.ThereIs(spawnedVehicle) && spawnedVehicle.IsPersistent) spawnedVehicle.MarkAsNoLongerNeeded();
                if (relationship != 0) Util.CleanUpRelationship(relationship);

                return true;
            }

            if (!Util.ThereIs(spawnedVehicle))
            {
                if (Util.BlipIsOn(spawnedPed)) spawnedPed.CurrentBlip.Remove();
                if (spawnedPed.IsPersistent) spawnedPed.MarkAsNoLongerNeeded();
                if (relationship != 0) Util.CleanUpRelationship(relationship);

                return true;
            }
            
            if (spawnedPed.IsDead || !spawnedVehicle.IsDriveable || !spawnedPed.IsInRangeOf(Game.Player.Character.Position, 500.0f))
            {
                if (Util.BlipIsOn(spawnedPed)) spawnedPed.CurrentBlip.Remove();
                if (spawnedPed.IsPersistent) spawnedPed.MarkAsNoLongerNeeded();
                if (spawnedVehicle.IsPersistent) spawnedVehicle.MarkAsNoLongerNeeded();
                if (relationship != 0) Util.CleanUpRelationship(relationship);

                return true;
            }

            if (spawnedVehicle.IsUpsideDown && spawnedVehicle.IsStopped) spawnedVehicle.PlaceOnGround();
            if (Util.ThereIs(spawnedPed))
            {
                CheckDispatch();
                CheckBlockable();
            }

            return false;
        }

        public void CheckBlockable()
        {
            if (blockCooldown < 15) blockCooldown++;
            else
            {
                blockCooldown = 0;
                AdvancedWorld.BlockRoadAgainst(spawnedPed, AdvancedWorld.CrimeType.AggressiveDriver);
            }
        }
    }
}