using GTA;
using GTA.Math;
using GTA.Native;
using System;
using System.Xml;
using System.Collections.Generic;
using System.Drawing;

namespace AdvancedWorld
{
    public class AdvancedWorld : Script
    {
        private static List<int> oldRelationships;
        private static List<int> newRelationships;
        private static int copID;
        private static int playerID;
        private static int count;

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
            oldRelationships = new List<int>
            {
                Function.Call<int>(Hash.GET_HASH_KEY, "CIVMALE"),
                Function.Call<int>(Hash.GET_HASH_KEY, "CIVFEMALE"),
                Function.Call<int>(Hash.GET_HASH_KEY, "SECURITY_GUARD"),
                Function.Call<int>(Hash.GET_HASH_KEY, "AMBIENT_GANG_LOST"),
                Function.Call<int>(Hash.GET_HASH_KEY, "AMBIENT_GANG_MEXICAN"),
                Function.Call<int>(Hash.GET_HASH_KEY, "AMBIENT_GANG_FAMILY"),
                Function.Call<int>(Hash.GET_HASH_KEY, "AMBIENT_GANG_BALLAS"),
                Function.Call<int>(Hash.GET_HASH_KEY, "AMBIENT_GANG_MARABUNTE"),
                Function.Call<int>(Hash.GET_HASH_KEY, "AMBIENT_GANG_CULT"),
                Function.Call<int>(Hash.GET_HASH_KEY, "AMBIENT_GANG_SALVA"),
                Function.Call<int>(Hash.GET_HASH_KEY, "AMBIENT_GANG_WEICHENG"),
                Function.Call<int>(Hash.GET_HASH_KEY, "AMBIENT_GANG_HILLBILLY"),
                Function.Call<int>(Hash.GET_HASH_KEY, "GANG_1"),
                Function.Call<int>(Hash.GET_HASH_KEY, "GANG_2"),
                Function.Call<int>(Hash.GET_HASH_KEY, "GANG_9"),
                Function.Call<int>(Hash.GET_HASH_KEY, "GANG_10"),
                Function.Call<int>(Hash.GET_HASH_KEY, "DEALER"),
                Function.Call<int>(Hash.GET_HASH_KEY, "PRIVATE_SECURITY"),
                Function.Call<int>(Hash.GET_HASH_KEY, "ARMY"),
                Function.Call<int>(Hash.GET_HASH_KEY, "PRISONER"),
                Function.Call<int>(Hash.GET_HASH_KEY, "FIREMAN"),
                Function.Call<int>(Hash.GET_HASH_KEY, "MEDIC")
            };

            newRelationships = new List<int>();
            copID = Function.Call<int>(Hash.GET_HASH_KEY, "COP");
            playerID = Function.Call<int>(Hash.GET_HASH_KEY, "PLAYER");
            count = 0;

