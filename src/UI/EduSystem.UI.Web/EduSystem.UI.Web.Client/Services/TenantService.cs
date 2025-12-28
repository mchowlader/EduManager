namespace EduSystem.UI.Web.Client.Services;

public class TenantTheme
{
    public string PrimaryColor { get; set; } = "#594AE2"; // Default Purple
    public string SecondaryColor { get; set; } = "#FF4081"; // Default Pink
    public string SidebarColor { get; set; } = "#1E1E2D"; // Default Dark
    public string? LogoUrl { get; set; } = "/icon.svg";
}

public interface ITenantService
{
    Task<TenantTheme> GetTenantThemeAsync();
}

public class TenantService : ITenantService
{
    public async Task<TenantTheme> GetTenantThemeAsync()
    {
        // SIMULATION: Returning the "Original/Default" Theme
        // This can be expanded later to fetch from an API if needed in WASM
        return await Task.FromResult(new TenantTheme
        {
            PrimaryColor = "#667eea", // Brand Panel Gradient Start
            SecondaryColor = "#764ba2", // Brand Panel Gradient End
            SidebarColor = "#667eea", // Sidebar background (start color)
            LogoUrl = null // Set to null to show the default MudIcon (School)
        });
    }
}
