using DerbyDash.Data;
using DerbyDash.Services;
using Microsoft.JSInterop;
using System.ComponentModel.DataAnnotations;

namespace DerbyDash.Components.Pages {
    public partial class Feedback {
        private FeedbackModel feedbackModel = new FeedbackModel();
        private bool isSubmitted = false;

        protected override async Task OnAfterRenderAsync(bool firstRender) {
            if (firstRender) {
                // Auto-detect browser information
                feedbackModel.BrowserInfo = await JSRuntime.InvokeAsync<string>("getBrowserInfo");
                StateHasChanged();
            }
        }

        private async Task HandleValidSubmit() {
            try {
                // Create the feedback entity
                var feedbackEntity = new DerbyDash.Data.Feedback {
                    Name = feedbackModel.Name,
                    Email = feedbackModel.Email,
                    FeedbackType = feedbackModel.FeedbackType ?? FeedbackType.None,
                    Subject = feedbackModel.Subject,
                    Message = feedbackModel.Message,
                    BrowserInfo = feedbackModel.BrowserInfo,
                    ContactConsent = feedbackModel.ContactConsent,
                };

                // Save to database
                bool success = await FeedbackService.AddFeedbackAsync(feedbackEntity);

                if (success) {
                    isSubmitted = true;
                } else {
                    // Handle error - could add an error message here
                    await JSRuntime.InvokeVoidAsync("alert", "There was an error submitting your feedback. Please try again.");
                }
            } catch (Exception ex) {
                // Log the exception and show error message
                await JSRuntime.InvokeVoidAsync("console.error", ex.Message);
                await JSRuntime.InvokeVoidAsync("alert", "An unexpected error occurred. Please try again later.");
            }
        }

        private void ResetForm() {
            feedbackModel = new FeedbackModel();
            isSubmitted = false;
            StateHasChanged();
        }

        public class FeedbackModel {
            [Required(ErrorMessage = "Please enter your name")]
            [StringLength(100, ErrorMessage = "Name is too long")]
            public string Name { get; set; } = string.Empty;

            [Required(ErrorMessage = "Please enter your email address")]
            [EmailAddress(ErrorMessage = "Please enter a valid email address")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Please select a feedback type")]
            public FeedbackType? FeedbackType { get; set; }

            [Required(ErrorMessage = "Please enter a subject")]
            [StringLength(200, ErrorMessage = "Subject is too long")]
            public string Subject { get; set; } = string.Empty;

            [Required(ErrorMessage = "Please enter your feedback message")]
            [StringLength(2000, ErrorMessage = "Message is too long (maximum 2000 characters)")]
            public string Message { get; set; } = string.Empty;

            public string BrowserInfo { get; set; } = string.Empty;

            public bool ContactConsent { get; set; } = true;
        }
    }
}