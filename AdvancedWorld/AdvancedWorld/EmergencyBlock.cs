using GTA;
using GTA.Math;
using GTA.Native;
using System.Collections.Generic;

namespace AdvancedWorld
{
    public class EmergencyBlock : Emergency
    {
        public EmergencyBlock(string name, Entity target, string emergencyType) : base(name, target, emergencyType) { }

        public override bool IsCreatedIn(Vector3 safePosition, List<string> models)
        {
            Road road = Util.GetNextPositionOnStreetWithHeading(safePosition);

            if (road.Position.Equals(Vector3.Zero)) return false;

            spawnedVehicle = Util.Create(name, road.Position, road.Heading + 90, false);

            if (!Util.ThereIs(spawnedVehicle)) return false;

            Stinger s = new Stinger(spawnedVehicle);

            if (s.IsCreatedIn(spawnedVehicle.Position - spawnedVehicle.ForwardVector * spawnedVehicle.Model.GetDimensions().Y)) ListManager.Add(s, ListManager.EventType.RoadBlock);
            else s.Restore();

            if (emergencyType == "LSPD")
            {
                for (int i = 0; i < 2; i++) members.Add(Util.Create(models[Util.GetRandomInt(models.Count)], spawnedVehicle.Position.Around(5.0f)));
            }
            else
            {
                string selectedModel = models[Util.GetRandomInt(models.Count)];

                if (selectedModel == null)
                {
                    Restore();
                    return false;
                }

                for (int i = 0; i < 2; i++) members.Add(Util.Create(selectedModel, spawnedVehicle.Position.Around(5.0f)));
            }

            foreach (Ped p in members)
            {
                if (!Util.ThereIs(p))
                {
                    Restore();
                    return false;
                }

                switch (emergencyType)
                {
                    case "ARMY":
                        {
                            p.Weapons.Give(WeaponHash.MachinePistol, 300, true, true);
                            p.Weapons.Give(WeaponHash.CombatMG, 500, false, false);
                            p.ShootRate = 1000;
                            p.Armor = 100;

                            break;
                        }

                    case "LSPD":
                        {
                            p.Weapons.Give(WeaponHash.Pistol, 100, true, true);
                            p.Weapons.Give(WeaponHash.PumpShotgun, 30, false, false);
                            p.ShootRate = 500;
                            p.Armor = 30;

                            break;
                        }

                    case "SWAT":
                        {
                            p.Weapons.Give(WeaponHash.SMG, 300, true, true);
                            p.Weapons.Give(WeaponHash.Pistol, 100, false, false);
                            p.ShootRate = 700;
                            p.Armor = 70;

                            break;
                        }
                }

                p.Weapons.Current.InfiniteAmmo = true;
                p.CanSwitchWeapons = true;

                Function.Call(Hash.SET_PED_ID_RANGE, p, 1000.0f);
                Function.Call(Hash.SET_PED_SEEING_RANGE, p, 1000.0f);
                Function.Call(Hash.SET_PED_HEARING_RANGE, p, 1000.0f);
                Function.Call(Hash.SET_PED_COMBAT_RANGE, p, 2);

                Function.Call(Hash.SET_PED_FLEE_ATTRIBUTES, p, 0, false);
                Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, p, 1, true);
                Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, p, 52, true);
                Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, p, 46, true);
                Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, p, 5, true);
                
                if (emergencyType == "ARMY") p.RelationshipGroup = Function.Call<int>(Hash.GET_HASH_KEY, emergencyType);
                else p.RelationshipGroup = Function.Call<int>(Hash.GET_HASH_KEY, "COP");

                p.NeverLeavesGroup = true;
                Function.Call(Hash.SET_PED_AS_COP, p, true);
                p.Task.FightAgainstHatedTargets(200.0f);
            }

            if (spawnedVehicle.HasSiren) spawnedVehicle.SirenActive = true;

            return true;
        }

        protected override void SetPedsOnDuty()
        {
            foreach (Ped p in members)
            {
                if (Util.ThereIs(p)) p.MarkAsNoLongerNeeded();
            }

            onDuty = true;
        }
    }
}