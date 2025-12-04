# Client Migration Status

## Completed
1. ✅ Created service interfaces (IRaceTeamService, IRaceService, IUserService)
2. ✅ Created adapter services (ClientRaceTeamService, ClientRaceService, ClientUserService)
3. ✅ Created API services (ApiAuthService, ApiRaceTeamService, ApiRaceService, ApiProblemsService)
4. ✅ Created stub services for missing interfaces
5. ✅ Fixed ILocalStorageService duplicate interface
6. ✅ Registered all services in Program.cs

## Remaining Issues

### Critical (Blocks Core Functionality)
1. Account pages use server-only Identity managers (UserManager, SignInManager)
   - Login.razor.cs
   - Register.razor.cs
   - Logout.razor
   - Other account management pages

2. Components use server-only HttpContext
   - Various Account pages
   - Error.razor
   - Other pages

3. Components use EntityFrameworkCore
   - ParentProgress.razor.cs
   - Other pages

### Medium Priority (Blocks Some Features)
1. Admin pages use server-only features
   - Admin/Index.razor
   - Admin/UserManagement.razor
   - Admin/ViewLogs.razor
   - Admin/FeedbackManagement.razor
   - Admin/SendBulkEmail.razor

2. Identity management pages use server-only features
   - Account/Pages/Manage/* pages
   - Account/Pages/ExternalLogin.razor
   - Account/Pages/ConfirmEmail.razor
   - Other account pages

3. Missing Data models
   - FAQ model
   - Other models

### Low Priority (Can Be Stubbed)
1. OfflineRace functionality
2. Some admin features
3. External login features

## Next Steps

1. Update Account Login/Register pages to use ApiAuthService
2. Update Account Logout to use ApiAuthService
3. Comment out or stub Admin pages
4. Update components to use client-side services
5. Fix HttpContext usage
6. Remove EntityFrameworkCore dependencies

## Files That Need Updates

### Account Pages (High Priority)
- Components/Account/Pages/Login.razor.cs
- Components/Account/Pages/Register.razor.cs
- Components/Account/Pages/Logout.razor
- Components/Account/Pages/Manage/*.razor.cs

### Core Pages (High Priority)
- Components/Pages/Race.razor.cs (mostly done)
- Components/Pages/Home.razor.cs (check if needs updates)
- Components/Pages/Feedback.razor (update to use IFeedbackService)

### Admin Pages (Can be stubbed for now)
- Components/Pages/Admin/*.razor (comment out or remove)

### Other Components
- Components/Layout/TopNavbar.razor.cs (update to use IAvatarService, remove UserManager)
- Components/Pages/ParentProgress.razor.cs (remove EntityFrameworkCore)
- Components/Pages/OfflineRace.razor.cs (update to use client services)

