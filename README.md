# 🐔 Chicken Run

A 3-lane endless runner built in **Unity 6**. Run as far as you can, collect eggs, dodge obstacles, and don't let the fox catch you.

**Group:** SE-G2
**Team:** Thoung Socheata, So Channolly, Hem Rothapanharith, Von Visal

---

## Gameplay

- The chicken runs forward automatically across 3 lanes.
- Collect **eggs** for points.
- Avoid **obstacles**:
  - **Low** obstacles (fence, rock, haybale...) - jump over them.
  - **High** obstacles (beam) - crouch under them.
- The game **speeds up** over time.
- **Fox:** the first hit brings the fox in. A second hit while the fox is near ends the game. If you stay safe long enough, the fox goes away.
- Your **best score** is saved on your device.

### Score

```
Score = (distance / 10) + (eggs x 10)
```

## Controls

| Action        | Keys                     |
| ------------- | ------------------------ |
| Move left     | `A` / `Left Arrow`       |
| Move right    | `D` / `Right Arrow`      |
| Jump          | `Space` / `W` / `Up`     |
| Crouch        | `S` / `Down Arrow`       |
| Pause / Resume| `Esc`                    |

## Requirements

- **Unity 6000.3.8f1** (install the exact version from Unity Hub)
- Render pipeline: URP
- Input: Unity Input System

## How to Run

1. Clone the repo:
   ```bash
   git clone https://github.com/RITHCD/ChickenRun-Game-Dev.git
   ```
2. Open **Unity Hub** -> **Add** -> select the cloned folder.
3. Open the project with Unity `6000.3.8f1` (first import takes a few minutes).
4. Open the scene `Assets/Game.unity` and press **Play**.

## Project Structure

```
Assets/
  Scripts/
    Game/        GameManager, ScoreManager, SpeedController, FoxManager, GameState
    World/       WorldManager (endless tiles), GroundTile, Obstacle
    UI/          Title / Play button animations
    PlayerControl.cs   Player movement, jump, crouch, camera, pause
  Prefabs/       Player, obstacles, tiles, world props
  Scenes/        Scenes
  Audio/ Image/  Sound and images
```

### Main scripts

| Script            | What it does                                                    |
| ----------------- | --------------------------------------------------------------- |
| `GameManager`     | Game state (Menu / Playing / Paused / GameOver), start, pause, restart |
| `ScoreManager`    | Distance, eggs, score, best score                               |
| `SpeedController` | Starts at 12, +0.25 per second, max 32                          |
| `FoxManager`      | Counts hits, shows the fox, ends the game on the 2nd hit        |
| `WorldManager`    | Spawns and recycles ground tiles, obstacles and eggs            |
| `PlayerControl`   | Lane movement, jump, crouch, camera follow, collisions          |

### Calling the game from other scripts

```csharp
GameManager.I.AddEgg();      // player collected an egg
GameManager.I.Hit();         // player hit an obstacle
GameManager.I.GameOver();    // end the game
GameManager.OnGameOver += () => { /* show Game Over panel */ };
```

## Git Workflow

Work on a branch first, then merge:

```bash
git checkout -b feature/my-feature
git add .
git commit -m "Describe your change"
git push -u origin feature/my-feature
```

Then open a **Pull Request** on GitHub and merge into `main`. Before starting new work:

```bash
git checkout main
git pull origin main
```

> Always open the project in Unity once before committing so `.meta` files are created, and commit them together with their files.
