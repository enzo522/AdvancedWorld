using GTA;
using System;
using System.Collections.Generic;

namespace YouAreNotAlone
{
    public class EventManager : Script
    {
        private static List<AdvancedEntity> aggressiveList;
        private static List<AdvancedEntity> carjackerList;
        private static List<AdvancedEntity> drivebyList;
        private static List<AdvancedEntity> gangList;
        private static List<AdvancedEntity> massacreList;
        private static List<AdvancedEntity> onFireList;
        private static List<AdvancedEntity> racerList;
        private static List<AdvancedEntity> replacedList;
        private static List<AdvancedEntity> terroristList;
        private static readonly object _lockObject = new object();
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
            aggressiveList = new List<AdvancedEntity>();
            carjackerList = new List<AdvancedEntity>();
            drivebyList = new List<AdvancedEntity>();
            gangList = new List<AdvancedEntity>();
            massacreList = new List<AdvancedEntity>();
            onFireList = new List<AdvancedEntity>();
            racerList = new List<AdvancedEntity>();
            replacedList = new List<AdvancedEntity>();
            terroristList = new List<AdvancedEntity>();
        }

        public static bool ReplaceSlotIsAvailable() { return replacedList.Count < 5; }
        
        public static void Add(AdvancedEntity en, EventType type)
        {
            lock (_lockObject)
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

            lock (_lockObject)
            {
                foreach (AggressiveDriver ad in aggressiveList) ad.CheckNitroable();
            }

            lock (_lockObject)
            {
                foreach (Racers r in racerList) r.CheckNitroable();
            }
        }

        private void CleanUp(List<AdvancedEntity> list)
        {
            if (list.Count < 1) return;

            lock (_lockObject)
            {
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    if (list[i].ShouldBeRemoved())
                    {
                        list[i].Restore(false);
                        list.RemoveAt(i);
                    }
                }
            }
        }
    }
}