            addOnCarNames = new List<string>();
            racerCarNames = new List<string>
            {
                "banshee",
                "cheetah",
                "elegy2",
                "entityxf",
                "infernus",
                "jester",
                "sentinel2",
                "vacca"
            };
            racerBikeNames = new List<string>
            {
                "akuma",
                "bati",
                "bati2",
                "carbonrs",
                "double",
                "nemesis",
                "pcj",
                "ruffian",
                "vader"
            };
            drivebyCarNames = new List<string>
            {
                "baller",
                "bison",
                "bodhi2",
                "buccaneer",
                "cavalcade2",
                "daemon",
                "dubsta",
                "emperor",
                "minivan",
                "patriot",
                "rancherxl",
                "regina",
                "sadler",
                "sanchez",
                "superd",
                "tailgater"
            };
            soldierCarNames = new List<string>
            {
                "rhino"
            };
            racingPosition = new List<Vector3>
            {
                new Vector3(1411.75f, 3012.3f, 41.1f),
                new Vector3(-981.0f, -2995.0f, 13.1f),
                new Vector3(2119.5f, 4806.3f, 41.2f),
                new Vector3(-80.7f, -1761.8f, 29.8f),
                new Vector3(-518.8f, -1210.0f, 18.33f),
                new Vector3(-714.85f, -932.65f, 19.2f),
                new Vector3(273.25f, -1261.05f, 29.3f),
                new Vector3(811.285f, -1030.9f, 26.4f),
                new Vector3(1212.5f, -1403.6f, 35.38f),
                new Vector3(2574.15f, 359.1f, 108.5f),
                new Vector3(1183.0f, -320.42f, 69.3f),
                new Vector3(629.05f, 274.0f, 103.0f),
                new Vector3(-1429.65f, -279.35f, 46.3f),
                new Vector3(-2087.57f, -321.15f, 13.1f),
                new Vector3(-1796.55f, 811.7f, 138.7f),
                new Vector3(-2558.05f, 2327.3f, 33.0f),
                new Vector3(48.188f, 2779.2f, 58.0f),
                new Vector3(263.0f, 2607.35f, 50.0f),
                new Vector3(1209.9f, 2658.7f, 37.9f),
                new Vector3(2539.25f, 2594.6f, 37.9f),
                new Vector3(2681.7f, 3266.4f, 55.2f),
                new Vector3(2009.1f, 3777.6f, 32.4f),
                new Vector3(1684.1f, 4932.15f, 42.2f),
                new Vector3(1705.5f, 6414.05f, 32.7f),
                new Vector3(171.62f, 6603.35f, 32.0f),
                new Vector3(-91.7f, 6423.2f, 31.6f),
                new Vector3(1043.25f, 2668.5f, 39.7f),
                new Vector3(-594.2f, 5025.4f, 140.3f)
            };
            models = new List<List<string>>
            {
                new List<string> { "a_m_m_og_boss_01", "mp_m_famdd_01", "g_f_y_families_01", "g_m_y_famca_01", "g_m_y_famdnf_01", "g_m_y_famfor_01" },
                new List<string> { "g_f_y_ballas_01", "g_m_y_ballaeast_01", "g_m_y_ballaorig_01", "g_m_y_ballasout_01" },
                new List<string> { "a_m_y_mexthug_01", "g_m_y_mexgoon_01", "g_m_y_mexgoon_02", "g_m_y_mexgoon_03", "g_f_y_vagos_01", "g_m_m_mexboss_01", "g_m_m_mexboss_02", "g_m_y_mexgang_01" },
                new List<string> { "g_m_y_lost_01", "g_m_y_lost_02", "g_m_y_lost_03", "g_f_y_lost_01", "g_m_m_armboss_01", "g_m_m_armgoon_01", "g_m_m_armlieut_01", "g_m_y_armgoon_02" },
                new List<string> { "g_m_y_azteca_01", "g_m_y_salvaboss_01", "g_m_y_salvagoon_01", "g_m_y_salvagoon_02", "g_m_y_salvagoon_03" },
                new List<string> { "g_m_y_korean_01", "g_m_y_korean_02", "g_m_y_korlieut_01", "g_m_m_korboss_01", "a_m_y_ktown_01", "a_m_y_ktown_02" },
                new List<string> { "a_m_m_eastsa_01", "a_m_m_eastsa_02", "a_m_m_malibu_01", "a_m_m_mexcntry_01", "a_m_m_mexlabor_01", "a_m_m_polynesian_01", "a_m_m_soucent_01", "a_m_m_soucent_03" },
                new List<string> { "a_m_m_stlat_02", "s_m_m_bouncer_01", "u_m_m_aldinapoli", "u_m_m_bikehire_01", "u_m_m_rivalpap", "u_m_m_willyfist", "u_m_y_baygor", "u_m_y_chip" },
                new List<string> { "s_m_y_dealer_01", "u_m_y_tattoo_01", "u_m_y_sbike", "u_m_y_party_01", "u_m_y_hippie_01", "u_m_y_gunvend_01", "u_m_y_fibmugger_01", "u_m_y_guido_01" },
                new List<string> { "s_f_y_hooker_01", "s_f_y_hooker_02", "s_f_y_hooker_03", "s_f_y_stripper_01", "s_f_y_stripper_02" }
            };

            DLCCheck();
            SetUp();
        }

