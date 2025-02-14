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

        [Parameter]
        public double LaneOffset { get; set; }

        [Parameter]
        public bool IsAnyCarAtTop { get; set; }

        private string GetLaneStyle() {
            return IsAnyCarAtTop ? $"transform: translateY({LaneOffset}px)" : "transform: translateY(0)";
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
