namespace AdvancedWorld
{
    public abstract class Criminal : EntitySet
    {
        protected int relationship;
        protected int dispatchCooldown;
        protected AdvancedWorld.CrimeType type;

        public Criminal(AdvancedWorld.CrimeType type) : base()
        {
            this.relationship = 0;
            this.dispatchCooldown = 0;
            this.type = type;
        }

        protected void CheckDispatch()
        {
            if (dispatchCooldown < 30) dispatchCooldown++;
            else
            {
                dispatchCooldown = 0;

                if (!Util.CopIsNear(spawnedPed.Position)) AdvancedWorld.Dispatch(spawnedPed, type);
            }
        }
    }
}