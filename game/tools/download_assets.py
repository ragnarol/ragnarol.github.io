#!/usr/bin/env python3
"""
Fading Suns RPG - Asset Download Script
Run this once on your own machine from the repo root:
    python3 game/tools/download_assets.py

Downloads all free CC0 assets and places them in the right Unity folders.
Requires: Python 3.8+, internet access (not needed inside Claude Code cloud).
"""

import os
import sys
import zipfile
import urllib.request
import urllib.error
import shutil
import json
import time
from pathlib import Path

GAME_ROOT = Path(__file__).parent.parent
SPRITES_DIR = GAME_ROOT / "Assets" / "Sprites"
AUDIO_DIR   = GAME_ROOT / "Assets" / "Audio"
TMP_DIR     = GAME_ROOT / "tools" / "_tmp"

# ── Destination folders ───────────────────────────────────────────────────────
DEST = {
    "tiles_city":       SPRITES_DIR / "Tiles" / "City",
    "tiles_palace":     SPRITES_DIR / "Tiles" / "Palace",
    "tiles_spaceport":  SPRITES_DIR / "Tiles" / "Spaceport",
    "tiles_wilderness": SPRITES_DIR / "Tiles" / "Wilderness",
    "tiles_generic":    SPRITES_DIR / "Tiles" / "Generic",
    "characters":       SPRITES_DIR / "Characters",
    "ui":               SPRITES_DIR / "UI",
    "music":            AUDIO_DIR / "Music",
    "sfx":              AUDIO_DIR / "SFX",
}

for d in DEST.values():
    d.mkdir(parents=True, exist_ok=True)
TMP_DIR.mkdir(parents=True, exist_ok=True)

# ── Asset manifest ────────────────────────────────────────────────────────────
# All assets below are free / CC0 / public domain.
# Sources: kenney.nl, opengameart.org, itch.io (no-login free packs)
ASSETS = [
    # ── TILES ────────────────────────────────────────────────────────────────
    {
        "name": "Kenney — Isometric Miniature Dungeon",
        "url":  "https://kenney.nl/media/pages/assets/isometric-miniature-dungeon/0a5d1b7982-1699889325/kenney_isometric-miniature-dungeon.zip",
        "dest": "tiles_palace",
        "strip_prefix": "PNG/",
        "license": "CC0",
        "notes": "Stone dungeon tiles — Palace cellars, ruins, underground areas",
    },
    {
        "name": "Kenney — Isometric Blocks",
        "url":  "https://kenney.nl/media/pages/assets/isometric-blocks/6f48e56d77-1699889294/kenney_isometric-blocks.zip",
        "dest": "tiles_generic",
        "strip_prefix": "PNG/",
        "license": "CC0",
        "notes": "130 generic blocks — good for prototyping all regions",
    },
    {
        "name": "Kenney — Isometric Landscape",
        "url":  "https://kenney.nl/media/pages/assets/isometric-landscape/dbbd4ab9c6-1699889313/kenney_isometric-landscape.zip",
        "dest": "tiles_wilderness",
        "strip_prefix": "PNG/",
        "license": "CC0",
        "notes": "Nature terrain — Wilderness region",
    },
    {
        "name": "Kenney — Isometric Tiles Buildings",
        "url":  "https://kenney.nl/media/pages/assets/isometric-tiles-buildings/bcdb879649-1721997218/kenney_isometric-tiles-buildings.zip",
        "dest": "tiles_city",
        "strip_prefix": "PNG/",
        "license": "CC0",
        "notes": "Urban tiles — City and Spaceport regions",
    },
    {
        "name": "Kenney — Isometric Miniature Prototype (Spaceport stand-in)",
        "url":  "https://kenney.nl/media/pages/assets/isometric-miniature-prototype/ee1b78acaf-1699889319/kenney_isometric-miniature-prototype.zip",
        "dest": "tiles_spaceport",
        "strip_prefix": "PNG/",
        "license": "CC0",
        "notes": "Neutral tiles usable for Spaceport interiors",
    },
    # ── UI ───────────────────────────────────────────────────────────────────
    {
        "name": "Kenney — UI Pack RPG Expansion",
        "url":  "https://kenney.nl/media/pages/assets/ui-pack-rpg-expansion/b1e8bea25c-1699889358/kenney_ui-pack-rpg-expansion.zip",
        "dest": "ui",
        "strip_prefix": "PNG/",
        "license": "CC0",
        "notes": "Buttons, panels, health bars, icons — full UI kit",
    },
    {
        "name": "Kenney — Pixel Platformer UI",
        "url":  "https://kenney.nl/media/pages/assets/pixel-platformer-ui/83de0d8b2b-1699889333/kenney_pixel-platformer-ui.zip",
        "dest": "ui",
        "strip_prefix": "PNG/",
        "license": "CC0",
        "notes": "Pixel art UI elements",
    },
    # ── CHARACTERS ───────────────────────────────────────────────────────────
    {
        "name": "Kenney — Tiny Dungeon (character bases)",
        "url":  "https://kenney.nl/media/pages/assets/tiny-dungeon/a6c1da6e95-1699889355/kenney_tiny-dungeon.zip",
        "dest": "characters",
        "strip_prefix": "PNG/",
        "license": "CC0",
        "notes": "Small character sprites for prototyping NPCs",
    },
    # ── AUDIO — SFX ──────────────────────────────────────────────────────────
    {
        "name": "Kenney — RPG Audio",
        "url":  "https://kenney.nl/media/pages/assets/rpg-audio/d9c5b5a5d2-1699889345/kenney_rpg-audio.zip",
        "dest": "sfx",
        "strip_prefix": "",
        "license": "CC0",
        "notes": "Swords, footsteps, UI clicks, magic — all core RPG SFX",
    },
    {
        "name": "Kenney — Interface Sounds",
        "url":  "https://kenney.nl/media/pages/assets/interface-sounds/81b30baf4a-1699889307/kenney_interface-sounds.zip",
        "dest": "sfx",
        "strip_prefix": "",
        "license": "CC0",
        "notes": "UI sounds for dialogue, menus, keyword clicks",
    },
    {
        "name": "Kenney — Sci-Fi Sounds",
        "url":  "https://kenney.nl/media/pages/assets/sci-fi-sounds/7c1b5ceedd-1699889348/kenney_sci-fi-sounds.zip",
        "dest": "sfx",
        "strip_prefix": "",
        "license": "CC0",
        "notes": "Blasters, terminals, tech sounds for Spaceport",
    },
]

