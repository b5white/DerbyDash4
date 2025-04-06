using DerbyDash.Components.Track;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;

namespace DerbyDash.Components.Layout {
    public partial class TrackContainer : IDisposable {
        [Parameter]
        public RaceComponents Track { get; set; } = new();

        [Parameter]
        public bool IsMoving { get; set; }

        [Parameter]
        public double Speed { get; set; }

        [Parameter]
        public double Distance { get; set; }
        
        [Inject]
        private IJSRuntime JSRuntime { get; set; }
        
        private ElementReference startLineElement;
        private bool startLineHidden = false;
        private System.Threading.Timer animationTimer;
        private DateTime animationStartTime;
        private bool animationStarted = false;
        
        protected override void OnInitialized()
        {
            // Set up a timer to check if the start line should be hidden
            animationTimer = new System.Threading.Timer(CheckStartLineVisibility, null, 1000, 200);
        }
        
        private void CheckStartLineVisibility(object state)
        {
            // Only proceed if animation is running
            if (Track.IsAnyCarAtTop && !Track.IsFinishLineVisible)
            {
                // If animation just started, record the start time
                if (!animationStarted)
                {
                    animationStarted = true;
                    animationStartTime = DateTime.Now;
                    return; // Wait for next check
                }
                
                // Calculate how long the animation has been running
                TimeSpan animationDuration = DateTime.Now - animationStartTime;
                
                // Calculate when the start line should be hidden based on speed class
                // Speed class 1 = 4s animation, Speed class 15 = 0.27s animation
                double hideTimeSeconds = 4.0 / Math.Max(1, Track.SpeedClass) * 0.8;
                
                // If enough time has passed, hide the start line
                if (animationDuration.TotalSeconds >= hideTimeSeconds && !startLineHidden)
                {
                    InvokeAsync(() => {
                        startLineHidden = true;
                        StateHasChanged();
                    });
                }
            }
            else
            {
                // Reset animation tracking if animation stops
                if (!Track.IsAnyCarAtTop)
                {
                    animationStarted = false;
                    
                    // If we're at the beginning of the race, make sure start line is visible
                    if (Distance < 10 && startLineHidden)
                    {
                        InvokeAsync(() => {
                            startLineHidden = false;
                            StateHasChanged();
                        });
                    }
                }
            }
        }
        
        protected override void OnParametersSet() {
            // Update speed class based on current speed
            Track.UpdateSpeedClass((int)Speed);
            
            // Reset start line visibility at the beginning of the race
            if (Distance < 1 && startLineHidden)
            {
                startLineHidden = false;
                animationStarted = false;
            }
        }
        
        public void Dispose()
        {
            animationTimer?.Dispose();
        }
    }
}
