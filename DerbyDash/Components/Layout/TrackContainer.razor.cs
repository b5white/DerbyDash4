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
        private double previousOffset = 0;
        private const double SCROLL_MULTIPLIER = 1.5;
        private const double ANIMATION_DURATION_BASE = 0.05;

        private string GetLaneStyle() {
            if (!Track.IsAnyCarAtTop) {
                return "transform: translateY(0)";
            }

            double scrollSpeed = Speed * SCROLL_MULTIPLIER;
            double currentOffset = Track.LaneOffset;
            
            // Calculate smooth transition
            string transform = $"transform: translateY({currentOffset}px)";
            string transition = $"transition: transform {ANIMATION_DURATION_BASE / Track.SpeedMultiplier}s linear";
            
            previousOffset = currentOffset;
            
            return $"{transform}; {transition}";
        }

        private string GetStartLineStyle() {
            if (!Track.IsAnyCarAtTop) {
                previousOffset = 0;
                return "transform: translateY(0)";
            }

            // Synchronize start line movement with lanes
            double scrollOffset = Track.LaneOffset;
            string transform = $"transform: translateY({scrollOffset}px)";
            string transition = $"transition: transform {ANIMATION_DURATION_BASE / Track.SpeedMultiplier}s linear";

            return $"{transform}; {transition}";
        }

        // private double lastOffset = 0;


        // private string GetStartLineStyle() {
        //     if (!Track.IsAnyCarAtTop) {
        //         lastOffset = 0;
        //         return "transform: translateY(0)";
        //     }

        //     // Only increase the offset when LaneOffset increases
        //     if (Track.LaneOffset > lastOffset) {
        //         lastOffset = Track.LaneOffset;
        //     }
            
        //     string transform = $"transform: translateY({lastOffset}px)";
        //     string transition = $"transition: transform {5f / Track.SpeedMultiplier}s linear";

        //     return $"{transform}; {transition}";
        // }



        // private string GetLaneStyle() {
        //     if (!Track.IsAnyCarAtTop) {

        //         return "transform: translateY(0)";
        //     }
        //     string transform = $"transform: translateY({Track.LaneOffset}px)";
        //     string transition = $"transition: transform {0.05f / Track.SpeedMultiplier}s linear";

        //     return $"{transform}; {transition}";
        // }


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
