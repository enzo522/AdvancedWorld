using GTA;
using GTA.Math;
using GTA.Native;
using System.Collections.Generic;

namespace AdvancedWorld
{
    public abstract class Emergency : EntitySet
    {
        protected List<Ped> members;
        protected string name;
        protected Entity target;
        protected string emergencyType;
        protected bool onDuty;

        public Emergency(string name, Entity target, string emergencyType) : base()
        {
            this.members = new List<Ped>();
            this.name = name;
            this.target = target;
            this.emergencyType = emergencyType;
            this.onDuty = false;
        }

        public abstract bool IsCreatedIn(Vector3 safePosition, List<string> models);

        public override void Restore()
        {
            foreach (Ped p in members)
            {
                if (Util.ThereIs(p)) p.Delete();
            }

            if (Util.ThereIs(spawnedVehicle)) spawnedVehicle.Delete();

            members.Clear();
        }

        protected abstract void SetPedsOnDuty();

        public override bool ShouldBeRemoved()
        {
            for (int i = members.Count - 1; i >= 0; i--)
            {
                if (!Util.ThereIs(members[i]))
                {
                    members.RemoveAt(i);
                    continue;
                }

                if (members[i].IsDead)
                {
                    members[i].MarkAsNoLongerNeeded();
                    members.RemoveAt(i);
                }
            }

            if (!Util.ThereIs(spawnedVehicle) || !Util.ThereIs(target) || target.IsDead || members.Count < 1 || !spawnedVehicle.IsInRangeOf(Game.Player.Character.Position, 500.0f))
            {
                foreach (Ped p in members)
                {
                    if (Util.ThereIs(p)) p.MarkAsNoLongerNeeded();
                }

                if (Util.ThereIs(spawnedVehicle)) spawnedVehicle.MarkAsNoLongerNeeded();

                members.Clear();
                return true;
            }

            if (!onDuty && spawnedVehicle.IsInRangeOf(target.Position, 50.0f)) SetPedsOnDuty();

            return false;
        }
    }
}