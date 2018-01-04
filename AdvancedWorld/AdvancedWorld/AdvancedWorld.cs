using GTA;
using GTA.Math;
using GTA.Native;
using System;
using System.Collections.Generic;

namespace AdvancedWorld
{
    public class AdvancedWorld : Script
    {
        public static int racerID;
        public static int teamAID;
        public static int teamBID;
        public static int cougarID;
        public static int warAID;
        public static int warBID;

        private static List<string> addOnCarNames;
        private static List<string> racerCarNames;
        private static List<string> racerBikeNames;
        private static List<string> soldierCarNames;
        private static List<string> drivebyCarNames;
        private static List<Vector3> racingPosition;
        private static List<List<string>> models;

        private List<EntitySet> replacedList;
        private List<EntitySet> carjackerList;
        private List<EntitySet> aggressiveList;
        private List<EntitySet> gangList;
        private List<EntitySet> massacreList;
        private List<EntitySet> racerList;
        private List<EntitySet> soldierList;
        private List<EntitySet> drivebyList;

        private float radius;
        private int eventTimeChecker;

        static AdvancedWorld()
        {
            racerID = World.AddRelationshipGroup("racer");
            teamAID = World.AddRelationshipGroup("teamA");
            teamBID = World.AddRelationshipGroup("teamB");
            cougarID = Function.Call<int>(Hash.GET_HASH_KEY, "COUGAR");
            warAID = World.AddRelationshipGroup("warA");
            warBID = World.AddRelationshipGroup("warB");

            addOnCarNames = new List<string>
            {
                "911r",
                "lp770",
                "p2",
                "one77",
                "2017chiron",
                "918",
                "slsamg",
                "amggtr",
                "rmodveneno",
                "laferrari",
                "ageraone",
                "lp770r",
                "lp670",
                "zondac",
                "huayra",
                "vulcan",
                "ferrari812",
                "db11",
                "16ss",
                "16challenger",
                "69charger",
                "j50",
                "500gtrlam",
                "f458",
                "mb300sl",
                "570s",
                "jeep2012",
                "911tbs",
                "lp700r",
                "p7",
                "16charger",
                "rmodbmwi8",
                "lp570",
                "m5",
                "rmodm4gts",
                "2014rs5",
                "audirs6tk",
                "2013rs7",
                "s3sedan",
                "m3f80"
            };
            racerCarNames = new List<string>
            {
                "infernus2",
                "cheetah2",
                "turismo2",
                "xa21",
                "gp1",
                "le7b",
                "nero",
                "nero2",
                "prototipo",
                "tyrus",
                "vagner",
                "zentorno",
                "cyclone",
                "visione",
                "comet6",
                "vacca",
                "sheava",
                "t20",
                "entityxf",
                "turismor",
                "cheetah",
                "italigtb",
                "italigtb2",
                "osiris",
                "penetrator",
                "reaper",
                "fmj",
                "infernus",
                "pfister811",
                "torero",
                "elegy",
                "banshee",
                "seven70",
                "verlierer2",
                "sultanrs2",
                "sultanrs3",
                "sultanrs4",
                "kuruma3",
                "futo3",
                "dukes3",
                "deluxo2",
                "es550",
                "blista4",
                "euros",
                "gauntlet4",
                "vigero3",
                "elegy2",
                "ardent",
                "jester",
                "sentinel2",
                "feltzer3",
                "rapidgt3",
                "supergt",
                "turismo",
                "cheetah3",
                "typhoon",
                "sentinel4",
                "sultan2",
                "uranus",
                "deluxo",
                "stromberg",
                "neon",
                "sentinel3",
                "sc1",
                "autarch",
                "gt500",
                "pariah",
                "savestra",
                "comet5",
                "z190",
                "viseris",
                "elegy4",
                "elegy5",
                "elegy6",
                "requiem",
                "dominator3"
            };
            racerBikeNames = new List<string>
            {
                "akuma",
                "double",
                "double2",
                "bati",
                "bati2",
                "shotaro",
                "carbonrs",
                "lectro",
                "diablous",
                "diablous2",
                "vortex",
                "ruffian",
                "fcr",
                "fcr2",
                "nemesis",
                "pcj",
                "hakuchou",
                "hakuchou2",
                "hakuchou3",
                "defiler",
                "vader",
                "nrg900",
                "kenshin"
            };
            soldierCarNames = new List<string>
            {
                "rhino",
                "apc",
                "apc2",
                "khanjali"
            };
            drivebyCarNames = new List<string>
            {
                "baller",
                "emperor",
                "warrener",
                "greenwood",
                "dubsta",
                "glendale",
                "marbelle"
            };
            racingPosition = new List<Vector3>
            {
                new Vector3(-985.771f, -2999.69f, 13.9451f),
                new Vector3(484.833f, -3050.8f, 6.22559f),
                new Vector3(1291.57f, -3332.31f, 5.90387f),
                new Vector3(2776.71f, -71.669f, 5.84047f),
                new Vector3(2693.67f, 1347.54f, 24.5206f),
                new Vector3(1975.48f, 2282.23f, 93.3224f),
                new Vector3(1643.27f, 21.6298f, 173.774f),
                new Vector3(-507.208f, -2763.61f, 6.00039f),
                new Vector3(-1175.25f, -1774.08f, 3.8469f),
                new Vector3(-1574.75f, -1038.91f, 13.0179f),
                new Vector3(-2340.05f, 270.959f, 169.467f),
                new Vector3(-1676.62f, 3084.9f, 31.5746f),
                new Vector3(-1993.77f, 3424.55f, 31.1371f),
                new Vector3(-1638.07f, 4728.91f, 53.4584f),
                new Vector3(-1577.96f, 5166.59f, 19.553f),
                new Vector3(-506.344f, 5241.31f, 80.335f),
                new Vector3(263.059f, 6942.46f, 7.98908f),
                new Vector3(1418.8f, 6593.94f, 12.7123f),
                new Vector3(2204.19f, 5566.35f, 53.8267f),
                new Vector3(3815.93f, 4461.35f, 3.89153f),
                new Vector3(3572.58f, 3658.64f, 33.8986f),
                new Vector3(1065.99f, 3084.33f, 41.0161f),
                new Vector3(-65.8666f, 897.338f, 235.538f),
                new Vector3(-1142.18f, 4926.55f, 220.483f),
                new Vector3(160.768f, -615.23f, 32.4245f),
                new Vector3(-1523.84f, -557.098f, 33.2945f),
                new Vector3(1860.75f, 2607.86f, 45.672f),
                new Vector3(3810.76f, 4463.18f, 4.2125f),
                new Vector3(922.476f, 49.637f, 80.7648f),
                new Vector3(1387.93f, -2063.35f, 51.9986f),
                new Vector3(306.545f, -2757.75f, 5.98824f),
                new Vector3(1303.65f, 4329.92f, 38.4774f),
                new Vector3(977.231f, -1824.01f, 31.1574f),
                new Vector3(-1395.39f, 76.5601f, 53.6247f),
                new Vector3(106.501f, -1939.97f, 20.8037f),
                new Vector3(2482.7f, 3825.13f, 40.4529f),
                new Vector3(1512.54f, 6333.62f, 23.9926f),
                new Vector3(-370.049f, -122.949f, 38.6958f),
                new Vector3(2122.25f, 4802.36f, 41.1265f),
                new Vector3(-456.922f, 1593.7f, 359.019f),
                new Vector3(-578.305f, 5246.03f, 70.4694f),
                new Vector3(450.458f, -1018.3f, 28.5145f),
                new Vector3(2941.58f, 2794.6f, 40.5438f),
                new Vector3(-836.424f, 4178.79f, 215.232f),
                new Vector3(-155.993f, -161.403f, 43.6212f),
                new Vector3(2057.23f, 2947.99f, 47.5835f),
                new Vector3(2527.91f, 2628.19f, 37.9449f),
                new Vector3(919.307f, -1551.14f, 30.7867f),
                new Vector3(1418.0f, 3619.04f, 34.8962f),
                new Vector3(-1580.55f, 2093.92f, 69.2248f),
                new Vector3(1084.22f, -205.161f, 56.5797f),
                new Vector3(1584.37f, 6448.85f, 25.1695f),
                new Vector3(3333.05f, 5153.71f, 18.278f),
                new Vector3(1384.27f, -741.696f, 67.1902f),
                new Vector3(2351.46f, 1845.66f, 102.163f),
                new Vector3(2302.07f, 2538.92f, 46.6373f),
                new Vector3(2012.53f, 3059.33f, 47.0805f),
                new Vector3(2759.61f, 3468.81f, 55.6943f)
            };
            models = new List<List<string>>
            {
                new List<string> { "a_m_m_og_boss_01", "mp_m_famdd_01", "g_f_y_families_01", "g_m_y_famca_01", "g_m_y_famdnf_01", "g_m_y_famfor_01" },
                new List<string> { "g_f_y_ballas_01", "g_m_y_ballaeast_01", "g_m_y_ballaorig_01", "g_m_y_ballasout_01" },
                new List<string> { "a_m_y_mexthug_01", "g_m_y_mexgoon_01", "g_m_y_mexgoon_02", "g_m_y_mexgoon_03", "g_f_y_vagos_01", "mp_m_g_vagfun_01" },
                new List<string> { "g_m_y_lost_01", "g_m_y_lost_02", "g_m_y_lost_03", "g_f_y_lost_01" },
                new List<string> { "a_m_m_eastsa_01", "a_m_m_eastsa_02", "a_m_m_malibu_01", "a_m_m_mexcntry_01", "a_m_m_mexlabor_01", "a_m_m_og_boss_01", "a_m_m_polynesian_01", "a_m_m_soucent_01", "a_m_m_soucent_03", "a_m_m_soucent_04" },
                new List<string> { "a_m_m_stlat_02", "s_m_m_bouncer_01", "s_m_m_lifeinvad_01", "u_m_m_aldinapoli", "u_m_m_bikehire_01", "u_m_m_filmdirector", "u_m_m_rivalpap", "u_m_m_willyfist", "u_m_y_baygor", "u_m_y_chip", "s_m_m_armoured_03" },
                new List<string> { "s_m_y_dealer_01", "u_m_y_tattoo_01", "u_m_y_sbike", "u_m_y_party_01", "u_m_y_paparazzi", "u_m_y_hippie_01", "u_m_y_gunvend_01", "u_m_y_fibmugger_01", "u_m_y_guido_01", "u_m_y_cyclist_01", "s_m_m_security_03" }
            };

            World.SetRelationshipBetweenGroups(Relationship.Hate, racerID, racerID);
            World.SetRelationshipBetweenGroups(Relationship.Hate, teamAID, teamBID);
            World.SetRelationshipBetweenGroups(Relationship.Hate, teamAID, warAID);
            World.SetRelationshipBetweenGroups(Relationship.Hate, teamAID, warBID);
            World.SetRelationshipBetweenGroups(Relationship.Hate, teamBID, warAID);
            World.SetRelationshipBetweenGroups(Relationship.Hate, teamBID, warBID);
            World.SetRelationshipBetweenGroups(Relationship.Hate, warAID, warBID);
            World.SetRelationshipBetweenGroups(Relationship.Hate, racerID, Function.Call<int>(Hash.GET_HASH_KEY, "COP"));
            World.SetRelationshipBetweenGroups(Relationship.Hate, teamAID, Function.Call<int>(Hash.GET_HASH_KEY, "COP"));
            World.SetRelationshipBetweenGroups(Relationship.Hate, teamBID, Function.Call<int>(Hash.GET_HASH_KEY, "COP"));
            World.SetRelationshipBetweenGroups(Relationship.Hate, warAID, Function.Call<int>(Hash.GET_HASH_KEY, "COP"));
            World.SetRelationshipBetweenGroups(Relationship.Hate, warBID, Function.Call<int>(Hash.GET_HASH_KEY, "COP"));
            World.SetRelationshipBetweenGroups(Relationship.Hate, cougarID, Function.Call<int>(Hash.GET_HASH_KEY, "COP"));
            World.SetRelationshipBetweenGroups(Relationship.Hate, cougarID, teamAID);
            World.SetRelationshipBetweenGroups(Relationship.Hate, cougarID, teamBID);
            World.SetRelationshipBetweenGroups(Relationship.Hate, cougarID, warAID);
            World.SetRelationshipBetweenGroups(Relationship.Hate, cougarID, warBID);

            World.SetRelationshipBetweenGroups(Relationship.Respect, teamAID, teamAID);
            World.SetRelationshipBetweenGroups(Relationship.Respect, teamBID, teamBID);
            World.SetRelationshipBetweenGroups(Relationship.Respect, warAID, warAID);
            World.SetRelationshipBetweenGroups(Relationship.Respect, warBID, warBID);
            World.SetRelationshipBetweenGroups(Relationship.Respect, teamAID, Function.Call<int>(Hash.GET_HASH_KEY, "PLAYER"));
            World.SetRelationshipBetweenGroups(Relationship.Respect, teamBID, Function.Call<int>(Hash.GET_HASH_KEY, "PLAYER"));
            World.SetRelationshipBetweenGroups(Relationship.Respect, warAID, Function.Call<int>(Hash.GET_HASH_KEY, "PLAYER"));
            World.SetRelationshipBetweenGroups(Relationship.Respect, warBID, Function.Call<int>(Hash.GET_HASH_KEY, "PLAYER"));
            World.SetRelationshipBetweenGroups(Relationship.Respect, cougarID, Function.Call<int>(Hash.GET_HASH_KEY, "PLAYER"));
            World.SetRelationshipBetweenGroups(Relationship.Respect, cougarID, cougarID);
        }

