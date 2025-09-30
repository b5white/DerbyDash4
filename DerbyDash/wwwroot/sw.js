// Service Worker for PWA offline support
const CACHE_NAME = 'derbydash-offline-v8';
const urlsToCache = [
    '/',
    '/css/bootstrap/bootstrap.min.css',
    '/css/app.css',
    '/js/offline.js',
    '/lib/bootstrap/dist/css/bootstrap.min.css',
    '/lib/bootstrap/dist/js/bootstrap.bundle.min.js',
    '/images/avatars/',
    '/favicon.png',
    // Add offline race pages
    '/offline-race/addition-4stable',
    '/offline-race/subtraction-4stable',
    '/offline-race/multiplication-4stable',
    // ACTUAL CAR IMAGES FROM ONLINE MODE
    '/images/RaceTrack/RaceCar1.png',
    '/images/RaceTrack/RaceCar2.png',
    '/images/RaceTrack/RaceCar3.png',
    '/images/RaceTrack/RaceCar4.png',
    '/images/RaceTrack/RaceCar5.png',
    '/images/RaceTrack/RaceCar6.png',
    '/images/RaceTrack/RaceCar7.png',
    '/images/RaceTrack/RaceCar8.png',
    '/images/RaceTrack/RaceCar9.png',
    '/images/RaceTrack/RaceCar10.png',
    '/images/RaceTrack/StartLine.png',
    '/images/RaceTrack/FinishLine.png',
    // Add other essential assets
    '/lib/font-awesome/css/all.min.css'
];

// Install event - cache resources
self.addEventListener('install', (event) => {
    console.log('Service Worker installing...');
    event.waitUntil(
        caches.open(CACHE_NAME)
            .then((cache) => {
                console.log('Caching app shell');
                return cache.addAll(urlsToCache);
            })
            .catch((error) => {
                console.error('Cache installation failed:', error);
            })
    );
});

// Activate event - clean up old caches
self.addEventListener('activate', (event) => {
    console.log('Service Worker activating...');
    event.waitUntil(
        caches.keys().then((cacheNames) => {
            return Promise.all(
                cacheNames.map((cacheName) => {
                    if (cacheName !== CACHE_NAME) {
                        console.log('Deleting old cache:', cacheName);
                        return caches.delete(cacheName);
                    }
                })
            );
        })
    );
});

