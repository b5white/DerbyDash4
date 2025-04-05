using DerbyDash.Data;

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
        public DateTime RaceDateTime { get; set; }
        public double Speed { get; set; } = 0;
        public double Distance { get; set; } = 0;
        public double TotalTime { get; set; } = 0;
        public int ImageId { get; set; } = -1;
        public string ImageUrl { get => $"Racecar{ImageId}.png"; }
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
            
            return Distance;
        }

        public void ResetFlexBasis(int itemCount, int gap) {
            // Calculate total gap and margin width
            int totalGapWidth = (itemCount - 1) * gap;
            int totalMarginWidth = itemCount * 2 * Margin;

            // Calculate available space
            string availableSpace = "(100% - " + (totalGapWidth + totalMarginWidth) + "px)";

            // Calculate flex-basis
            FlexBasis = $"calc({availableSpace} / {itemCount})";
        }

        public void InitializeFastEddyTimeIncrements(Random random, int index) {
            double currentTime = 0;
            double currentSpeed = 0;
            double currentDistance = 0;
            RaceDateTime = DateTime.Now;

            for (int i = 0; i < 10; i++) {
                double timeIncrement = ((random.NextDouble() * 2) + 1) * index;
                currentTime += timeIncrement;
                currentDistance += currentSpeed * timeIncrement;
                currentSpeed++; // Assumes speed increment of 1
                SpeedIncrements.Add(new SpeedIncrement {
                    Time = currentTime,
                    Speed = currentSpeed,
                    Distance = currentDistance
                });
               
            }
        }
    }
}
