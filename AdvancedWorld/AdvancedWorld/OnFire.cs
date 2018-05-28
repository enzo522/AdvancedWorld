﻿using GTA;

namespace AdvancedWorld
{
    public class OnFire : AdvancedEntity
    {
        public Vehicle OnFireVehicle { get; private set; }

        public OnFire() { }

        public bool IsCreatedIn(float radius, bool instantly)
        {
            Vehicle[] nearbyVehicles = World.GetNearbyVehicles(Game.Player.Character.Position, radius);

            if (nearbyVehicles.Length < 1) return false;

            for (int trycount = 0; trycount < 5; trycount++)
            {
                Vehicle selectedVehicle = nearbyVehicles[Util.GetRandomInt(nearbyVehicles.Length)];

                if (Util.WeCanReplace(selectedVehicle))
                {
                    OnFireVehicle = selectedVehicle;

                    if (Util.BlipIsOn(OnFireVehicle))
                    {
                        OnFireVehicle.CurrentBlip.Remove();
                        Script.Wait(100);
                    }

                    OnFireVehicle.IsPersistent = true;

                    if (instantly)
                    {
                        Util.AddBlipOn(OnFireVehicle, 0.7f, BlipSprite.PersonalVehicleCar, BlipColor.Red, "Vehicle Explosion");
                        OnFireVehicle.Explode();
                    }
                    else
                    {
                        Util.AddBlipOn(OnFireVehicle, 0.7f, BlipSprite.PersonalVehicleCar, BlipColor.Yellow, "Vehicle on Fire");
                        OnFireVehicle.EngineHealth = -900.0f;
                        OnFireVehicle.IsDriveable = false;
                    }

                    break;
                }
            }

            return true;
        }

        public override void Restore(bool instantly)
        {
            Util.NaturallyRemove(OnFireVehicle);
        }

        public override bool ShouldBeRemoved()
        {
            if (!Util.ThereIs(OnFireVehicle) || !OnFireVehicle.IsOnFire || !OnFireVehicle.IsInRangeOf(Game.Player.Character.Position, 500.0f))
            {
                Restore(false);
                return true;
            }

            return false;
        }
    }
}