// Fetch event - serve from cache when offline
self.addEventListener('fetch', (event) => {
    // Skip non-GET requests
    if (event.request.method !== 'GET') {
        return;
    }

    // Skip requests to external domains
    if (!event.request.url.startsWith(self.location.origin)) {
        return;
    }

    // Handle car images and race track images specifically
    if (event.request.url.includes('/images/RaceTrack/')) {
        event.respondWith(
            caches.match(event.request).then((response) => {
                if (response) {
                    return response;
                }
                return fetch(event.request).then((response) => {
                    // Cache the image for future use
                    if (response.status === 200) {
                        const responseClone = response.clone();
                        caches.open(CACHE_NAME).then((cache) => {
                            cache.put(event.request, responseClone);
                        });
                    }
                    return response;
                }).catch(() => {
                    // If both cache and network fail, return a placeholder
                    console.log('Failed to load image:', event.request.url);
                    return new Response('', { status: 404 });
                });
            })
        );
        return;
    }

    // Handle offline race pages specifically
    if (event.request.url.includes('/offline-race/')) {
        event.respondWith(
            // Always redirect to standalone offline race when server is unavailable
            fetch(event.request).catch(() => {
                // Extract problem type from URL
                const url = new URL(event.request.url);
                const pathParts = url.pathname.split('/');
                const problemType = pathParts[pathParts.length - 1] || 'addition-4stable';
                
                return new Response(`
                    <!DOCTYPE html>
                    <html lang="en">
                    <head>
                        <meta charset="utf-8" />
                        <meta name="viewport" content="width=device-width, initial-scale=1.0" />
                        <title>DerbyDash - Offline Racing</title>
                        <link rel="stylesheet" href="/lib/bootstrap/dist/css/bootstrap.min.css" />
                        <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css" />
                        <link rel="stylesheet" href="/app.css" />
                        <style>
                            body { margin: 0; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif; background: #f5f5f5; overflow-x: hidden; }
                            .offline-banner { position: fixed; top: 0; left: 0; right: 0; background: linear-gradient(135deg, #ff6b6b, #ee5a52); color: white; padding: 8px 0; text-align: center; z-index: 1000; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }
                            
                            /* EXACT SAME TRACK STYLES AS ONLINE MODE */
                            .road {
                                width: 100%;
                                height: 90vh;
                                max-width: 100vw;
                                background-color: #404040;
                                position: relative;
                                overflow: hidden;
                                perspective: 1000px;
                                margin-top: 50px;
                            }
                            
                            .track-background {
                                position: absolute;
                                width: 100%;
                                height: 1120px;
                                transform-style: preserve-3d;
                                will-change: transform;
                                transform: translateZ(0);
                                display: block;
                            }
                            
                            .track-background.animated:not(.finish-visible) {
                                animation: moveRoad 3s linear infinite;
                                animation-play-state: running;
                                transform-origin: top center;
                            }
                            
                            .lane-group {
                                position: absolute;
                                width: 10px;
                                height: calc(100vh + 560px);
                                transform-style: preserve-3d;
                                overflow: hidden;
                                transition: all 0.3s ease-in-out;
                                top: -280px;
                            }
                            
                            .lane {
                                width: 100%;
                                height: 70px;
                                background-color: #ffff00;
                                margin-bottom: 210px;
                                transform-style: preserve-3d;
                            }
                            
                            .lane-group:nth-child(1) { left: 15%; }
                            .lane-group:nth-child(2) { left: 32%; }
                            .lane-group:nth-child(3) { left: 49%; }
                            .lane-group:nth-child(4) { left: 66%; }
                            .lane-group:nth-child(5) { left: 83%; }
                            
                            @keyframes moveRoad {
                                0% { transform: translateY(0); }
                                100% { transform: translateY(560px); }
                            }
                            
                            /* Speed variations for dynamic animation speeds */
                            .track-background.animated.speed-1:not(.finish-visible) { animation-duration: 18.0s; }
                            .track-background.animated.speed-2:not(.finish-visible) { animation-duration: 9.0s; }
                            .track-background.animated.speed-3:not(.finish-visible) { animation-duration: 6.0s; }
                            .track-background.animated.speed-4:not(.finish-visible) { animation-duration: 3.9s; }
                            .track-background.animated.speed-5:not(.finish-visible) { animation-duration: 3.12s; }
                            .track-background.animated.speed-6:not(.finish-visible) { animation-duration: 2.6s; }
                            .track-background.animated.speed-7:not(.finish-visible) { animation-duration: 2.23s; }
                            .track-background.animated.speed-8:not(.finish-visible) { animation-duration: 1.65s; }
                            .track-background.animated.speed-9:not(.finish-visible) { animation-duration: 1.47s; }
                            .track-background.animated.speed-10:not(.finish-visible) { animation-duration: 1.32s; }
                            .track-background.animated.speed-11:not(.finish-visible) { animation-duration: 1.09s; }
                            .track-background.animated.speed-12:not(.finish-visible) { animation-duration: 1.0s; }
                            .track-background.animated.speed-13:not(.finish-visible) { animation-duration: 0.92s; }
                            .track-background.animated.speed-14:not(.finish-visible) { animation-duration: 0.86s; }
                            .track-background.animated.speed-15:not(.finish-visible) { animation-duration: 0.8s; }
                            
                            .track-container {
                                position: relative;
                                width: 100%;
                                height: 100%;
                                z-index: 1;
                            }
                            
                            .start-line, .finish-line {
                                position: absolute;
                                width: 100%;
                                text-align: center;
                                z-index: 2;
                                transform-style: preserve-3d;
                                transition: all 0.3s ease-in-out;
                            }
                            
                            .start-line {
                                top: calc(100vh - 450px);
                                left: 0;
                                width: 100%;
                            }
                            
                            .start-line.hidden {
                                opacity: 0;
                                visibility: hidden;
                                display: none;
                            }
                            
                            .start-line img, .finish-line img {
                                width: 100vw;
                                max-width: none;
                                height: auto;
                                object-fit: cover;
                            }
                            
                            .cars-container {
                                position: relative;
                                width: 100%;
                                height: 100%;
                                display: flex;
                                justify-content: space-between;
                                z-index: 3;
                            }
                            
                            .car-dynamic {
                                position: relative;
                                width: 16%;
                                margin: 5px;
                                transform-style: preserve-3d;
                                will-change: transform;
                                transition: top 0.5s ease-out;
                            }
                            
                            .car-dynamic img {
                                width: 100%;
                                height: auto;
                                object-fit: contain;
                            }
                            
                            .player-indicator {
                                position: absolute;
                                bottom: calc(100% + 6px);
                                left: 50%;
                                transform: translateX(-50%);
                                display: flex;
                                flex-direction: column;
                                align-items: center;
                                gap: 4px;
                                z-index: 5;
                                pointer-events: none;
                            }
                            
                            .player-label {
                                display: inline-block;
                                padding: 2px 10px;
                                font-size: 12px;
                                font-weight: 700;
                                letter-spacing: 0.5px;
                                color: #fff;
                                text-transform: uppercase;
                                line-height: 1.4;
                                background: linear-gradient(135deg, #1b6ec2 0%, #1861ac 100%);
                                border-radius: 999px;
                                box-shadow: 0 2px 6px rgba(0, 0, 0, 0.25);
                                white-space: nowrap;
                            }
                            
                            .player-arrow {
                                width: 0;
                                height: 0;
                                font-size: 0;
                                border-left: 6px solid transparent;
                                border-right: 6px solid transparent;
                                border-top: 8px solid #1861ac;
                                filter: drop-shadow(0 1px 2px rgba(0,0,0,0.25));
                            }
                            
                            /* Start Screen */
                            .race-start-container { text-align: center; padding: 40px 20px; background: rgba(255,255,255,0.95); margin: 20px; border-radius: 12px; margin-top: 70px; }
                            .race-title { font-size: 2.5rem; color: #2c3e50; margin-bottom: 10px; }
                            .race-subtitle { font-size: 1.2rem; color: #7f8c8d; margin-bottom: 30px; }
                            .race-start-button { background: #28a745; color: white; border: none; padding: 15px 30px; font-size: 1.3rem; border-radius: 8px; cursor: pointer; }
                            .race-start-button:hover { background: #218838; }
                            
                            /* Game HUD */
                            .game-hud { position: fixed; bottom: 0; left: 0; right: 0; background: rgba(0,0,0,0.8); color: white; padding: 20px; z-index: 100; }
                            .math-problem { text-align: center; margin-bottom: 15px; }
                            .problem-bubble { background: #fff; color: #000; padding: 15px; border-radius: 12px; display: inline-block; }
                            .problem-text { font-size: 2rem; font-weight: bold; margin: 0; }
                            .answer-box { text-align: center; }
                            .answer-input { font-size: 1.5rem; padding: 10px; width: 150px; text-align: center; border: 2px solid #007bff; border-radius: 6px; }
                            
                            /* Results */
                            .results-container { text-align: center; padding: 40px 20px; background: rgba(255,255,255,0.95); margin: 20px; border-radius: 12px; margin-top: 70px; }
                            .results-header h1 { color: #2c3e50; }
                            .encouraging-message { color: #28a745; font-size: 1.5rem; margin: 20px 0; }
                            .play-again-button { background: #007bff; color: white; border: none; padding: 15px 30px; font-size: 1.2rem; border-radius: 8px; cursor: pointer; margin: 20px 10px; }
                            .play-again-button:hover { background: #0056b3; }
                            
                            /* Race Stats */
                            .race-stats { display: flex; justify-content: center; gap: 20px; margin: 30px 0; flex-wrap: wrap; }
                            .stat-card { background: #f8f9fa; padding: 20px; border-radius: 8px; min-width: 120px; text-align: center; border: 2px solid #dee2e6; }
                            .stat-label { font-size: 0.9rem; color: #6c757d; margin-bottom: 5px; }
                            .stat-value { font-size: 1.8rem; font-weight: bold; color: #495057; }
                            .stat-card.improvement { border-color: #28a745; }
                            .stat-card.improvement .stat-value { color: #28a745; }
                            
                            /* Leaderboard */
                            .leaderboard { margin: 30px 0; }
                            .leaderboard-title { color: #2c3e50; margin-bottom: 20px; }
                            .score-list { max-width: 300px; margin: 0 auto; }
                            .score-item { display: flex; justify-content: space-between; padding: 10px 15px; margin: 5px 0; background: #f8f9fa; border-radius: 6px; }
                            .score-item.current-score { background: #007bff; color: white; font-weight: bold; }
                            .rank { font-weight: bold; }
                            
                            /* Speed Indicator */
                            .speed-indicator { position: fixed; top: 70px; right: 20px; background: rgba(0,0,0,0.8); color: white; padding: 15px; border-radius: 8px; z-index: 50; }
                            .speed-label { font-size: 0.8rem; margin-bottom: 5px; }
                            .speed-value { font-size: 1.5rem; font-weight: bold; }
                            
                            /* Responsive */
                            @media (max-width: 768px) {
                                .road { top: 15vh; }
                                .lane { height: 70px; }
                                .car-dynamic { width: 15%; }
                                .player-label { font-size: 11px; padding: 2px 8px; }
                                .player-arrow { border-left-width: 5px; border-right-width: 5px; border-top-width: 7px; }
                                .lane-group { width: 8px; }
                                .start-line { top: calc(100vh - 350px); left: 0; width: 100%; }
                                .problem-text { font-size: 1.5rem; }
                                .race-stats { flex-direction: column; align-items: center; }
                            }
                        </style>
                    </head>
                    <body>
                        <div class="offline-banner">
                            <i class="fas fa-wifi-slash"></i>
                            <strong>Offline Practice Racing</strong> - No server connection needed!
                        </div>

                        <div class="race-container">
                            <!-- Start Screen -->
                            <div id="startScreen" class="race-start-container">
                                <h1 class="race-title" id="gameTitle">Math Racing Practice</h1>
                                <p class="race-subtitle">Ready to race with numbers offline?</p>
                                <div style="margin: 30px 0;">
                                    <div style="background: #f8f9fa; padding: 20px; border-radius: 8px; display: inline-block;">
                                        <div style="font-size: 2rem; margin-bottom: 10px;">🏎️</div>
                                        <h3>Practice Racer</h3>
                                        <p style="color: #666;">is ready to race!</p>
                                    </div>
                                </div>
                                <button class="race-start-button" onclick="startRace()">
                                    <i class="fas fa-flag-checkered"></i> Start Race
                                </button>
                                <div style="margin-top: 20px; font-size: 1rem; color: #666;">
                                    <p>Solve math problems to accelerate your car.</p>
                                    <p>Beat the other cars to win the race!</p>
                                </div>
                            </div>

                            <!-- Race Screen -->
                            <div id="raceScreen" style="display: none;">
                                <!-- Speed Indicator -->
                                <div class="speed-indicator">
                                    <div class="speed-label">YOUR SPEED</div>
                                    <div class="speed-value" id="speedValue">0 kph</div>
                                </div>

                                <!-- EXACT SAME TRACK STRUCTURE AS ONLINE MODE -->
                                <div class="road">
                                    <div class="track-background" id="trackBackground">
                                        <div class="lane-group">
                                            <div class="lane"></div><div class="lane"></div><div class="lane"></div><div class="lane"></div><div class="lane"></div>
                                            <div class="lane"></div><div class="lane"></div><div class="lane"></div><div class="lane"></div><div class="lane"></div>
                                            <div class="lane"></div><div class="lane"></div><div class="lane"></div><div class="lane"></div><div class="lane"></div>
                                            <div class="lane"></div><div class="lane"></div><div class="lane"></div><div class="lane"></div><div class="lane"></div>
                                            <div class="lane"></div><div class="lane"></div><div class="lane"></div><div class="lane"></div><div class="lane"></div>
                                            <div class="lane"></div><div class="lane"></div><div class="lane"></div><div class="lane"></div><div class="lane"></div>
                                            <div class="lane"></div><div class="lane"></div><div class="lane"></div><div class="lane"></div><div class="lane"></div>
                                            <div class="lane"></div><div class="lane"></div><div class="lane"></div><div class="lane"></div><div class="lane"></div>
                                        </div>
                                        <div class="lane-group">
                                            <div class="lane"></div><div class="lane"></div><div class="lane"></div><div class="lane"></div><div class="lane"></div>
                                            <div class="lane"></div><div class="lane"></div><div class="lane"></div><div class="lane"></div><div class="lane"></div>
                                            <div class="lane"></div><div class="lane"></div><div class="lane"></div><div class="lane"></div><div class="lane"></div>
                                            <div class="lane"></div><div class="lane"></div><div class="lane"></div><div class="lane"></div><div class="lane"></div>
                                            <div class="lane"></div><div class="lane"></div><div class="lane"></div><div class="lane"></div><div class="lane"></div>
                                            <div class="lane"></div><div class="lane"></div><div class="lane"></div><div class="lane"></div><div class="lane"></div>
                                            <div class="lane"></div><div class="lane"></div><div class="lane"></div><div class="lane"></div><div class="lane"></div>
                                            <div class="lane"></div><div class="lane"></div><div class="lane"></div><div class="lane"></div><div class="lane"></div>
                                        </div>
                                        <div class="lane-group">
                                            <div class="lane"></div><div class="lane"></div><div class="lane"></div><div class="lane"></div><div class="lane"></div>
                                            <div class="lane"></div><div class="lane"></div><div class="lane"></div><div class="lane"></div><div class="lane"></div>
                                            <div class="lane"></div><div class="lane"></div><div class="lane"></div><div class="lane"></div><div class="lane"></div>
                                            <div class="lane"></div><div class="lane"></div><div class="lane"></div><div class="lane"></div><div class="lane"></div>
                                            <div class="lane"></div><div class="lane"></div><div class="lane"></div><div class="lane"></div><div class="lane"></div>
                                            <div class="lane"></div><div class="lane"></div><div class="lane"></div><div class="lane"></div><div class="lane"></div>
                                            <div class="lane"></div><div class="lane"></div><div class="lane"></div><div class="lane"></div><div class="lane"></div>
                                            <div class="lane"></div><div class="lane"></div><div class="lane"></div><div class="lane"></div><div class="lane"></div>
                                        </div>
                                        <div class="lane-group">
                                            <div class="lane"></div><div class="lane"></div><div class="lane"></div><div class="lane"></div><div class="lane"></div>
                                            <div class="lane"></div><div class="lane"></div><div class="lane"></div><div class="lane"></div><div class="lane"></div>
                                            <div class="lane"></div><div class="lane"></div><div class="lane"></div><div class="lane"></div><div class="lane"></div>
                                            <div class="lane"></div><div class="lane"></div><div class="lane"></div><div class="lane"></div><div class="lane"></div>
                                            <div class="lane"></div><div class="lane"></div><div class="lane"></div><div class="lane"></div><div class="lane"></div>
                                            <div class="lane"></div><div class="lane"></div><div class="lane"></div><div class="lane"></div><div class="lane"></div>
                                            <div class="lane"></div><div class="lane"></div><div class="lane"></div><div class="lane"></div><div class="lane"></div>
                                            <div class="lane"></div><div class="lane"></div><div class="lane"></div><div class="lane"></div><div class="lane"></div>
                                        </div>
                                        <div class="lane-group">
                                            <div class="lane"></div><div class="lane"></div><div class="lane"></div><div class="lane"></div><div class="lane"></div>
                                            <div class="lane"></div><div class="lane"></div><div class="lane"></div><div class="lane"></div><div class="lane"></div>
                                            <div class="lane"></div><div class="lane"></div><div class="lane"></div><div class="lane"></div><div class="lane"></div>
                                            <div class="lane"></div><div class="lane"></div><div class="lane"></div><div class="lane"></div><div class="lane"></div>
                                            <div class="lane"></div><div class="lane"></div><div class="lane"></div><div class="lane"></div><div class="lane"></div>
                                            <div class="lane"></div><div class="lane"></div><div class="lane"></div><div class="lane"></div><div class="lane"></div>
                                            <div class="lane"></div><div class="lane"></div><div class="lane"></div><div class="lane"></div><div class="lane"></div>
                                            <div class="lane"></div><div class="lane"></div><div class="lane"></div><div class="lane"></div><div class="lane"></div>
                                        </div>

                                        <div class="start-line" id="startLine">
                                            <img alt="Start Line" />
                                        </div>
                                    </div>

                                    <div class="track-container">
                                        <div class="finish-line" id="finishLine" style="top: 9999px;">
                                            <img alt="Finish Line" />
                                        </div>

                                        <div class="cars-container">
                                            <div class="car-dynamic player-car" id="playerCar" style="top: 9999px;">
                                                <div class="player-indicator">
                                                    <span class="player-label">YOU</span>
                                                    <div class="player-arrow"></div>
                                                </div>
                                                <img alt="Your Race car" />
                                            </div>
                                            <div class="car-dynamic ai-car" id="car2" style="top: 9999px;">
                                                <img alt="AI Race car" />
                                            </div>
                                            <div class="car-dynamic ai-car" id="car3" style="top: 9999px;">
                                                <img alt="AI Race car" />
                                            </div>
                                            <div class="car-dynamic ai-car" id="car4" style="top: 9999px;">
                                                <img alt="AI Race car" />
                                            </div>
                                        </div>
                                    </div>
                                </div>

                                <!-- Game HUD -->
                                <div class="game-hud">
                                    <div class="math-problem">
                                        <div class="problem-bubble">
                                            <h2 class="problem-text" id="problemText">Loading...</h2>
                                        </div>
                                    </div>
                                    <div class="answer-box">
                                        <input class="answer-input" type="tel" id="answerInput" 
                                               onkeypress="handleKeyPress(event)" oninput="handleInput(event)" placeholder="Type answer..." autofocus />
                                    </div>
                                </div>
                            </div>

                            <!-- Results Screen -->
                            <div id="resultsScreen" style="display: none;" class="results-container">
                                <div class="results-header">
                                    <h1 id="raceTitle">Math Racing Practice</h1>
                                    <h2 class="encouraging-message" id="encouragingMessage">Great job!</h2>
                                </div>

                                <button class="play-again-button" onclick="restartRace()">
                                    <i class="fas fa-play"></i> Race Again
                                </button>

                                <div class="race-stats">
                                    <div class="stat-card">
                                        <div class="stat-label">Your Time</div>
                                        <div class="stat-value" id="finalTime">0.000</div>
                                    </div>
                                    <div class="stat-card">
                                        <div class="stat-label">Session Average</div>
                                        <div class="stat-value" id="sessionAverage">0.000</div>
                                    </div>
                                    <div class="stat-card improvement" id="improvementCard" style="display: none;">
                                        <div class="stat-label">Improvement</div>
                                        <div class="stat-value" id="improvement">+0.000</div>
                                    </div>
                                </div>

                                <div class="leaderboard">
                                    <h2 class="leaderboard-title">Session Top 5</h2>
                                    <div class="score-list" id="scoreList">
                                        <div class="score-item"><span class="rank">1</span><span class="time">0</span></div>
                                        <div class="score-item"><span class="rank">2</span><span class="time">0</span></div>
                                        <div class="score-item"><span class="rank">3</span><span class="time">0</span></div>
                                        <div class="score-item"><span class="rank">4</span><span class="time">0</span></div>
                                        <div class="score-item"><span class="rank">5</span><span class="time">0</span></div>
                                    </div>
                                </div>

                                <div style="margin-top: 30px;">
                                    <button class="play-again-button" onclick="clearSession()">Clear Session</button>
                                    <button class="play-again-button" onclick="goToMenu()">Back to Menu</button>
                                </div>
                            </div>
                        </div>

                        <script>
                            // Game state
                            let raceStarted = false;
                            let raceFinished = false;
                            let currentProblemIndex = 0;
                            let totalProblems = 50; // Increased from 30 to match longer race
                            let correctAnswers = 0;
                            let startTime = 0;
                            let problems = [];
                            let currentAnswer = 0;
                            let problemType = '${problemType}';
                            let playerSpeed = 0;
                            let playerDistance = 0;
                            let aiCars = [];
                            let raceDistance = 800; // pixels
                            let sessionTimes = [];
                            let lastAnswerTime = 0;
                            let currentTimeIndex = 0;
                            let elapsedAnswerTimes = new Array(50).fill(0); // Same as online mode

                            // Start line visibility tracking (same as TrackContainer.razor.cs)
                            let startLineHidden = false;
                            let animationStarted = false;
                            let animationStartTime = 0;
                            let encouragingWords = [
                                "Great job!", "Fantastic!", "Amazing work!", "You're improving!", "Keep it up!",
                                "Excellent racing!", "Outstanding!", "You're on fire!", "Brilliant!", "Well done!"
                            ];

                            // Initialize AI cars with same logic as online mode FastEddy
                            function initializeAICars() {
                                // Using the same time increments as online mode Car.cs
                                const aiTimingData = [
                                    { // Car 2 - Index 1 from online mode
                                        times: [3.54, 4.90, 6.66, 7.97, 10.10],
                                        currentIndex: 0,
                                        speed: 0,
                                        distance: 0,
                                        totalTime: 0,
                                        id: 'car2'
                                    },
                                    { // Car 3 - Index 2 from online mode  
                                        times: [2.59, 3.93, 4.98, 6.88, 9.09, 16.01],
                                        currentIndex: 0,
                                        speed: 0,
                                        distance: 0,
                                        totalTime: 0,
                                        id: 'car3'
                                    },
                                    { // Car 4 - Index 3 from online mode
                                        times: [4.26, 5.69, 7.08, 8.98, 35.29, 36.71],
                                        currentIndex: 0,
                                        speed: 0,
                                        distance: 0,
                                        totalTime: 0,
                                        id: 'car4'
                                    }
                                ];
                                
                                aiCars = aiTimingData;
                            }

                            // Initialize
                            document.addEventListener('DOMContentLoaded', function() {
                                document.getElementById('gameTitle').textContent = getTitle(problemType);
                                document.getElementById('raceTitle').textContent = getTitle(problemType);
                                
                                // Update image sources with current server URL
                                updateImageSources();
                            });
                            
                            function updateImageSources() {
                                // Try to use the current server if available, otherwise use fallbacks
                                const serverUrl = 'http://localhost:5030'; // You can make this dynamic if needed
                                
                                // Update car images
                                const carImages = [
                                    { id: 'playerCar', src: serverUrl + '/images/RaceTrack/RaceCar1.png', fallback: '🏎️ YOU', color: '#4444ff' },
                                    { id: 'car2', src: serverUrl + '/images/RaceTrack/RaceCar2.png', fallback: '🚗 AI', color: '#44ff44' },
                                    { id: 'car3', src: serverUrl + '/images/RaceTrack/RaceCar3.png', fallback: '🚙 AI', color: '#ff4444' },
                                    { id: 'car4', src: serverUrl + '/images/RaceTrack/RaceCar4.png', fallback: '🚐 AI', color: '#ff44ff' }
                                ];
                                
                                carImages.forEach(car => {
                                    const carElement = document.getElementById(car.id);
                                    if (carElement) {
                                        const img = carElement.querySelector('img');
                                        if (img) {
                                            img.src = car.src;
                                            img.onerror = function() {
                                                this.style.display = 'none';
                                                this.parentElement.innerHTML += '<div style="width:100%;height:40px;background:' + car.color + ';border-radius:8px;display:flex;align-items:center;justify-content:center;color:white;font-weight:bold;">' + car.fallback + '</div>';
                                            };
                                        }
                                    }
                                });
                                
                                // Update start/finish line images
                                const startImg = document.querySelector('#startLine img');
                                if (startImg) {
                                    startImg.src = serverUrl + '/images/RaceTrack/StartLine.png';
                                    startImg.onerror = function() {
                                        this.style.display = 'none';
                                        this.parentElement.innerHTML += '<div style="width:100vw;height:20px;background:repeating-linear-gradient(45deg,#000,#000 10px,#fff 10px,#fff 20px);">START</div>';
                                    };
                                }
                                
                                const finishImg = document.querySelector('#finishLine img');
                                if (finishImg) {
                                    finishImg.src = serverUrl + '/images/RaceTrack/FinishLine.png';
                                    finishImg.onerror = function() {
                                        this.style.display = 'none';
                                        this.parentElement.innerHTML += '<div style="width:100vw;height:20px;background:repeating-linear-gradient(45deg,#000,#000 10px,#fff 10px,#fff 20px);">FINISH</div>';
                                    };
                                }
                            }

                            function getTitle(type) {
                                switch(type) {
                                    case 'addition-4stable': return 'Addition 4s Racing';
                                    case 'addition-2stable': return 'Addition 2s Racing';
                                    case 'addition-3stable': return 'Addition 3s Racing';
                                    case 'addition-5stable': return 'Addition 5s Racing';
                                    case 'subtraction-4stable': return 'Subtraction 4s Racing';
                                    case 'multiplication-4stable': return 'Multiplication 4s Racing';
                                    default: return 'Math Racing';
                                }
                            }

                            function generateProblems(type) {
                                problems = [];
                                for (let i = 0; i < totalProblems; i++) {
                                    let problem = generateSingleProblem(type);
                                    problems.push(problem);
                                }
                            }

                            function generateSingleProblem(type) {
                                switch (type) {
                                    case 'addition-4stable':
                                        return generate4sAddition();
                                    case 'addition-2stable':
                                        return generate2sAddition();
                                    case 'addition-3stable':
                                        return generate3sAddition();
                                    case 'addition-5stable':
                                        return generate5sAddition();
                                    case 'subtraction-4stable':
                                        return generate4sSubtraction();
                                    case 'multiplication-4stable':
                                        return generate4sMultiplication();
                                    default:
                                        return generate4sAddition();
                                }
                            }

                            function generate4sAddition() {
                                const a = Math.floor(Math.random() * 10) + 1;
                                const b = 4;
                                return { question: a + ' + ' + b, answer: a + b };
                            }

                            function generate2sAddition() {
                                const a = Math.floor(Math.random() * 10) + 1;
                                const b = 2;
                                return { question: a + ' + ' + b, answer: a + b };
                            }

                            function generate3sAddition() {
                                const a = Math.floor(Math.random() * 10) + 1;
                                const b = 3;
                                return { question: a + ' + ' + b, answer: a + b };
                            }

                            function generate5sAddition() {
                                const a = Math.floor(Math.random() * 10) + 1;
                                const b = 5;
                                return { question: a + ' + ' + b, answer: a + b };
                            }

                            function generate4sSubtraction() {
                                const result = Math.floor(Math.random() * 10) + 1;
                                const b = 4;
                                const a = result + b;
                                return { question: a + ' - ' + b, answer: result };
                            }

                            function generate4sMultiplication() {
                                const a = Math.floor(Math.random() * 12) + 1;
                                const b = 4;
                                return { question: a + ' × ' + b, answer: a * b };
                            }

                            function startRace() {
                                raceStarted = true;
                                raceFinished = false;
                                currentProblemIndex = 0;
                                correctAnswers = 0;
                                startTime = Date.now();
                                playerSpeed = 0;
                                playerDistance = 0;
                                lastAnswerTime = 0;
                                
                                // Reset start line visibility (same as online mode)
                                startLineHidden = false;
                                animationStarted = false;
                                animationStartTime = 0;
                                const startLineElement = document.getElementById('startLine');
                                if (startLineElement) {
                                    startLineElement.classList.remove('hidden');
                                }

                                // Reset elapsed answer times (same as online mode)
                                elapsedAnswerTimes.fill(0);
                                currentTimeIndex = 0;
                                
                                initializeAICars();
                                generateProblems(problemType);
                                
                                document.getElementById('startScreen').style.display = 'none';
                                document.getElementById('raceScreen').style.display = 'block';
                                document.getElementById('resultsScreen').style.display = 'none';
                                
                                // Reset car positions (same as online mode - start at bottom)
                                document.getElementById('playerCar').style.top = '9999px';
                                document.getElementById('car2').style.top = '9999px';
                                document.getElementById('car3').style.top = '9999px';
                                document.getElementById('car4').style.top = '9999px';
                                
                                // Reset track animation
                                document.getElementById('trackBackground').className = 'track-background';
                                
                                showNextProblem();
                                startRaceLoop();
                            }

                            function showNextProblem() {
                                if (currentProblemIndex < totalProblems) {
                                    const problem = problems[currentProblemIndex];
                                    document.getElementById('problemText').textContent = problem.question + ' = ?';
                                    currentAnswer = problem.answer;
                                    document.getElementById('answerInput').value = '';
                                    document.getElementById('answerInput').focus();
                                } else if (!raceFinished) {
                                    finishRace();
                                }
                            }

                            function handleKeyPress(event) {
                                if (event.key === 'Enter' && raceStarted && !raceFinished) {
                                    checkAnswer();
                                }
                            }

                            function handleInput(event) {
                                if (!raceStarted || raceFinished) return;
                                
                                const userAnswer = parseInt(event.target.value);
                                
                                // Auto-submit when correct answer is entered (same as online mode OnAfter)
                                if (userAnswer === currentAnswer) {
                                    checkAnswer();
                                } else if (event.target.value.length >= currentAnswer.toString().length) {
                                    // If they've typed enough digits but it's wrong, clear the input (same as online mode)
                                    setTimeout(() => {
                                        event.target.value = '';
                                    }, 100);
                                }
                            }

                            function checkAnswer() {
                                const userAnswer = parseInt(document.getElementById('answerInput').value);
                                const currentTime = (Date.now() - startTime) / 1000;

                                if (userAnswer === currentAnswer) {
                                    correctAnswers++;
                                    // Record the elapsed time when answer was given (same as online mode)
                                    try {
                                        elapsedAnswerTimes[currentTimeIndex++] = currentTime;
                                    } catch (e) {
                                        // Reset index if array is full
                                        currentTimeIndex = 0;
                                        elapsedAnswerTimes[currentTimeIndex++] = currentTime;
                                    }
                                    lastAnswerTime = currentTime; // Track when answer was given
                                    currentProblemIndex++;
                                    showNextProblem();
                                } else {
                                    // Wrong answer, no speed increase but still move to next problem
                                    currentProblemIndex++;
                                    showNextProblem();
                                }
                            }

                            // Start line helper functions (same logic as TrackContainer.razor.cs)
                            function getAnimationDurationForSpeedClass(speedClass) {
                                const BaseAnimationSpeed = 8.0;
                                return BaseAnimationSpeed / speedClass;
                            }

                            function calculateHidePercentage(speedClass) {
                                if (speedClass <= 3)
                                    return 0.99; // Hide very late for slow speeds
                                else if (speedClass <= 7)
                                    return 0.85; // Hide significantly later for medium speeds
                                else if (speedClass <= 11)
                                    return 0.70; // Hide later for faster speeds
                                else
                                    return 0.60; // Hide later for very fast speeds
                            }

                            function checkStartLineVisibility(isAnyCarAtTop, speedClass, isFinishLineVisible) {
                                // Only proceed if animation is running
                                if (isAnyCarAtTop && !isFinishLineVisible) {
                                    // If animation just started, record the start time
                                    if (!animationStarted) {
                                        animationStarted = true;
                                        animationStartTime = Date.now();
                                        return; // Wait for next check
                                    }

                                    // Calculate how long the animation has been running
                                    const animationDuration = (Date.now() - animationStartTime) / 1000;

                                    // Get the current animation duration based on speed class
                                    const currentAnimationDuration = getAnimationDurationForSpeedClass(speedClass);

                                    // Calculate when the start line should be hidden
                                    const hidePercentage = calculateHidePercentage(speedClass);
                                    const hideTimeSeconds = currentAnimationDuration * hidePercentage;

                                    // If enough time has passed, hide the start line
                                    if (animationDuration >= hideTimeSeconds && !startLineHidden) {
                                        startLineHidden = true;
                                        const startLineElement = document.getElementById('startLine');
                                        if (startLineElement) {
                                            startLineElement.classList.add('hidden');
                                        }
                                    }
                                } else {
                                    // Reset animation tracking if animation stops
                                    if (!isAnyCarAtTop) {
                                        animationStarted = false;

                                        // If we're at the beginning of the race, make sure start line is visible
                                        if (playerDistance < 10 && startLineHidden) {
                                            startLineHidden = false;
                                            const startLineElement = document.getElementById('startLine');
                                            if (startLineElement) {
                                                startLineElement.classList.remove('hidden');
                                            }
                                        }
                                    }
                                }
                            }

                            function startRaceLoop() {
                                if (raceStarted && !raceFinished) {
                                    updateRace();
                                    setTimeout(startRaceLoop, 100); // 10 FPS
                                }
                            }

                            function updateRace() {
                                const elapsed = (Date.now() - startTime) / 1000;
                                
                                // EXACT SAME RACING MECHANICS AS ONLINE MODE
                                const VISIBLE_TRACK_LENGTH = 60.0;
                                const TOP_MARGIN = 0.0;
                                const TOP_MULTIPLIER = 7.0;
                                const TRACK_HEIGHT = 70.0;
                                const TOTAL_DISTANCE = 400; // Increased from 200 to make races longer
                                const START_LINE_INITIAL_TOP = 490; // Same as TrackContainer.razor.cs
                                const START_LINE_FINAL_TOP = -70;
                                
                                // Update player car distance using EXACT same logic as online mode
                                // Calculate distance based on time elapsed since each answer (same as CalculateNewDistance)
                                let currentPlayerDistance = 0;
                                const SPEED_MULTIPLIER = 7.0; // Same as RaceComponents.SPEED_MULTIPLIER
                                const speedIncrement = 1; // Same as online mode

                                for (let i = 0; i < elapsedAnswerTimes.length && elapsedAnswerTimes[i] > 0; i++) {
                                    const raceTime = elapsed - elapsedAnswerTimes[i];
                                    // Apply the multiplier to the distance calculation (same as online mode)
                                    currentPlayerDistance += raceTime * speedIncrement * SPEED_MULTIPLIER;
                                }

                                playerDistance = Math.min(currentPlayerDistance, TOTAL_DISTANCE);

                                // Update player speed for display (same calculation as online mode)
                                let speedCount = 0;
                                for (let i = 0; i < elapsedAnswerTimes.length && elapsedAnswerTimes[i] > 0; i++) {
                                    speedCount++;
                                }
                                playerSpeed = speedCount * speedIncrement;
                                
                                // Update AI cars using same logic as online mode CalculateCurrentDistance
                                aiCars.forEach(car => {
                                    if (car.totalTime === 0) {
                                        // Find the current speed increment based on elapsed time
                                        let lastTime = 0;
                                        let lastDistance = 0;
                                        let currentSpeed = 0;
                                        
                                        for (let i = 0; i < car.times.length; i++) {
                                            if (car.times[i] <= elapsed) {
                                                lastTime = car.times[i];
                                                currentSpeed = i + 1; // Speed increments by 1 each time
                                                if (i === 0) {
                                                    lastDistance = 0;
                                                } else {
                                                    lastDistance += (currentSpeed - 1) * (car.times[i] - car.times[i - 1]) * 7.0; // Same SPEED_MULTIPLIER
                                                }
                                            } else {
                                                break;
                                            }
                                        }
                                        
                                        const timeElapsed = elapsed - lastTime;
                                        const additionalDistance = currentSpeed * timeElapsed * 7.0; // SPEED_MULTIPLIER
                                        car.distance = Math.min(lastDistance + additionalDistance, TOTAL_DISTANCE);
                                        car.speed = currentSpeed;
                                        
                                        if (car.distance >= TOTAL_DISTANCE && car.totalTime === 0) {
                                            car.totalTime = elapsed;
                                        }
                                    }
                                });
                                
                                // Find the lead car's distance (same logic as online ScaleRace)
                                const allCars = [
                                    { distance: playerDistance, speed: playerSpeed, element: document.getElementById('playerCar') },
                                    ...aiCars.map(car => ({ ...car, element: document.getElementById(car.id) }))
                                ];
                                
                                const leadDistance = Math.min(Math.max(...allCars.map(c => c.distance)), TOTAL_DISTANCE);
                                const visibleStart = Math.max(0, leadDistance - VISIBLE_TRACK_LENGTH);
                                
                                // Check if any car has reached the top position
                                const isAnyCarAtTop = allCars.some(car => {
                                    const relativePosition = (car.distance - visibleStart) / VISIBLE_TRACK_LENGTH;
                                    const targetTop = (TOP_MARGIN + (1 - relativePosition) * TRACK_HEIGHT) * TOP_MULTIPLIER;
                                    return targetTop <= TOP_MARGIN * TOP_MULTIPLIER;
                                });
                                
                                // Update track animation based on fastest car speed
                                const fastestSpeed = Math.max(...allCars.map(c => c.speed));
                                const speedClass = Math.min(Math.max(Math.floor(fastestSpeed * 1.5), 1), 15);
                                const trackBackground = document.getElementById('trackBackground');
                                
                                if (isAnyCarAtTop && leadDistance < TOTAL_DISTANCE) {
                                    trackBackground.className = 'track-background animated speed-' + speedClass;
                                } else {
                                    trackBackground.className = 'track-background';
                                }
                                
                                // Check start line visibility (same logic as online mode)
                                const isFinishLineVisible = leadDistance >= TOTAL_DISTANCE;
                                checkStartLineVisibility(isAnyCarAtTop, speedClass, isFinishLineVisible);
                                
                                // Position all cars using the same logic as online mode
                                allCars.forEach(car => {
                                    const relativePosition = (car.distance - visibleStart) / VISIBLE_TRACK_LENGTH;
                                    const targetTop = (TOP_MARGIN + (1 - relativePosition) * TRACK_HEIGHT) * TOP_MULTIPLIER;
                                    car.element.style.top = targetTop + 'px';
                                });
                                
                                // Update finish line position
                                const finishRelativePosition = (TOTAL_DISTANCE - visibleStart) / VISIBLE_TRACK_LENGTH;
                                const finishTop = (TOP_MARGIN + (1 - finishRelativePosition) * TRACK_HEIGHT) * TOP_MULTIPLIER;
                                document.getElementById('finishLine').style.top = finishTop + 'px';
                                
                                // Update speed display (same formula as online)
                                document.getElementById('speedValue').textContent = Math.round(playerSpeed * 7 * 1.09) + 'kph';
                                
                                // Check if player finished
                                if (playerDistance >= TOTAL_DISTANCE && !raceFinished) {
                                    finishRace();
                                }
                            }

                            function finishRace() {
                                raceFinished = true;
                                const finalTime = (Date.now() - startTime) / 1000;
                                
                                // Add to session times
                                sessionTimes.push(finalTime);
                                sessionTimes.sort((a, b) => a - b); // Sort by time
                                if (sessionTimes.length > 5) {
                                    sessionTimes = sessionTimes.slice(0, 5); // Keep only top 5
                                }
                                
                                document.getElementById('raceScreen').style.display = 'none';
                                document.getElementById('resultsScreen').style.display = 'block';
                                
                                // Update results
                                document.getElementById('finalTime').textContent = finalTime.toFixed(3);
                                
                                // Calculate session average
                                const sessionAvg = sessionTimes.reduce((a, b) => a + b, 0) / sessionTimes.length;
                                document.getElementById('sessionAverage').textContent = sessionAvg.toFixed(3);
                                
                                // Show improvement if applicable
                                if (sessionTimes.length > 1) {
                                    const improvement = sessionTimes[sessionTimes.length - 2] - finalTime;
                                    if (improvement > 0) {
                                        document.getElementById('improvement').textContent = '+' + improvement.toFixed(3);
                                        document.getElementById('improvementCard').style.display = 'block';
                                    }
                                }
                                
                                // Update leaderboard
                                updateLeaderboard();
                                
                                // Show encouraging message
                                const randomMessage = encouragingWords[Math.floor(Math.random() * encouragingWords.length)];
                                document.getElementById('encouragingMessage').textContent = randomMessage;
                            }

                            function updateLeaderboard() {
                                const scoreList = document.getElementById('scoreList');
                                const currentTime = sessionTimes[sessionTimes.length - 1];
                                
                                for (let i = 0; i < 5; i++) {
                                    const scoreItem = scoreList.children[i];
                                    const timeSpan = scoreItem.querySelector('.time');
                                    
                                    if (i < sessionTimes.length) {
                                        timeSpan.textContent = sessionTimes[i].toFixed(3);
                                        if (sessionTimes[i] === currentTime) {
                                            scoreItem.classList.add('current-score');
                                        } else {
                                            scoreItem.classList.remove('current-score');
                                        }
                                    } else {
                                        timeSpan.textContent = '0';
                                        scoreItem.classList.remove('current-score');
                                    }
                                }
                            }

                            function restartRace() {
                                document.getElementById('resultsScreen').style.display = 'none';
                                document.getElementById('startScreen').style.display = 'block';
                            }

                            function clearSession() {
                                sessionTimes = [];
                                updateLeaderboard();
                                document.getElementById('sessionAverage').textContent = '0.000';
                                document.getElementById('improvementCard').style.display = 'none';
                            }

                            function goToMenu() {
                                // Try to navigate to offline menu
                                try {
                                    window.location.href = '/offline-menu';
                                } catch (e) {
                                    showOfflineMenu();
                                }
                            }

                            function showOfflineMenu() {
                                document.body.innerHTML = \`
                                    <div style="padding: 20px; max-width: 800px; margin: 0 auto; text-align: center;">
                                        <div style="background: linear-gradient(135deg, #ff6b6b, #ee5a52); color: white; padding: 15px; margin-bottom: 30px; border-radius: 8px;">
                                            <i class="fas fa-wifi-slash"></i>
                                            <strong>Offline Racing Menu</strong>
                                        </div>
                                        <h1>Choose Your Racing Practice</h1>
                                        <div style="display: grid; grid-template-columns: repeat(auto-fit, minmax(250px, 1fr)); gap: 20px; margin: 30px 0;">
                                            <a href="/offline-race/addition-4stable" style="text-decoration: none;">
                                                <div style="background: #007bff; color: white; padding: 30px; border-radius: 8px; text-align: center; transition: transform 0.2s;">
                                                    <i class="fas fa-plus" style="font-size: 2rem; margin-bottom: 10px;"></i>
                                                    <h3>Addition 4s Racing</h3>
                                                    <p>Race with addition problems!</p>
                                                </div>
                                            </a>
                                            <a href="/offline-race/subtraction-4stable" style="text-decoration: none;">
                                                <div style="background: #dc3545; color: white; padding: 30px; border-radius: 8px; text-align: center;">
                                                    <i class="fas fa-minus" style="font-size: 2rem; margin-bottom: 10px;"></i>
                                                    <h3>Subtraction 4s Racing</h3>
                                                    <p>Race with subtraction problems!</p>
                                                </div>
                                            </a>
                                            <a href="/offline-race/multiplication-4stable" style="text-decoration: none;">
                                                <div style="background: #ffc107; color: black; padding: 30px; border-radius: 8px; text-align: center;">
                                                    <i class="fas fa-times" style="font-size: 2rem; margin-bottom: 10px;"></i>
                                                    <h3>Multiplication 4s Racing</h3>
                                                    <p>Race with multiplication problems!</p>
                                                </div>
                                            </a>
                                        </div>
                                    </div>
                                \`;
                            }
                        </script>
                    </body>
                    </html>
                `, {
                    headers: { 'Content-Type': 'text/html' }
                });
            })
        );
        return;
    }

    // For all other requests, use cache-first strategy
    event.respondWith(
        caches.match(event.request)
            .then((response) => {
                // Return cached version if available
                if (response) {
                    return response;
                }

                // Otherwise fetch from network
                return fetch(event.request).then((response) => {
                    // Don't cache non-successful responses
                    if (!response || response.status !== 200 || response.type !== 'basic') {
                        return response;
                    }

                    // Clone the response for caching
                    const responseToCache = response.clone();

                    caches.open(CACHE_NAME)
                        .then((cache) => {
                            cache.put(event.request, responseToCache);
                        });

                    return response;
                }).catch(() => {
                    // If fetch fails and not in cache, return offline page for HTML requests
                    if (event.request.headers.get('accept').includes('text/html')) {
                        return caches.match('/') || new Response(`
                            <!DOCTYPE html>
                            <html>
                            <head>
                                <title>DerbyDash - Offline</title>
                                <meta charset="utf-8" />
                                <meta name="viewport" content="width=device-width, initial-scale=1.0" />
                                <style>
                                    body { font-family: Arial, sans-serif; text-align: center; padding: 50px; }
                                    .offline-message { color: #666; }
                                    .offline-actions { margin-top: 30px; }
                                    .btn { 
                                        display: inline-block; 
                                        padding: 10px 20px; 
                                        background: #007bff; 
                                        color: white; 
                                        text-decoration: none; 
                                        border-radius: 5px; 
                                        margin: 5px;
                                    }
                                </style>
                            </head>
                            <body>
                                <div class="offline-message">
                                    <h1>🏁 DerbyDash</h1>
                                    <h2>You're Offline</h2>
                                    <p>No internet connection detected, but you can still practice!</p>
                                    <div class="offline-actions">
                                        <a href="/offline-race/addition-4stable" class="btn">Practice Addition</a>
                                        <a href="/offline-race/subtraction-4stable" class="btn">Practice Subtraction</a>
                                        <a href="/offline-race/multiplication-4stable" class="btn">Practice Multiplication</a>
                                    </div>
                                </div>
                            </body>
                            </html>
                        `, {
                            headers: { 'Content-Type': 'text/html' }
                        });
                    }
                });
            })
    );
});

// Handle messages from the main thread
self.addEventListener('message', (event) => {
    if (event.data && event.data.type === 'SKIP_WAITING') {
        self.skipWaiting();
    }
});
