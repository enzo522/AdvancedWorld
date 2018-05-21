using GTA;
using GTA.Math;
using GTA.Native;
using System.Collections.Generic;

namespace AdvancedWorld
{
    public class Shield : AdvancedEntity
    {
        private List<string> shieldModels;
        private Ped owner;
        private Prop shield;
        private AttachState currentState;

        public enum AttachState
        {
            None,
            Inactive,
            MeleeCombat,
            NormalCombat,
            Reloading
        }

        public Shield(Ped p)
        {
            this.shieldModels = new List<string> { "prop_ballistic_shield", "prop_riot_shield" };
            this.owner = p;
        }

        public bool IsCreatedIn(Vector3 position)
        {
            if (position.Equals(Vector3.Zero)) return false;

            Model m = shieldModels[Util.GetRandomInt(shieldModels.Count)];
            shield = World.CreateProp(m, position, true, true);
            m.MarkAsNoLongerNeeded();

            if (!Util.ThereIs(shield)) return false;
            
            shield.IsPersistent = true;
            shield.IsFireProof = true;
            shield.IsMeleeProof = true;
            shield.IsInvincible = true;
            shield.IsVisible = false;
            shield.LodDistance = 1000;

            Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, owner, 0, false);
            owner.CanPlayGestures = false;
            currentState = AttachState.None;

            return true;
        }

        public override void Restore(bool instantly)
        {
            if (instantly)
            {
                if (Util.ThereIs(shield)) shield.Delete();
            }
            else
            {
                if (Util.ThereIs(shield)) shield.MarkAsNoLongerNeeded();
            }
        }

        public override bool ShouldBeRemoved()
        {
            if (!Util.ThereIs(shield)) return true;
            if (!Util.ThereIs(owner) || owner.IsDead || !shield.IsInRangeOf(Game.Player.Character.Position, 500.0f))
            {
                Restore(false);
                return true;
            }

            if (owner.IsInVehicle()) Detach(false);
            else if (owner.IsRagdoll) Detach(true);
            else if (owner.IsInMeleeCombat) Attach(AttachState.MeleeCombat);
            else if (owner.IsInCombat) Attach(AttachState.NormalCombat);
            else if (owner.IsReloading) Attach(AttachState.Reloading);
            else Attach(AttachState.Inactive);

            return false;
        }

        private void Attach(AttachState state)
        {
            if (currentState != state)
            {
                Vector3 position = Vector3.Zero;
                Vector3 rotation = Vector3.Zero;
                int boneIndex = 0;

                switch (state)
                {
                    case AttachState.Inactive:
                        {
                            position = new Vector3(0.09f, -0.255f, 0.26f);
                            rotation = new Vector3(5.44f, 1.08f, -363.882f);
                            boneIndex = Function.Call<int>(Hash.GET_PED_BONE_INDEX, owner, 0);
                            break;
                        }

                    case AttachState.MeleeCombat:
                        {
                            position = new Vector3(0.0f, -0.07f, 0.0f);
                            rotation = new Vector3(39.0f, 188.5f, 4.0f);
                            boneIndex = Function.Call<int>(Hash.GET_PED_BONE_INDEX, owner, 36029);
                            break;
                        }

                    case AttachState.NormalCombat:
                        {
                            position = new Vector3(-0.255f, 0.11f, -0.18f);
                            rotation = new Vector3(57.4701f, 198.83f, 25.4501f);
                            boneIndex = Function.Call<int>(Hash.GET_PED_BONE_INDEX, owner, 36029);
                            break;
                        }

                    case AttachState.Reloading:
                        {
                            position = new Vector3(0.5f, 0.045f, -0.04f);
                            rotation = new Vector3(-248.81f, 8.92f, -126.71f);
                            boneIndex = Function.Call<int>(Hash.GET_PED_BONE_INDEX, owner, 5232);
                            break;
                        }
                }

                if (position.Equals(Vector3.Zero) || rotation.Equals(Vector3.Zero) || boneIndex == 0) return;

                Detach(false);
                shield.AttachTo(owner, boneIndex, position, rotation);
                shield.IsVisible = true;
                Function.Call(Hash.SET_WEAPON_ANIMATION_OVERRIDE, owner, Function.Call<int>(Hash.GET_HASH_KEY, "Gang1H"));

                currentState = state;
            }
        }

        private void Detach(bool shouldBeVisible)
        {
            if (currentState != AttachState.None)
            {
                shield.Detach();
                shield.IsVisible = shouldBeVisible;
                Function.Call(Hash.SET_WEAPON_ANIMATION_OVERRIDE, owner, Function.Call<int>(Hash.GET_HASH_KEY, "Default"));

                currentState = AttachState.None;
            }
        }
    }
}