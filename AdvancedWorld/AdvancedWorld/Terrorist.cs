﻿using GTA;
using GTA.Math;

namespace AdvancedWorld
{
    public class Terrorist : Criminal
    {
        private string name;

        public Terrorist(string name) : base(AdvancedWorld.CrimeType.Terrorist)
        {
            this.name = name;
        }

        public bool IsCreatedIn(float radius)
        {
            Vector3 safePosition = Util.GetSafePositionIn(radius);

            if (safePosition.Equals(Vector3.Zero)) return false;

            Vector3 position = World.GetNextPositionOnStreet(safePosition, true);

            if (position.Equals(Vector3.Zero)) return false;

            spawnedVehicle = Util.Create(name, position, Util.GetRandomInt(360), true);

            if (!Util.ThereIs(spawnedVehicle)) return false;

            spawnedPed = spawnedVehicle.CreateRandomPedOnSeat(VehicleSeat.Driver);

            if (!Util.ThereIs(spawnedPed))
            {
                spawnedVehicle.Delete();
                return false;
            }

            Script.Wait(50);
            Util.Tune(spawnedVehicle, false, false);
            relationship = Util.NewRelationship(AdvancedWorld.CrimeType.Terrorist);

            spawnedPed.RelationshipGroup = relationship;
            spawnedPed.AlwaysKeepTask = true;
            spawnedPed.BlockPermanentEvents = true;
            spawnedPed.Task.FightAgainstHatedTargets(400.0f);

            if (!Util.BlipIsOn(spawnedPed))
            {
                Util.AddBlipOn(spawnedPed, 0.7f, BlipSprite.Tank, BlipColor.Red, "Terrorist " + spawnedVehicle.FriendlyName);
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

            if (Util.ThereIs(spawnedPed)) CheckDispatch();

            return false;
        }
    }
}