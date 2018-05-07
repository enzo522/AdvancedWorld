﻿namespace AdvancedWorld
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
            if (dispatchCooldown < 15) dispatchCooldown++;
            else
            {
                dispatchCooldown = 0;

                if (!Util.AnyEmergencyIsNear(spawnedPed.Position, AdvancedWorld.EmergencyType.Cop)) AdvancedWorld.DispatchAgainst(spawnedPed, type);
            }
        }
    }
}