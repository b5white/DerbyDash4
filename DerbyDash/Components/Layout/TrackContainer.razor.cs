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
            bool anyCarAtTop = Track.Cars.Any(car => car.Top <= 0);
            Console.WriteLine($"StartLine Top: {anyCarAtTop}");

            if (anyCarAtTop) {
                var speedMultiplier = 5;
                var baseOffset = (Distance * speedMultiplier) % 280;
                return $"transform: translateY({baseOffset}px)";
            }
            return "transform: translateY(0)";
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