        private static void DLCCheck()
        {
            if (Function.Call<bool>(Hash.IS_DLC_PRESENT, Function.Call<int>(Hash.GET_HASH_KEY, "mpapartment")))
            {
                racerCarNames.Add("verlierer2");
                drivebyCarNames.Add("baller3");
            }

            if (Function.Call<bool>(Hash.IS_DLC_PRESENT, Function.Call<int>(Hash.GET_HASH_KEY, "mpassault")))
            {
                racerCarNames.Add("dominator3");
                racerCarNames.Add("ellie");
                racerCarNames.Add("entity2");
                racerCarNames.Add("flashgt");
                racerCarNames.Add("gb200");
                racerCarNames.Add("jester3");
                racerCarNames.Add("hotring");
                racerCarNames.Add("taipan");
                racerCarNames.Add("tezeract");
                racerCarNames.Add("tyrant");
            }

            if (Function.Call<bool>(Hash.IS_DLC_PRESENT, Function.Call<int>(Hash.GET_HASH_KEY, "mpbiker")))
            {
                racerBikeNames.Add("defiler");
                racerBikeNames.Add("hakuchou2");
                racerBikeNames.Add("shotaro");
                racerBikeNames.Add("vortex");
                drivebyCarNames.Add("manchez");
                drivebyCarNames.Add("nightblade");
                drivebyCarNames.Add("wolfsbane");
            }

            if (Function.Call<bool>(Hash.IS_DLC_PRESENT, Function.Call<int>(Hash.GET_HASH_KEY, "mpbusiness")))
            {
                racerCarNames.Add("turismor");
            }

            if (Function.Call<bool>(Hash.IS_DLC_PRESENT, Function.Call<int>(Hash.GET_HASH_KEY, "mpbusiness2")))
            {
                racerCarNames.Add("zentorno");
                drivebyCarNames.Add("huntley");
                drivebyCarNames.Add("thrust");
            }

            if (Function.Call<bool>(Hash.IS_DLC_PRESENT, Function.Call<int>(Hash.GET_HASH_KEY, "mpchristmas2017")))
            {
                racerCarNames.Add("autarch");
                racerCarNames.Add("comet5");
                racerCarNames.Add("deluxo");
                racerCarNames.Add("gt500");
                racerCarNames.Add("neon");
                racerCarNames.Add("pariah");
                racerCarNames.Add("savestra");
                racerCarNames.Add("sc1");
                racerCarNames.Add("sentinel3");
                racerCarNames.Add("stromberg");
                racerCarNames.Add("viseris");
                racerCarNames.Add("z190");
                drivebyCarNames.Add("hermes");
                soldierCarNames.Add("khanjali");
            }

            if (Function.Call<bool>(Hash.IS_DLC_PRESENT, Function.Call<int>(Hash.GET_HASH_KEY, "mpexecutive")))
            {
                racerCarNames.Add("fmj");
                racerCarNames.Add("pfister811");
                racerCarNames.Add("prototipo");
                racerCarNames.Add("reaper");
                racerCarNames.Add("seven70");
            }

            if (Function.Call<bool>(Hash.IS_DLC_PRESENT, Function.Call<int>(Hash.GET_HASH_KEY, "mpgunrunning")))
            {
                racerCarNames.Add("ardent");
                racerCarNames.Add("cheetah2");
                racerCarNames.Add("torero");
                racerCarNames.Add("vagner");
                racerCarNames.Add("xa21");
                soldierCarNames.Add("apc");
            }

            if (Function.Call<bool>(Hash.IS_DLC_PRESENT, Function.Call<int>(Hash.GET_HASH_KEY, "mpheist")))
            {
                racerBikeNames.Add("lectro");
                drivebyCarNames.Add("enduro");
            }

            if (Function.Call<bool>(Hash.IS_DLC_PRESENT, Function.Call<int>(Hash.GET_HASH_KEY, "mphipster")))
            {
                drivebyCarNames.Add("glendale");
                drivebyCarNames.Add("warrener");
            }

            if (Function.Call<bool>(Hash.IS_DLC_PRESENT, Function.Call<int>(Hash.GET_HASH_KEY, "mpimportexport")))
            {
                racerCarNames.Add("comet3");
                racerCarNames.Add("elegy");
                racerCarNames.Add("italigtb");
                racerCarNames.Add("italigtb2");
                racerCarNames.Add("nero");
                racerCarNames.Add("nero2");
                racerCarNames.Add("penetrator");
                racerCarNames.Add("tempesta");
                racerBikeNames.Add("diablous");
                racerBikeNames.Add("diablous2");
                racerBikeNames.Add("fcr");
                racerBikeNames.Add("fcr2");
            }

            if (Function.Call<bool>(Hash.IS_DLC_PRESENT, Function.Call<int>(Hash.GET_HASH_KEY, "mpjanuary2016")))
            {
                racerCarNames.Add("banshee2");
                racerCarNames.Add("sultanrs");
            }

            if (Function.Call<bool>(Hash.IS_DLC_PRESENT, Function.Call<int>(Hash.GET_HASH_KEY, "mplowrider")))
            {
                drivebyCarNames.Add("buccaneer2");
                drivebyCarNames.Add("chino2");
                drivebyCarNames.Add("voodoo");
            }

            if (Function.Call<bool>(Hash.IS_DLC_PRESENT, Function.Call<int>(Hash.GET_HASH_KEY, "mplowrider2")))
            {
                drivebyCarNames.Add("faction3");
                drivebyCarNames.Add("sabregt2");
                drivebyCarNames.Add("virgo2");
            }

            if (Function.Call<bool>(Hash.IS_DLC_PRESENT, Function.Call<int>(Hash.GET_HASH_KEY, "mplts")))
            {
                racerBikeNames.Add("hakuchou");
                drivebyCarNames.Add("innovation");
            }

            if (Function.Call<bool>(Hash.IS_DLC_PRESENT, Function.Call<int>(Hash.GET_HASH_KEY, "mpluxe")))
            {
                racerCarNames.Add("feltzer3");
                racerCarNames.Add("osiris");
            }

            if (Function.Call<bool>(Hash.IS_DLC_PRESENT, Function.Call<int>(Hash.GET_HASH_KEY, "mpluxe2")))
            {
                racerCarNames.Add("t20");
                drivebyCarNames.Add("chino");
                drivebyCarNames.Add("vindicator");
            }

            if (Function.Call<bool>(Hash.IS_DLC_PRESENT, Function.Call<int>(Hash.GET_HASH_KEY, "mpsmuggler")))
            {
                racerCarNames.Add("cyclone");
                racerCarNames.Add("rapidgt3");
                racerCarNames.Add("visione");
            }

            if (Function.Call<bool>(Hash.IS_DLC_PRESENT, Function.Call<int>(Hash.GET_HASH_KEY, "mpspecialraces")))
            {
                racerCarNames.Add("gp1");
                racerCarNames.Add("infernus2");
                racerCarNames.Add("turismo2");
            }

            if (Function.Call<bool>(Hash.IS_DLC_PRESENT, Function.Call<int>(Hash.GET_HASH_KEY, "mpstunt")))
            {
                racerCarNames.Add("le7b");
                racerCarNames.Add("sheava");
                racerCarNames.Add("tyrus");
                drivebyCarNames.Add("bf400");
                drivebyCarNames.Add("gargoyle");
            }

            if (Function.Call<bool>(Hash.IS_DLC_PRESENT, Function.Call<int>(Hash.GET_HASH_KEY, "spupgrade")))
            {
                drivebyCarNames.Add("stalion");
            }

            if (Function.Call<bool>(Hash.IS_DLC_PRESENT, Function.Call<int>(Hash.GET_HASH_KEY, "pres")))
            {
                racerCarNames.Add("cheetah3");
                racerCarNames.Add("sentinel4");
                racerCarNames.Add("supergt");
                racerCarNames.Add("turismo");
                racerCarNames.Add("typhoon");
                racerCarNames.Add("uranus");
                racerBikeNames.Add("double2");
                racerBikeNames.Add("hakuchou3");
                racerBikeNames.Add("nrg900");
                drivebyCarNames.Add("huntley2");
                drivebyCarNames.Add("marbelle");
                soldierCarNames.Add("apc2");
            }
        }

