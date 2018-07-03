﻿using GTA;
using System;
using System.Collections.Generic;
using System.Threading;

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
        
        public static bool Add(AdvancedEntity en, EventType type)
        {
            switch (type)
            {
                case EventType.AggressiveDriver: return SafelyAddTo(aggressiveList, en);
                case EventType.Carjacker: return SafelyAddTo(carjackerList, en);
                case EventType.Driveby: return SafelyAddTo(drivebyList, en);
                case EventType.Fire: return SafelyAddTo(onFireList, en);
                case EventType.GangTeam: return SafelyAddTo(gangList, en);
                case EventType.Massacre: return SafelyAddTo(massacreList, en);
                case EventType.Racer: return SafelyAddTo(racerList, en);
                case EventType.ReplacedVehicle: return SafelyAddTo(replacedList, en);
                case EventType.Terrorist: return SafelyAddTo(terroristList, en);
                default: return false;
            }
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
                SafelyCleanUp(aggressiveList);
                SafelyCleanUp(carjackerList);
                SafelyCleanUp(drivebyList);
                SafelyCleanUp(gangList);
                SafelyCleanUp(massacreList);
                SafelyCleanUp(onFireList);
                SafelyCleanUp(racerList);
                SafelyCleanUp(replacedList);
                SafelyCleanUp(terroristList);

                timeChecker = 0;
            }
            else timeChecker++;

            SafelyCheckAbilityOf(aggressiveList);
            SafelyCheckAbilityOf(racerList);
        }

        private static bool SafelyAddTo(List<AdvancedEntity> list, AdvancedEntity item)
        {
            if (list == null || item == null) return false;

            bool lockTaken = false;

            try
            {
                Monitor.Enter(list, ref lockTaken);
                list.Add(item);
            }
            finally
            {
                if (lockTaken) Monitor.Exit(list);
            }

            return lockTaken;
        }

        private static void SafelyCleanUp(List<AdvancedEntity> list)
        {
            if (list == null || list.Count < 1) return;

            bool lockTaken = false;

            try
            {
                Monitor.Enter(list, ref lockTaken);

                for (int i = list.Count - 1; i >= 0; i--)
                {
                    if (list[i].ShouldBeRemoved())
                    {
                        list[i].Restore(false);
                        list.RemoveAt(i);
                    }
                }
            }
            finally
            {
                if (lockTaken) Monitor.Exit(list);
            }
        }

        private static void SafelyCheckAbilityOf(List<AdvancedEntity> list)
        {
            if (list == null || list.Count < 1) return;

            bool lockTaken = false;

            try
            {
                Monitor.Enter(list, ref lockTaken);

                foreach (AdvancedEntity ae in list.FindAll(item => item is ICheckable)) ((ICheckable)ae).CheckAbilityUsable();
            }
            finally
            {
                if (lockTaken) Monitor.Exit(list);
            }
        }
    }
}