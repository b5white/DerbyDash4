namespace DerbyDash {
    class TODO {
        // Filipe
        // DONE Hide the 2 factor link on the Profile page, AKA "/Account/Manage".
        //      Don't remove the code because we might want to add it back in later.
        // DONE Add a Preferences page with a link to it from the Profile page.
        //      Add a way for them to choose their avatar and then show it on the front page and the race page.
        // DONE Link the Start Race button to starting the race with the most recent.
        // DONE Default the Remeber Me to true, rename, use to save cookies or not.
        // TODO Repeat for Registration page and pass to email conf
        // DONE FAQ page
        // DONE Add a cookie service to handle all cookie read/write actions like save the last racer.
        // DONE Add a feedback page so users can report errors
        // TODO Add anchors on the FAQ page so we can link to the specific sections, like FAQ#subscription
        // DONE Subscription page
        // DONE Subscription service with the fake data
        // DONE Add a link to the FAQ page from the Subscription page.
        // DONE Move avatar to Racer
        // DONE Add an explainer page before the login page 
        // DONE Add "You need to log in to race." to all menus if they are not logged in.
        // TODO Handle the screen shift when using a phone so can still see top of page.
        // DONE Move Get/Save LastRace from Race to Racer Service
        // DONE Create a UserService to handle all user related actions
        //    GetUserId
        //    IsLoggedIn
        //    Anything else using GetAuthenticationStateAsync
        // TODO Save the Race position in the list at time it was first run, in Race
        // TODO Get rid of anything related to saving race count in the database
        //    We'll calculate it based on the number of races in the DB.
        // TODO Send a date range to the GetCount routines indicating AllTime.
        //    Or, instead of sending a data range, we could send a range type.
        //    AllTime, Last Week, Last30Days, Last90Days, LastYear
        //    That way, the routine itself is reponsible for the date details.
        // TODO In Race.Reset, break the FireAndForget code out into a separate routine.
        //    I want Reset to be higher level and it has too many details.
        // TODO Why do we have both @racer.RaceCount and @CurrentRacerRaceCount?
        // DONE Move the css in topnavbar.razor over the the css file
        // TODO Use ActiveRacer if its set instead of getting it from the cookie all the time.
        // TODO Use ActiveRacer in GetUserID if it is set
        // DONE Populate UserId in AddRacer
        // TODO If race team has no races, then on the Race Team page, show a message that suggests they try the app with multiplying squares before handing it over to their kids so they can see how it works.
        //    See if after just 5 to 10 races they don't know their squares better.
        // TODO Redo the menu pages to use two columns of thinner buttons
        // DONE On the RaceTeam page, at runtime, vary the width of your screen. The hint words run over each other.
        // TODO Check the cars and StartLine to make sure the Tops are both relative to the same parent.
        //    Check  position, margin, padding, display, align-items, and parent.
        //    Something has to be set differently for them to not be vertically aligned.
        // TODO Add progress panel with list of steps to each of the reg pages.
        // TODO Change RaceTeam page to show progress panel if coming from reg, and profile menu if not.
        // TODO Add current userId and racerId to feedback in service.

        // Brad
        // TODO Stop deleting old races
        // TODO Enable logging to the database
        // TODO Enable email
        // TODO Get Feedback and Subscription reading/writing to DB.

        // DO LATER
        // TODO Update the DerbyDash logo to the new one.
        // TODO Update racers avatars feature for user
    }
}