        public AdvancedWorld()
        {
            replacedList = new List<EntitySet>();
            carjackerList = new List<EntitySet>();
            aggressiveList = new List<EntitySet>();
            gangList = new List<EntitySet>();
            massacreList = new List<EntitySet>();
            racerList = new List<EntitySet>();
            soldierList = new List<EntitySet>();
            drivebyList = new List<EntitySet>();

            radius = 100.0f;
            eventTimeChecker = 0;

            Interval = 500;
            Tick += OnTick;
        }

        private void OnTick(Object sender, EventArgs e)
        {
            CleanUp(replacedList);
            CleanUp(carjackerList);
            CleanUp(aggressiveList);
            CleanUp(gangList);
            CleanUp(massacreList);
            CleanUp(racerList);
            CleanUp(soldierList);
            CleanUp(drivebyList);

            if (eventTimeChecker == 15 || eventTimeChecker == 30 || eventTimeChecker == 45)
            {
                if (replacedList.Count < 5)
                {
                    ReplacedVehicle rv = new ReplacedVehicle(addOnCarNames[Util.GetRandomInt(addOnCarNames.Count)]);

                    if (rv.IsCreatedIn(radius))
                    {
                        replacedList.Add(rv);
                        Function.Call(Hash.FLASH_MINIMAP_DISPLAY);
                    }
                }

                eventTimeChecker++;
            }
            else if (eventTimeChecker == 60)
            {
                switch (Util.GetRandomInt(9))
                {
                    case 0:
                        {
                            Carjacker cj = new Carjacker();

                            if (cj.IsCreatedIn(radius))
                            {
                                carjackerList.Add(cj);
                                Function.Call(Hash.FLASH_MINIMAP_DISPLAY);
                            }

                            break;
                        }

                    case 1:
                        {
                            AggressiveDriver ad = new AggressiveDriver(racerCarNames[Util.GetRandomInt(racerCarNames.Count)]);

                            if (ad.IsCreatedIn(radius))
                            {
                                aggressiveList.Add(ad);
                                Function.Call(Hash.FLASH_MINIMAP_DISPLAY);
                            }

                            break;
                        }

                    case 2:
                        {
                            Vehicle[] nearbyVehicles = World.GetNearbyVehicles(Game.Player.Character.Position, radius);

                            if (nearbyVehicles.Length <= 0)
                            {
                                nearbyVehicles = null;
                                break;
                            }

                            Vehicle explosiveVehicle = nearbyVehicles[Util.GetRandomInt(nearbyVehicles.Length)];

                            if (Util.WeCanReplace(explosiveVehicle))
                            {
                                Util.AddBlipOn(explosiveVehicle, 0.7f, BlipSprite.PersonalVehicleCar, BlipColor.Red, "Vehicle Explosion");
                                explosiveVehicle.Explode();
                                Function.Call(Hash.FLASH_MINIMAP_DISPLAY);
                            }

                            break;
                        }

                    case 3:
                        {
                            Vehicle[] nearbyVehicles = World.GetNearbyVehicles(Game.Player.Character.Position, radius);

                            if (nearbyVehicles.Length <= 0)
                            {
                                nearbyVehicles = null;
                                break;
                            }

                            Vehicle undriveableVehicle = nearbyVehicles[Util.GetRandomInt(nearbyVehicles.Length)];

                            if (Util.WeCanReplace(undriveableVehicle))
                            {
                                Util.AddBlipOn(undriveableVehicle, 0.7f, BlipSprite.PersonalVehicleCar, BlipColor.Yellow, "Vehicle on Fire");
                                undriveableVehicle.EngineHealth = -900.0f;
                                Function.Call(Hash.FLASH_MINIMAP_DISPLAY);
                            }

                            break;
                        }

                    case 4:
                        {
                            Vector3 safePosition = Util.GetSafePositionIn(radius);

                            if (safePosition.Equals(Vector3.Zero)) break;

                            GangTeam teamA = new GangTeam();
                            GangTeam teamB = new GangTeam();

                            int teamANum = Util.GetRandomInt(models.Count);
                            int teamBNum = -1;

                            while (teamBNum == -1 || teamANum == teamBNum) teamBNum = Util.GetRandomInt(models.Count);

                            if (teamANum == -1 || teamBNum == -1) break;

                            if (teamA.IsCreatedIn(radius, World.GetNextPositionOnSidewalk(safePosition.Around(5.0f)), models[teamANum], teamAID, BlipColor.Green, "A Team"))
                            {
                                gangList.Add(teamA);
                                Function.Call(Hash.FLASH_MINIMAP_DISPLAY);
                            }

                            if (teamB.IsCreatedIn(radius, World.GetNextPositionOnSidewalk(safePosition.Around(5.0f)), models[teamBNum], teamBID, BlipColor.Red, "B Team"))
                            {
                                gangList.Add(teamB);
                                Function.Call(Hash.FLASH_MINIMAP_DISPLAY);
                            }

                            foreach (GangTeam gt in gangList) gt.PerformTask();

                            break;
                        }

                    case 5:
                        {
                            Vector3 safePosition = Util.GetSafePositionIn(radius);

                            if (safePosition.Equals(Vector3.Zero)) break;

                            for (int i = 0; i < 4; i++)
                            {
                                Massacre ms = new Massacre();

                                if (ms.IsCreatedIn(radius, World.GetNextPositionOnSidewalk(safePosition.Around(5.0f))))
                                {
                                    massacreList.Add(ms);
                                    Function.Call(Hash.FLASH_MINIMAP_DISPLAY);
                                }
                            }
                            
                            break;
                        }

                    case 6:
                        {
                            Vector3 goal = racingPosition[Util.GetRandomInt(racingPosition.Count)];
                            Vector3 safePosition = Util.GetSafePositionIn(radius);

                            if (safePosition.Equals(Vector3.Zero)) break;

                            int random = Util.GetRandomInt(2);

                            for (int i = 0; i < 4; i++)
                            {
                                Racer r = null;

                                if (random == 0)
                                    r = new Racer(racerCarNames[Util.GetRandomInt(racerCarNames.Count)], goal);
                                else
                                    r = new Racer(racerBikeNames[Util.GetRandomInt(racerBikeNames.Count)], goal);

                                if (r.IsCreatedIn(radius, World.GetNextPositionOnStreet(safePosition.Around(10.0f), true)))
                                {
                                    racerList.Add(r);
                                    Function.Call(Hash.FLASH_MINIMAP_DISPLAY);
                                }

                            }

                            break;
                        }

                    case 7:
                        {
                            Vector3 safePosition = Util.GetSafePositionIn(radius);

                            if (safePosition.Equals(Vector3.Zero)) break;

                            for (int i = 0; i < 4; i++)
                            {
                                Soldier s = new Soldier(soldierCarNames[Util.GetRandomInt(soldierCarNames.Count)]);

                                if (s.IsCreatedIn(radius, World.GetNextPositionOnStreet(safePosition.Around(10.0f), true), i))
                                {
                                    soldierList.Add(s);
                                    Function.Call(Hash.FLASH_MINIMAP_DISPLAY);
                                }

                            }
                            
                            foreach (Soldier s in soldierList) s.PerformTask();

                            break;
                        }

                    case 8:
                        {
                            Driveby db = new Driveby(drivebyCarNames[Util.GetRandomInt(drivebyCarNames.Count)]);

                            if (db.IsCreatedIn(radius, models[Util.GetRandomInt(models.Count)]))
                            {
                                drivebyList.Add(db);
                                Function.Call(Hash.FLASH_MINIMAP_DISPLAY);
                            }

                            break;
                        }
                }

                eventTimeChecker = 0;
            }
            else eventTimeChecker++;
        }

        private void CleanUp(List<EntitySet> l)
        {
            for (int i = l.Count - 1; i >= 0; i--)
            {
                if (l[i].ShouldBeRemoved())
                {
                    l.RemoveAt(i);
                    break;
                }
            }
        }
    }
}