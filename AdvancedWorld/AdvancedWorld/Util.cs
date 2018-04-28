using GTA;
using GTA.Math;
using System;
using System.Drawing;

namespace AdvancedWorld
{
    public static class Util
    {
        private static Array vehicleColors = Enum.GetValues(typeof(VehicleColor));
        private static Array mods = Enum.GetValues(typeof(VehicleMod));
        private static Array neonColors = Enum.GetValues(typeof(KnownColor));
        private static Array neonLights = Enum.GetValues(typeof(VehicleNeonLight));
        private static Array tints = Enum.GetValues(typeof(VehicleWindowTint));
        private static Random dice = new Random();
        private static int[] wheelTypes = { 0, 1, 2, 3, 4, 5, 7, 8, 9 };
        private static int[] wheelColors = { 156, 0, 1, 11, 2, 8, 122, 27, 30, 45, 35, 33, 136, 135, 36, 41, 138, 37, 99, 90, 95, 115, 109, 153, 154, 88, 89, 91, 55, 125, 53, 56, 151, 82, 64, 87, 70, 140, 81, 145, 142, 134 };

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
            if (!ThereIs(en) || en.IsInRangeOf(Game.Player.Character.Position, 50.0f)) return false;
            else
            {
                RaycastResult r = World.Raycast(GameplayCamera.Position, en.Position, IntersectOptions.Map);

                return (r.DitHitAnything && r.HitCoords.DistanceTo(en.Position) > 10.0f);
            }
        }

        public static Vector3 GetSafePositionIn(float radius)
        {
            Entity[] nearbyEntities = World.GetNearbyEntities(Game.Player.Character.Position, radius);

            if (nearbyEntities.Length > 0)
            {
                foreach (Entity en in nearbyEntities)
                {
                    if (ThereIs(en) && !en.IsPersistent && (!en.IsOnScreen || SomethingIsBetween(en))) return en.Position;
                }
            }

            return Vector3.Zero;
        }

        public static bool WeCanReplace(Vehicle v)
        {
            if (ThereIs(v) && (v.Model.IsCar || v.Model.IsBike || v.Model.IsQuadbike) && v.IsDriveable) return !Game.Player.Character.IsInVehicle(v);

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
                Script.Wait(100);
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
                Script.Wait(100);
                m.MarkAsNoLongerNeeded();

                if (ThereIs(v))
                {
                    v.PrimaryColor = (VehicleColor)vehicleColors.GetValue(dice.Next(vehicleColors.Length));
                    v.SecondaryColor = (VehicleColor)vehicleColors.GetValue(dice.Next(vehicleColors.Length));

                    return v;
                }
            }

            return null;
        }

        public static void Tune(Vehicle v, bool neonsAreNeeded, bool wheelsAreNeeded)
        {
            if (ThereIs(v))
            {
                v.InstallModKit();
                v.ToggleMod(VehicleToggleMod.Turbo, true);
                v.WindowTint = (VehicleWindowTint)tints.GetValue(dice.Next(tints.Length));

                foreach (VehicleMod m in mods)
                {
                    if (m != VehicleMod.Horns && m != VehicleMod.FrontWheels && m != VehicleMod.BackWheels && v.GetModCount(m) > 0) v.SetMod(m, dice.Next(-1, v.GetModCount(m)), false);
                }

                if (wheelsAreNeeded)
                {
                    if (v.Model.IsCar)
                    {
                        v.WheelType = (VehicleWheelType)wheelTypes[dice.Next(wheelTypes.Length)];
                        v.SetMod(VehicleMod.FrontWheels, dice.Next(-1, v.GetModCount(VehicleMod.FrontWheels)), false);
                    }
                    else if (v.Model.IsBike || v.Model.IsQuadbike)
                    {
                        v.WheelType = VehicleWheelType.BikeWheels;
                        int modIndex = dice.Next(-1, v.GetModCount(VehicleMod.FrontWheels));

                        v.SetMod(VehicleMod.FrontWheels, modIndex, false);
                        v.SetMod(VehicleMod.BackWheels, modIndex, false);
                    }

                    v.RimColor = (VehicleColor)wheelColors[dice.Next(wheelColors.Length)];
                }

                if (neonsAreNeeded)
                {
                    v.NeonLightsColor = Color.FromKnownColor((KnownColor)neonColors.GetValue(dice.Next(neonColors.Length)));

                    foreach (VehicleNeonLight n in neonLights) v.SetNeonLightsOn(n, true);
                }
            }
        }
    }
}