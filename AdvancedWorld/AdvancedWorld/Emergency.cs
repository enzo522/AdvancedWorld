using GTA;
using GTA.Math;
using System.Collections.Generic;

namespace AdvancedWorld
{
    public abstract class Emergency : EntitySet
    {
        protected List<Ped> members;
        protected string name;
        protected Entity target;

        public Emergency(string name, Entity target) : base()
        {
            this.members = new List<Ped>();
            this.name = name;
            this.target = target;
        }

        public abstract bool IsCreatedIn(Vector3 position, List<string> models);

        public override void Restore()
        {
            foreach (Ped p in members)
            {
                if (Util.ThereIs(p)) p.Delete();
            }

            if (Util.ThereIs(spawnedVehicle)) spawnedVehicle.Delete();

            members.Clear();
        }

        public override bool ShouldBeRemoved()
        {
            spawnedPed = null;

            for (int i = members.Count - 1; i >= 0; i--)
            {
                if (!Util.ThereIs(members[i]))
                {
                    members.RemoveAt(i);
                    continue;
                }

                if (members[i].Equals(spawnedVehicle.Driver) && !members[i].IsDead) spawnedPed = members[i];
            }

            if (!Util.ThereIs(spawnedVehicle) || members.Count < 1 || !Util.ThereIs(spawnedPed) || spawnedPed.IsInRangeOf(target.Position, 50.0f) || !spawnedPed.IsInRangeOf(Game.Player.Character.Position, 500.0f))
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