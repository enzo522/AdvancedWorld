namespace AdvancedWorld
{
    public abstract class Criminal : EntitySet
    {
        protected int relationship;
        protected int dispatchCooldown;
        protected AdvancedWorld.CrimeType type;

        public Criminal() : base()
        {
            this.relationship = 0;
            this.dispatchCooldown = 60;
            this.type = AdvancedWorld.CrimeType.None;
        }

        protected void CheckDispatch()
        {
            if (dispatchCooldown < 60) dispatchCooldown++;
            else
            {
                dispatchCooldown = 0;

                if (!Util.CopIsNear(spawnedPed.Position)) AdvancedWorld.Dispatch(spawnedPed, type);
            }
        }
    }
}