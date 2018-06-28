using GTA;
using GTA.Math;
using GTA.Native;
using System.Collections.Generic;

namespace YouAreNotAlone
{
    public class OnFire : AdvancedEntity
    {
        public Vehicle OnFireVehicle { get; private set; }

        private int dispatchCooldown;
        private bool instantly;

        public OnFire()
        {
            this.dispatchCooldown = 10;
            this.instantly = false;
            Logger.Write(true, "OnFire event selected.", "");
        }

        public bool IsCreatedIn(float radius, bool instantly)
        {
            Vehicle[] nearbyVehicles = World.GetNearbyVehicles(Game.Player.Character.Position, radius);

            if (nearbyVehicles.Length < 1)
            {
                Logger.Error("OnFire: There is no vehicle near.", "");

                return false;
            }

            this.instantly = instantly;

            for (int trycount = 0; trycount < 5; trycount++)
            {
                Vehicle selectedVehicle = nearbyVehicles[Util.GetRandomIntBelow(nearbyVehicles.Length)];

                if (Util.WeCanReplace(selectedVehicle))
                {
                    Logger.Write(false, "OnFire: Found a proper vehicle.", "");
                    OnFireVehicle = selectedVehicle;
                    OnFireVehicle.IsPersistent = true;

                    if (this.instantly)
                    {
                        Logger.Write(false, "OnFire: Time to explode selected vehicle.", "");
                        OnFireVehicle.Explode();
                    }
                    else
                    {
                        Logger.Write(false, "OnFire: Time to set selected vehicle on fire.", "");
                        OnFireVehicle.EngineHealth = -900.0f;
                        OnFireVehicle.IsDriveable = false;
                    }

                    break;
                }
            }

            return true;
        }

        private bool AnyFireNear()
        {
            OutputArgument outPos = new OutputArgument();

            if (Function.Call<bool>(Hash.GET_CLOSEST_FIRE_POS, outPos, OnFireVehicle.Position.X, OnFireVehicle.Position.Y, OnFireVehicle.Position.Z))
            {
                Vector3 position = outPos.GetResult<Vector3>();

                if (!position.Equals(Vector3.Zero) && OnFireVehicle.IsInRangeOf(position, 200.0f))
                {
                    Logger.Write(false, "OnFire: Found fire position.", "");

                    return true;
                }
            }
            
            if (Util.ThereIs(new List<Entity>(World.GetNearbyEntities(OnFireVehicle.Position, 200.0f)).Find(e => Util.ThereIs(e) && e.IsOnFire)))
            {
                Logger.Write(false, "OnFire: Found entity on fire.", "");

                return true;
            }

            Logger.Write(false, "OnFire: There is no fire near.", "");

            return false;
        }

        public override void Restore(bool instantly)
        {
            Logger.Write(false, "OnFire: Restore naturally.", "");
            Util.NaturallyRemove(OnFireVehicle);
        }

        public override bool ShouldBeRemoved()
        {
            if (!Util.ThereIs(OnFireVehicle) || !AnyFireNear() || !OnFireVehicle.IsInRangeOf(Game.Player.Character.Position, 500.0f))
            {
                Logger.Write(false, "OnFire: On fire vehicle need to be restored.", "");
                Restore(false);

                return true;
            }

            if (!Util.BlipIsOn(OnFireVehicle))
            {
                if (instantly) Util.AddBlipOn(OnFireVehicle, 0.7f, BlipSprite.PersonalVehicleCar, BlipColor.Red, "Vehicle Explosion");
                else Util.AddBlipOn(OnFireVehicle, 0.7f, BlipSprite.PersonalVehicleCar, BlipColor.Yellow, "Vehicle on Fire");
            }

            if (dispatchCooldown < 15) dispatchCooldown++;
            else
            {
                Main.DispatchAgainst(OnFireVehicle, EventManager.EventType.Fire);
                Logger.Write(false, "OnFire: Dispatch against", "Fire");
                dispatchCooldown = 0;
            }

            return false;
        }
    }
}