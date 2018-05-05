using GTA;
using GTA.Math;
using GTA.Native;

namespace AdvancedWorld
{
    public abstract class EmergencyFire : Emergency
    {
        protected Vector3 targetPosition;

        public EmergencyFire(string name, Entity target) : base(name, target) { targetPosition = target.Position; }

        protected abstract void SetPedOnDuty(Ped p);

        protected void SetPedsOffDuty()
        {
            if (Util.ThereIs(spawnedVehicle))
            {
                for (int i = -1; i < spawnedVehicle.PassengerSeats; i++)
                {
                    if (spawnedVehicle.IsSeatFree((VehicleSeat)i) || spawnedVehicle.GetPedOnSeat((VehicleSeat)i).IsDead)
                    {
                        for (int m = 0; m < members.Count; m++)
                        {
                            if (!members[m].IsDead && !members[m].IsSittingInVehicle(spawnedVehicle) && !Function.Call<bool>(Hash.GET_IS_TASK_ACTIVE, members[m], 160))
                            {
                                members[m].Task.EnterVehicle(spawnedVehicle, (VehicleSeat)i, -1, 2.0f, 1);
                                break;
                            }
                        }
                    }
                }
            }

            if (EveryoneIsSitting() || !Util.ThereIs(spawnedVehicle))
            {
                foreach (Ped ped in members)
                {
                    ped.AlwaysKeepTask = false;
                    ped.BlockPermanentEvents = false;
                    ped.MarkAsNoLongerNeeded();
                }
            }
        }

        protected bool EveryoneIsSitting()
        {
            foreach (Ped p in members)
            {
                if (!p.IsDead && !p.IsSittingInVehicle(spawnedVehicle)) return false;
            }

            return true;
        }

        public override bool ShouldBeRemoved()
        {
            for (int i = members.Count - 1; i >= 0; i--)
            {
                if (!Util.ThereIs(members[i]))
                {
                    members.RemoveAt(i);
                    continue;
                }

                if (members[i].IsInRangeOf(targetPosition, 30.0f) && members[i].IsPersistent) SetPedOnDuty(members[i]);
                if (members[i].IsDead)
                {
                    members[i].MarkAsNoLongerNeeded();
                    members.RemoveAt(i);
                }
                else spawnedPed = members[i];
            }

            if (!Util.ThereIs(spawnedVehicle) || !Util.ThereIs(target) || members.Count < 1 || !spawnedVehicle.IsInRangeOf(Game.Player.Character.Position, 500.0f))
            {
                foreach (Ped p in members)
                {
                    if (Util.ThereIs(p)) p.MarkAsNoLongerNeeded();
                }

                if (Util.ThereIs(spawnedVehicle)) spawnedVehicle.MarkAsNoLongerNeeded();

                members.Clear();
                return true;
            }

            return false;
        }
    }
}