namespace FitnessTracker.BackEnd.Models
{
    public class AppUser
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;

        // We never store plaintext passwords
        public byte[] PasswordHash { get; set; } = Array.Empty<byte>();
        public byte[] PasswordSalt { get; set; } = Array.Empty<byte>();
    }
}
