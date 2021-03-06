﻿using GTA;
using GTA.Math;
using GTA.Native;

namespace YouAreNotAlone
{
    public class ReplacedVehicle : EntitySet
    {
        private string name;

        public ReplacedVehicle(string name) : base()
        {
            this.name = name;
            Logger.Write(true, "ReplacedVehicle event selected.", this.name);
        }

        public bool IsCreatedIn(float radius)
        {
            Vehicle[] nearbyVehicles = World.GetNearbyVehicles(Game.Player.Character.Position, radius);

            if (nearbyVehicles.Length < 1)
            {
                Logger.Error("ReplacedVehicle: There is no vehicle near. Abort.", name);

                return false;
            }

            for (int trycount = 0; trycount < 5; trycount++)
            {
                Vehicle selectedVehicle = nearbyVehicles[Util.GetRandomIntBelow(nearbyVehicles.Length)];

                if (Util.WeCanReplace(selectedVehicle) && !selectedVehicle.IsPersistent && !selectedVehicle.IsAttached() && !Util.ThereIs(selectedVehicle.GetEntityAttachedTo()) && Util.SomethingIsBetweenPlayerAnd(selectedVehicle))
                {
                    Logger.Write(false, "ReplacedVehicle: Replaceable vehicle found.", name);
                    Vector3 selectedPosition = selectedVehicle.Position;
                    float selectedHeading = selectedVehicle.Heading;
                    float selectedSpeed = selectedVehicle.Speed;
                    bool driverIsNeeded = Util.ThereIs(selectedVehicle.Driver);
                    string blipName;
                    BlipColor blipColor;

                    selectedVehicle.Delete();
                    spawnedVehicle = Util.Create(name, selectedPosition, selectedHeading, true);

                    if (!Util.ThereIs(spawnedVehicle))
                    {
                        Logger.Write(false, "ReplacedVehicle: Couldn't create selected vehicle. Abort.", name);
                        Restore(true);

                        continue;
                    }

                    Logger.Write(false, "ReplacedVehicle: Created vehicle.", name);

                    if (driverIsNeeded)
                    {
                        spawnedPed = spawnedVehicle.CreateRandomPedOnSeat(VehicleSeat.Driver);

                        if (Util.ThereIs(spawnedPed))
                        {
                            Script.Wait(50);
                            spawnedVehicle.EngineRunning = true;
                            spawnedVehicle.Speed = selectedSpeed;
                            spawnedPed.RelationshipGroup = Function.Call<int>(Hash.GET_HASH_KEY, "CIV" + spawnedPed.Gender.ToString().ToUpper());
                            spawnedPed.Task.CruiseWithVehicle(spawnedVehicle, 20.0f, (int)DrivingStyle.Normal);
                            spawnedPed.MarkAsNoLongerNeeded();
                            Logger.Write(false, "ReplacedVehicle: Created driver.", name);
                        }
                        else Logger.Write(false, "ReplacedVehicle: Couldn't create driver in replacing vehicle.", name);
                    }

                    if (Util.GetRandomIntBelow(3) == 1)
                    {
                        blipName = "Tuned ";
                        blipColor = BlipColor.Blue;
                        Util.Tune(spawnedVehicle, Util.GetRandomIntBelow(2) == 1, Util.GetRandomIntBelow(2) == 1, false);
                        Logger.Write(false, "ReplacedVehicle: Tune replacing vehicle.", name);
                    }
                    else
                    {
                        blipName = "";
                        blipColor = BlipColor.White;
                        Logger.Write(false, "ReplacedVehicle: Remain stock replacing vehicle.", name);
                    }
                    
                    if (Util.BlipIsOn(spawnedVehicle))
                    {
                        Logger.Error("ReplacedVehicle: Blip is already on replacing vehicle. Abort.", name);
                        Restore(true);
                    }
                    else
                    {
                        Util.AddBlipOn(spawnedVehicle, 0.7f, BlipSprite.PersonalVehicleCar, blipColor, blipName + VehicleInfo.GetNameOf(spawnedVehicle.Model.Hash));
                        Logger.Write(false, "ReplacedVehicle: Create replacing vehicle successfully.", name);

                        return true;
                    }
                }
            }

            Restore(true);

            return false;
        }

        public override void Restore(bool instantly)
        {
            if (instantly)
            {
                Logger.Write(false, "ReplacedVehicle: Restore instantly.", name);

                if (Util.ThereIs(spawnedPed)) spawnedPed.Delete();
                if (Util.ThereIs(spawnedVehicle)) spawnedVehicle.Delete();
            }
            else
            {
                Logger.Write(false, "ReplacedVehicle: Restore naturally.", name);
                Util.NaturallyRemove(spawnedPed);
                Util.NaturallyRemove(spawnedVehicle);
            }
        }

        public override bool ShouldBeRemoved()
        {
            if (!Util.WeCanEnter(spawnedVehicle) || !spawnedVehicle.IsInRangeOf(Game.Player.Character.Position, 200.0f))
            {
                Logger.Write(false, "ReplacedVehicle: Replaced vehicle need to be restored.", name);

                return true;
            }

            return false;
        }
    }
}