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

<<<<<<< HEAD
        [Parameter]
        public double LaneOffset { get; set; }

        [Parameter]
        public bool IsAnyCarAtTop { get; set; }

        [Parameter]
        public float SpeedMultiplier { get; set; } = 1.0f;

        [Parameter]
        public double ContinuousOffset { get; set; }

        private string GetStartLineStyle() {
            if (!IsAnyCarAtTop) {
                // Keep start line static until a car reaches top
                return "transform: translateY(0)";
            }
            
            // Begin movement after car reaches top
            string transform = $"transform: translateY({ContinuousOffset}px)";
            string transition = $"transition: transform {SpeedMultiplier}s linear";
            
            return $"{transform}; {transition}";
        }



=======
>>>>>>> f0123cc547cd293069d6395a5b1712dacaca1ec3
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
