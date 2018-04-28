using GTA;
using GTA.Math;
using System.Collections.Generic;

namespace AdvancedWorld
{
    public class Soldier : EntitySet
    {
        private List<Ped> members;
        private List<string> names;
        private int relationship;

        public Soldier(string nameA, string nameB) : base()
        {
            this.members = new List<Ped>();
            this.names = new List<string>();
            this.names.Add(nameA);
            this.names.Add(nameB);
            this.relationship = 0;
        }

        public bool IsCreatedIn(float radius, Vector3 safePosition, BlipColor teamColor, int teamID)
        {
            this.relationship = teamID;

            for (int i = 0; i < 2; i++)
            {
                Vehicle v = Util.Create(names[i], World.GetNextPositionOnStreet(safePosition, true), Util.GetRandomInt(360));

                if (!Util.ThereIs(v)) continue;

                Ped p = v.CreateRandomPedOnSeat(VehicleSeat.Driver);

                if (!Util.ThereIs(p))
                {
                    v.Delete();
                    continue;
                }

                p.RelationshipGroup = relationship;
                p.AlwaysKeepTask = true;
                Util.Tune(v, false, false);

                if (!Util.BlipIsOn(p))
                {
                    Util.AddBlipOn(p, 0.7f, BlipSprite.Tank, teamColor, "War " + v.FriendlyName);
                    members.Add(p);
                }
                else
                {
                    p.Delete();
                    v.Delete();
                }
            }

            foreach (Ped p in members)
            {
                if (!Util.ThereIs(p))
                {
                    Restore();
                    return false;
                }
            }

            return true;
        }

        public override void Restore()
        {
            foreach (Ped p in members)
            {
                if (Util.ThereIs(p))
                {
                    if (Util.ThereIs(p.CurrentVehicle)) p.CurrentVehicle.Delete();
                    if (Util.BlipIsOn(p)) p.CurrentBlip.Remove();

                    p.Delete();
                }
            }

            if (relationship != 0) AdvancedWorld.CleanUpRelationship(relationship);

            members.Clear();
        }

        public void PerformTask()
        {
            foreach (Ped p in members)
            {
                TaskSequence ts = new TaskSequence();
                ts.AddTask.FightAgainstHatedTargets(100.0f);
                ts.AddTask.CruiseWithVehicle(p.CurrentVehicle, 50.0f, (int)DrivingStyle.AvoidTrafficExtremely);
                ts.Close();

                p.Task.PerformSequence(ts);
                ts.Dispose();
            }
        }

        public override bool ShouldBeRemoved()
        {
            for (int i = members.Count - 1; i >= 0; i--)
            {
                if (!Util.ThereIs(members[i]))
                {
                    if (Util.ThereIs(members[i].CurrentVehicle) && members[i].CurrentVehicle.IsPersistent) members[i].CurrentVehicle.MarkAsNoLongerNeeded();

                    members.RemoveAt(i);
                    continue;
                }

                if (!Util.ThereIs(members[i].CurrentVehicle))
                {
                    if (Util.BlipIsOn(members[i])) members[i].CurrentBlip.Remove();
                    if (members[i].IsPersistent) members[i].MarkAsNoLongerNeeded();

                    members.RemoveAt(i);
                    continue;
                }

                if ((members[i].IsDead || !members[i].CurrentVehicle.IsDriveable) && Util.BlipIsOn(members[i])) members[i].CurrentBlip.Remove();
                if (!members[i].IsInRangeOf(Game.Player.Character.Position, 500.0f))
                {
                    if (Util.BlipIsOn(members[i])) members[i].CurrentBlip.Remove();
                    if (members[i].IsPersistent) members[i].MarkAsNoLongerNeeded();
                    if (members[i].CurrentVehicle.IsPersistent) members[i].CurrentVehicle.MarkAsNoLongerNeeded();

                    members.RemoveAt(i);
                }
            }

            if (members.Count < 1)
            {
                AdvancedWorld.CleanUpRelationship(relationship);
                return true;
            }
            else return false;
        }
    }
}