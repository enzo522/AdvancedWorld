﻿using GTA;
using GTA.Math;
using GTA.Native;

namespace AdvancedWorld
{
    public class AggressiveDriver : EntitySet
    {
        private string name;
        private int relationship;

        public AggressiveDriver(string name) : base()
        {
            this.name = name;
            this.relationship = 0;
        }

        public bool IsCreatedIn(float radius)
        {
            Vector3 safePosition = Util.GetSafePositionIn(radius);

            if (safePosition.Equals(Vector3.Zero)) return false;

            spawnedVehicle = Util.Create(name, World.GetNextPositionOnStreet(safePosition, true), Util.GetRandomInt(360));

            if (!Util.ThereIs(spawnedVehicle)) return false;

            spawnedPed = spawnedVehicle.CreateRandomPedOnSeat(VehicleSeat.Driver);

            if (!Util.ThereIs(spawnedPed))
            {
                spawnedVehicle.Delete();
                return false;
            }

            Function.Call(Hash.SET_DRIVER_ABILITY, spawnedPed, 1.0f);
            Function.Call(Hash.SET_DRIVER_AGGRESSIVENESS, spawnedPed, 1.0f);
            Util.Tune(spawnedVehicle, true, true);
            relationship = AdvancedWorld.NewRelationship(0);

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
            if (relationship != 0) AdvancedWorld.CleanUpRelationship(spawnedPed.RelationshipGroup);
        }

        public override bool ShouldBeRemoved()
        {
            if (!Util.ThereIs(spawnedPed))
            {
                if (Util.ThereIs(spawnedVehicle) && spawnedVehicle.IsPersistent) spawnedVehicle.MarkAsNoLongerNeeded();
                return true;
            }

            if (!Util.ThereIs(spawnedVehicle))
            {
                if (Util.BlipIsOn(spawnedPed)) spawnedPed.CurrentBlip.Remove();
                if (spawnedPed.IsPersistent) spawnedPed.MarkAsNoLongerNeeded();

                AdvancedWorld.CleanUpRelationship(spawnedPed.RelationshipGroup);
                return true;
            }

            if (spawnedVehicle.IsUpsideDown && spawnedVehicle.IsStopped) spawnedVehicle.PlaceOnGround();
            if (spawnedPed.IsDead || !spawnedVehicle.IsDriveable || !spawnedPed.IsInRangeOf(Game.Player.Character.Position, 500.0f))
            {
                if (Util.BlipIsOn(spawnedPed)) spawnedPed.CurrentBlip.Remove();
                if (spawnedPed.IsPersistent) spawnedPed.MarkAsNoLongerNeeded();
                if (spawnedVehicle.IsPersistent) spawnedVehicle.MarkAsNoLongerNeeded();

                AdvancedWorld.CleanUpRelationship(spawnedPed.RelationshipGroup);
                return true;
            }

            return false;
        }
    }
}