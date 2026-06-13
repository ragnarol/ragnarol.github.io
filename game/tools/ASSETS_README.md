# Getting Game Assets

All assets used for this project are **free / CC0** (public domain — no attribution required,
commercial use OK). They are **not committed to the repo** (too large; see `.gitignore`).

Run the download script once on your own machine to populate the asset folders.

---

## Quick Start

**macOS / Linux:**
```bash
cd /path/to/ragnarol.github.io
python3 game/tools/download_assets.py
```

**Windows (PowerShell):**
```powershell
cd C:\path\to\ragnarol.github.io
.\game\tools\download_assets.ps1
```

The script downloads everything and places files into:

| Folder | Contents |
|---|---|
| `Assets/Sprites/Tiles/City/` | Urban tiles (Kenney Buildings) |
| `Assets/Sprites/Tiles/Palace/` | Stone dungeon tiles (Kenney Dungeon) |
| `Assets/Sprites/Tiles/Spaceport/` | Neutral/prototype tiles |
| `Assets/Sprites/Tiles/Wilderness/` | Landscape/nature tiles |
| `Assets/Sprites/Tiles/Generic/` | Isometric blocks (all-purpose) |
| `Assets/Sprites/Characters/` | Tiny Dungeon character sprites |
| `Assets/Sprites/UI/` | RPG UI kit |
| `Assets/Audio/SFX/` | RPG + Interface + Sci-Fi sounds |
| `Assets/Audio/Music/` | *(add manually — see below)* |

---

## If the Script Fails

Kenney URL hashes sometimes change after asset updates. Download manually from:

| Asset | URL | Goes into |
|---|---|---|
| Isometric Miniature Dungeon | https://kenney.nl/assets/isometric-miniature-dungeon | `Tiles/Palace/` |
| Isometric Blocks | https://kenney.nl/assets/isometric-blocks | `Tiles/Generic/` |
| Isometric Landscape | https://kenney.nl/assets/isometric-landscape | `Tiles/Wilderness/` |
| Isometric Tiles Buildings | https://kenney.nl/assets/isometric-tiles-buildings | `Tiles/City/` |
| Isometric Miniature Prototype | https://kenney.nl/assets/isometric-miniature-prototype | `Tiles/Spaceport/` |
| UI Pack RPG Expansion | https://kenney.nl/assets/ui-pack-rpg-expansion | `Sprites/UI/` |
| Tiny Dungeon | https://kenney.nl/assets/tiny-dungeon | `Sprites/Characters/` |
| RPG Audio | https://kenney.nl/assets/rpg-audio | `Audio/SFX/` |
| Interface Sounds | https://kenney.nl/assets/interface-sounds | `Audio/SFX/` |
| Sci-Fi Sounds | https://kenney.nl/assets/sci-fi-sounds | `Audio/SFX/` |

Extract the ZIP and copy the `PNG/` (or audio) files into the corresponding folder.

---

## Music (manual — itch.io requires browser)

The script can't download itch.io assets automatically (requires browser interaction).
Get these free packs by visiting each link, clicking Download, and dropping files into
`Assets/Audio/Music/`:

| Pack | URL | Use for |
|---|---|---|
| Dark Fantasy Studio — Dark Fantasy Music | https://itch.io/game-assets/free/tag-dark-fantasy/tag-music | Palace, Palace at night |
| Free RPG Music — Wanderer | https://itch.io/game-assets/free/genre-rpg/tag-music | City day |
| Free Music Loop Bundle | https://itch.io/game-assets/free/tag-ambient/tag-music | Spaceport, Wilderness |
| RPG Essentials SFX — Free | https://itch.io/game-assets/free/genre-rpg/tag-sound-effects | Extra SFX |

---

## Upgrading Later (Paid, ~$15–50 total)

Once the game design is locked:

1. **Sci-Fi Isometric Pixel Art** (Unity Asset Store, ~$15) → better Spaceport tiles
2. **5600+ RPG Icon Pack** (Unity Asset Store, ~$15) → inventory icons
3. **Custom character sprites** (commission from itch.io artists) → proper isometric
   heroes, a Vorox warrior, an Ur-Obun elder. Budget ~$150–300 for all three.

---

## Unity Import Settings

After running the script, in the Unity Editor:

1. Select all sprites in `Assets/Sprites/` → set **Texture Type: Sprite (2D and UI)**
2. Set **Filter Mode: Point (no filter)** (pixel-perfect)
3. Set **Compression: None** or **RGBA 32 bit** for pixel art
4. For tiles: use the **Tile Palette** window to create tiles from the sprites
5. For audio: select all files in `Assets/Audio/` → set **Load Type: Compressed in Memory**
