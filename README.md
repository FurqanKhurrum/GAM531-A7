## 📖 Overview

This project builds upon the base repository [**SpriteGameOpenTk**](https://github.com/mouraleonardo/SpriteGameOpenTk), extending it into a more dynamic and responsive 2D sprite animation system.  
Developed in **C#** using **OpenTK 4.x** and **OpenGL 3.3**, it introduces advanced animation control, input-driven state management, and a smooth camera-follow system with a multi-tile scrolling background.

The core objective was to expand the basic walking animation into a fully featured player controller — integrating realistic motion, multiple animation states, and improved scene rendering for a polished gameplay feel.

![gameDemo](https://github.com/user-attachments/assets/65e759dd-32d7-467d-9c89-538c4e8f7179)

---

## ⚙️ Implemented Features

### 🎮 Movement & Mechanics
- **Jumping**: Uses vertical velocity and gravity simulation for realistic arcs.  
- **Running / Sprinting**: Doubles movement speed when holding `Shift` or `Right Shift`.  
- **Attack Combos**: Implements 3 chained attacks (`J` or `Z`), with cooldown timing and combo rotation.  
- **Defending**: Defensive stance triggered by `K` or `X`.  
- **Idle, Walk, Run, Jump, Attack, Defend** states all supported through a robust state machine.

### 🎞️ Animation System
- Each animation state references its own sprite sheet (Idle, Walk, Run, Jump, Attack1–3, Defend, etc.).  
- Animation frame timing and transitions are dynamically managed via per-state frame durations.  
- Directional flipping (left/right) handled through matrix scaling, maintaining consistent visuals.  
- Attack animations play once; movement animations loop continuously.

### 🧠 State Machine Logic
The character’s behavior is managed by a **finite state machine**:
1. Input is read every frame (`InputState` struct).  
2. Logic determines transitions based on current state, inputs, and physics.  
3. Animation frames update based on elapsed time and state-specific frame timing.  

Priority order ensures proper handling of overlapping actions:
> Attack → Defend → Jump → Move → Idle  

### 🎥 Camera & Scene System
- Implemented a **smooth camera follow** centered on the player using interpolation (`CameraSmoothness`).  
- **Orthographic projection** updates dynamically based on camera position.  
- **Background system**: loads a high-resolution image, preserving its aspect ratio and tiling it seamlessly across the scene width (no stretching or zoom distortion).

### 🔊 Audio System
- **NAudio integration** for sound effects and background music
- Unique sounds for punches, kicks, jumps, and landing
- Audio plays on every input for instant feedback
- Separate volume controls for music and sound effects
---

## 🧩 Technical Highlights
- Built with **OpenGL 3.3 Core Profile** via OpenTK.  
- Custom GLSL shader pipeline for sprite rendering.  
- **NAudio** for cross-platform audio playback  
- Modular helper classes:
  - `AudioManager.cs` — Sound effect and music management  
  - `ShaderHelper.cs` — shader compilation/linking  
  - `SpriteSheetHelper.cs` — UV frame calculations  
  - `TextureLoader.cs` — image loading via ImageSharp  
  - `Character.cs` — main physics and animation controller  
  - `SpriteAnimationGame.cs` — main game loop and rendering system  

---

## 💡 Challenges & Solutions
| Challenge | Solution |
|------------|-----------|
| Maintaining consistent sprite scaling across different resolutions | Introduced orthographic projection and pixel-based background scaling. |
| Preventing animation state conflicts (e.g., attack interrupting jump) | Added strict priority-based state transition logic. |
| Character clipping beyond scene edges | Implemented position clamping and adjustable scene width. |
| Background stretching issues | Reworked scaling to preserve original image proportions during rendering. |
| Audio not playing on every button press | Decoupled audio playback from animation cooldown timers |

---

## 🏁 Controls
| Key | Action |
|-----|---------|
| **← / A** | Move Left |
| **→ / D** | Move Right |
| **Shift** | Sprint |
| **Space / W** | Jump |
| **J / Z** | Attack |
| **K / X** | Defend |

---

## 🧰 Setup & Run Instructions

### Prerequisites
- **.NET 8 SDK** or later installed  
- **OpenTK 4.x** (automatically restored via NuGet)  
- **SixLabors.ImageSharp** for texture loading
- **NAudio 2.2.1** for audio playback  

### Steps to Run
1. Clone or extract the project folder.  
2. Open the solution in **Visual Studio 2022** or run from terminal:  
   ```bash
   dotnet run
   ```
3. Ensure your `/Assets/Sprites/` folder contains all animation textures and `Background.png`.
4. Add audio files to `/Assets/Audio/`:
   - punch.wav, jump.wav
5. Use the controls above to interact with the scene.  

### Notes
- The game uses an **orthographic projection** of 800×600.  
- To adjust scene width or camera smoothing, modify `SceneWidth` and `CameraSmoothness` in `SpriteAnimationGame.cs`.

---

## 🧮 References

## Character Sprite
- https://craftpix.net/freebies/free-knight-character-sprites-pixel-art

## Background Image
- https://craftpix.net/freebies/free-pixel-art-fantasy-2d-battlegrounds  

### Audio Resources
- [Freesound.org](https://freesound.org/) - Free sound effects library

---
