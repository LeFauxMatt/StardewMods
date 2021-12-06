namespace HelpForHire.Models
{
    internal record ChoreConfig
    {
        public ChoreConfig(int dailyRate, int unitRate = 0)
        {
            this.DailyRate = dailyRate;
            this.UnitRate = unitRate;
        }

        public bool Enabled { get; set; } = true;

        public int DailyRate { get; set; }

        public int UnitRate { get; set; }
    }
}