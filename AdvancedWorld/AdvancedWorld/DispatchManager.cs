using GTA;
using System;
using System.Collections.Generic;

namespace YouAreNotAlone
{
    public class DispatchManager : Script
    {
        private static List<AdvancedEntity> armyList;
        private static List<AdvancedEntity> armyHeliList;
        private static List<AdvancedEntity> armyRoadblockList;
        private static List<AdvancedEntity> copList;
        private static List<AdvancedEntity> copHeliList;
        private static List<AdvancedEntity> copRoadblockList;
        private static List<AdvancedEntity> emList;
        private static List<AdvancedEntity> shieldList;
        private static List<AdvancedEntity> stingerList;
        private static readonly object _lockObject = new object();
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
            armyList = new List<AdvancedEntity>();
            armyHeliList = new List<AdvancedEntity>();
            armyRoadblockList = new List<AdvancedEntity>();
            copList = new List<AdvancedEntity>();
            copHeliList = new List<AdvancedEntity>();
            copRoadblockList = new List<AdvancedEntity>();
            emList = new List<AdvancedEntity>();
            shieldList = new List<AdvancedEntity>();
            stingerList = new List<AdvancedEntity>();
        }

        public static void Add(AdvancedEntity en, DispatchType type)
        {
            lock (_lockObject)
            {
                switch (type)
                {
                    case DispatchType.ArmyGround:
                        {
                            armyList.Add(en);

                            break;
                        }

                    case DispatchType.ArmyHeli:
                        {
                            armyHeliList.Add(en);

                            break;
                        }

                    case DispatchType.ArmyRoadBlock:
                        {
                            armyRoadblockList.Add(en);

                            break;
                        }

                    case DispatchType.CopGround:
                        {
                            copList.Add(en);

                            break;
                        }

                    case DispatchType.CopHeli:
                        {
                            copHeliList.Add(en);

                            break;
                        }

                    case DispatchType.CopRoadBlock:
                        {
                            copRoadblockList.Add(en);

                            break;
                        }

                    case DispatchType.Emergency:
                        {
                            emList.Add(en);

                            break;
                        }

                    case DispatchType.Shield:
                        {
                            shieldList.Add(en);

                            break;
                        }

                    case DispatchType.Stinger:
                        {
                            stingerList.Add(en);

                            break;
                        }
                }
            }

            Logger.Write(false, "DispatchManager: Added new entity.", type.ToString());
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
                CleanUp(armyList);
                CleanUp(armyHeliList);
                CleanUp(armyRoadblockList);
                CleanUp(copList);
                CleanUp(copHeliList);
                CleanUp(copRoadblockList);
                CleanUp(emList);
                CleanUp(shieldList);
                CleanUp(stingerList);

                timeChecker = 0;
            }
            else timeChecker++;

            lock (_lockObject)
            {
                foreach (Shield s in shieldList) s.CheckShieldable();
            }

            lock (_lockObject)
            {
                foreach (Stinger s in stingerList) s.CheckStingable();
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