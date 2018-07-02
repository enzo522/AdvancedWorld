using GTA;
using System;
using System.Collections.Concurrent;

namespace YouAreNotAlone
{
    public class EventManager : Script
    {
        private static ConcurrentBag<AdvancedEntity> aggressiveList;
        private static ConcurrentBag<AdvancedEntity> carjackerList;
        private static ConcurrentBag<AdvancedEntity> drivebyList;
        private static ConcurrentBag<AdvancedEntity> gangList;
        private static ConcurrentBag<AdvancedEntity> massacreList;
        private static ConcurrentBag<AdvancedEntity> onFireList;
        private static ConcurrentBag<AdvancedEntity> racerList;
        private static ConcurrentBag<AdvancedEntity> replacedList;
        private static ConcurrentBag<AdvancedEntity> terroristList;
        private int timeChecker;

        public enum EventType
        {
            AggressiveDriver,
            Carjacker,
            Driveby,
            Fire,
            GangTeam,
            Massacre,
            Racer,
            ReplacedVehicle,
            Terrorist
        }

        static EventManager()
        {
            aggressiveList = new ConcurrentBag<AdvancedEntity>();
            carjackerList = new ConcurrentBag<AdvancedEntity>();
            drivebyList = new ConcurrentBag<AdvancedEntity>();
            gangList = new ConcurrentBag<AdvancedEntity>();
            massacreList = new ConcurrentBag<AdvancedEntity>();
            onFireList = new ConcurrentBag<AdvancedEntity>();
            racerList = new ConcurrentBag<AdvancedEntity>();
            replacedList = new ConcurrentBag<AdvancedEntity>();
            terroristList = new ConcurrentBag<AdvancedEntity>();
        }

        public static bool ReplaceSlotIsAvailable() { return replacedList.Count < 5; }
        
        public static void Add(AdvancedEntity en, EventType type)
        {
            switch (type)
            {
                case EventType.AggressiveDriver:
                    {
                        aggressiveList.Add(en);

                        break;
                    }

                case EventType.Carjacker:
                    {
                        carjackerList.Add(en);

                        break;
                    }

                case EventType.Driveby:
                    {
                        drivebyList.Add(en);

                        break;
                    }

                case EventType.Fire:
                    {
                        onFireList.Add(en);

                        break;
                    }

                case EventType.GangTeam:
                    {
                        gangList.Add(en);

                        break;
                    }

                case EventType.Massacre:
                    {
                        massacreList.Add(en);

                        break;
                    }

                case EventType.Racer:
                    {
                        racerList.Add(en);

                        break;
                    }

                case EventType.ReplacedVehicle:
                    {
                        replacedList.Add(en);

                        break;
                    }

                case EventType.Terrorist:
                    {
                        terroristList.Add(en);

                        break;
                    }
            }

            Logger.Write(false, "EventManager: Added new entity.", type.ToString());
        }

        public EventManager()
        {
            timeChecker = 0;
            Tick += OnTick;

            Logger.Write(true, "EventManager started.", "");
        }

        private void OnTick(Object sender, EventArgs e)
        {
            if (timeChecker == 100)
            {
                CleanUp(aggressiveList);
                CleanUp(carjackerList);
                CleanUp(drivebyList);
                CleanUp(gangList);
                CleanUp(massacreList);
                CleanUp(onFireList);
                CleanUp(racerList);
                CleanUp(replacedList);
                CleanUp(terroristList);

                timeChecker = 0;
            }
            else timeChecker++;

            foreach (AggressiveDriver ad in aggressiveList) ad.CheckNitroable();
            foreach (Racers r in racerList) r.CheckNitroable();
        }

        private void CleanUp(ConcurrentBag<AdvancedEntity> cb)
        {
            if (cb.Count < 1) return;

            foreach (AdvancedEntity ae in cb)
            {
                if (ae.ShouldBeRemoved())
                {
                    AdvancedEntity item = null;

                    cb.TryTake(out item);
                    item.Restore(false);
                }
            }
        }
    }
}