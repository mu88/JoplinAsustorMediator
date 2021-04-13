namespace JoplinAsustorMediator
{
    public class AppSettings
    {
        public string JoplinUrl { get; init; } = string.Empty;

        public bool CustomTlsValidation { get; init; } = false;

        public string CertThumbprint { get; init; } = string.Empty;
    }
}