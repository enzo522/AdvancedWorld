using GTA;
using GTA.Math;
using GTA.Native;
using System.Collections.Generic;

namespace AdvancedWorld
{
    public abstract class EntitySet : AdvancedEntity
    {
        protected Ped spawnedPed;
        protected Vehicle spawnedVehicle;

        public EntitySet() { }

        protected bool ReadyToGoWith(List<Ped> members)
        {
            bool result = true;

            foreach (Ped p in members)
            {
                if (Util.ThereIs(p))
                {
                    if (p.IsDead)
                    {
                        if (p.Equals(spawnedVehicle.Driver) && spawnedVehicle.IsStopped)
                        {
                            spawnedVehicle.OpenDoor(VehicleDoor.FrontLeftDoor, false, true);
                            Script.Wait(100);
                            Vector3 offset = p.Position + p.RightVector * (-1.01f);
                            p.Position = new Vector3(offset.X, offset.Y, offset.Z - 1.0f);
                        }

                        Util.NaturallyRemove(p);
                    }
                    else if (!p.IsSittingInVehicle(spawnedVehicle)) result = false;
                }
            }

            return result;
        }

        protected bool VehicleSeatsCanBeSeatedBy(List<Ped> members)
        {
            if (!Util.ThereIs(spawnedVehicle)) return false;

            int startingSeat = 0;

            if (Util.ThereIs(spawnedVehicle.Driver) && Util.WeCanGiveTaskTo(spawnedVehicle.Driver)) Function.Call(Hash.TASK_VEHICLE_TEMP_ACTION, spawnedVehicle.Driver, spawnedVehicle, 1, 1000);
            else startingSeat = -1;

            for (int i = startingSeat, j = 0; j < members.Count; j++)
            { 
                if (Util.ThereIs(members[j]) && members[j].IsOnFoot && Util.WeCanGiveTaskTo(members[j]))
                {
                    if (!Function.Call<bool>(Hash.GET_IS_TASK_ACTIVE, members[j], 160))
                    {
                        while (!spawnedVehicle.IsSeatFree((VehicleSeat)i) && !spawnedVehicle.GetPedOnSeat((VehicleSeat)i).IsDead)
                        {
                            if (++i >= spawnedVehicle.PassengerSeats) return false;
                        }

                        members[j].Task.EnterVehicle(spawnedVehicle, (VehicleSeat)i++, -1, 2.0f, 1);
                    }
                    else if (Function.Call<bool>(Hash.GET_IS_TASK_ACTIVE, members[j], 195))
                    {
                        if ((spawnedVehicle.Model.IsBicycle || spawnedVehicle.Model.IsBike || spawnedVehicle.Model.IsQuadbike) &&
                            spawnedVehicle.IsUpsideDown && spawnedVehicle.IsStopped && !spawnedVehicle.PlaceOnGround()) spawnedVehicle.PlaceOnNextStreet();

                        if (spawnedVehicle.IsSeatFree((VehicleSeat)Function.Call<int>(Hash.GET_SEAT_PED_IS_TRYING_TO_ENTER, members[j])))
                            members[j].SetIntoVehicle(spawnedVehicle, (VehicleSeat)Function.Call<int>(Hash.GET_SEAT_PED_IS_TRYING_TO_ENTER, members[j]));
                        else members[j].Task.ClearAllImmediately();
                    }
                }
            }

            return true;
        }
    }
}