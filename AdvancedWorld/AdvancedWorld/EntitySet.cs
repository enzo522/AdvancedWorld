using GTA;
using GTA.Native;
using System.Collections.Generic;

namespace AdvancedWorld
{
    public abstract class EntitySet : AdvancedEntity
    {
        protected Ped spawnedPed;
        protected Vehicle spawnedVehicle;

        public EntitySet() { }

        protected bool VehicleSeatsCanBeSeatedBy(List<Ped> members)
        {
            if (!Util.ThereIs(spawnedVehicle)) return false;

            int startingSeat = 0;

            if (Util.ThereIs(spawnedVehicle.Driver) && !spawnedVehicle.Driver.IsDead) Function.Call(Hash.TASK_VEHICLE_TEMP_ACTION, spawnedVehicle.Driver, spawnedVehicle, 1, 1000);
            else startingSeat = -1;

            for (int i = startingSeat, j = 0; j < members.Count; j++)
            {
                if (Util.ThereIs(members[j]) && !members[j].IsSittingInVehicle(spawnedVehicle))
                {
                    if (!Function.Call<bool>(Hash.GET_IS_TASK_ACTIVE, members[j], 160))
                    /*{
                        if (members[j].IsStopped && !members[j].IsGettingIntoAVehicle)
                        {
                            if (spawnedVehicle.IsSeatFree((VehicleSeat)Function.Call<int>(Hash.GET_SEAT_PED_IS_TRYING_TO_ENTER, members[j])))
                                members[j].SetIntoVehicle(spawnedVehicle, (VehicleSeat)Function.Call<int>(Hash.GET_SEAT_PED_IS_TRYING_TO_ENTER, members[j]));
                            else members[j].Task.ClearAllImmediately();
                        }
                    }
                    else*/
                    {
                        while (!spawnedVehicle.IsSeatFree((VehicleSeat)i) && !spawnedVehicle.GetPedOnSeat((VehicleSeat)i).IsDead)
                        {
                            if (++i >= spawnedVehicle.PassengerSeats) return false;
                        }

                        members[j].Task.EnterVehicle(spawnedVehicle, (VehicleSeat)i++, -1, 2.0f, 1);
                    }
                }
            }

            return true;
        }
    }
}