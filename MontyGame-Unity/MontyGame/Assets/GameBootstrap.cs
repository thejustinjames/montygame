using UnityEngine;

/// <summary>
/// Builds the ENTIRE test scene in code when you press Play.
/// No manual Unity editor setup needed — just hit Play.
///
/// Runs automatically via [RuntimeInitializeOnLoadMethod] — this static method
/// is called by Unity at runtime with NO GameObject required. It:
///   1. Cleans up any leftover objects from manual attempts
///   2. Creates a camera
///   3. Creates the player (yellow square) with physics + controller
///   4. Creates the ground
///   5. Creates 5 numbered tiles
///
/// You can safely have an empty scene — this builds everything.
/// </summary>
public static class GameBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void BuildScene()
    {
        Debug.Log("🔧 GameBootstrap: Building scene from code...");

        CleanupOldObjects();
        CreateCamera();
        CreateGround();
        CreateTiles();
        CreatePlayer();

        Debug.Log("✅ GameBootstrap: Scene built! Use ← → to move, SPACE to jump.");
    }

    // ---- Sprite helper: makes a solid-colour square sprite in code ----
    static Sprite MakeSquareSprite(Color color)
    {
        var tex = new Texture2D(4, 4);
        var pixels = new Color[16];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = color;
        tex.SetPixels(pixels);
        tex.filterMode = FilterMode.Point;
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4);
    }

    static void CleanupOldObjects()
    {
        // Find everything once, then destroy — Destroy() is deferred, so we must
        // NOT re-Find in a loop (that would hang). Collect first, delete after.
        var all = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (var go in all)
        {
            string n = go.name;
            if (n == "Player" || n == "Ground" || n == "_Setup" ||
                n.StartsWith("Tile_") || n.StartsWith("Label_"))
            {
                Object.Destroy(go);
            }
        }
    }

    static void CreateCamera()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            var camObj = new GameObject("Main Camera");
            cam = camObj.AddComponent<Camera>();
            camObj.tag = "MainCamera";
        }
        cam.transform.position = new Vector3(0, 0, -10);
        cam.orthographic = true;
        cam.orthographicSize = 6f;
        cam.backgroundColor = new Color(0.53f, 0.81f, 0.92f); // sky blue
        Debug.Log("✓ Camera ready (orthographic, sky-blue)");
    }

    static void CreateGround()
    {
        var ground = new GameObject("Ground");
        ground.tag = "Ground";
        ground.transform.position = new Vector3(0, -3.5f, 0);
        ground.transform.localScale = new Vector3(30, 1, 1);

        var sr = ground.AddComponent<SpriteRenderer>();
        sr.sprite = MakeSquareSprite(new Color(0.35f, 0.25f, 0.15f)); // brown earth
        sr.sortingOrder = 0;

        ground.AddComponent<BoxCollider2D>();
        Debug.Log("✓ Ground created");
    }

    static void CreateTiles()
    {
        // Colours for the 5 test tiles
        Color[] colors =
        {
            new Color(0.30f, 0.69f, 0.31f), // green   - normal
            new Color(0.30f, 0.69f, 0.31f), // green   - normal
            new Color(0.61f, 0.35f, 0.71f), // purple  - portal
            new Color(0.10f, 0.55f, 0.72f), // blue    - whirlpool
            new Color(1.00f, 0.76f, 0.03f), // gold    - goal
        };

        for (int i = 1; i <= 5; i++)
        {
            float x = -6f + (i - 1) * 3f; // -6, -3, 0, 3, 6

            var tile = new GameObject($"Tile_{i}");
            tile.tag = "Ground"; // player can stand on tiles
            tile.transform.position = new Vector3(x, -2f, 0);
            tile.transform.localScale = new Vector3(2f, 0.6f, 1);

            var sr = tile.AddComponent<SpriteRenderer>();
            sr.sprite = MakeSquareSprite(colors[i - 1]);
            sr.sortingOrder = 1;

            tile.AddComponent<BoxCollider2D>();

            // Number label above the tile
            var label = new GameObject($"Label_{i}");
            label.transform.position = new Vector3(x, -1.2f, -1);
            var tm = label.AddComponent<TextMesh>();
            tm.text = i.ToString();
            tm.characterSize = 0.2f;
            tm.fontSize = 60;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.color = Color.white;
        }
        Debug.Log("✓ 5 tiles created (green, green, purple, blue, gold)");
    }

    static void CreatePlayer()
    {
        var player = new GameObject("Player");
        player.tag = "Player";
        player.transform.position = new Vector3(-6f, 1f, 0);
        player.transform.localScale = new Vector3(0.8f, 0.8f, 1);

        var sr = player.AddComponent<SpriteRenderer>();
        sr.sprite = MakeSquareSprite(new Color(1f, 0.85f, 0.2f)); // Dino yellow
        sr.sortingOrder = 2;

        var rb = player.AddComponent<Rigidbody2D>();
        rb.gravityScale = 2f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        player.AddComponent<BoxCollider2D>();
        player.AddComponent<PlayerController>();

        Debug.Log("✓ Player created (yellow square) at tile 1");
    }
}
