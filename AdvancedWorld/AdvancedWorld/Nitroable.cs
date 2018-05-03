using GTA;
using GTA.Math;
using GTA.Native;
using System.Collections.Generic;

namespace AdvancedWorld
{
    public abstract class Nitroable : Criminal
    {
        private List<string> exhausts;
        private int nitroAmount;

        public Nitroable(AdvancedWorld.CrimeType type) : base(type)
        {
            exhausts = new List<string>
            {
                "exhaust",
                "exhaust_2",
                "exhaust_3",
                "exhaust_4",
                "exhaust_5",
                "exhaust_6",
                "exhaust_7",
                "exhaust_8",
                "exhaust_9",
                "exhaust_10",
                "exhaust_11",
                "exhaust_12",
                "exhaust_13",
                "exhaust_14",
                "exhaust_15",
                "exhaust_16"
            };
            nitroAmount = 300;
        }

        private bool CanSafelyUseNitroBetween(Vector3 v1, Vector3 v2)
        {
            RaycastResult r = World.Raycast(v1, v2, IntersectOptions.Everything);

            return !r.DitHitAnything;
        }

        public void CheckNitroable()
        {
            if (spawnedVehicle.Speed > 15.0f && spawnedVehicle.CurrentGear > 0 && CanSafelyUseNitroBetween(spawnedVehicle.Position, spawnedVehicle.ForwardVector * 1.5f))
            {
                if (nitroAmount > 0)
                {
                    spawnedVehicle.EnginePowerMultiplier = 7.0f;
                    spawnedVehicle.EngineTorqueMultiplier = 7.0f;

                    float pitch = Function.Call<float>(Hash.GET_ENTITY_PITCH, spawnedVehicle);

                    if (Function.Call<bool>(Hash.HAS_NAMED_PTFX_ASSET_LOADED, "core"))
                    {
                        foreach (string exhaust in exhausts)
                        {
                            if (spawnedVehicle.HasBone(exhaust))
                            {
                                float scale = spawnedVehicle.Speed / 50;
                                Vector3 offset = spawnedVehicle.GetBoneCoord(exhaust);
                                Vector3 exhPosition = spawnedVehicle.GetOffsetFromWorldCoords(offset);
                                Function.Call(Hash._SET_PTFX_ASSET_NEXT_CALL, "core");
                                Function.Call<bool>(Hash.START_PARTICLE_FX_NON_LOOPED_ON_ENTITY, "veh_backfire", spawnedVehicle, exhPosition.X, exhPosition.Y, exhPosition.Z, 0.0f, pitch, 0.0f, scale, false, false, false);
                            }
                        }
                    }
                    else Function.Call(Hash.REQUEST_NAMED_PTFX_ASSET, "core");

                    nitroAmount -= 2;
                }
                else
                {
                    spawnedVehicle.EnginePowerMultiplier = 1.0f;
                    spawnedVehicle.EngineTorqueMultiplier = 1.0f;
                    nitroAmount = 0;
                }
            }
            else
            {
                if (nitroAmount < 300) nitroAmount++;
                else nitroAmount = 300;
            }
        }
    }
}
