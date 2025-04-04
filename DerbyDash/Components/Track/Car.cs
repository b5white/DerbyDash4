using DerbyDash.Data;
using DerbyDash.HelperUtilities;

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
            // Console.WriteLine($"{index}, {currentTime}, {lastIndex}, {lastTime}, {timeElapsed}, {lastDistance}, {additionalDistance}, {Distance}, {lastSpeed}");
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

            double[,] times = {
                { 6.06282234191895, 7.19820785522461, 8.50334739685059,10.16606712341310,11.96782112121580,15.98963165283200 },
                { 8.16321277618408, 9.86215686798096,12.87038707733150,15.70635509490970,19.12981796264650,0},
                { 3.53985285758972, 4.89573097229004, 6.66387176513672, 7.97128963470459,10.10218238830570,0 },
                { 2.59133434295654, 3.92602872848511, 4.98245096206665, 6.87673616409302, 9.09002208709717,16.00716781616210 },
                { 4.26439666748047, 5.69009923934937, 7.07970571517944, 8.97908496856689,35.29488754272460,36.70681762695310 }
            };

            double[,] distances = {
                { 0,  7.19820785522461, 24.20490264892580, 54.70310401916500,102.57438850402800,182.52254676818800 },
                { 0,  9.86215686798096, 35.60293102264400, 82.72199630737300,159.24126815795900,-1, },
                { 0,  4.89573097229004, 18.22347450256350, 42.13734340667720, 82.54607295989990,-1, },
                { 0,  3.92602872848511, 13.89093065261840, 34.52113914489750, 70.88122749328610,150.91706657409700 },
                { 0,  5.69009923934937, 19.84951066970830, 46.78676557540890,187.96631574630700,371.50040388107300 }
            };

            for (int i = 0; i < 6; i++) {
                double timeIncrement = ((random.NextDouble() * 2) + 1) * index; // Random time increment between 2 and 10
                currentTime = times[index, i];
                currentDistance += currentSpeed * timeIncrement;
                if (!Utilities.AreDoublesEqual(currentDistance, distances[index, i], 0.0001)) {
                    Console.WriteLine($">Car:{index}, index:{i}, currentDistance:{currentDistance}, currentTime:{currentTime}, currentSpeed:{currentSpeed}, currentDistance:{currentDistance}");
                }
                currentSpeed++; // Assumes speed increment of 1
                SpeedIncrements.Add(new SpeedIncrement {
                    Time = currentTime,
                    Speed = currentSpeed,
                    Distance = currentDistance
                });
                //Console.WriteLine($">index:{index}, timeIncrement:{timeIncrement}, currentTime:{currentTime}, currentSpeed:{currentSpeed}, currentDistance:{currentDistance}");
            }
        }
    }
}
