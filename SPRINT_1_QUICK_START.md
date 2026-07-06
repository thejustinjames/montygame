# Sprint 1: Quick Start Guide

**Goal:** Get a Unity project running with MontyGame.Core integrated, 5-tile level, playable in 7–10 days.

---

## 🚀 Start Here (30 min setup)

### Step 1: Create Unity Project

```bash
# Option A: Unity Hub (recommended for GUI)
# 1. Open Unity Hub
# 2. New Project → 2D → Create
# 3. Name: "MontyGame" or "MontyGame-Shell"
# 4. Version: 2022.3 LTS or later
# 5. Location: /Users/justin/Documents/GitHub/MontyGame/MontyGame-Unity

# Option B: Command line (if you have Unity CLI installed)
unity -createProject MontyGame-Unity -projectPath ./MontyGame-Unity
```

### Step 2: Folder Structure

```bash
# Inside your new Unity project:
MontyGame-Unity/
├── Assets/
│   ├── Scenes/              # Your scene files
│   ├── Scripts/             # C# MonoBehaviours
│   │   ├── Controllers/
│   │   ├── UI/
│   │   ├── Board/
│   │   └── Player/
│   ├── Sprites/             # Graphics (will be placeholder boxes for now)
│   ├── Prefabs/             # Player, tiles, etc.
│   ├── Audio/               # Music, SFX (add later)
│   └── Resources/           # Config files
├── Packages/                # Dependencies
└── ProjectSettings/         # Unity config
```

### Step 3: Reference MontyGame.Core

**Option A: Project Reference (Simplest for now)**

1. Open your Unity project's `Assets` folder
2. Create a symlink or copy the `MontyGame.sln` path
3. In Visual Studio or your IDE:
   - Add project reference to `src/MontyGame.Core/MontyGame.Core.csproj`
   - Build to verify no errors

**Option B: Build & Copy DLL (Later)**

```bash
# In terminal, from MontyGame root:
dotnet build src/MontyGame.Core
# Copy bin/Debug/net8.0/MontyGame.Core.dll to Unity/Assets/Plugins/
```

**Option C: NuGet (Post-MVP)**

```bash
# Pack the Core for NuGet
dotnet pack src/MontyGame.Core
# Push to NuGet or local feed
```

---

## 🎮 Build Plan: 5 Phases (7–10 days)

### Phase 1: Player Controller (Day 1–2)

**Create `Assets/Scripts/Player/PlayerController.cs`:**

```csharp
using UnityEngine;

public class PlayerController : MonoBehaviour {
    [SerializeField] float moveSpeed = 6f;
    [SerializeField] float jumpForce = 10f;
    [SerializeField] float groundDrag = 5f;
    
    private Rigidbody2D rb;
    private bool isGrounded;
    
    void Start() {
        rb = GetComponent<Rigidbody2D>();
    }
    
    void Update() {
        HandleMovement();
        HandleJump();
    }
    
    void HandleMovement() {
        float input = Input.GetAxis("Horizontal");
        rb.velocity = new Vector2(input * moveSpeed, rb.velocity.y);
    }
    
    void HandleJump() {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded) {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            isGrounded = false;
        }
    }
    
    void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.CompareTag("Ground")) {
            isGrounded = true;
        }
    }
    
    void OnCollisionExit2D(Collision2D collision) {
        if (collision.gameObject.CompareTag("Ground")) {
            isGrounded = false;
        }
    }
}
```

**Create Player Prefab:**
1. Create empty GameObject: "Player"
2. Add Rigidbody2D (Body Type: Dynamic, Gravity Scale: 1)
3. Add BoxCollider2D (size: 0.5 × 1)
4. Add PlayerController script
5. Add sprite (placeholder: colored quad) or use SpriteRenderer with white circle
6. Tag it "Player"
7. Save as `Assets/Prefabs/Player.prefab`

**Test:** Can you move left/right and jump? Does it feel responsive?

---

### Phase 2: Board & Tiles (Day 2–3)

**Create `Assets/Scripts/Board/Tile.cs`:**

```csharp
using UnityEngine;

public class Tile : MonoBehaviour {
    public int tileId;
    public string tileName;
    
    void Start() {
        // Update label with tile ID
        GetComponentInChildren<TextMesh>().text = $"Tile {tileId}";
    }
}
```

**Create Tile Prefab:**
1. Create empty GameObject: "Tile"
2. Add SpriteRenderer (sprite: white square or colored quad)
3. Add BoxCollider2D (size: 2 × 1, is Trigger: false)
4. Add Text mesh showing tile ID
5. Add Tile script
6. Save as `Assets/Prefabs/Tile.prefab`

**Create `Assets/Scripts/Board/BoardManager.cs`:**

```csharp
using UnityEngine;

public class BoardManager : MonoBehaviour {
    [SerializeField] GameObject tilePrefab;
    [SerializeField] Transform boardParent;
    
    void Start() {
        CreateTestBoard();
    }
    
    void CreateTestBoard() {
        // Create 5 tiles in a row
        for (int i = 1; i <= 5; i++) {
            var tile = Instantiate(tilePrefab, boardParent);
            tile.transform.position = new Vector3(i * 3, 0, 0); // 3 units apart
            var tileScript = tile.GetComponent<Tile>();
            tileScript.tileId = i;
            tileScript.tileName = $"Test Tile {i}";
        }
    }
}
```

