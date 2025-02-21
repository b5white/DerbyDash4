using DerbyDash.Components.Track;
using Microsoft.AspNetCore.Components;

namespace DerbyDash.Components.Layout {
    public partial class TrackContainer {
        [Parameter]
        public RaceComponents Track { get; set; } = new();

        [Parameter]
        public bool IsMoving { get; set; }

        [Parameter]
        public double Speed { get; set; }

        [Parameter]
        public double Distance { get; set; }

        private string GetLaneStyle() {
            if (!Track.IsAnyCarAtTop) {
                return "transform: translateY(0)";
            }
            string transform = $"transform: translateY({Track.LaneOffset}px)";
            string transition = $"transition: transform {0.05f / Track.SpeedMultiplier}s linear";

            return $"{transform}; {transition}";
        }


        //protected override void OnAfterRender(bool firstRender) {
        //    // You can add any additional logic here if needed
        //}
    }

    public class SpeedIncrement {
        public double Time { get; set; }
        public double Speed { get; set; }
        public double Distance { get; set; }
    }
}
