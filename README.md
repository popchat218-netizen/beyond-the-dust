# BEYOND THE DUST

**A first-person desert survival prototype about finding a way through a world that wants you gone.**

You wake beyond the old trade road with a failing water filter, a handful of salvage, and something moving across the dunes. Search the wreckage, manage the heat, and build a purifier before nightfall.

## Play the prototype

### In a browser

Open `web/index.html` to play the browser edition, or deploy the repository to Render with the included `render.yaml`. It runs as a static site and does not need Unity installed.

Move with `W A S D` or the on-screen pad. Walk up to pale wreckage and amber supply caches and press `E` to collect them. Press `E` again to drink when no item is nearby. Collect 5 salvage, press `C` to build the purifier, and reach the violet signal mast to transmit. Avoid the sandstalker and keep your hydration above zero. Press `P` to pause.

### In Unity

Open the project folder in **Unity 2022.3 LTS or newer** and press Play from `Assets/Scenes/DesertRun.unity`.

| Input | Action |
| --- | --- |
| `W A S D` | Move |
| Mouse | Look |
| `Space` | Jump |
| `Shift` | Sprint |
| `E` | Scavenge or drink from a nearby source |
| `C` | Craft a water filter when you have enough salvage |
| `P` | Pause or resume |
| `Esc` | Release the mouse |

Click the game view to capture the mouse again. The current slice is a self-contained, code-built environment, so it has no external art or package dependencies.

## The first run

- **Scavenge:** Search pale blue wreckage markers for salvage and amber supply caches for water.
- **Survive:** Hydration drains over time. Sprinting accelerates the drain, and the sun keeps the pressure on.
- **Craft:** Collect 5 salvage and press `C` to assemble the purifier. It slows dehydration and restores a little water.
- **Watch the ridge:** A sandstalker patrols the basin. Keep your distance; it will pursue and strike if you let it close.
- **Reach the beacon:** Find the violet signal mast and transmit after the purifier is online.
- **Pause the run:** Press `P` to pause, resume, or restart from the field menu.

## Project layout

- `web/` — a self-contained browser edition, hosted as a Render static site.
- `render.yaml` — Render Static Site blueprint for the browser edition.
- `Assets/Scenes/DesertRun.unity` — the playable scene.
- `Assets/Scripts/` — movement, survival loop, scavenging, creature AI, and procedural world.
- `ProjectSettings/` — project identity and build scene.

The terrain, props, lighting, pickups, and creature are assembled from Unity primitives at runtime. Replace the blockout assets with authored models and audio as production art becomes available.
