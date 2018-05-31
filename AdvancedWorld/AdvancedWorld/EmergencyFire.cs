using GTA;
using GTA.Math;
using GTA.Native;
using System.Collections.Generic;

namespace YouAreNotAlone
{
    public abstract class EmergencyFire : Emergency
    {
        protected Vector3 targetPosition;

        public EmergencyFire(string name, Entity target, string emergencyType) : base(name, target, emergencyType)
        {
            this.blipName += emergencyType == "FIREMAN" ? "Fire Fighter" : "Paramedic";
            Util.CleanUp(this.relationship, DispatchManager.DispatchType.Cop);
            this.relationship = 0;
            this.targetPosition = target.Position;
            Logger.Write("EmergencyFire: Time to dispatch.", emergencyType + " " + name);
        }

        public override bool IsCreatedIn(Vector3 safePosition, List<string> models)
        {
            Road road = new Road(Vector3.Zero, 0.0f);

            for (int cnt = 0; cnt < 5; cnt++)
            {
                road = Util.GetNextPositionOnStreetWithHeading(safePosition.Around(50.0f));

                if (!road.Position.Equals(Vector3.Zero))
                {
                    Logger.Write("EmergencyFire: Found proper road.", emergencyType + " " + name);

                    break;
                }
            }

            if (road.Position.Equals(Vector3.Zero))
            {
                Logger.Error("EmergencyFire: Couldn't find proper road. Abort.", emergencyType + " " + name);

                return false;
            }

            spawnedVehicle = Util.Create(name, road.Position, road.Heading, false);

            if (!Util.ThereIs(spawnedVehicle))
            {
                Logger.Write("EmergencyFire: Couldn't create vehicle. Abort.", emergencyType + " " + name);

                return false;
            }

            int max = emergencyType == "FIREMAN" ? 3 : 1;

            for (int i = -1; i < spawnedVehicle.PassengerSeats && i < max; i++)
            {
                if (spawnedVehicle.IsSeatFree((VehicleSeat)i))
                {
                    members.Add(spawnedVehicle.CreatePedOnSeat((VehicleSeat)i, models[Util.GetRandomIntBelow(models.Count)]));
                    Script.Wait(50);
                }
            }

            Logger.Write("EmergencyFire: Created members.", emergencyType + " " + name);

            foreach (Ped p in members)
            {
                if (!Util.ThereIs(p))
                {
                    Logger.Error("EmergencyFire: There is a member who doesn't exist. Abort.", emergencyType + " " + name);
                    Restore(true);

                    return false;
                }

                if (emergencyType == "FIREMAN")
                {
                    p.Weapons.Give(WeaponHash.FireExtinguisher, 100, true, true);
                    p.Weapons.Current.InfiniteAmmo = true;
                    p.CanSwitchWeapons = true;
                    p.IsFireProof = true;
                }

                AddVarietyTo(p);
                p.RelationshipGroup = Function.Call<int>(Hash.GET_HASH_KEY, emergencyType);
                p.AlwaysKeepTask = true;
                p.BlockPermanentEvents = true;
                Logger.Write("EmergencyFire: Characteristics are set.", emergencyType + " " + name);
            }

            if (Util.ThereIs(spawnedVehicle.Driver))
            {
                Function.Call(Hash.SET_DRIVER_ABILITY, spawnedVehicle.Driver, 1.0f);
                Function.Call(Hash.SET_DRIVER_AGGRESSIVENESS, spawnedVehicle.Driver, 1.0f);
            }

            spawnedVehicle.EngineRunning = true;
            SetPedsOnDuty(true);
            Logger.Write("EmergencyFire: Ready to dispatch.", emergencyType + " " + name);

            return true;
        }

