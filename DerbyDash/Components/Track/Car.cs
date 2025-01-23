using DerbyDash.Components.Layout;

namespace DerbyDash.Components.Track {
    public class Car {
        public List<SpeedIncrement> SpeedIncrements { get; set; } = new List<SpeedIncrement>();
        public float Top { get; set; } = 200;
        public string TopStr {
            get {
                return Top.ToString() + "px";
            }
        }

        public int Margin = 0;
        public string MarginStr {
            get {
                return Margin.ToString() + "px";
            }
        }
        public int index;
        public double Speed { get; set; } = 0;
        public double Distance { get; set; } = 0;
        public string ImageUrl { get; set; } = "";
        public string FlexBasis { get; set; } = $"calc((100% - 170px) / 6)";

        // Method to calculate current distance
        public double CalculateCurrentDistance(double currentTime) {
            int lastIndex = -1;
            double lastTime = 0;
            double lastDistance = 0;
            double lastSpeed = 0;
            for (int i = 0; i < SpeedIncrements.Count; i++) {
                if (SpeedIncrements[i].Time <= currentTime) {
                    lastIndex = i;
                } else {
                    break;
                }
            }
            if (lastIndex != -1) {
                lastTime = SpeedIncrements[lastIndex].Time;
                lastDistance = SpeedIncrements[lastIndex].Distance;
                lastSpeed = SpeedIncrements[lastIndex].Speed;
            }
            double timeElapsed = currentTime - lastTime;
            double additionalDistance = lastSpeed * timeElapsed;
            Distance = lastDistance + additionalDistance;
            Speed = lastSpeed;
            Console.WriteLine($"{index}, {currentTime}, {lastIndex}, {lastTime}, {timeElapsed}, {lastDistance}, {additionalDistance}, {Distance}, {lastSpeed}");
            return Distance;
        }

        public void ResetFlexBasis(int itemCount, int gap) {
            // Calculate total gap and margin width
            var totalGapWidth = (itemCount - 1) * gap;
            var totalMarginWidth = itemCount * 2 * Margin;

            // Calculate available space
            var availableSpace = "(100% - " + (totalGapWidth + totalMarginWidth) + "px)";

            // Calculate flex-basis
            FlexBasis = $"calc({availableSpace} / {itemCount})";
        }

        public void InitializeFastEddyTimeIncrements(Random random) {
            double currentTime = 0;
            double currentSpeed = 0;
            double currentDistance = 0;

            for (int i = 0; i < 10; i++) {
                double timeIncrement = random.NextDouble() * (14 - 5) + 5; // Random time increment between 2 and 10
                currentTime += timeIncrement;
                currentDistance += currentSpeed * timeIncrement;
                currentSpeed++; // Assumes speed increment of 1
                SpeedIncrements.Add(new SpeedIncrement {
                    Time = currentTime,
                    Speed = currentSpeed,
                    Distance = currentDistance
                });
                Console.WriteLine($"{index}, {timeIncrement}, {currentTime}, {currentSpeed}, {currentDistance}");
            }
        }
    }

}
