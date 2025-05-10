using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace DerbyDash.Components.Pages {

    public partial class Home: ComponentBase {
        [Inject]
        public required NavigationManager NavManager { get; set; }

        [Inject]
        public required IJSRuntime JSRuntime { get; set; }

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

        // Method to handle the Start Racing button click
        private async Task StartRacing() {
            // Get the last played race from cookie
            lastPlayedRace = await JSRuntime.InvokeAsync<string>("getCookie", "lastPlayedRace");

            // If there's a last played race, refresh the cookie with a new 90-day expiration and navigate to it
            if (!string.IsNullOrEmpty(lastPlayedRace)) {
                // Refresh the cookie with a new 90-day expiration
                await JSRuntime.InvokeVoidAsync("setCookie", "lastPlayedRace", lastPlayedRace, 90);

                // Navigate to the last played race
                NavManager.NavigateTo($"/race/{lastPlayedRace}");
            } else {
                // Default navigation if no last played race is found
                NavManager.NavigateTo("/RaceSetsMenu");
            }
        }
    }
}