        public override void Restore(bool instantly)
        {
            if (instantly)
            {
                Logger.Write("EmergencyFire: Restore instantly.", emergencyType + " " + name);

                foreach (Ped p in members)
                {
                    if (Util.ThereIs(p)) p.Delete();
                }

                if (Util.ThereIs(spawnedVehicle)) spawnedVehicle.Delete();
            }
            else
            {
                Logger.Write("EmergencyFire: Restore naturally.", emergencyType + " " + name);

                foreach (Ped p in members)
                {
                    if (Util.ThereIs(p) && p.IsPersistent)
                    {
                        p.AlwaysKeepTask = false;
                        p.BlockPermanentEvents = false;
                        Util.NaturallyRemove(p);
                    }
                }

                if (Util.ThereIs(spawnedVehicle) && spawnedVehicle.IsPersistent)
                {
                    if (spawnedVehicle.HasSiren && spawnedVehicle.SirenActive) spawnedVehicle.SirenActive = false;

                    Util.NaturallyRemove(spawnedVehicle);
                }
            }

            members.Clear();
        }

        protected new void AddEmergencyBlip(bool onVehicle)
        {
            if (onVehicle)
            {
                Logger.Write("EmergencyFire: Members are in vehicle. Add blip on vehicle.", emergencyType + " " + name);

                if (Util.WeCanEnter(spawnedVehicle))
                {
                    if (!Util.BlipIsOn(spawnedVehicle)) Util.AddBlipOn(spawnedVehicle, 0.7f, BlipSprite.Hospital, BlipColor.Red, blipName);
                }
                else if (Util.BlipIsOn(spawnedVehicle) && spawnedVehicle.CurrentBlip.Sprite.Equals(BlipSprite.Hospital)) spawnedVehicle.CurrentBlip.Remove();

                foreach (Ped p in members)
                {
                    if (Util.BlipIsOn(p) && p.CurrentBlip.Sprite.Equals(BlipSprite.Hospital)) p.CurrentBlip.Remove();
                }
            }
            else
            {
                Logger.Write("EmergencyFire: Members are on foot. Add blips on members.", emergencyType + " " + name);

                if (Util.BlipIsOn(spawnedVehicle) && spawnedVehicle.CurrentBlip.Sprite.Equals(BlipSprite.Hospital)) spawnedVehicle.CurrentBlip.Remove();

                foreach (Ped p in members)
                {
                    if (Util.WeCanGiveTaskTo(p))
                    {
                        if (!Util.BlipIsOn(p)) Util.AddBlipOn(p, 0.5f, BlipSprite.Hospital, BlipColor.Red, blipName);
                    }
                    else if (Util.BlipIsOn(p) && p.CurrentBlip.Sprite.Equals(BlipSprite.Hospital)) p.CurrentBlip.Remove();
                }
            }
        }

        protected new abstract void SetPedsOnDuty(bool onVehicleDuty);
        protected new void SetPedsOffDuty()
        {
            if (ReadyToGoWith(members))
            {
                if (Util.ThereIs(spawnedVehicle.Driver))
                {
                    Logger.Write("EmergencyFire: Time to be off duty.", emergencyType + " " + name);

                    if (spawnedVehicle.HasSiren && spawnedVehicle.SirenActive) spawnedVehicle.SirenActive = false;
                    if (Util.BlipIsOn(spawnedVehicle) && spawnedVehicle.CurrentBlip.Sprite.Equals(BlipSprite.Hospital)) spawnedVehicle.CurrentBlip.Remove();
                    if (!Function.Call<bool>(Hash.GET_IS_TASK_ACTIVE, spawnedVehicle.Driver, 151))
                    {
                        foreach (Ped p in members)
                        {
                            if (Util.ThereIs(p))
                            {
                                if (Util.BlipIsOn(p) && p.CurrentBlip.Sprite.Equals(BlipSprite.Hospital)) p.CurrentBlip.Remove();
                                if (Util.WeCanGiveTaskTo(p))
                                {
                                    if (p.Equals(spawnedVehicle.Driver)) p.Task.CruiseWithVehicle(spawnedVehicle, 20.0f, (int)DrivingStyle.Normal);
                                    else p.Task.Wait(1000);
                                }
                            }
                        }
                    }
                }
                else
                {
                    Logger.Write("EmergencyFire: There is no driver when off duty. Re-enter everyone.", emergencyType + " " + name);

                    foreach (Ped p in members)
                    {
                        if (Util.WeCanGiveTaskTo(p)) p.Task.LeaveVehicle(spawnedVehicle, false);
                    }
                }
            }
            else
            {
                if (!VehicleSeatsCanBeSeatedBy(members))
                {
                    Logger.Write("EmergencyFire: Something wrong with assigning seats when off duty. Re-enter everyone.", emergencyType + " " + name);

                    foreach (Ped p in members)
                    {
                        if (Util.WeCanGiveTaskTo(p)) p.Task.LeaveVehicle(spawnedVehicle, false);
                    }
                }
                else Logger.Write("EmergencyFire: Assigned seats successfully when off duty.", emergencyType + " " + name);
            }
        }

