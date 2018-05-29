﻿using GTA;
using GTA.Math;
using GTA.Native;
using System;
using System.Drawing;
using System.Collections.Generic;

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
        
        private static List<int> oldRelationships = new List<int>
        {
            Function.Call<int>(Hash.GET_HASH_KEY, "CIVMALE"),
            Function.Call<int>(Hash.GET_HASH_KEY, "CIVFEMALE"),
            Function.Call<int>(Hash.GET_HASH_KEY, "SECURITY_GUARD"),
            Function.Call<int>(Hash.GET_HASH_KEY, "AMBIENT_GANG_LOST"),
            Function.Call<int>(Hash.GET_HASH_KEY, "AMBIENT_GANG_MEXICAN"),
            Function.Call<int>(Hash.GET_HASH_KEY, "AMBIENT_GANG_FAMILY"),
            Function.Call<int>(Hash.GET_HASH_KEY, "AMBIENT_GANG_BALLAS"),
            Function.Call<int>(Hash.GET_HASH_KEY, "AMBIENT_GANG_MARABUNTE"),
            Function.Call<int>(Hash.GET_HASH_KEY, "AMBIENT_GANG_CULT"),
            Function.Call<int>(Hash.GET_HASH_KEY, "AMBIENT_GANG_SALVA"),
            Function.Call<int>(Hash.GET_HASH_KEY, "AMBIENT_GANG_WEICHENG"),
            Function.Call<int>(Hash.GET_HASH_KEY, "AMBIENT_GANG_HILLBILLY"),
            Function.Call<int>(Hash.GET_HASH_KEY, "GANG_1"),
            Function.Call<int>(Hash.GET_HASH_KEY, "GANG_2"),
            Function.Call<int>(Hash.GET_HASH_KEY, "GANG_9"),
            Function.Call<int>(Hash.GET_HASH_KEY, "GANG_10"),
            Function.Call<int>(Hash.GET_HASH_KEY, "DEALER"),
            Function.Call<int>(Hash.GET_HASH_KEY, "PRIVATE_SECURITY"),
            Function.Call<int>(Hash.GET_HASH_KEY, "ARMY"),
            Function.Call<int>(Hash.GET_HASH_KEY, "PRISONER"),
            Function.Call<int>(Hash.GET_HASH_KEY, "FIREMAN"),
            Function.Call<int>(Hash.GET_HASH_KEY, "MEDIC")
        };
        private static List<int> criminalRelationships = new List<int>();
        private static List<int> copRelationships = new List<int>();
        private static List<int> armyRelationships = new List<int>();
        private static int copID = Function.Call<int>(Hash.GET_HASH_KEY, "COP");
        private static int playerID = Function.Call<int>(Hash.GET_HASH_KEY, "PLAYER");
        private static int count = 0;

        public static int GetRandomIntBelow(int maxValue)
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

        public static bool SomethingIsBetweenPlayerAnd(Entity en)
        {
            if (!ThereIs(en) || en.IsInRangeOf(Game.Player.Character.Position, 50.0f)) return false;
            else
            {
                RaycastResult r = World.Raycast(GameplayCamera.Position, en.Position, IntersectOptions.Map);

                return !en.IsOnScreen || en.IsOccluded || (r.DitHitAnything && r.HitCoords.DistanceTo(Game.Player.Character.Position) < 50.0f);
            }
        }

        public static bool SomethingIsBetweenPlayerPositionAnd(Vector3 position)
        {
            if (Game.Player.Character.IsInRangeOf(position, 50.0f)) return false;
            else
            {
                RaycastResult r = World.Raycast(GameplayCamera.Position, position, IntersectOptions.Map);

                return Vector3.Subtract(GameplayCamera.Position + GameplayCamera.Direction * 100.0f, position).Length() > 100.0f || (r.DitHitAnything && r.HitCoords.DistanceTo(Game.Player.Character.Position) < 50.0f);
            }
        }

        public static Vector3 GetSafePositionIn(float radius)
        {
            Entity[] nearbyEntities = World.GetNearbyEntities(Game.Player.Character.Position, radius);

            if (nearbyEntities.Length > 0)
            {
                foreach (Entity en in nearbyEntities)
                {
                    if (ThereIs(en) && SomethingIsBetweenPlayerAnd(en)) return en.Position;
                }
            }

            return Vector3.Zero;
        }

        public static Vector3 GetSafePositionNear(Vector3 position)
        {
            Entity[] nearbyEntities = World.GetNearbyEntities(position, 100.0f);

            if (nearbyEntities.Length > 0)
            {
                foreach (Entity en in nearbyEntities)
                {
                    if (ThereIs(en) && SomethingIsBetweenPlayerAnd(en)) return en.Position;
                }
            }

            return Vector3.Zero;
        }

        public static bool WeCanReplace(Vehicle v)
        {
            if (ThereIs(v) && (v.Model.IsCar || v.Model.IsBike || v.Model.IsQuadbike) && v.IsDriveable) return !Game.Player.Character.IsInVehicle(v);

            return false;
        }

        public static bool WeCanEnter(Vehicle v)
        {
            return v.IsDriveable && !v.IsOnFire && (v.Model.IsBicycle || v.Model.IsBike || v.Model.IsQuadbike || !v.IsUpsideDown || !v.IsStopped);
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
                Script.Wait(50);
                m.MarkAsNoLongerNeeded();

                if (ThereIs(p)) return p;
            }

            return null;
        }

        public static Vehicle Create(Model m, Vector3 v3, float h, bool withColors)
        {
            if (m.IsValid && !v3.Equals(Vector3.Zero))
            {
                Vehicle v = World.CreateVehicle(m, v3, h);
                Script.Wait(100);
                m.MarkAsNoLongerNeeded();

                if (ThereIs(v))
                {
                    if (withColors)
                    {
                        v.PrimaryColor = (VehicleColor)vehicleColors.GetValue(dice.Next(vehicleColors.Length));
                        v.SecondaryColor = (VehicleColor)vehicleColors.GetValue(dice.Next(vehicleColors.Length));
                    }

                    return v;
                }
            }

            return null;
        }

        public static void Tune(Vehicle v, bool withNeons, bool withWheels)
        {
            if (ThereIs(v))
            {
                v.InstallModKit();
                v.ToggleMod(VehicleToggleMod.Turbo, true);
                v.CanTiresBurst = GetRandomIntBelow(2) == 1;
                v.WindowTint = (VehicleWindowTint)tints.GetValue(dice.Next(tints.Length));

                foreach (VehicleMod m in mods)
                {
                    if (m != VehicleMod.Horns && m != VehicleMod.FrontWheels && m != VehicleMod.BackWheels && v.GetModCount(m) > 0) v.SetMod(m, dice.Next(-1, v.GetModCount(m)), false);
                }

                if (withWheels)
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

                if (withNeons)
                {
                    v.NeonLightsColor = Color.FromKnownColor((KnownColor)neonColors.GetValue(dice.Next(neonColors.Length)));

                    foreach (VehicleNeonLight n in neonLights) v.SetNeonLightsOn(n, true);
                }
            }
        }

        public static int NewRelationshipOf(EventManager.EventType type)
        {
            int newRel = World.AddRelationshipGroup((count++).ToString());

            switch (type)
            {
                case EventManager.EventType.AggressiveDriver:
                case EventManager.EventType.Carjacker:
                case EventManager.EventType.Racer:
                    {
                        foreach (int i in criminalRelationships) World.SetRelationshipBetweenGroups(Relationship.Hate, newRel, i);
                        
                        criminalRelationships.Add(newRel);
                        World.SetRelationshipBetweenGroups(Relationship.Hate, newRel, copID);

                        break;
                    }

                case EventManager.EventType.Driveby:
                case EventManager.EventType.Massacre:
                case EventManager.EventType.Terrorist:
                    {
                        foreach (int i in oldRelationships) World.SetRelationshipBetweenGroups(Relationship.Hate, newRel, i);
                        foreach (int i in criminalRelationships) World.SetRelationshipBetweenGroups(Relationship.Hate, newRel, i);
                        
                        criminalRelationships.Add(newRel);
                        World.SetRelationshipBetweenGroups(Relationship.Hate, newRel, copID);
                        World.SetRelationshipBetweenGroups(Relationship.Respect, newRel, newRel);

                        if (!Main.CriminalsCanFightWithPlayer) World.SetRelationshipBetweenGroups(Relationship.Respect, newRel, playerID);

                        break;
                    }

                case EventManager.EventType.GangTeam:
                    {
                        foreach (int i in criminalRelationships) World.SetRelationshipBetweenGroups(Relationship.Hate, newRel, i);

                        criminalRelationships.Add(newRel);
                        World.SetRelationshipBetweenGroups(Relationship.Hate, newRel, copID);
                        World.SetRelationshipBetweenGroups(Relationship.Respect, newRel, newRel);

                        if (!Main.CriminalsCanFightWithPlayer) World.SetRelationshipBetweenGroups(Relationship.Respect, newRel, playerID);

                        break;
                    }
            }

            return newRel;
        }

        public static int NewRelationshipOf(DispatchManager.DispatchType type)
        {
            int newRel = World.AddRelationshipGroup((count++).ToString());

            switch (type)
            {
                case DispatchManager.DispatchType.Army:
                case DispatchManager.DispatchType.ArmyHeli:
                case DispatchManager.DispatchType.ArmyRoadBlock:
                    {
                        foreach (int i in criminalRelationships) World.SetRelationshipBetweenGroups(Relationship.Hate, newRel, i);
                        foreach (int i in armyRelationships) World.SetRelationshipBetweenGroups(Relationship.Respect, newRel, i);
                        foreach (int i in copRelationships) World.SetRelationshipBetweenGroups(Relationship.Respect, newRel, i);

                        armyRelationships.Add(newRel);
                        World.SetRelationshipBetweenGroups(Relationship.Respect, newRel, copID);
                        World.SetRelationshipBetweenGroups(Relationship.Respect, newRel, Function.Call<int>(Hash.GET_HASH_KEY, "ARMY"));
                        World.SetRelationshipBetweenGroups(Relationship.Respect, newRel, newRel);

                        if (!Main.DispatchesCanFightWithPlayer) World.SetRelationshipBetweenGroups(Relationship.Respect, newRel, playerID);

                        break;
                    }

                case DispatchManager.DispatchType.Cop:
                case DispatchManager.DispatchType.CopHeli:
                case DispatchManager.DispatchType.CopRoadBlock:
                    {
                        foreach (int i in criminalRelationships) World.SetRelationshipBetweenGroups(Relationship.Hate, newRel, i);
                        foreach (int i in armyRelationships) World.SetRelationshipBetweenGroups(Relationship.Respect, newRel, i);
                        foreach (int i in copRelationships) World.SetRelationshipBetweenGroups(Relationship.Respect, newRel, i);

                        copRelationships.Add(newRel);
                        World.SetRelationshipBetweenGroups(Relationship.Respect, newRel, copID);
                        World.SetRelationshipBetweenGroups(Relationship.Respect, newRel, Function.Call<int>(Hash.GET_HASH_KEY, "ARMY"));
                        World.SetRelationshipBetweenGroups(Relationship.Respect, newRel, newRel);

                        if (!Main.DispatchesCanFightWithPlayer) World.SetRelationshipBetweenGroups(Relationship.Respect, newRel, playerID);

                        break;
                    }
            }

            return newRel;
        }

        public static void CleanUp(int relationship)
        {
            World.RemoveRelationshipGroup(relationship);

            if (criminalRelationships.Contains(relationship)) criminalRelationships.Remove(relationship);
        }

        public static void CleanUp(int relationship, DispatchManager.DispatchType type)
        {
            switch (type)
            {
                case DispatchManager.DispatchType.Army:
                case DispatchManager.DispatchType.ArmyHeli:
                case DispatchManager.DispatchType.ArmyRoadBlock:
                    {
                        if (armyRelationships.Contains(relationship)) armyRelationships.Remove(relationship);
                        break;
                    }

                case DispatchManager.DispatchType.Cop:
                case DispatchManager.DispatchType.CopHeli:
                case DispatchManager.DispatchType.CopRoadBlock:
                    {
                        if (copRelationships.Contains(relationship)) copRelationships.Remove(relationship);
                        break;
                    }
            }
        }

        public static bool AnyEmergencyIsNear(Vector3 position, DispatchManager.DispatchType type)
        {
            Ped[] nearbyPeds = World.GetNearbyPeds(position, 100.0f);

            if (nearbyPeds.Length < 1) return false;

            foreach (Ped p in nearbyPeds)
            {
                if (!p.Equals(Game.Player.Character) && !p.IsDead)
                {
                    switch (type)
                    {
                        case DispatchManager.DispatchType.Army:
                            {
                                foreach (int i in armyRelationships)
                                {
                                    if (p.RelationshipGroup == i) return true;
                                }

                                break;
                            }

                        case DispatchManager.DispatchType.Cop:
                            {
                                foreach (int i in copRelationships)
                                {
                                    if (p.RelationshipGroup == i) return true;
                                }

                                break;
                            }
                    }
                }
            }

            return false;
        }

        public static Road GetNextPositionOnStreetWithHeading(Vector3 position)
        {
            OutputArgument outPos = new OutputArgument();
            OutputArgument roadHeading = new OutputArgument();

            for (int i = 1; i < 20; i++)
            {
                if (Function.Call<bool>(Hash.GET_NTH_CLOSEST_VEHICLE_NODE_WITH_HEADING, position.X, position.Y, position.Z, i, outPos, roadHeading, new OutputArgument(), 9, 3.0f, 2.5f))
                {
                    Vector3 roadPos = outPos.GetResult<Vector3>();

                    if (SomethingIsBetweenPlayerPositionAnd(roadPos) && !Function.Call<bool>(Hash.IS_POINT_OBSCURED_BY_A_MISSION_ENTITY, roadPos.X, roadPos.Y, roadPos.Z, 5.0f, 5.0f, 5.0f, 0))
                        return new Road(roadPos, roadHeading.GetResult<float>());
                }
            }

            return new Road(Vector3.Zero, 0.0f);
        }

        public static void NaturallyRemove(Entity en)
        {
            if (ThereIs(en))
            {
                if (en.IsPersistent) Function.Call(Hash.SET_ENTITY_AS_MISSION_ENTITY, en, false, true);
                if (BlipIsOn(en)) en.CurrentBlip.Remove();

                en.MarkAsNoLongerNeeded();
            }
        }

        public static bool NewTaskCanBeDoneBy(Ped p)
        {
            return !p.IsDead && !p.IsInjured;
        }
    }
}