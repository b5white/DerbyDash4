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

            double[,] times = {
                { 6.06282234191895, 7.19820785522461, 8.50334739685059,10.16606712341310,11.96782112121580,15.98963165283200 },
                { 8.16321277618408, 9.86215686798096,12.87038707733150,15.70635509490970,19.12981796264650,0},
                { 3.53985285758972, 4.89573097229004, 6.66387176513672, 7.97128963470459,10.10218238830570,0 },
                { 2.59133434295654, 3.92602872848511, 4.98245096206665, 6.87673616409302, 9.09002208709717,16.00716781616210 },
                { 4.26439666748047, 5.69009923934937, 7.07970571517944, 8.97908496856689,35.29488754272460,36.70681762695310 }
            };

            double[,] distances = {
                { 0, 1.13538551330566, 3.74566459655762, 8.73382377624515, 15.940839767456, 36.0498924255369 },
                { 0, 1.69894409179688, 7.71540451049796, 16.2233085632326, 29.9171600341797, 0 },
                { 0, 1.35587811470032, 4.89215970039368, 8.81441330909729, 17.3379843235017, 0 },
                { 0, 1.33469438552857, 3.44753885269165, 9.13039445877076, 17.9835381507874, 52.569266796112 },
                { 0, 1.4257025718689, 4.20491552352904, 9.90305328369139, 115.166263580322, 122.225914001465 }
            };

            for (int i = 0; i < 6; i++) {
                currentTime = times[index, i];
                if (i == 0) {
                    currentDistance = 0;
                } else {
                    currentDistance += currentSpeed * (currentTime - times[index, i]);
                }
                if (!Utilities.AreDoublesEqual(currentDistance, distances[index, i], 0.0001)) {
                    Console.WriteLine($">Car:{index}, index:{i}, currentDistance:{currentDistance}, currentTime:{currentTime}, currentSpeed:{currentSpeed}, currentDistance:{currentDistance}");
                }
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
