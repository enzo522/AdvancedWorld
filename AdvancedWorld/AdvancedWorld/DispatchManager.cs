using GTA;
using System;
using System.Collections.Concurrent;

namespace YouAreNotAlone
{
    public class DispatchManager : Script
    {
        private static ConcurrentBag<AdvancedEntity> armyList;
        private static ConcurrentBag<AdvancedEntity> armyHeliList;
        private static ConcurrentBag<AdvancedEntity> armyRoadblockList;
        private static ConcurrentBag<AdvancedEntity> copList;
        private static ConcurrentBag<AdvancedEntity> copHeliList;
        private static ConcurrentBag<AdvancedEntity> copRoadblockList;
        private static ConcurrentBag<AdvancedEntity> emList;
        private static ConcurrentBag<AdvancedEntity> shieldList;
        private static ConcurrentBag<AdvancedEntity> stingerList;
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
            armyList = new ConcurrentBag<AdvancedEntity>();
            armyHeliList = new ConcurrentBag<AdvancedEntity>();
            armyRoadblockList = new ConcurrentBag<AdvancedEntity>();
            copList = new ConcurrentBag<AdvancedEntity>();
            copHeliList = new ConcurrentBag<AdvancedEntity>();
            copRoadblockList = new ConcurrentBag<AdvancedEntity>();
            emList = new ConcurrentBag<AdvancedEntity>();
            shieldList = new ConcurrentBag<AdvancedEntity>();
            stingerList = new ConcurrentBag<AdvancedEntity>();
        }

        public static void Add(AdvancedEntity en, DispatchType type)
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

            foreach (Shield s in shieldList) s.CheckShieldable();
            foreach (Stinger s in stingerList) s.CheckStingable();
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