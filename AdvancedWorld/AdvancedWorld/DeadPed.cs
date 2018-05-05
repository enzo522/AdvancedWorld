using GTA;
using GTA.Math;

namespace AdvancedWorld
{
    public class DeadPed
    {
        private Ped p;

        public DeadPed(Ped p)
        {
            this.p = p;
            this.IsChecked = false;
            this.Position = p.Position;
        }

        public bool IsChecked { get; set; }
        public Vector3 Position { get; }
        public bool Equals(Ped ped) { return p.Equals(ped); }
    }
}