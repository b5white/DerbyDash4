namespace DerbyDash.Shared.Track {
    public class CarDto {
        public int Index { get; set; }
        public int ImageId { get; set; }
        public int RaceId { get; set; }
        public double TotalTime { get; set; }
        public DateTime RaceDateTime { get; set; }
        public List<SpeedIncrementData> SpeedIncrements { get; set; } = new();
        public float Top { get; set; } = 200;
        public double Speed { get; set; } = 0;
        public double Distance { get; set; } = 0;
        public string ImageUrl { get; set; } = "";
        public string FlexBasis { get; set; } = "calc((100% - 170px) / 6)";
    }

    public class RaceComponentDto {
        public float Top { get; set; } = 200;
        public string ImageUrl { get; set; } = "";
    }

    public class RaceTrackDto {
        public List<CarDto> Cars { get; set; } = new();
        public RaceComponentDto StartLine { get; set; } = new();
        public RaceComponentDto FinishLine { get; set; } = new();
        public int ProblemId { get; set; }
        public int RacerId { get; set; }
    }

    public class SpeedIncrementData {
        public double Time { get; set; }
        public double Speed { get; set; }
        public double Distance { get; set; }
    }
}
