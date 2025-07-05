# Angel Upgrade Setup Guide

## Overview
The Angel upgrade spawns protective angel shields when enemies die. These shields have 3 lives and award 10 points each time they block an enemy projectile.

## Implementation Details

### 1. Scripts Created
- `AngelController.cs` - Controls individual angel shield behavior
- `AngelUpgradeHandler.cs` - Manages angel spawning and cleanup
- Updated `EnemyController.cs` - Added position-based death events
- Updated `GoblinSpawner.cs` - Added angel death event handling
- Updated `GameFlowManager.cs` - Added angel cleanup on reset
- Updated `Upgrade.cs` - Added Angel to the UpgradeEffect enum
- Created `Angel.asset` - The upgrade asset file

### 2. How It Works
1. **Passive Upgrade**: Angel is a passive upgrade that activates automatically
2. **Enemy Death Trigger**: When any enemy dies, an angel shield spawns nearby
3. **Protection**: Angel shields block enemy projectiles (stones)
4. **Points**: Each hit awards 10 points to the player
5. **Lives**: Each angel has 3 lives before being destroyed
6. **Cleanup**: Angels are automatically cleaned up on game reset

### 3. Setup Instructions

#### Create Angel Prefab:
1. Create an empty GameObject in your scene
2. Add a SpriteRenderer component
3. Assign your angel sprite
4. Add a Collider2D component (set as trigger)
5. Add the `AngelController` script
6. Configure the settings:
   - `maxLives`: 3 (default)
   - `fadeOutTime`: 0.5f (default)
   - `angelColor`: White with transparency (default)
7. Create a prefab from this GameObject

#### Add AngelUpgradeHandler to GameController:
1. Select your GameController GameObject
2. Add the `AngelUpgradeHandler` component
3. Assign the angel prefab to the `angelPrefab` field
4. Configure settings:
   - `spawnRadius`: 2f (distance from enemy death)
   - `maxAngelsOnScreen`: 5 (maximum angels allowed)

#### Add to Upgrade Database:
1. Open your Upgrade Database asset
2. Add the Angel.asset to the allUpgrades list
3. The upgrade will now appear in upgrade selection

### 4. Features
- **Automatic Spawning**: Angels spawn automatically when enemies die
- **Position-Based**: Angels spawn at random positions around enemy death location
- **Projectile Protection**: Blocks enemy stones and projectiles
- **Point Rewards**: 10 points per hit on angel shields
- **Visual Feedback**: Angels flash red when hit
- **Fade Out**: Smooth fade out animation when destroyed
- **Limit Management**: Maximum 5 angels on screen (configurable)
- **Automatic Cleanup**: All angels removed on game reset

### 5. Technical Details
- **Event System**: Uses C# events for enemy death notifications
- **Position Tracking**: Enemy death position is passed directly to spawn system
- **Collision Detection**: Uses OnTriggerEnter2D for projectile detection
- **Tag System**: Responds to "EnemyProjectile" and "Stone" tags
- **Memory Management**: Automatic cleanup prevents memory leaks

## Notes
- Angels are passive shields that don't require player interaction
- The system automatically manages the maximum number of angels
- Angel shields work independently of player shields
- Points are awarded through the existing PointSystem
- The upgrade is passive, so no cooldown or activation required 