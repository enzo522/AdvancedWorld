﻿namespace AdvancedWorld
{
    public abstract class Criminal : EntitySet
    {
        protected int relationship;
        protected int dispatchCooldown;
        protected int blockCooldown;
        protected ListManager.EventType type;

        public Criminal(ListManager.EventType type) : base()
        {
            this.relationship = 0;
            this.dispatchCooldown = 10;
            this.blockCooldown = 0;
            this.type = type;
        }

        protected void CheckDispatch()
        {
            if (dispatchCooldown < 20) dispatchCooldown++;
            else
            {
                dispatchCooldown = 0;

                if (!Util.AnyEmergencyIsNear(spawnedPed.Position, "COP")) AdvancedWorld.DispatchAgainst(spawnedPed, type);
            }
        }

        protected void CheckBlockable()
        {
            if (blockCooldown < 20) blockCooldown++;
            else
            {
                blockCooldown = 0;
                AdvancedWorld.BlockRoadAgainst(spawnedPed, type);
            }
        }
    }
}