namespace GreenEye.Models
{
    //public class CropDiseaseHistory
    //{
    //    public int Id { get; set; }
    //    public string? PredicatedDisease { get; set; }
    //    public string? Cause { get; set; }
    //    public string? PeakSeason { get; set; }
    //    public string? Remedy { get; set; }
    //    public string? ImageUrl { get; set; }
    //    public DateTime SentAt { get; set; }
    //    public string? Confidence { get; set; }
    //}

    public class CropDiseaseHistory
    {
        public int Id { get; set; }
        
        public string? ImageUrl { get; set; }

        public string? PredicatedDisease { get; set; }

        public string? Cause { get; set; }

        public string? Remedy { get; set; }

        public string? PeakSeason { get; set; }

        public double Confidence { get; set; }

        public DateTime SentAt { get; set; }

        public bool IsDeleted { get; set; } = false;

        public string? UserId { get; set; }   // Identity User
        public ApplicationUser? User { get; set; }
    }

}