        private static void SetUp()
        {
            XmlDocument doc = new XmlDocument();

            for (int time = 0; time < 500; time++)
            {
                doc.Load(@"scripts\\AdvancedWorld.xml");

                if (doc != null) break;
            }

            if (doc == null) return;

            XmlElement element = doc.DocumentElement;

            foreach (XmlElement e in element.SelectNodes("//Replace/model")) addOnCarNames.Add(e.GetAttribute("id"));
            foreach (XmlElement e in element.SelectNodes("//RaceCar/model")) racerCarNames.Add(e.GetAttribute("id"));
            foreach (XmlElement e in element.SelectNodes("//RaceBike/model")) racerBikeNames.Add(e.GetAttribute("id"));
            foreach (XmlElement e in element.SelectNodes("//Driveby/model")) drivebyCarNames.Add(e.GetAttribute("id"));
        }

        public static int NewRelationship(int type)
        {
            int newRel = World.AddRelationshipGroup(count.ToString());
            newRelationships.Add(newRel);
            count++;
            World.SetRelationshipBetweenGroups(Relationship.Hate, newRel, copID);

            switch (type)
            {
                case 0: // carjacker, aggressive, racer
                    {
                        foreach (int i in newRelationships) World.SetRelationshipBetweenGroups(Relationship.Hate, newRel, i);

                        break;
                    }

                case 1: // massacre, driveby
                    {
                        foreach (int i in oldRelationships) World.SetRelationshipBetweenGroups(Relationship.Hate, newRel, i);
                        foreach (int i in newRelationships) World.SetRelationshipBetweenGroups(Relationship.Hate, newRel, i);

                        World.SetRelationshipBetweenGroups(Relationship.Respect, newRel, newRel);
                        World.SetRelationshipBetweenGroups(Relationship.Respect, newRel, playerID);
                        break;
                    }

                case 2: // ganglist, solider
                    {
                        foreach (int i in newRelationships) World.SetRelationshipBetweenGroups(Relationship.Hate, newRel, i);

                        World.SetRelationshipBetweenGroups(Relationship.Hate, newRel, Function.Call<int>(Hash.GET_HASH_KEY, "ARMY"));
                        World.SetRelationshipBetweenGroups(Relationship.Respect, newRel, newRel);
                        World.SetRelationshipBetweenGroups(Relationship.Respect, newRel, playerID);
                        break;
                    }
            }

            return newRel;
        }

