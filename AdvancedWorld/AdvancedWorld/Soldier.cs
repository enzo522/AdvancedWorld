using GTA;
using GTA.Math;

namespace AdvancedWorld
{
    public class Soldier : EntitySet
    {
        private string name;

        public Soldier(string name) : base()
        {
            this.name = name;
        }

        public bool IsCreatedIn(float radius, Vector3 safePosition, int teamNum)
        {
            spawnedVehicle = Util.Create(name, safePosition, Util.GetRandomInt(360));

            if (!Util.ThereIs(spawnedVehicle)) return false;

            spawnedPed = spawnedVehicle.CreateRandomPedOnSeat(VehicleSeat.Driver);

            if (!Util.ThereIs(spawnedPed))
            {
                spawnedVehicle.Delete();
                return false;
            }

            if (Util.BlipIsOn(spawnedPed))
            {
                spawnedPed.Delete();
                spawnedVehicle.Delete();
                return false;
            }

            if (teamNum < 2)
            {
                spawnedPed.RelationshipGroup = AdvancedWorld.warAID;
                Util.AddBlipOn(spawnedPed, 0.7f, BlipSprite.Tank, BlipColor.Green, "War " + spawnedVehicle.FriendlyName);
            }
            else if (teamNum < 4)
            {
                spawnedPed.RelationshipGroup = AdvancedWorld.warBID;
                Util.AddBlipOn(spawnedPed, 0.7f, BlipSprite.Tank, BlipColor.Red, "War " + spawnedVehicle.FriendlyName);
            }
            else
            {
                spawnedPed.Delete();
                spawnedVehicle.Delete();
                return false;
            }

            Util.Tune(spawnedVehicle, false);
            spawnedPed.AlwaysKeepTask = true;

            return true;
        }

        public void PerformTask()
        {
            TaskSequence ts = new TaskSequence();
            ts.AddTask.FightAgainstHatedTargets(400.0f);
            ts.AddTask.CruiseWithVehicle(spawnedVehicle, 100.0f, (int)DrivingStyle.AvoidTrafficExtremely);
            ts.Close();

            spawnedPed.Task.PerformSequence(ts);
            ts.Dispose();
        }

        public override bool ShouldBeRemoved()
        {
            if (!Util.ThereIs(spawnedPed))
            {
                if (spawnedVehicle.IsPersistent) spawnedVehicle.MarkAsNoLongerNeeded();
                return true;
            }

            if (!Util.ThereIs(spawnedVehicle))
            {
                if (spawnedPed.IsPersistent) spawnedPed.MarkAsNoLongerNeeded();
                return true;
            }

            if ((spawnedPed.IsDead || !spawnedVehicle.IsDriveable) && Util.BlipIsOn(spawnedPed)) spawnedPed.CurrentBlip.Remove();
            if (!spawnedPed.IsInRangeOf(Game.Player.Character.Position, 500.0f))
            {
                if (spawnedPed.IsPersistent) spawnedPed.MarkAsNoLongerNeeded();
                if (spawnedVehicle.IsPersistent) spawnedVehicle.MarkAsNoLongerNeeded();

                return true;
            }

            return false;
        }
    }
}
