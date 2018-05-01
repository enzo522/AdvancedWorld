namespace AdvancedWorld
{
    public abstract class Criminal : EntitySet
    {
        protected int relationship;
        protected int dispatchCooldown;

        public Criminal() : base()
        {
            this.relationship = 0;
            this.dispatchCooldown = 0;
        }

        protected void CheckDispatch(AdvancedWorld.CrimeType type)
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