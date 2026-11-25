namespace MyEmploylog;

public static class AppConfig
{
    // From your Entra app registration (Overview page)
    public const string ClientId = "e2f3d77e-054b-40b7-a849-67a9b635d484";
    public const string TenantId = "c2f5d25b-7e3e-451b-bd40-6d78d7aa9ffc";

    // Must match ApplicationId in MyEmploylog.csproj (used for the iOS redirect URI)
    public const string BundleId = "mu.ceb.myemploylog";
}