# ── Helpers ───────────────────────────────────────────────────────────────────

def download_with_progress(url: str, dest_path: Path, name: str) -> bool:
    print(f"  Downloading {name}...")
    headers = {
        "User-Agent": "Mozilla/5.0 (compatible; FadingSunsAssetDownloader/1.0)"
    }
    try:
        req = urllib.request.Request(url, headers=headers)
        with urllib.request.urlopen(req, timeout=60) as response:
            total = int(response.headers.get("Content-Length", 0))
            downloaded = 0
            with open(dest_path, "wb") as f:
                while True:
                    chunk = response.read(65536)
                    if not chunk:
                        break
                    f.write(chunk)
                    downloaded += len(chunk)
                    if total:
                        pct = downloaded * 100 // total
                        print(f"\r    {pct}% ({downloaded // 1024} KB)", end="", flush=True)
            print()
        return True
    except urllib.error.HTTPError as e:
        print(f"  HTTP {e.code} — {name}")
        print(f"    URL may have changed. Visit https://kenney.nl to find the updated link.")
        return False
    except Exception as e:
        print(f"  Error downloading {name}: {e}")
        return False


def extract_to(zip_path: Path, dest: Path, strip_prefix: str):
    """Extract ZIP, optionally stripping a path prefix from all entries."""
    with zipfile.ZipFile(zip_path) as zf:
        for member in zf.namelist():
            # Only extract image and audio files
            if not any(member.lower().endswith(ext) for ext in
                       (".png", ".jpg", ".ogg", ".wav", ".mp3", ".aiff")):
                continue

            # Strip prefix
            rel = member
            if strip_prefix and rel.startswith(strip_prefix):
                rel = rel[len(strip_prefix):]

            # Flatten sub-folders: keep only filename
            filename = Path(rel).name
            if not filename:
                continue

            target = dest / filename
            # Don't overwrite existing files
            if target.exists():
                continue

            with zf.open(member) as src, open(target, "wb") as dst:
                shutil.copyfileobj(src, dst)


# ── Main ──────────────────────────────────────────────────────────────────────

def main():
    print("=" * 60)
    print("Fading Suns RPG — Asset Downloader")
    print("=" * 60)
    print(f"Game root: {GAME_ROOT}")
    print()

    failed = []
    succeeded = []

    for asset in ASSETS:
        name = asset["name"]
        url  = asset["url"]
        dest = DEST[asset["dest"]]
        strip = asset.get("strip_prefix", "")

        print(f"[{asset['license']}] {name}")
        print(f"  → {dest.relative_to(GAME_ROOT)}")

        zip_path = TMP_DIR / (name.replace(" ", "_").replace("—", "-") + ".zip")

        if zip_path.exists():
            print("  (cached, skipping download)")
        else:
            ok = download_with_progress(url, zip_path, name)
            if not ok:
                failed.append(name)
                print()
                continue

        try:
            extract_to(zip_path, dest, strip)
            print(f"  Extracted OK")
            succeeded.append(name)
        except Exception as e:
            print(f"  Extract error: {e}")
            failed.append(name)

        print()
        time.sleep(0.3)  # Be polite to servers

    # ── Summary ───────────────────────────────────────────────────────────────
    print("=" * 60)
    print(f"Done. {len(succeeded)} succeeded, {len(failed)} failed.")
    if failed:
        print("\nFailed assets (download manually from the URLs in the script):")
        for f in failed:
            print(f"  - {f}")
        print("\nFor Kenney assets, visit https://kenney.nl/assets and")
        print("click 'Download' — all are free CC0.")

    # Write a manifest of what was downloaded
    manifest = {
        "downloaded": succeeded,
        "failed": failed,
        "destinations": {k: str(v.relative_to(GAME_ROOT)) for k, v in DEST.items()}
    }
    manifest_path = GAME_ROOT / "tools" / "asset_manifest.json"
    with open(manifest_path, "w") as f:
        json.dump(manifest, f, indent=2)
    print(f"\nManifest written to {manifest_path.relative_to(GAME_ROOT)}")

    # Clean up tmp if all succeeded
    if not failed:
        shutil.rmtree(TMP_DIR, ignore_errors=True)
        print("Temp files cleaned up.")


if __name__ == "__main__":
    main()
