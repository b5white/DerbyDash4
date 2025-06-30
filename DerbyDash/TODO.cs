namespace DerbyDash {
    class TODO {
        // ** Filipe **
        // DONE Hide the 2 factor link on the Profile page, AKA "/Account/Manage".
        //      Don't remove the code because we might want to add it back in later.
        // DONE Add a Preferences page with a link to it from the Profile page.
        //      Add a way for them to choose their avatar and then show it on the front page and the race page.
        // DONE Link the Start Race button to starting the race with the most recent.
        // DONE Default the Remember Me to true, rename, use to save cookies or not.
        // DONE FAQ page
        // DONE Add a cookie service to handle all cookie read/write actions like save the last racer.
        // DONE Add a feedback page so users can report errors
        // DONE Subscription page
        // DONE Subscription service with the fake data
        // DONE Add a link to the FAQ page from the Subscription page.
        // DONE Move avatar to Racer
        // DONE Add an explainer page before the login page 
        // DONE Add "You need to log in to race." to all menus if they are not logged in.
        // DONE Move Get/Save LastRace from Race to Racer Service
        // DONE Create a UserService to handle all user related actions
        //    GetUserId
        //    IsLoggedIn
        //    Anything else using GetAuthenticationStateAsync
        // DONE On the RaceTeam page, at runtime, vary the width of your screen. The hint words run over each other.
        // DONE Populate UserId in AddRacer
        // DONE b.Property<string>("LastPlayedRace").HasColumnType("nvarchar(max)");
        //         should be
        //      b.Property<string>("LastPlayedRace").HasColumnType("nvarchar(100)");
        // DONE Show password isn't working on registration or login.
        // DONE On mobile, the menu stays up too long. At least close when they make a selection.
        // DONE On mobile, when on profile page, hide top menu bar.
        //      Once they pop up the keyboard, there isn't much space left.
        // DONE If race team has no races, then on the Race Team page, add a button Next ->
        //      that takes them to an explainer page.
        //      It suggests they try the app with multiplying squares before handing it over
        //      to their kids so they can see how it works.
        //      See if after just 5 to 10 races they don't know their squares better.        
        //      I'll have someone write the instructions for this page.
        //      Just need a link at the bottom, Let's Race, that takes them to the SmallSquares race page.
        // DONE Add progress panel with list of steps to each of the reg pages.
        //      Change RaceTeam page to show progress panel if coming from reg,
        //      and profile menu if not.
        // DONE Top menu needs to refresh the race count after each race.
        // DONE Subscription should look like it's part of the profile submenu, like the others do.
        // DONE align cars from leftside to right side of the screen.
        // DONE Don't default the active racer, except for when first adding the race team and when reading from a cookie.
        // DONE Racing should require them to have a chosen racer, and if not, redirect them to the RaceTeam page to choose one.
        // DONE Redo the menu pages to use two columns of thinner buttons
        // DONE Add a Resend Email link to the RegisterConfirmation page?
        //    It should just resend the email, but stay on the Confirmation page.
        // DONE Use GetUserID everywhere we need a UserId.
        // DONE Get rid of anything related to saving race count in the database
        //    We'll calculate it based on the number of races in the DB.
        // DONE Move GetUserID from RTS to the User manager.
        // DONE Use ActiveRacer instead of getting it from the cookie all the time.
        // DONE Default the Remember Me to true, rename, use to save cookies or not, for Registration page and pass to email conf
        // DONE Change name from DerbyDash to TurboFlash
        // DONE Handle the screen shift when using a phone so can still see top of page.        
        // DONE Check the cars and StartLine to make sure the Tops are both relative to the same parent.
        //    Check  position, margin, padding, display, align-items, and parent.
        //    Something has to be set differently for them to not be vertically aligned.
        // DONE Add a partially transparent results panel that pops up in the middle of the screen
        //     at the end of the race, and shows their new time and how many of
        //     the previous top 5 they beat.
        //     It stays up until 1 second after all racers finish.
        //     It says and says "Great race! You won." if they came in first.
        //     Or "Good race." if not.
        //     Would pop up right away, even while other racers
        //     are finishing.
        // DONE Current car Top never goes below 50% of screen height.
        // DONE Dress up the confirmation email. Currently "Please confirm your account by clicking here."
        // DONE Rewrite UserService so it either doesn't throw exceptions just because user isn't loggedin, or don't log the exception.
        // DONE Add a way to change/delete the racer name on the RaceTeam page.

        // DONE Save the Race position in the list at time it was first run, in Race
        // DONE Add current userId and racerId to feedback in service.
        // DONE Get avatars working and attached to racers
        // DONE Put the avatar, in a larger form, on the Start Race pages.
        // DONE In a smaller form on the results page after a race.

        // ** Brad **
        // DONE Get Feedback and Subscription reading/writing to DB.
        // DONE Subscription service with real data
        // DONE Stop deleting old races
        // DONE Enable logging to the database
        // DONE Handle exceptions
        // DONE Automatically log inner exceptions
        // DONE Sometimes getting No Racers on the TopMenu bar. **I think this should be fixed but needs more testing.**
        // DONE Race counts are still incorrect. Getting reports of counts bleeding over from other race types.
        // DONE Enable email
        // TODO Admin page for viewing logs
        // TODO Add SessionId to log
        // TODO Add a link to the most recent log messages for SessionId to the feedback report
        // TODO Review Cookie.SameSite across all three cookie settings.

        // ** Either Filipe or Brad **
        // HOLD Add version number to Feedback report

        // ** Later **
        // Show userId and racerId in admin feedback page.
        // Add anchors on the FAQ page so we can link to the specific sections, like FAQ#subscription
        // In Race.Reset, break the FireAndForget code out into a separate routine.
        //    I want Reset to be higher level and it has too many details.
        // Send a date range to the GetCount routines indicating AllTime.
        //    Or, instead of sending a data range, we could send a range type.
        //    AllTime, Last Week, Last30Days, Last90Days, LastYear
        //    That way, the routine itself is reponsible for the date details.
        // In the feedback response, we need a field for public response and a private response
        // We need a way for people to review their feedback submissions and our responses
        //     Note: We don't need a way to reply or ask for more details.
        //     They've already noted whether we can reply, in which case we handle it via email.
        //     And then record the final resolution here.
        // Update the DerbyDash logo.
        // Limit racers to 8 per subscription.
        //     Make this a User field so admin can increase it per user if needed.
        // Show user and racer counts in admin feedback page.
        // Parents review progress page
        // Race counts for user and racer on the parents review
        // Move the css in *.razor over the the css files
        // Video page
        // Accessory page

    }
}
