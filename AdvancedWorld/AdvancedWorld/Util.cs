using GTA;
using GTA.Math;
using System;
using System.Drawing;

namespace AdvancedWorld
{
    public static class Util
    {
        private static Array vehicleColorValues = Enum.GetValues(typeof(VehicleColor));
        private static Array modValues = Enum.GetValues(typeof(VehicleMod));
        private static Array neonColorValues = Enum.GetValues(typeof(KnownColor));
        private static Random dice = new Random();

        public static int GetRandomInt(int maxValue)
        {
            return dice.Next(maxValue);
        }

        public static bool ThereIs(Entity en)
        {
            return (en != null && en.Exists());
        }

        public static bool BlipIsOn(Entity en)
        {
            return (en.CurrentBlip != null && en.CurrentBlip.Exists());
        }

        public static bool SomethingIsBetween(Entity en)
        {
            if (ThereIs(en) && !en.IsInRangeOf(Game.Player.Character.Position, 30.0f))
            {
                Vector3 gp = Game.Player.Character.Position;
                RaycastResult r = World.Raycast(new Vector3(gp.X, gp.Y, gp.Z + 3.0f), en.Position, IntersectOptions.Map);

                return r.DitHitAnything;
            }

            return false;
        }

        public static Vector3 GetSafePositionIn(float radius)
        {
            Entity[] nearbyEntities = World.GetNearbyEntities(Game.Player.Character.Position, radius);

            if (nearbyEntities.Length > 0)
            {
                foreach (Entity en in nearbyEntities)
                {
                    if (ThereIs(en) && !en.IsPersistent && !en.IsOnScreen && !en.IsInRangeOf(Game.Player.Character.Position, 50.0f))
                        return en.Position;
                }
            }

            return Vector3.Zero;
        }

        public static bool WeCanReplace(Vehicle v)
        {
            if (ThereIs(v) && (v.Model.IsCar || v.Model.IsBike) && !BlipIsOn(v) && !v.IsOnFire && v.IsDriveable)
                return (!Game.Player.Character.IsInVehicle() || !v.Equals(Game.Player.Character.CurrentVehicle));

            return false;
        }

        public static void AddBlipOn(Entity en, float scale, BlipSprite bs, BlipColor bc, string bn)
        {
            if (ThereIs(en))
            {
                en.AddBlip();
                en.CurrentBlip.Scale = scale;
                en.CurrentBlip.Sprite = bs;
                en.CurrentBlip.Color = bc;
                en.CurrentBlip.Name = bn;
                en.CurrentBlip.IsShortRange = true;
            }
        }

        public static Ped Create(Model m, Vector3 v3)
        {
            if (m.IsValid && !v3.Equals(Vector3.Zero))
            {
                Ped p = World.CreatePed(m, v3);
                m.MarkAsNoLongerNeeded();

                if (ThereIs(p)) return p;
            }

            return null;
        }

        public static Vehicle Create(Model m, Vector3 v3, float h)
        {
            if (m.IsValid && !v3.Equals(Vector3.Zero))
            {
                Vehicle v = World.CreateVehicle(m, v3, h);
                m.MarkAsNoLongerNeeded();

                if (ThereIs(v))
                {
                    v.PrimaryColor = (VehicleColor)vehicleColorValues.GetValue(dice.Next(vehicleColorValues.Length));
                    v.SecondaryColor = (VehicleColor)vehicleColorValues.GetValue(dice.Next(vehicleColorValues.Length));

                    return v;
                }
            }

            return null;
        }

        public static void Tune(Vehicle v, bool neonIsNeeded)
        {
            if (ThereIs(v))
            {
                v.InstallModKit();
                v.ToggleMod(VehicleToggleMod.Turbo, true);

                foreach (VehicleMod m in modValues)
                {
                    if (m != VehicleMod.Horns && m != VehicleMod.FrontWheels && m != VehicleMod.BackWheels && v.GetModCount(m) > 0)
                        v.SetMod(m, dice.Next(-1, v.GetModCount(m)), false);
                }

                if (neonIsNeeded)
                {
                    v.NeonLightsColor = Color.FromKnownColor((KnownColor)neonColorValues.GetValue(dice.Next(neonColorValues.Length)));
                    v.SetNeonLightsOn(VehicleNeonLight.Back, true);
                    v.SetNeonLightsOn(VehicleNeonLight.Front, true);
                    v.SetNeonLightsOn(VehicleNeonLight.Left, true);
                    v.SetNeonLightsOn(VehicleNeonLight.Right, true);
                }
            }
        }
    }
}
