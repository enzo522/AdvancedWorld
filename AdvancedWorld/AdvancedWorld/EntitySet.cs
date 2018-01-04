using GTA;

namespace AdvancedWorld
{
    public abstract class EntitySet
    {
        protected Ped spawnedPed;
        protected Vehicle spawnedVehicle;

        public EntitySet() { }

        public abstract bool ShouldBeRemoved();
    }
}