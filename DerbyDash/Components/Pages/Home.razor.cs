using DerbyDash.Components.Layout;
using Microsoft.AspNetCore.Components;

namespace DerbyDash.Components.Pages {

    public partial class Home: ComponentBase {
        private string appName = "Derby Dash";
        private string selectedGif = "";

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
    }
}
