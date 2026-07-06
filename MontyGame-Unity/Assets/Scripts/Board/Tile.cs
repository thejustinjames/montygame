using UnityEngine;
using TMPro;

/// <summary>
/// Represents a single tile on the board.
/// </summary>
public class Tile : MonoBehaviour
{
    [SerializeField] private int tileId;
    [SerializeField] private string tileName = "Tile";
    private TextMeshPro tileLabel;

    void Start()
    {
        tileLabel = GetComponentInChildren<TextMeshPro>();
        if (tileLabel != null)
        {
            tileLabel.text = $"{tileId}";
        }
    }

    public int GetTileId() => tileId;

    public string GetTileName() => tileName;

    public void SetTileInfo(int id, string name)
    {
        tileId = id;
        tileName = name;
        if (tileLabel != null)
        {
            tileLabel.text = $"{tileId}";
        }
    }

    public Vector3 GetCenterPosition() => transform.position;
}
