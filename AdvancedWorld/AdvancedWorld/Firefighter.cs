using GTA;
using GTA.Math;
using GTA.Native;
using System.Collections.Generic;

namespace AdvancedWorld
{
    public class Firefighter : Emergency
    {
        public Firefighter(string name, Entity target) : base(name, target) { }

        public override bool IsCreatedIn(Vector3 safePosition, List<string> models)
        {
            Vector3 position = World.GetNextPositionOnStreet(safePosition, true);

            if (position.Equals(Vector3.Zero)) return false;

            spawnedVehicle = Util.Create(name, position, target.Heading, false);

            if (!Util.ThereIs(spawnedVehicle)) return false;

            for (int i = -1; i < spawnedVehicle.PassengerSeats; i++)
            {
                if (spawnedVehicle.IsSeatFree((VehicleSeat)i))
                {
                    members.Add(spawnedVehicle.CreatePedOnSeat((VehicleSeat)i, models[Util.GetRandomInt(models.Count)]));
                    Script.Wait(50);
                }
            }

            foreach (Ped p in members)
            {
                if (!Util.ThereIs(p))
                {
                    Restore();
                    return false;
                }

                p.RelationshipGroup = Function.Call<int>(Hash.GET_HASH_KEY, "FIREMAN");
            }

            if (spawnedVehicle.HasSiren) spawnedVehicle.SirenActive = true;

            foreach (Ped p in members)
            {
                if (p.Equals(spawnedVehicle.Driver)) Function.Call(Hash.TASK_VEHICLE_CHASE, p, target);
                else p.Task.FightAgainstHatedTargets(100.0f);
            }

            return true;
        }
    }
}