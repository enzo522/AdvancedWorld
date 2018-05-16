﻿using GTA;
using GTA.Math;
using System.Collections.Generic;

namespace AdvancedWorld
{
    public class Stinger : AdvancedEntity
    {
        private List<string> wheels;
        private Vehicle owner;
        private Prop stinger;
        private Vector2[] points;
        
        public Stinger(Vehicle v)
        {
            this.wheels = new List<string>
            {
                "wheel_lf",
                "wheel_rf",
                "wheel_lm1",
                "wheel_rm1",
                "wheel_lr",
                "wheel_rr"
            };
            this.owner = v;
        }

        public bool IsCreatedIn(Vector3 position)
        {
            if (position.Equals(Vector3.Zero)) return false;

            Model m = "p_ld_stinger_s";
            stinger = World.CreateProp(m, position, true, true);
            m.MarkAsNoLongerNeeded();

            if (!Util.ThereIs(stinger)) return false;

            stinger.Heading = owner.Heading;
            stinger.IsPersistent = true;
            stinger.IsFireProof = true;
            stinger.IsInvincible = true;
            stinger.FreezePosition = true;
            stinger.LodDistance = 1000;

            Vector3 dimension = stinger.Model.GetDimensions();

            Vector3[] v3 = new Vector3[4];
            points = new Vector2[4];

            v3[0] = stinger.Position + stinger.RightVector * dimension.X / 2 - stinger.ForwardVector * dimension.Y / 2;
            v3[1] = stinger.Position - stinger.RightVector * dimension.X / 2 - stinger.ForwardVector * dimension.Y / 2;
            v3[2] = stinger.Position - stinger.RightVector * dimension.X / 2 + stinger.ForwardVector * dimension.Y / 2;
            v3[3] = stinger.Position + stinger.RightVector * dimension.X / 2 + stinger.ForwardVector * dimension.Y / 2;

            for (int i = 0; i < 4; i++) points[i] = new Vector2(v3[i].X, v3[i].Y);

            return true;
        }

        public override void Restore()
        {
            if (Util.ThereIs(stinger)) stinger.Delete();
        }

        public override bool ShouldBeRemoved()
        {
            if (!Util.ThereIs(stinger)) return true;
            if (!Util.ThereIs(owner) || !stinger.IsInRangeOf(Game.Player.Character.Position, 500.0f))
            {
                stinger.MarkAsNoLongerNeeded();
                return true;
            }

            return false;
        }

        public void CheckStingable()
        {
            Vehicle v = World.GetClosestVehicle(stinger.Position, 10.0f);

            if (!Util.ThereIs(v)) return;

            if (v.IsTouching(stinger) && v.CanTiresBurst)
            {
                for (int i = 0; i < wheels.Count; i++)
                {
                    if (v.HasBone(wheels[i]) && !v.IsTireBurst(i) && StingerAreaContains(v.GetBoneCoord(wheels[i]))) v.BurstTire(i);
                }
            }
        }

        private bool StingerAreaContains(Vector3 v3)
        {
            for (int i = 0, j = 3; i < 4; j = i++)
            {
                if ((points[i].Y > v3.Y != points[j].Y > v3.Y)
                    && (v3.X < (points[j].X - points[i].X) * (v3.Y - points[i].Y) / (points[j].Y - points[i].Y) + points[i].X)) return true;
            }

            return false;
        }
    }
}