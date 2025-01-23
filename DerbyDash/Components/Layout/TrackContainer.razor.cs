using Microsoft.AspNetCore.Components;
using System.Drawing;
using DerbyDash.Components.Track;

namespace DerbyDash.Components.Layout {
    public partial class TrackContainer {
        [Parameter]
        public RaceComponents Track { get; set; } = new();


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
