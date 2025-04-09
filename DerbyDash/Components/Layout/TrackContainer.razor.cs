using DerbyDash.Components.Track;
using Microsoft.AspNetCore.Components;

namespace DerbyDash.Components.Layout {
    public partial class TrackContainer: IDisposable {
        [Parameter]
        public RaceComponents Track { get; set; } = new();

        [Parameter]
        public bool IsMoving { get; set; }

        [Parameter]
        public double Distance { get; set; }

        private ElementReference startLineElement;
        private bool startLineHidden = false;
        private System.Threading.Timer animationTimer;
        private DateTime animationStartTime;
        private bool animationStarted = false;

        private const float START_LINE_INITIAL_TOP = 490f; // Match your CSS value
        private const float START_LINE_FINAL_TOP = -70f;   // Where the start line ends up


        // Add a constant for speed scaling to match visual effects
        private const double SPEED_SCALING_FACTOR = RaceComponents.SPEED_MULTIPLIER;

        protected override void OnInitialized() {
            // Set up a timer to check if the start line should be hidden
            animationTimer = new System.Threading.Timer(CheckStartLineVisibility, null, 1000, 200);
        }

        private void CheckStartLineVisibility(object state) {
            // Only proceed if animation is running
            if (Track.IsAnyCarAtTop && !Track.IsFinishLineVisible) {
                // If animation just started, record the start time
                if (!animationStarted) {
                    animationStarted = true;
                    animationStartTime = DateTime.Now;
                    return; // Wait for next check
                }

                // Calculate how long the animation has been running
                TimeSpan animationDuration = DateTime.Now - animationStartTime;

                // Get the current animation duration based on speed class
                double currentAnimationDuration = GetAnimationDurationForSpeedClass(Track.SpeedClass);

                // Calculate when the start line should be hidden
                double hidePercentage = CalculateHidePercentage(Track.SpeedClass);
                double hideTimeSeconds = currentAnimationDuration * hidePercentage;

                // Calculate the current position of the start line based on animation progress
                double animationProgress = (animationDuration.TotalSeconds % currentAnimationDuration) / currentAnimationDuration;

                // Update the start line's Top property to reflect its visual position
                // The start line moves from its initial position (e.g., 490px) to off-screen (e.g., -70px)
                const float START_LINE_INITIAL_TOP = 490f; // Adjust based on your CSS
                const float START_LINE_FINAL_TOP = -70f;   // Adjust based on your CSS
                float currentTop = START_LINE_INITIAL_TOP + (float)(animationProgress * (START_LINE_FINAL_TOP - START_LINE_INITIAL_TOP));

                // Update the start line's Top property
                Track.StartLine.Top = currentTop;

                // If enough time has passed, hide the start line
                if (animationDuration.TotalSeconds >= hideTimeSeconds && !startLineHidden) {
                    InvokeAsync(() => {
                        startLineHidden = true;
                        StateHasChanged();
                    });
                }
            } else {
                // Reset animation tracking if animation stops
                if (!Track.IsAnyCarAtTop) {
                    animationStarted = false;

                    // If we're at the beginning of the race, make sure start line is visible
                    if (Distance < 10 && startLineHidden) {
                        InvokeAsync(() => {
                            startLineHidden = false;
                            // Reset start line position
                            Track.StartLine.Top = 490f; // Initial position
                            StateHasChanged();
                        });
                    }
                }
            }
        }



        // Helper method to get the animation duration based on speed class
        private double GetAnimationDurationForSpeedClass(int speedClass) {
            double BaseAnimationSpeed = 8.0;
            // These values match the CSS animation durations in TrackContainer.razor.css
            speedClass = Math.Clamp(speedClass, 1, 15);
            return BaseAnimationSpeed / speedClass;
        }

        // Helper method to calculate the appropriate hide percentage based on speed class
        private double CalculateHidePercentage(int speedClass) {
            // For slower animations (lower speed classes), we can use a lower percentage
            // For faster animations (higher speed classes), we need a higher percentage
            // This ensures the start line disappears at visually consistent points
            if (speedClass <= 3)
                return 0.96;  // Slower speeds need less time before hiding
            else if (speedClass <= 7)
                return 0.6;  // Medium speeds
            else if (speedClass <= 11)
                return 0.5;  // Faster speeds
            else
                return 0.3;  // Very fast speeds need more time before hiding

            // Alternative approach: linear interpolation between 0.3 and 0.6
            // return 0.3 + ((double)speedClass - 1) / 14 * 0.3;
        }


        protected override void OnParametersSet() {
            // Update speed class based on the fastest car's speed, not just the player car
            double fastestSpeed = Track.Cars.Max(car => car.Speed);

            // Apply the scaling factor to match visual speed
            Track.UpdateSpeedClass((int)(fastestSpeed * SPEED_SCALING_FACTOR));

            // Reset start line visibility at the beginning of the race
            if (Distance < 1 && startLineHidden) {
                startLineHidden = false;
                animationStarted = false;
            }
        }

        private double GetAnimationProgress() {
            if (!animationStarted)
                return 0;

            double currentAnimationDuration = GetAnimationDurationForSpeedClass(Track.SpeedClass);
            TimeSpan animationDuration = DateTime.Now - animationStartTime;
            return (animationDuration.TotalSeconds % currentAnimationDuration) / currentAnimationDuration;
        }



        public void Dispose() {
            animationTimer?.Dispose();
        }
    }
}
