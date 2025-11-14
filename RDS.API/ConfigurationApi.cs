namespace RDS.API;

public static class ConfigurationApi
{
    public static string ConnectionString { get; set; } = string.Empty;

    public const string CorsPolicyName = "wasm";
    public static string BackendUrl { get; set; } = string.Empty;
    public static string FrontendUrl { get; set; } = string.Empty;
}