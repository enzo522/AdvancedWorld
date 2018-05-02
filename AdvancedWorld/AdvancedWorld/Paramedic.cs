using GTA;
using GTA.Math;
using GTA.Native;
using System.Collections.Generic;

namespace AdvancedWorld
{
    public class Paramedic : Emergency
    {
        public Paramedic(string name, Entity target) : base(name, target) { }

        public override bool IsCreatedIn(Vector3 position, List<string> models)
        {
            spawnedVehicle = Util.Create(name, World.GetNextPositionOnStreet(position, true), target.Heading, false);

            if (!Util.ThereIs(spawnedVehicle)) return false;

            for (int i = -1; i < 1; i++)
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

                p.RelationshipGroup = Function.Call<int>(Hash.GET_HASH_KEY, "MEDIC");
            }

            if (spawnedVehicle.HasSiren) spawnedVehicle.SirenActive = true;

            foreach (Ped p in members)
            {
                if (p.Equals(spawnedVehicle.Driver)) Function.Call(Hash.TASK_VEHICLE_CHASE, p, target);
            }

            return true;
        }
    }
}