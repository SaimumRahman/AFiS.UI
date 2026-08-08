namespace JM.UITests;

public static class TestSettings
{
    public const string UiBaseAddress = "http://localhost:5000";
    public const string ApiBaseAddress = "https://localhost:2000";
    public const string AuthBaseAddress = "http://localhost:9000";

    public const string Username = "cubictech@gmail.com";
    public const string Password = "123Abc*";

    // Playwright
    public const bool Headless = true;
    public const string? LaunchOptionsTraceName = null; // set to "trace.zip" to record
}
