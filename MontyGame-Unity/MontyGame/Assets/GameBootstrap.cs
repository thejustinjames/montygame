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
        CreateSquares();
        CreateConnectors();
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
                n.StartsWith("Token") || n.StartsWith("Tile_") || n.StartsWith("Label_") ||
                n.StartsWith("Sq_") || n.StartsWith("Link_") || n.StartsWith("Star_"))
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

    static void CreateSquares()
    {
        for (int n = 1; n <= BoardLayout.Squares; n++)
        {
            Vector3 pos = BoardLayout.SquareToWorld(n);

            var sq = new GameObject($"Sq_{n}");
            sq.transform.position = pos;
            sq.transform.localScale = new Vector3(BoardLayout.Cell * 0.94f, BoardLayout.Cell * 0.94f, 1);

            var sr = sq.AddComponent<SpriteRenderer>();
            sr.sprite = MakeSquareSprite(SquareColor(n));
            sr.sortingOrder = 0;

            // number label
            var label = new GameObject($"Label_{n}");
            label.transform.position = pos + new Vector3(-0.34f, 0.30f, -1);
            var tm = label.AddComponent<TextMesh>();
            tm.text = n.ToString();
            tm.characterSize = 0.07f;
            tm.fontSize = 48;
            tm.color = new Color(1f, 1f, 1f, 0.75f);
        }
        Debug.Log("✓ 100 squares created");
    }

    static Color SquareColor(int n)
    {
        if (n == BoardLayout.Boss) return new Color(0.80f, 0.15f, 0.15f);          // boss red
        if (BoardLayout.Ladders.ContainsKey(n)) return new Color(0.20f, 0.55f, 0.30f); // ladder base green
        if (BoardLayout.Snakes.ContainsKey(n)) return new Color(0.55f, 0.25f, 0.55f);  // snake head purple
        // checkerboard of two calm blues
        int row = (n - 1) / BoardLayout.Grid;
        int col = (n - 1) % BoardLayout.Grid;
        bool even = (row + col) % 2 == 0;
        return even ? new Color(0.17f, 0.24f, 0.34f) : new Color(0.22f, 0.30f, 0.42f);
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
        foreach (var kv in BoardLayout.Ladders)
            DrawLink($"Link_L{kv.Key}", kv.Key, kv.Value, new Color(0.35f, 0.85f, 0.45f)); // green up
        foreach (var kv in BoardLayout.Snakes)
            DrawLink($"Link_S{kv.Key}", kv.Key, kv.Value, new Color(0.85f, 0.45f, 0.85f)); // purple down
        Debug.Log("✓ Ladder/snake links drawn");
    }

    static void DrawLink(string name, int from, int to, Color color)
    {
        var obj = new GameObject(name);
        var lr = obj.AddComponent<LineRenderer>();
        lr.material = LineMaterial();
        lr.startColor = lr.endColor = color;
        lr.startWidth = lr.endWidth = 0.12f;
        lr.numCapVertices = 4;
        lr.sortingOrder = 1;
        lr.positionCount = 2;
        lr.SetPosition(0, BoardLayout.SquareToWorld(from) + new Vector3(0, 0, -0.5f));
        lr.SetPosition(1, BoardLayout.SquareToWorld(to) + new Vector3(0, 0, -0.5f));
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
        token.transform.position = BoardLayout.SquareToWorld(1) + offset;

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
