using DerbyDash.Data;
using DerbyDash.Exceptions;
using DerbyDash.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using DerbyDash.Components.Shared;

namespace DerbyDash.Components.Account.Pages.Manage {
    public partial class RaceTeam: IDisposable {
        private string? message;
        private List<Racer> raceTeam = new();
        protected int TeamRaceCount { get; set; } = 0;
        private List<RegistrationProgress.RegistrationStep> registrationSteps = new();
        
        // Edit dialog properties
        private bool showEditDialog = false;
        private Racer? racerToEdit;
        private EditRacerModel editRacerInput = new();
        
        // Delete dialog properties
        private bool showDeleteDialog = false;
        private Racer? racerToDelete;
        
        [SupplyParameterFromQuery]
        private bool IsFromRegistration { get; set; }

        [CascadingParameter]
        private HttpContext HttpContext { get; set; } = default!;

        [Inject]
        public UserManager<ApplicationUser> UserManager { get; set; } = default!;

        [Inject]
        internal IdentityUserAccessor UserAccessor { get; set; } = default!;

        [Inject]
        internal IdentityRedirectManager RedirectManager { get; set; } = default!;

        [Inject]
        internal IRaceTeamService RaceTeamService { get; set; } = default!;

        [Inject]
        public ILogger<RaceTeam> Logger { get; set; } = default!;

        [SupplyParameterFromForm]
        private InputModel Input { get; set; } = new();

        protected override async Task OnInitializedAsync() {
            // Initialize registration steps if coming from registration flow
            if (IsFromRegistration)
            {
                var stepsHelper = new RegistrationSteps { CurrentStep = "race-team" };
                registrationSteps = stepsHelper.GetRegistrationSteps();
            }
            
            // Subscribe to racer changes
            RaceTeamService.OnRacerChanged += HandleRacerChangedAsync;
            await ReloadUsers();
        }

        private async void HandleRacerChangedAsync() {
            try {
                Logger.LogInformation("HandleRacerChangedAsync started");
                
                // Reload the racers from the service (no need to clear first since ReloadUsers creates a new list)
                await ReloadUsers();
                
                // Ensure UI is updated on the UI thread
                await InvokeAsync(() => {
                    StateHasChanged();
                    Logger.LogInformation("HandleRacerChangedAsync UI updated");
                });
                
                Logger.LogInformation("HandleRacerChangedAsync completed");
            } catch (Exception ex) {
                Logger.LogError(ex, "Error in HandleRacerChangedAsync");
                // Don't rethrow to prevent UI breaking
            }
        }

        private async Task OnValidSubmitAsync() {
            try {
                Logger.LogInformation("Adding new racer: {RacerName}", Input.RacerName);

                Racer newRacer = new() {
                    Name = Input.RacerName,
                };

                await RaceTeamService.AddRacer(newRacer);

                Logger.LogInformation("Racer added successfully: {RacerName}", Input.RacerName);

                message = "The team member has been added";
                Input = new(); // Clear the form

                // No need to call ReloadUsers() here as it will be called by the HandleRacerChangedAsync
                // method when the OnRacerChanged event is triggered by RaceTeamService.AddRacer
                // Just ensure the UI is updated
                StateHasChanged();
            } catch (DuplicateRacerException) {
                // Preserve the entered name and show error
                message = $"Error: '{Input.RacerName}' already exists on the team";
                Logger.LogWarning("Duplicate racer name attempted: {RacerName}", Input.RacerName);
            } catch (Exception ex) {
                Logger.LogError(ex, "Error adding racer: {RacerName}", Input.RacerName);
                message = "Error adding racer: " + ex.Message;
            }
        }

        protected async Task SetActiveRacer(Racer racer) {
            try {
                await RaceTeamService.SetActiveRacer(racer);
                message = $"{racer.Name} is now the active racer.";
            } catch (Exception ex) {
                Logger.LogError(ex, "Error setting active racer");
                message = "Error setting active racer.";
            }
        }

