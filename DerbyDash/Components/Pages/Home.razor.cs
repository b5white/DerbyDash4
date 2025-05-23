﻿using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace DerbyDash.Components.Pages {

    public partial class Home: ComponentBase {
        [Inject]
        public required IJSRuntime JSRuntime { get; set; }

        [Inject]
        public required ILogger<Home> Logger { get; set; }

        private string appName = "Derby Dash";
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

        protected override void OnInitialized() {
            // Select a random GIF from the array
            Random random = new Random();
            int index = random.Next(gifs.Length);
            selectedGif = gifs[index];
        }

        [Inject]
        public required NavigationManager NavManager { get; set; }

        // Method to handle the Start Racing button click
        private async Task StartRacing() {
            try {
                // TODO reimplement this
                // Get the last played race from the database
                //   lastPlayedRace = await RaceService.GetLastPlayedRaceAsync();
                await Task.CompletedTask; // Just to use 'await'

                // If there's a last played race, navigate to it
                if (!string.IsNullOrEmpty(lastPlayedRace)) {
                    Logger.LogInformation($"Navigating to last played race: {lastPlayedRace}");

                    // Navigate to the last played race using NavigationManager instead of RedirectManager
                    NavManager.NavigateTo($"/race/{lastPlayedRace}");
                } else {
                    Logger.LogInformation("No last played race found, navigating to race selection menu");

                    // Default navigation if no last played race is found
                    NavManager.NavigateTo("/RaceSetsMenu");
                }
            } catch (Exception ex) {
                Logger.LogError(ex, "Error retrieving last played race");

                // Navigate to race selection menu if there's an error
                NavManager.NavigateTo("/RaceSetsMenu");
            }
        }
    }
}
