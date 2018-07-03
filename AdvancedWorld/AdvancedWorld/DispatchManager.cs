﻿using GTA;
using System;
using System.Collections.Generic;
using System.Threading;

namespace YouAreNotAlone
{
    public class DispatchManager : Script
    {
        private static List<AdvancedEntity> armyGroundList;
        private static List<AdvancedEntity> armyHeliList;
        private static List<AdvancedEntity> armyRoadblockList;
        private static List<AdvancedEntity> copGroundList;
        private static List<AdvancedEntity> copHeliList;
        private static List<AdvancedEntity> copRoadblockList;
        private static List<AdvancedEntity> emList;
        private static List<AdvancedEntity> shieldList;
        private static List<AdvancedEntity> stingerList;
        private int timeChecker;

        public enum DispatchType
        {
            ArmyGround,
            ArmyHeli,
            ArmyRoadBlock,
            CopGround,
            CopHeli,
            CopRoadBlock,
            Emergency,
            Shield,
            Stinger
        }

        static DispatchManager()
        {
            armyGroundList = new List<AdvancedEntity>();
            armyHeliList = new List<AdvancedEntity>();
            armyRoadblockList = new List<AdvancedEntity>();
            copGroundList = new List<AdvancedEntity>();
            copHeliList = new List<AdvancedEntity>();
            copRoadblockList = new List<AdvancedEntity>();
            emList = new List<AdvancedEntity>();
            shieldList = new List<AdvancedEntity>();
            stingerList = new List<AdvancedEntity>();
        }

        public static bool Add(AdvancedEntity en, DispatchType type)
        {
            switch (type)
            {
                case DispatchType.ArmyGround: return SafelyAddTo(armyGroundList, en);
                case DispatchType.ArmyHeli: return SafelyAddTo(armyHeliList, en);
                case DispatchType.ArmyRoadBlock: return SafelyAddTo(armyRoadblockList, en);
                case DispatchType.CopGround: return SafelyAddTo(copGroundList, en);
                case DispatchType.CopHeli: return SafelyAddTo(copHeliList, en);
                case DispatchType.CopRoadBlock: return SafelyAddTo(copRoadblockList, en);
                case DispatchType.Emergency: return SafelyAddTo(emList, en);
                case DispatchType.Shield: return SafelyAddTo(shieldList, en);
                case DispatchType.Stinger: return SafelyAddTo(stingerList, en);
                default: return false;
            }
        }

        public DispatchManager()
        {
            timeChecker = 0;
            Tick += OnTick;

            Logger.Write(true, "DispatchManager started.", "");
        }

        private void OnTick(Object sender, EventArgs e)
        {
            if (timeChecker == 100)
            {
                SafelyCleanUp(armyGroundList);
                SafelyCleanUp(armyHeliList);
                SafelyCleanUp(armyRoadblockList);
                SafelyCleanUp(copGroundList);
                SafelyCleanUp(copHeliList);
                SafelyCleanUp(copRoadblockList);
                SafelyCleanUp(emList);
                SafelyCleanUp(shieldList);
                SafelyCleanUp(stingerList);

                timeChecker = 0;
            }
            else timeChecker++;

            SafelyCheckAbilityOf(shieldList);
            SafelyCheckAbilityOf(stingerList);
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