**Create board in scene:**
1. Create empty GameObject: "Board"
2. Add BoardManager script
3. Assign Tile prefab and set Board as parent
4. Play scene; see 5 tiles appear

**Test:** Can player jump across tiles? Does spacing feel right?

---

### Phase 3: GameEngine Integration (Day 3–4)

**Create `Assets/Scripts/GameController.cs`:**

```csharp
using UnityEngine;
using MontyGame.Core;

public class GameController : MonoBehaviour {
    private GameEngine gameEngine;
    private World world;
    
    void Start() {
        // Initialize core game engine
        world = World1Factory.CreateWorld1();
        gameEngine = new GameEngine(world, new UnityRandom());
        
        Debug.Log("Game started. Players: Dino & Cat");
    }
    
    public void OnRollDiceClicked() {
        int roll = gameEngine.RollDice();
        Debug.Log($"Rolled: {roll}");
        
        var result = gameEngine.MovePlayer(gameEngine.CurrentPlayerId, roll);
        Debug.Log($"Moved to tile {result.NewTile}");
    }
}
```

**Create `Assets/Scripts/UnityRandom.cs` (adapter for Core's IRandom):**

```csharp
using MontyGame.Core;
using Random = System.Random;

public class UnityRandom : IRandom {
    private Random random = new Random();
    
    public int Next(int maxExclusive) {
        return random.Next(maxExclusive);
    }
}
```

**Add UI:**
1. Create Canvas in scene
2. Add Button: "Roll Dice"
3. Wire button to GameController.OnRollDiceClicked()
4. Add Text display for position: "Tile X of 25"

**Test:** 
- Click "Roll Dice"
- Check Debug.Log for roll and movement
- Verify GameEngine working from Unity

---

### Phase 4: Player Movement Integration (Day 4–5)

**Create `Assets/Scripts/PlayerMover.cs`:**

```csharp
using UnityEngine;
using MontyGame.Core;

public class PlayerMover : MonoBehaviour {
    public GameController gameController;
    private Vector3 targetPosition;
    private float speed = 5f;
    
    void Update() {
        // If gameController has new position, move towards it
        // (Will implement when GameController provides position updates)
    }
    
    public void MoveToTile(int tileId) {
        // Calculate tile position and animate player there
        // Find tile by ID, get its position
        // Smoothly move player to that tile
    }
}
```

**Wire GameController to PlayerMover:**
1. GameController calls gameEngine.MovePlayer()
2. PlayerMover listens for position changes
3. Animates player movement to new tile

**Test:**
- Roll dice
- See player move on screen to new tile
- Verify distance matches roll value

---

### Phase 5: Polish & Validation (Day 5–7)

**Playtesting:**
- [ ] Roll dice multiple times
- [ ] Walk/jump across all 5 tiles
- [ ] Does platforming feel forgiving?
- [ ] Is jumping responsive?
- [ ] No crashes?

**Physics tuning (if needed):**
- Jump too high? Lower `jumpForce` in PlayerController
- Jump too short? Raise `jumpForce`
- Movement too slow? Raise `moveSpeed`
- Landing feels harsh? Increase ground drag

**Code cleanup:**
- Add comments
- Organize scripts into folders
- Verify architecture (no game rules in Unity)

**Save & commit:**
```bash
git add Assets/
git commit -m "Sprint 1: Basic Unity shell with 5-tile prototype and player controller"
git push
```

---

## 🧪 Validation Checklist

**Core (should still pass):**
- [ ] `dotnet test` → 78/78 passing ✅
- [ ] `dotnet run --project src/MontyGame.Cli -- --auto` → Victory ✅

**Unity (new):**
- [ ] Player moves with arrow keys ✅
- [ ] Player jumps with space ✅
- [ ] 5 tiles render in scene ✅
- [ ] Clicking "Roll Dice" shows roll in Debug.Log ✅
- [ ] Player moves based on roll amount ✅
- [ ] No crashes running for 5+ minutes ✅
- [ ] 60+ FPS in Game view ✅
- [ ] Platforming feels forgiving (subjective) ✅

---

## 🎯 Success Criteria (End of Sprint 1)

✅ Player can navigate 5-tile level with platformer controls  
✅ Clicking "Roll Dice" moves player correct distance  
✅ Platforming physics feel gentle and forgiving  
✅ No crashes; clean architecture (Core separate from Unity)  
✅ Ready to scale to 25 tiles in Sprint 2  

---

## 📚 References

- **Physics tuning:** `/Docs/adr/ARCHITECTURE_PLATFORMER_PHYSICS.md`
- **Core API:** `src/MontyGame.Core/GameEngine.cs`
- **Tile data:** `src/MontyGame.Core/Board.cs`
- **Full spec:** `/Planning/SPRINT_1_SPEC.md`

---

## 💡 Tips

1. **Iterate on feel early** — Physics tuning should happen first; everything else depends on it
2. **Keep it simple** — Placeholder sprites are fine; focus on gameplay
3. **Verify integration** — Test Core + Unity connection before adding complexity
4. **Commit often** — Every working phase; git is your friend
5. **Playtest yourself** — Does it feel fun? That's the real metric

---

**You're ready! Start with Step 1 above.** 🚀

Questions? Check `/Planning/SPRINT_1_SPEC.md` for detailed breakdown.
