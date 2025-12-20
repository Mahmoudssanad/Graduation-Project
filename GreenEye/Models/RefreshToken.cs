namespace GreenEye.Models
{
    [Owned]
    public class RefreshToken
    {
        public string? Token { get; set; }
        public DateTime ExpiresOn { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? RevokedOn { get; set; } // الالغاء
        public bool? IsRevoked { get; set; } // الالغاء

        // (Fileds) => so not exist on table
        public bool IsExpired => DateTime.UtcNow >= ExpiresOn;
        public bool IsActive => !IsExpired && RevokedOn == null;
    }
}
