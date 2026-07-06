using UnityEngine;

/// <summary>
/// Builds the ENTIRE 10x10 Snakes-&-Ladders board in code when you press Play.
/// No manual Unity editor setup needed. Runs automatically via
/// [RuntimeInitializeOnLoadMethod] (no GameObject required).
///
/// Builds: top-down camera, 100 numbered squares, ladder/snake connector
/// lines, collectible stars, the Dino token, and the game manager.
/// </summary>
public static class GameBootstrap
{
    static Material lineMat;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void BuildScene()
    {
        Debug.Log("🔧 GameBootstrap: Building 10x10 board...");

        CleanupOldObjects();
        CreateCamera();
        CreateBackground();
        CreateSquares();
        CreateConnectors();
        CreateMarkers();
        CreateCollectibles();
        CreateTokens();
        CreateGameManager();

        Debug.Log("✅ Board built! Press ROLL to climb to 100 and beat the boss.");
    }

    // ---- helpers ----
    static Sprite MakeSquareSprite(Color color)
    {
        var tex = new Texture2D(4, 4);
        var px = new Color[16];
        for (int i = 0; i < px.Length; i++) px[i] = color;
        tex.SetPixels(px);
        tex.filterMode = FilterMode.Point;
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4);
    }

    static void CleanupOldObjects()
    {
        var all = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (var go in all)
        {
            string n = go.name;
            if (n == "Player" || n == "Ground" || n == "_Setup" || n == "_GameManager" ||
                n == "Background" || n.StartsWith("Token") || n.StartsWith("Tile_") ||
                n.StartsWith("Label_") || n.StartsWith("Sq_") || n.StartsWith("Link_") ||
                n.StartsWith("Star_") || n.StartsWith("Marker_") || n == "Carrier")
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
        cam.orthographicSize = 6.2f; // fits the 10-unit board + margin
        cam.backgroundColor = new Color(0.10f, 0.13f, 0.20f); // deep space
        Debug.Log("✓ Camera framed on board");
    }

    static void CreateBackground()
    {
        var tex = Resources.Load<Texture2D>("backdrop");
        if (tex == null) { Debug.LogWarning("⚠ backdrop not found in Resources"); return; }

        var bg = new GameObject("Background");
        // Shifted DOWN + scaled up so the watermarked lower strip of the image
        // sits below the visible frame — only clean galaxy (top) + jungle canopy
        // (bottom) remain on screen.
        bg.transform.position = new Vector3(0, -5f, 20);
        var sr = bg.AddComponent<SpriteRenderer>();
        sr.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
                                  new Vector2(0.5f, 0.5f), 100);
        sr.sortingOrder = -100; // behind board, tokens, everything

        // scale to COVER the whole play area (board + zoom margin), no distortion
        const float need = 32f;
        Vector3 s = sr.sprite.bounds.size;
        float scale = Mathf.Max(need / s.x, need / s.y);
        bg.transform.localScale = new Vector3(scale, scale, 1);
        Debug.Log("✓ Jungle background placed");
    }

    static Sprite tileCosmic, tileJungle;

    static Sprite LoadTileSprite(string name)
    {
        var tex = Resources.Load<Texture2D>(name);
        if (tex == null) return null;
        // pixelsPerUnit = width -> the whole tile (body + shadow padding) is 1 cell,
        // so the visible body is ~0.75 cell and neighbours have gaps = they "float"
        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
                             new Vector2(0.5f, 0.5f), tex.width);
    }

    static void CreateSquares()
    {
        tileCosmic = LoadTileSprite("tile_cosmic");
        tileJungle = LoadTileSprite("tile_jungle");

        for (int n = 1; n <= BoardLayout.Squares; n++)
        {
            Vector3 pos = BoardLayout.SquareToWorld(n);
            int row = (n - 1) / BoardLayout.Grid;
            int col = (n - 1) % BoardLayout.Grid;
            bool cosmic = (row + col) % 2 == 0;

            var sq = new GameObject($"Sq_{n}");
            sq.transform.position = pos;

            var sr = sq.AddComponent<SpriteRenderer>();
            Sprite s = cosmic ? tileCosmic : tileJungle;
            sr.sprite = s != null ? s : MakeSquareSprite(cosmic ? new Color(0.2f, 0.16f, 0.4f) : new Color(0.2f, 0.4f, 0.2f));
            sr.color = SpecialTint(n);   // tints ladder/snake/boss squares
            sr.sortingOrder = 0;

            // number label
            var label = new GameObject($"Label_{n}");
            label.transform.position = pos + new Vector3(-0.30f, 0.28f, -1);
            var tm = label.AddComponent<TextMesh>();
            tm.text = n.ToString();
            tm.characterSize = 0.07f;
            tm.fontSize = 48;
            tm.color = new Color(1f, 1f, 1f, 0.85f);
        }
        Debug.Log("✓ 100 floating cosmic/jungle tiles created");
    }

    // Multiplicative tint to flag special squares (white = normal tile)
    static Color SpecialTint(int n)
    {
        if (n == BoardLayout.Boss) return new Color(1f, 0.45f, 0.45f);            // boss red
        if (BoardLayout.Ladders.ContainsKey(n)) return new Color(0.75f, 1f, 0.5f); // ladder yellow-green
        if (BoardLayout.Snakes.ContainsKey(n)) return new Color(1f, 0.6f, 1f);     // snake pink
        return Color.white;
    }

    static Material LineMaterial()
    {
        if (lineMat == null)
        {
            var shader = Shader.Find("Sprites/Default") ?? Shader.Find("Unlit/Color");
            lineMat = new Material(shader);
        }
        return lineMat;
    }

    static void CreateConnectors()
    {
        // Ladders: NO lines — the spaceship/Hulk badge + fly-up animation shows the path.
        // Snakes: an imaginative animated trail — swirling vortex, or a jungle vine (T-Rex).
        foreach (var kv in BoardLayout.Snakes)
        {
            bool vine = BoardLayout.TrexSnakes.Contains(kv.Key);
            CreateWavyLink($"Link_S{kv.Key}", kv.Key, kv.Value, vine);
        }
        Debug.Log("✓ Snake trails drawn (vortex swirls + jungle vine)");
    }

    static void CreateWavyLink(string name, int from, int to, bool vine)
    {
        var obj = new GameObject(name);
        var lr = obj.AddComponent<LineRenderer>();
        lr.material = LineMaterial();
        lr.sortingOrder = 1;

        var link = obj.AddComponent<WavyLink>();
        link.a = BoardLayout.SquareToWorld(from);
        link.b = BoardLayout.SquareToWorld(to);
        if (vine) // jungle vine for the T-Rex
        {
            link.color = new Color(0.35f, 0.72f, 0.30f);
            link.width = 0.17f; link.amplitude = 0.30f; link.waves = 2.5f; link.speed = 0.8f;
        }
        else // meandering, faster-swirling vortex trail
        {
            link.color = new Color(0.80f, 0.45f, 0.95f);
            link.width = 0.12f; link.amplitude = 0.32f; link.waves = 4f; link.speed = 2.4f;
        }
    }

    static void CreateMarkers()
    {
        foreach (var b in BoardLayout.Ladders.Keys)
            SpawnMarker($"Marker_L{b}", BoardLayout.LadderMarker(b), b);
        foreach (var h in BoardLayout.Snakes.Keys)
            SpawnMarker($"Marker_S{h}", BoardLayout.SnakeMarker(h), h);
        Debug.Log("✓ Ladder/snake markers placed (ships, Hulk, vortexes, T-Rex)");
    }

    static void SpawnMarker(string name, string texName, int square)
    {
        var tex = Resources.Load<Texture2D>(texName);
        if (tex == null) return;
        var go = new GameObject(name);
        go.transform.position = BoardLayout.SquareToWorld(square) + new Vector3(0, 0, -1.5f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
                                  new Vector2(0.5f, 0.5f), tex.width);
        sr.sortingOrder = 3; // above tiles + lines, below tokens
        float target = BoardLayout.Cell * 0.72f;
        float w = sr.sprite.bounds.size.x;
        if (w > 0.001f) go.transform.localScale = Vector3.one * (target / w);

        // the vortex swirl spins so it reads as a live, moving vortex
        if (texName == "snake_vortex") go.AddComponent<Spinner>().speed = 80f;
    }

    static void CreateCollectibles()
    {
        foreach (int n in BoardLayout.Collectibles)
        {
            var star = new GameObject($"Star_{n}");
            star.transform.position = BoardLayout.SquareToWorld(n) + new Vector3(0.25f, -0.25f, -1);
            star.transform.localScale = new Vector3(0.3f, 0.3f, 1);
            var sr = star.AddComponent<SpriteRenderer>();
            sr.sprite = MakeSquareSprite(new Color(1f, 0.85f, 0.1f)); // gold star (square placeholder)
            sr.sortingOrder = 2;
        }
        Debug.Log("✓ Collectibles placed");
    }

    static void CreateTokens()
    {
        // Two players share the board — offset within a square so both are visible
        CreateToken("Token_0", "Dino", new Vector3(-0.20f, 0.12f, -2.0f), new Color(1f, 0.85f, 0.2f));
        CreateToken("Token_1", "Cat", new Vector3(0.20f, -0.12f, -2.1f), new Color(0.3f, 0.85f, 0.9f));
        Debug.Log("✓ Dino + Cat tokens on square 1");
    }

    static void CreateToken(string objName, string spriteName, Vector3 offset, Color fallback)
    {
        var token = new GameObject(objName);
        // start just off the board, below square 1
        token.transform.position = BoardLayout.SquareToWorld(1) + new Vector3(0f, -1.25f, 0f) + offset;

        var sr = token.AddComponent<SpriteRenderer>();
        sr.sprite = LoadCharacterSprite(spriteName);
        sr.sortingOrder = 5;

        float target = BoardLayout.Cell * 0.55f; // smaller so two fit in a square
        if (sr.sprite != null)
        {
            float w = sr.sprite.bounds.size.x;
            if (w > 0.001f) token.transform.localScale = Vector3.one * (target / w);
        }
        else
        {
            sr.sprite = MakeSquareSprite(fallback);
            token.transform.localScale = Vector3.one * target;
        }
    }

    static Sprite LoadCharacterSprite(string name)
    {
        // Load the kid's drawing (PNG in Resources) and make a sprite in code,
        // so it works whether the PNG imported as Texture or Sprite.
        var tex = Resources.Load<Texture2D>(name);
        if (tex == null) return null;
        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
                             new Vector2(0.5f, 0.5f), tex.width);
    }

    static void CreateGameManager()
    {
        var gm = new GameObject("_GameManager");
        gm.AddComponent<GameController>();
        Debug.Log("✓ GameManager ready (ROLL to play)");
    }
}
