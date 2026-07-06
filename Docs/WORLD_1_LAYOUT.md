# World 1: Dino Jungle — Board Layout

**Theme:** Lush green jungle with vines, trees, ancient ruins  
**Total Tiles:** 25 (arranged in a winding path, inspired by classic Snakes & Ladders)  
**Difficulty:** Easy (introduction to mechanics)  
**Story Context:** "Help Dino & Cat find the Portal Key hidden at the end of this jungle!"

---

## Board Map (Text Layout)

```
[START]
  1 ─── 2 ─── 3 ─── 4
              │
  8 ─── 7 ─── 6 ─── 5
  │
  9 ─── 10 ─── 11 ─── 12
              │
  16 ─── 15 ─── 14 ─── 13
  │
  17 ─── 18 ─── 19 ─── 20
              │
  24 ─── 23 ─── 22 ─── 21
  │
  25 ─── [GOAL / Portal Key]
```

---

## Tile Descriptions & Types

| Tile | Type | Description | Effect |
|------|------|-------------|--------|
| 1 | START | Lush jungle entrance | Player spawn point |
| 2 | Normal | Forest path with ferns | No effect |
| 3 | Normal | Moss-covered rocks | No effect |
| 4 | Normal | Jungle vines | No effect |
| 5 | **Portal** ↑ | Hidden waterfall (shimmers) | Warp forward +4 tiles (to tile 9) |
| 6 | Normal | Muddy ground | Slightly slowed movement (visual only) |
| 7 | Normal | Ancient stone ruin | No effect; hints at mystery |
| 8 | **Whirlpool** ↓ | Swampy vortex | Spin backward -3 tiles (to tile 5) |
| 9 | Normal | Vine bridge | No effect |
| 10 | Normal | Bamboo grove | No effect |
| 11 | Normal | River crossing | No effect |
| 12 | **Elevator** ⬆ | Hanging vine lift | Rise up +3 tiles (to tile 15) |
| 13 | Normal | Fern patch | No effect |
| 14 | Normal | Stone carved path | No effect |
| 15 | **Portal** ↑ | Underground cave entrance (glows) | Warp forward +3 tiles (to tile 18) |
| 16 | Normal | Jungle canopy | No effect |
| 17 | Normal | Hidden waterhole | No effect |
| 18 | **Hyperspace Jump** | Mysterious glowing orb | Random teleport: 50% forward to tile 21, 50% back to tile 14 |
| 19 | Normal | Ancient temple steps | No effect |
| 20 | Normal | Overgrown statue | No effect |
| 21 | **Whirlpool** ↓ | Quicksand pit | Spin backward -4 tiles (to tile 17) |
| 22 | Normal | Jungle clearing | No effect |
| 23 | Normal | Exotic flowers | No effect |
| 24 | **BOSS** 🦖 | Giant T-Rex encounter | Mini-boss: Pattern-dodge challenge (win = advance, fail = retry) |
| 25 | **GOAL** | Portal Key chamber (shimmers magically) | Victory! Return home → Story ending |

---

## Obstacles & Hazards (Per Tile Section)

### Section 1 (Tiles 1–5): "Welcome to the Jungle"
**Difficulty:** Very Easy  
**Obstacle Types:** Rolling rocks, simple pits  
**Learning:** Introduction to jumping, basic platforming

### Section 2 (Tiles 6–12): "The Muddy Path"
**Difficulty:** Easy  
**Obstacle Types:** Mud patches (slow you down), vines to swing, small gaps  
**Learning:** Timing jumps, pattern recognition (mud slows you)

### Section 3 (Tiles 13–20): "Temple Ruins"
**Difficulty:** Easy-to-Moderate  
**Obstacle Types:** Crumbling platforms, lava pits, moving stone blocks  
**Learning:** Spatial reasoning (platform sequencing), dodge timing

### Section 4 (Tiles 21–24): "The Final Approach"
**Difficulty:** Moderate  
**Obstacle Types:** Quicksand, collapsing bridges, faster-moving hazards  
**Learning:** Risk/reward (take the shortcut or play it safe?), boss encounter

---

## Power-Ups & Collectibles

**Available Power-Ups (scattered throughout):**
- **Sprint Boost** (green leaf): Move +2 extra tiles next turn
- **Dinosaur Stomp** (footprint icon): Shake ground to clear obstacles in current tile
- **Shield** (blue gem): Protect against one obstacle hit

**Placement Strategy:**
- Spread evenly (1 per 5–6 tiles on average)
- More clustered near boss (Section 4) to prepare player
- Optional to collect (kids explore at own pace)

---

## Story Beats

**Intro (Before Tile 1):**
> "Welcome to Dino Jungle! Dino and Cat are lost! Help them find the Portal Key to return home. Watch out for obstacles and use the magical portals to leap ahead!"

**Mid-Game (Tile 12):**
> "Great job! You're halfway there. The temple ruins ahead hold more secrets. Keep going!"

**Pre-Boss (Tile 23):**
> "Oh no! A Giant T-Rex is guarding the final chamber! Stay calm and dodge its attacks. You can do this!"

**Victory (Tile 25):**
> "You found the Portal Key! Dino and Cat are going home! *Celebration sequence* THE END!"

---

## Educational Mapping

| Learning Goal | How It's Taught |
| --- | --- |
| **Counting** | Roll dice (1–6), add up tile moves ("Roll 4, I'm at tile 5") |
| **Spatial Reasoning** | Jump gaps, time platform jumps, navigate tile-to-tile flow |
| **Pattern Recognition** | Observe portal locations, whirlpool clusters, predict board flow |
| **Turn-Taking** | Multiplayer mode (wait for other player, celebrate/support) |

---

## MVP Buildout Phases

**Phase 1 (Sprint 1 Prototype):** Tiles 1–5 only (5-tile test level)  
**Phase 2 (Sprint 2 Full Build):** All 25 tiles with mechanics  
**Phase 3 (Sprint 3–4 Polish):** Story, animations, audio, UI integration

---

## Visual Reference

Refer to `/Research/` for mood boards:
- Dinosaur references: `trex.jpeg`, `trex2.jpeg`, `dinosours.jpeg`, `terradactile.jpeg`
- Jungle/forest vibes: `backdrop.jpeg`, `space.jpeg` (for inspiration on layering)
- Custom characters: `/Research/MontyDrawings/` (Dino & Cat)

**Aesthetic Goal:** Bright, playful, hand-drawn style. Friendly dinosaurs and tropical colors. Nothing scary or frustrating.
