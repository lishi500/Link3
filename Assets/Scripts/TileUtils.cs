using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileUtils : Singleton<TileUtils>
{
    public bool IsNeighbourTile(Tile a, Tile b) {
        return Mathf.Abs(a.xIndex - b.xIndex) <= 1 && Mathf.Abs(a.yIndex - b.yIndex) <= 1;
    }

    public bool IsSameType(Tile a, Tile b)
    {
        return a != null && b!= null && a.piece != null && b.piece != null && a.piece.type == b.piece.type;
    }

    public List<GamePiece> GetPiecesByTileList(List<Tile> tiles) {
        List<GamePiece> gamePieces = new List<GamePiece>();
        foreach (Tile tile in tiles) {
            gamePieces.Add(tile.piece);
        }

        return gamePieces;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