        private async Task ReloadUsers() {
            try {
                Logger.LogInformation("ReloadUsers started");

                // Get the racers directly from the service
                // The service handles its own user authentication
                var freshRacers = await RaceTeamService.GetRacers();
                
                // Create a new list to avoid reference issues
                raceTeam = new List<Racer>(freshRacers);

                // Get team race count
                TeamRaceCount = await RaceTeamService.GetTeamRaceCountAsync();

                Logger.LogInformation($"Loaded {raceTeam.Count} racers with {TeamRaceCount} total races");
            } catch (Exception ex) {
                Logger.LogError(ex, "Error in ReloadUsers");
                // Initialize with empty list to avoid null reference exceptions
                raceTeam = new List<Racer>();
                TeamRaceCount = 0;
            }
        }

        private sealed class InputModel {
            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "Racer name")]
            public string RacerName { get; set; } = "";
        }
        
        private sealed class EditRacerModel {
            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "Racer name")]
            public string RacerName { get; set; } = "";
        }

        // Edit racer methods
        private void OpenEditDialog(Racer racer) {
            racerToEdit = racer;
            editRacerInput.RacerName = racer.Name;
            showEditDialog = true;
            StateHasChanged();
        }
        
        private void CloseEditDialog() {
            showEditDialog = false;
            racerToEdit = null;
            StateHasChanged();
        }
        
        private async Task SaveRacerEdit() {
            try {
                if (racerToEdit == null) {
                    message = "Error: No racer selected for editing";
                    return;
                }
                
                // Check if the name is already in use by another racer
                var existingRacer = raceTeam.FirstOrDefault(r => 
                    r.Id != racerToEdit.Id && 
                    r.Name.Equals(editRacerInput.RacerName, StringComparison.OrdinalIgnoreCase));
                
                if (existingRacer != null) {
                    message = $"Error: '{editRacerInput.RacerName}' already exists on the team";
                    return;
                }
                
                // Update the racer name
                racerToEdit.Name = editRacerInput.RacerName;
                
                // Save to database
                await RaceTeamService.UpdateRacer(racerToEdit);
                
                message = $"Racer name updated to '{racerToEdit.Name}'";
                CloseEditDialog();
                
                // No need to call ReloadUsers() here as it will be called by the HandleRacerChangedAsync
                // method when the OnRacerChanged event is triggered by RaceTeamService.UpdateRacer
                // Just ensure the UI is updated
                StateHasChanged();
            }
            catch (Exception ex) {
                Logger.LogError(ex, "Error updating racer name");
                message = "Error updating racer name: " + ex.Message;
            }
        }
        
        // Delete racer methods
        private void ConfirmDeleteRacer(Racer racer) {
            racerToDelete = racer;
            showDeleteDialog = true;
            StateHasChanged();
        }
        
        private void CloseDeleteDialog() {
            showDeleteDialog = false;
            racerToDelete = null;
            StateHasChanged();
        }
        
        private async Task DeleteRacer() {
            try {
                if (racerToDelete == null) {
                    message = "Error: No racer selected for deletion";
                    return;
                }
                
                string racerName = racerToDelete.Name;
                int racerId = racerToDelete.Id;
                
                // Delete the racer
                await RaceTeamService.RemoveRacer(racerId);
                
                message = $"Racer '{racerName}' has been deleted";
                CloseDeleteDialog();
                
                // No need to call ReloadUsers() here as it will be called by the HandleRacerChangedAsync
                // method when the OnRacerChanged event is triggered by RaceTeamService.RemoveRacer
                // Just ensure the UI is updated
                StateHasChanged();
            }
            catch (Exception ex) {
                Logger.LogError(ex, "Error deleting racer");
                message = "Error deleting racer: " + ex.Message;
            }
        }

        public void Dispose() {
            // Unsubscribe from racer changes
            RaceTeamService.OnRacerChanged -= HandleRacerChangedAsync;
        }
    }
}