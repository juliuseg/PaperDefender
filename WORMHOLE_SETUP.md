# WormHole Upgrade Setup Guide

## Overview
The WormHole upgrade allows players to place a wormhole and shoot bullets from it instead of the standard spawn point.

## Implementation Details

### 1. Scripts Created
- `WormHoleController.cs` - Controls the wormhole's visual effects and behavior
- `WormHolePrefab.cs` - Helper script for creating the wormhole prefab
- Updated `Player_Controller.cs` - Added wormhole placement and bullet spawn point logic
- Updated `Upgrade.cs` - Added WormHole to the UpgradeEffect enum
- Created `WormHole.asset` - The upgrade asset file

### 2. How It Works
1. **Activation**: When WormHole upgrade is selected (number key), left-click places/moves the wormhole
2. **Placement**: First click creates the wormhole, subsequent clicks move it
3. **Shooting**: When WormHole upgrade is active, bullets spawn from the wormhole instead of player
4. **Persistence**: Wormhole remains active until game reset or player dies

### 3. Setup Instructions

#### Create WormHole Prefab:
1. Create an empty GameObject in your scene
2. Add a SpriteRenderer component
3. Assign a circular sprite (you can use a simple circle sprite)
4. Add the `WormHoleController` script
5. Set the sprite color to purple or your preferred color
6. Create a prefab from this GameObject
7. Assign the prefab to the `WormHolePrefab` field in Player_Controller

#### Add to Upgrade Database:
1. Open your Upgrade Database asset
2. Add the WormHole.asset to the allUpgrades list
3. The upgrade will now appear in upgrade selection

#### Configure Player_Controller:
1. Assign the WormHole prefab to the `WormHolePrefab` field
2. The system will automatically handle the rest

### 4. Usage
1. Collect the WormHole upgrade
2. Press the number key to select it (e.g., key 1, 2, 3, etc.)
3. Left-click to place the wormhole
4. Left-click again to move the wormhole to a new position
5. When WormHole is active, bullets will spawn from the wormhole
6. When WormHole is not selected, bullets spawn from the wormhole if it exists

### 5. Features
- Visual pulsing and rotation animation
- Automatic cleanup on game reset
- Seamless integration with existing bullet system
- No cooldown (can be modified in the asset file)

## Notes
- The wormhole uses the same bullet force and behavior as normal shooting
- Light flash effects also spawn from the wormhole when active
- The system automatically falls back to player spawn point when wormhole is not available 