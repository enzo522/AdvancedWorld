﻿using GTA;
using GTA.Math;
using GTA.Native;
using System.Collections.Generic;

namespace AdvancedWorld
{
    public class LSPD : Emergency
    {
        public LSPD(string name, Entity target) : base(name, target) { }

        public override bool IsCreatedIn(Vector3 safePosition, List<string> models)
        {
            Vector3 position = World.GetNextPositionOnStreet(safePosition, true);

            if (position.Equals(Vector3.Zero)) return false;

            spawnedVehicle = Util.Create(name, position, target.Heading, false);

            if (!Util.ThereIs(spawnedVehicle)) return false;
            
            for (int i = -1; i < 1; i++)
            {
                if (spawnedVehicle.IsSeatFree((VehicleSeat)i))
                {
                    members.Add(spawnedVehicle.CreatePedOnSeat((VehicleSeat)i, models[Util.GetRandomInt(models.Count)]));
                    Script.Wait(50);
                }
            }
            
            foreach (Ped p in members)
            {
                if (!Util.ThereIs(p))
                {
                    Restore();
                    return false;
                }

                p.Weapons.Give(WeaponHash.Pistol, 100, true, true);
                p.Weapons.Give(WeaponHash.PumpShotgun, 30, false, false);
                p.Weapons.Current.InfiniteAmmo = true;
                p.ShootRate = 1000;

                p.Armor = 30;
                p.CanSwitchWeapons = true;

                Function.Call(Hash.SET_PED_AS_COP, p, false);
                p.AlwaysKeepTask = true;
                p.BlockPermanentEvents = true;
            }

            spawnedVehicle.EngineRunning = true;
            
            if (spawnedVehicle.HasSiren) spawnedVehicle.SirenActive = true;

            foreach (Ped p in members)
            {
                if (p.Equals(spawnedVehicle.Driver))
                {
                    if (((Ped)target).IsInVehicle()) Function.Call(Hash._TASK_VEHICLE_FOLLOW, p, spawnedVehicle, target, 100.0f, (int)DrivingStyle.AvoidTrafficExtremely, 10.0f);
                    else p.Task.DriveTo(spawnedVehicle, target.Position, 10.0f, 100.0f, (int)DrivingStyle.AvoidTrafficExtremely);
                }
                else p.Task.FightAgainstHatedTargets(100.0f);
            }
            
            return true;
        }
    }
}