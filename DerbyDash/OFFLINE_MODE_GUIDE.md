# TurboFlash Offline Mode Guide

## Overview

TurboFlash now supports offline mode, allowing users to practice math racing even without an internet connection. This feature is implemented using Blazor Server with PWA (Progressive Web App) capabilities and offline service implementations.

## How to Access Offline Mode

### Method 1: Direct URL Access (True Offline Mode)
Navigate directly to offline race URLs - **these will work even when the server is stopped**:
- **Addition Practice**: `/offline-race/addition-4stable`
- **Subtraction Practice**: `/offline-race/subtraction-4stable`
- **Multiplication Practice**: `/offline-race/multiplication-4stable`
- **Other variations**: 
  - `/offline-race/addition-2stable`
  - `/offline-race/addition-3stable`
  - `/offline-race/addition-5stable`

**Important**: When the server is stopped, these URLs will be served by the service worker with a complete standalone page that works without any server connection.

### Method 2: Offline Menu
Visit `/offline-menu` to see all available practice modes with a user-friendly interface.

### Method 3: Automatic Offline Detection
When the browser detects you're offline or the server is stopped, the service worker will automatically serve standalone offline practice pages.

## How It Works When Server Is Stopped

When you stop the TurboFlash server and visit `/offline-race/addition-4stable` (or any other problem type), here's what happens:

1. **Service Worker Intercepts**: The service worker catches the request when the server is unavailable
2. **Standalone Page Served**: A complete HTML page with embedded JavaScript is served directly from the service worker
3. **No "Rejoining Server"**: You'll see a clean offline practice interface instead of server reconnection messages
4. **Full Functionality**: Math problems, timing, scoring, and results all work without server connection

## Features of Offline Mode

### ✅ What Works Offline
- **IDENTICAL Racing Experience** - Exact same racing as online mode with authentic track, lanes, and car positioning
- **Original Track System** - Same yellow lane markers, road animation, and 3D perspective as online racing
- **Authentic Car Movement** - Cars positioned using identical distance/speed calculations as online mode
- **FastEddy AI Opponents** - AI cars use the exact same timing data and speed increments as online mode
- **Real Racing Physics** - Same SPEED_MULTIPLIER (7.0), distance calculations, and car positioning logic
- **Dynamic Track Animation** - Road moves at different speeds based on car speed, just like online
- **Player Indicator** - "YOU" label and arrow above your car, identical to online racing
- **Speed Display** - Same speed calculation formula (speed × 7 × 1.09) as online mode
- **Start/Finish Lines** - Proper start and finish line positioning with same visual appearance
- **Math Problems** - All problems generated client-side (addition, subtraction, multiplication tables)
- **Session Leaderboard** - Track your improvements during the session with top 5 times

### ⚠️ Offline Limitations
- **No account sync** - Progress is not saved to your online account
- **Session-only data** - Data is lost when you close the browser
- **No leaderboards** - Can't compete with other players
- **Limited racer customization** - Uses pre-defined practice racers

## Getting Started with Offline Mode


### Step 1: Access Offline Mode
1. **While Online**: Navigate to `/offline-menu` to see all practice options
2. **While Offline**: The service worker will automatically show offline options

### Step 2: Choose Your Practice Type
Select from available math problem types:
- **Addition Tables** (2s, 3s, 4s, 5s)
- **Subtraction Tables** (4s)
- **Multiplication Tables** (4s)

### Step 3: Start Racing!
1. Click "Start Practice Race"
2. Solve math problems to accelerate your car
3. Beat your session times to improve
4. Track your progress in real-time

## Technical Implementation

### Architecture
- **Offline Services**: `OfflineRaceService` and `OfflineRaceTeamService` handle race logic without server dependencies
- **Service Worker**: Caches essential resources and handles offline routing
- **PWA Manifest**: Enables installation and offline capabilities
- **Client-side Storage**: Session data stored in memory (not persistent)

### Key Components
- **OfflineRace.razor**: Main offline race component
- **OfflineMenu.razor**: Practice mode selection interface
- **Service Worker** (`sw.js`): Handles offline resource caching
- **Offline Detection** (`offline.js`): Detects network status changes


## Troubleshooting

### Common Issues

**Q: Offline mode isn't working**
A: Ensure your browser supports service workers and that you've visited the site online first to cache resources.

**Q: My progress disappeared**
A: Offline mode uses session storage. Progress is only kept during your current browser session.

**Q: Can I save my offline progress?**
A: No, offline progress is not saved to your account. Go online and use the regular race mode to save progress permanently.

**Q: Why can't I see my regular racers?**
A: Offline mode uses practice racers that don't require server authentication. Your regular racers are only available online.


## Development Notes

### For Developers
The offline implementation follows a clean architecture pattern:

```
Services/
├── IRaceService.cs (Interface)
├── RaceService.cs (Online implementation)
└── Offline/
    ├── OfflineRaceService.cs (Offline implementation)
    ├── OfflineRaceTeamService.cs (Offline team management)
    └── OfflineDetectionService.cs (Network status detection)
```

### Adding New Offline Features
1. Create offline service implementation
2. Register in DI container
3. Update service worker cache list
4. Add offline-specific UI components

## Future Enhancements

Potential improvements for offline mode:
- **Persistent Storage**: Save offline progress using IndexedDB
- **Sync Capability**: Sync offline progress when back online
- **More Problem Types**: Add division and mixed operations
- **Custom Racers**: Allow racer creation in offline mode
- **Offline Achievements**: Local achievement system

---

**Note**: Offline mode is designed for practice and learning. For the full TurboFlash experience with progress tracking, leaderboards, and social features, use the online mode.
