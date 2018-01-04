using GTA;
using GTA.Math;
using GTA.Native;

namespace AdvancedWorld
{
    public class ReplacedVehicle : EntitySet
    {
        private string name;

        public ReplacedVehicle(string name) : base()
        {
            this.name = name;
        }

        public bool IsCreatedIn(float radius)
        {
            Vehicle[] nearbyVehicles = World.GetNearbyVehicles(Game.Player.Character.Position, radius);

            if (nearbyVehicles.Length <= 0) return false;

            Vehicle selectedVehicle = nearbyVehicles[Util.GetRandomInt(nearbyVehicles.Length)];

            if (Util.WeCanReplace(selectedVehicle) && !selectedVehicle.IsPersistent && !Function.Call<bool>(Hash.IS_VEHICLE_ATTACHED_TO_TRAILER, selectedVehicle) && (!selectedVehicle.IsOnScreen || Util.SomethingIsBetween(selectedVehicle)))
            {
                Vector3 selectedPosition = selectedVehicle.Position;
                float selectedHeading = selectedVehicle.Heading;
                float selectedSpeed = selectedVehicle.Speed;
                bool selectedEngineRunning = selectedVehicle.EngineRunning;
                string selectedBlipName;
                BlipColor selectedBlipColor;

                selectedVehicle.Delete();
                spawnedVehicle = Util.Create(name, selectedPosition, selectedHeading);

                if (!Util.ThereIs(spawnedVehicle)) return false;
                if (selectedEngineRunning)
                {
                    Ped driver = spawnedVehicle.CreateRandomPedOnSeat(VehicleSeat.Driver);

                    if (Util.ThereIs(driver))
                    {
                        spawnedVehicle.EngineRunning = true;
                        driver.MarkAsNoLongerNeeded();
                    }
                }

                if (Util.GetRandomInt(3) == 1)
                {
                    selectedBlipName = "Tuned ";
                    selectedBlipColor = BlipColor.Blue;
                    Util.Tune(spawnedVehicle, true);
                }
                else
                {
                    selectedBlipName = "";
                    selectedBlipColor = BlipColor.White;
                }

                if (spawnedVehicle.FriendlyName.Equals("NULL")) selectedBlipName += spawnedVehicle.DisplayName.ToUpper();
                else selectedBlipName += spawnedVehicle.FriendlyName;

                if (!Util.BlipIsOn(spawnedVehicle))
                {
                    Util.AddBlipOn(spawnedVehicle, 0.7f, BlipSprite.PersonalVehicleCar, selectedBlipColor, selectedBlipName);
                    return true;
                }
                else spawnedVehicle.Delete();
            }

            return false;
        }

        public override bool ShouldBeRemoved()
        {
            if (!Util.ThereIs(spawnedVehicle)) return true;
            if ((!spawnedVehicle.IsDriveable && (!Util.ThereIs(spawnedVehicle.Driver) || !Game.Player.Character.Equals(spawnedVehicle.Driver))) || !spawnedVehicle.IsInRangeOf(Game.Player.Character.Position, 200.0f))
            {
                if (Util.BlipIsOn(spawnedVehicle)) spawnedVehicle.CurrentBlip.Remove();
                if (spawnedVehicle.IsPersistent) spawnedVehicle.MarkAsNoLongerNeeded();
                
                return true;
            }

            return false;
        }
    }
}