        public static void CleanUpRelationship(int relationship)
        {
            World.RemoveRelationshipGroup(relationship);

            if (newRelationships.Contains(relationship)) newRelationships.Remove(relationship);
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
                if (replacedList.Count < 7)
                {
                    ReplacedVehicle rv = new ReplacedVehicle(addOnCarNames[Util.GetRandomInt(addOnCarNames.Count)]);

                    if (rv.IsCreatedIn(radius))
                    {
                        replacedList.Add(rv);
                        Function.Call(Hash.FLASH_MINIMAP_DISPLAY);
                    }
                    else rv.Restore();
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
                            else cj.Restore();

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
                            else ad.Restore();

                            break;
                        }

                    case 2:
                        {
                            Vehicle[] nearbyVehicles = World.GetNearbyVehicles(Game.Player.Character.Position, radius);

                            if (nearbyVehicles.Length < 1)
                            {
                                nearbyVehicles = null;
                                break;
                            }

                            for (int trycount = 0; trycount < 5; trycount++)
                            {
                                Vehicle explosiveVehicle = nearbyVehicles[Util.GetRandomInt(nearbyVehicles.Length)];

                                if (Util.WeCanReplace(explosiveVehicle) && !Util.BlipIsOn(explosiveVehicle))
                                {
                                    Util.AddBlipOn(explosiveVehicle, 0.7f, BlipSprite.PersonalVehicleCar, BlipColor.Red, "Vehicle Explosion");
                                    explosiveVehicle.Explode();
                                    Function.Call(Hash.FLASH_MINIMAP_DISPLAY);

                                    break;
                                }
                                else explosiveVehicle = null;
                            }

                            nearbyVehicles = null;
                            break;
                        }

                    case 3:
                        {
                            Vehicle[] nearbyVehicles = World.GetNearbyVehicles(Game.Player.Character.Position, radius);

                            if (nearbyVehicles.Length < 1)
                            {
                                nearbyVehicles = null;
                                break;
                            }

                            for (int trycount = 0; trycount < 5; trycount++)
                            {
                                Vehicle undriveableVehicle = nearbyVehicles[Util.GetRandomInt(nearbyVehicles.Length)];

                                if (Util.WeCanReplace(undriveableVehicle) && !Util.BlipIsOn(undriveableVehicle))
                                {
                                    Util.AddBlipOn(undriveableVehicle, 0.7f, BlipSprite.PersonalVehicleCar, BlipColor.Yellow, "Vehicle on Fire");
                                    undriveableVehicle.EngineHealth = -900.0f;
                                    Function.Call(Hash.FLASH_MINIMAP_DISPLAY);

                                    break;
                                }
                                else undriveableVehicle = null;
                            }

                            nearbyVehicles = null;
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

                            int relationshipA = AdvancedWorld.NewRelationship(2);
                            int relationshipB = AdvancedWorld.NewRelationship(2);

                            if (relationshipA == 0 || relationshipB == 0) break;

                            World.SetRelationshipBetweenGroups(Relationship.Hate, relationshipA, relationshipB);

                            if (teamA.IsCreatedIn(radius, World.GetNextPositionOnSidewalk(safePosition.Around(5.0f)), models[teamANum], relationshipA, BlipColor.Green, "A Team") && teamB.IsCreatedIn(radius, World.GetNextPositionOnSidewalk(safePosition.Around(5.0f)), models[teamBNum], relationshipB, BlipColor.Red, "B Team"))
                            {
                                gangList.Add(teamA);
                                gangList.Add(teamB);

                                teamA.PerformTask();
                                teamB.PerformTask();

                                Function.Call(Hash.FLASH_MINIMAP_DISPLAY);
                            }
                            else
                            {
                                teamA.Restore();
                                teamB.Restore();
                            }

                            break;
                        }

                    case 5:
                        {
                            Vector3 safePosition = Util.GetSafePositionIn(radius);

                            if (safePosition.Equals(Vector3.Zero)) break;

                            Massacre ms = new Massacre();
                            int relationship = AdvancedWorld.NewRelationship(1);

                            if (relationship == 0) break;
                            if (ms.IsCreatedIn(radius, safePosition, relationship))
                            {
                                massacreList.Add(ms);
                                Function.Call(Hash.FLASH_MINIMAP_DISPLAY);
                            }
                            else ms.Restore();

                            break;
                        }

                    case 6:
                        {
                            Vector3 goal = racingPosition[Util.GetRandomInt(racingPosition.Count)];
                            Vector3 safePosition = Util.GetSafePositionIn(radius);
                            int random = Util.GetRandomInt(4);
                            int heading = Util.GetRandomInt(360);

                            if (safePosition.Equals(Vector3.Zero)) break;

                            for (int i = 0; i < 4; i++)
                            {
                                Racer r = null;

                                if (random == 0) r = new Racer(racerBikeNames[Util.GetRandomInt(racerBikeNames.Count)], goal);
                                else r = new Racer(racerCarNames[Util.GetRandomInt(racerCarNames.Count)], goal);

                                if (r.IsCreatedIn(radius, World.GetNextPositionOnStreet(safePosition, true), heading))
                                {
                                    racerList.Add(r);
                                    Function.Call(Hash.FLASH_MINIMAP_DISPLAY);
                                }
                                else r.Restore();
                            }

                            break;
                        }

                    case 7:
                        {
                            Vector3 safePosition = Util.GetSafePositionIn(radius);

                            if (safePosition.Equals(Vector3.Zero)) break;

                            Soldier sA = new Soldier(soldierCarNames[Util.GetRandomInt(soldierCarNames.Count)], soldierCarNames[Util.GetRandomInt(soldierCarNames.Count)]);
                            Soldier sB = new Soldier(soldierCarNames[Util.GetRandomInt(soldierCarNames.Count)], soldierCarNames[Util.GetRandomInt(soldierCarNames.Count)]);

                            int relationshipA = AdvancedWorld.NewRelationship(2);
                            int relationshipB = AdvancedWorld.NewRelationship(2);

                            if (relationshipA == 0 || relationshipB == 0) break;

                            World.SetRelationshipBetweenGroups(Relationship.Hate, relationshipA, relationshipB);

                            if (sA.IsCreatedIn(radius, safePosition, BlipColor.Green, relationshipA) && sB.IsCreatedIn(radius, safePosition, BlipColor.Red, relationshipB))
                            {
                                soldierList.Add(sA);
                                soldierList.Add(sB);

                                sA.PerformTask();
                                sB.PerformTask();
                                Function.Call(Hash.FLASH_MINIMAP_DISPLAY);
                            }
                            else
                            {
                                sA.Restore();
                                sB.Restore();
                            }

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
                            else db.Restore();

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
                if (l[i].ShouldBeRemoved()) l.RemoveAt(i);
            }
        }
    }
}