using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages the board layout and tile rendering.
/// For Sprint 1: creates a simple 5-tile test board.
/// </summary>
public class BoardManager : MonoBehaviour
{
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private Transform boardParent;
    [SerializeField] private float tileSpacing = 3f;
    [SerializeField] private int numTiles = 5;

    private List<Tile> tiles = new List<Tile>();

    void Start()
    {
        CreateTestBoard();
    }

    public void CreateTestBoard()
    {
        if (tilePrefab == null)
        {
            Debug.LogError("Tile prefab not assigned!");
            return;
        }

        // Clear any existing tiles
        foreach (Transform child in boardParent)
        {
            Destroy(child.gameObject);
        }
        tiles.Clear();

        // Create tiles in a horizontal line
        for (int i = 1; i <= numTiles; i++)
        {
            Vector3 tilePosition = new Vector3(i * tileSpacing, 0, 0);
            GameObject tileObject = Instantiate(tilePrefab, tilePosition, Quaternion.identity, boardParent);
            tileObject.name = $"Tile_{i}";

            Tile tileScript = tileObject.GetComponent<Tile>();
            if (tileScript != null)
            {
                tileScript.SetTileInfo(i, $"Test Tile {i}");
                tiles.Add(tileScript);
            }

            // Tag as ground for player collision
            tileObject.tag = "Ground";
        }

        Debug.Log($"Created {numTiles}-tile test board");
    }

    public Tile GetTile(int tileId)
    {
        if (tileId >= 1 && tileId <= tiles.Count)
        {
            return tiles[tileId - 1];
        }
        return null;
    }

    public Vector3 GetTilePosition(int tileId)
    {
        Tile tile = GetTile(tileId);
        if (tile != null)
        {
            return tile.GetCenterPosition();
        }
        Debug.LogWarning($"Tile {tileId} not found!");
        return Vector3.zero;
    }

    public int GetNumTiles() => numTiles;
}