        protected abstract new bool TargetIsFound();
        private new void AddVarietyTo(Ped p)
        {
            if (emergencyType == "FIREMAN")
            {
                switch (Util.GetRandomIntBelow(3))
                {
                    case 1:
                        Function.Call(Hash.SET_PED_PROP_INDEX, p, 0, 0, 0, false);
                        break;

                    case 2:
                        Function.Call(Hash.SET_PED_PROP_INDEX, p, 1, 0, 0, false);
                        break;
                }
            }
            else
            {
                switch (Util.GetRandomIntBelow(4))
                {
                    case 1:
                        Function.Call(Hash.SET_PED_PROP_INDEX, p, 0, 0, 0, false);
                        break;

                    case 2:
                        Function.Call(Hash.SET_PED_PROP_INDEX, p, 1, 0, 0, false);
                        break;

                    case 3:
                        Function.Call(Hash.SET_PED_PROP_INDEX, p, 0, 0, 0, false);
                        Function.Call(Hash.SET_PED_PROP_INDEX, p, 1, 0, 0, false);
                        break;
                }
            }
        }

        public override bool ShouldBeRemoved()
        {
            int alive = 0;

            for (int i = members.Count - 1; i >= 0; i--)
            {
                if (!Util.ThereIs(members[i]))
                {
                    members.RemoveAt(i);

                    continue;
                }

                if (Util.WeCanGiveTaskTo(members[i])) alive++;
                else if (Util.BlipIsOn(members[i])) members[i].CurrentBlip.Remove();
            }

            Logger.Write("EmergencyFire: Alive members - " + alive.ToString(), emergencyType + " " + name);

            if (!Util.ThereIs(spawnedVehicle) || !Util.WeCanEnter(spawnedVehicle) || alive < 1 || members.Count < 1)
            {
                Logger.Write("EmergencyFire: Emergency fire need to be restored.", emergencyType + " " + name);
                Restore(false);

                return true;
            }

            if (!TargetIsFound())
            {
                if (!spawnedVehicle.IsInRangeOf(Game.Player.Character.Position, 200.0f))
                {
                    Logger.Write("EmergencyFire: Target not found and too far from player. Time to be restored.", emergencyType + " " + name);
                    Restore(false);

                    return true;
                }
                else
                {
                    Logger.Write("EmergencyFire: Target not found. Time to be off duty.", emergencyType + " " + name);
                    SetPedsOffDuty();
                }
            }
            else
            {
                if (!spawnedVehicle.IsInRangeOf(Game.Player.Character.Position, 500.0f))
                {
                    Logger.Write("EmergencyFire: Target found but too far from player. Time to be restored.", emergencyType + " " + name);
                    Restore(false);

                    return true;
                }
                else
                {
                    Logger.Write("EmergencyFire: Target found. Time to be on duty.", emergencyType + " " + name);
                    SetPedsOnDuty(!spawnedVehicle.IsInRangeOf(targetPosition, 30.0f));
                }
            }

            return false;
        }
    }
}