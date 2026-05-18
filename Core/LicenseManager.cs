using System.Threading.Tasks;

namespace Auto_Uploader
{
    public class LicenseManager
    {
        public sealed class StoredLicenseInfo
        {
            public string Key { get; init; } = "";
            public bool IsActive { get; init; }
            public System.DateTime? ExpiryDate { get; init; }
        }

        public static Task<bool> CheckLicenseOnlineAsync() => Task.FromResult(true);
        public static Task<StoredLicenseInfo?> GetStoredLicenseInfoAsync() => Task.FromResult<StoredLicenseInfo?>(null);
        public static string? GetStoredLicenseKey() => null;
    }
}
