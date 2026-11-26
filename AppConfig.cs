namespace MyEmploylog;

public static class AppConfig
{
    // From your Entra app registration (Overview page)
    public const string ClientId = "";
    public const string TenantId = "";

    // Must match ApplicationId in MyEmploylog.csproj (used for the iOS redirect URI)
    public const string BundleId = "mu.ceb.myemploylog";
}
