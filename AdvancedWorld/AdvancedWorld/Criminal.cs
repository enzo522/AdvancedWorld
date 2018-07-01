﻿namespace YouAreNotAlone
{
    public abstract class Criminal : EntitySet
    {
        protected int relationship;
        protected int dispatchCooldown;
        protected int dispatchTry;
        protected int blockCooldown;
        protected EventManager.EventType type;

        public Criminal(EventManager.EventType type) : base()
        {
            this.dispatchCooldown = 7;
            this.dispatchTry = 0;
            this.blockCooldown = 0;
            this.type = type;
            this.relationship = Util.NewRelationshipOf(type);
        }

        protected void CheckDispatch()
        {
            if (dispatchCooldown < 15) dispatchCooldown++;
            else if (!Util.AnyEmergencyIsNear(spawnedPed.Position, DispatchManager.DispatchType.Cop, type))
            {
                if (Main.DispatchAgainst(spawnedPed, type))
                {
                    Logger.Write(false, "Dispatch against", type.ToString());
                    dispatchCooldown = 0;
                }
                else if (++dispatchTry > 5)
                {
                    Logger.Write(false, "Couldn't dispatch aginst", type.ToString());
                    dispatchCooldown = 0;
                    dispatchTry = 0;
                }
            }
        }

        protected void CheckBlockable()
        {
            if (blockCooldown < 15) blockCooldown++;
            else if (Main.BlockRoadAgainst(spawnedPed, type))
            {
                Logger.Write(false, "Block road against", type.ToString());
                blockCooldown = 0;
            }
        }
    }
}