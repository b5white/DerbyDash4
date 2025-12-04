using DerbyDash.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace DerbyDash.Components.Pages {

    public partial class Home: ComponentBase {
        [Inject] public required IJSRuntime JSRuntime { get; set; }
        [Inject] public required ILogger<Home> Logger { get; set; }
        [Inject] public required NavigationManager NavManager { get; set; }
        [Inject] public required IRaceTeamService RaceTeamService { get; set; }

        private string appName = "TurboFlash";
        private string selectedGif = "";
        private string? lastPlayedRace;

        private string[] gifs = {
            "images/horse-running1.gif",
            "images/horse-running2.gif",
            "images/horse-running3.gif",
            "images/horse-running4.gif",
            "images/horse-running5.gif",
            "images/horse-running6.gif",
            "images/horse-running7.gif"
        };

        protected async override Task OnInitializedAsync() {
            // Select a random GIF from the array
            Random random = new Random();
            int index = random.Next(gifs.Length);
            selectedGif = gifs[index];
            await base.OnInitializedAsync();
            return;
        }

    // Note: Avoid calling StateHasChanged in lifecycle methods like OnParametersSetAsync to prevent render loops

        // Method to handle the Start Racing button click
        private async Task StartRacing() {
            try {
                Logger.LogInformation("StartRacing button clicked");

                // Get the last played race from the service
                lastPlayedRace = await RaceTeamService.GetLastPlayedRaceAsync();

                Logger.LogInformation($"Retrieved last played race: {lastPlayedRace ?? "null"}");

                // Navigate accordingly
                if (!string.IsNullOrEmpty(lastPlayedRace)) {
                    Logger.LogInformation($"Navigating to last played race: /race/{lastPlayedRace}");
                    NavManager.NavigateTo($"/race/{lastPlayedRace}", forceLoad: true);
                } else {
                    Logger.LogInformation("No last played race found, navigating to /RaceSetsMenu");
                    NavManager.NavigateTo("/RaceSetsMenu", forceLoad: true);
                }
            } catch (Exception ex) {
                Logger.LogError(ex, "Error retrieving last played race");
                Logger.LogInformation("Redirecting to /RaceSetsMenu");
                NavManager.NavigateTo("/RaceSetsMenu", forceLoad: true);
            }
        }
    }
}
