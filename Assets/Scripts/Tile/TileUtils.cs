using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TileUtils : Singleton<TileUtils> {
    public bool IsNeighbourTile(Tile a, Tile b) {
        return Mathf.Abs(a.xIndex - b.xIndex) <= 1 && Mathf.Abs(a.yIndex - b.yIndex) <= 1 && a != b;
    }

    public bool IsSameType(Tile a, Tile b) {
        return a != null && b != null && a.piece != null && b.piece != null && a.piece.type == b.piece.type;
    }

    public List<GamePiece> GetPiecesByTileList(List<Tile> tiles) {
        List<GamePiece> gamePieces = new List<GamePiece>();
        foreach (Tile tile in tiles) {
            gamePieces.Add(tile.piece);
        }

        return gamePieces;
    }

    public List<Tile> GetAllNeighourTileWithSameType(Tile tile) {
        List<Tile> neighbours = GetSurroundingTiles(tile);

        return neighbours.FindAll(t => IsSameType(tile, t));
    }

    public List<Tile> GetSurroundingTiles(Tile tile) {
        List<Tile> neighours = new List<Tile>();
        Board board = GameManager.Instance.GetGameBoard();
        int x = tile.xIndex;
        int y = tile.yIndex;

        neighours.Add(board.GetTile(x, y + 1)); // UP
        neighours.Add(board.GetTile(x, y - 1)); // DOWN
        neighours.Add(board.GetTile(x - 1, y)); // LEFT
        neighours.Add(board.GetTile(x + 1, y)); // RIGHT
        neighours.Add(board.GetTile(x - 1, y + 1)); // UP_LEFT
        neighours.Add(board.GetTile(x + 1, y + 1)); // UP_RIGHT
        neighours.Add(board.GetTile(x - 1, y - 1)); // DOWN_LEFT
        neighours.Add(board.GetTile(x + 1, y - 1)); // DOWN_RIGHT

        return neighours.FindAll(t => t != null);
    }

    public DirectionType GetTailDirection(List<Tile> tiles) {
        if (tiles == null || tiles.Count == 1) {
            return DirectionType.CENTER;
        }
        Tile lastTile = tiles[tiles.Count - 1];
        Tile secLast = tiles[tiles.Count - 2];
        int secX = secLast.xIndex;
        int secY = secLast.yIndex;
        int lastX = lastTile.xIndex;
        int lastY = lastTile.yIndex;

        if (secX == lastX && lastY - secY == 1) {
            return DirectionType.UP;
        } else if (secX == lastX && secY - lastY == 1) {
            return DirectionType.DOWN;
        } else if (secY == lastY && lastX - secX == 1) {
            return DirectionType.LEFT;
        } else if (secY == lastY && secX - lastX == 1) {
            return DirectionType.RIGHT;
        } else if (lastY - secY == 1 && lastX - secX == 1) {
            return DirectionType.UP_LEFT;
        } else if (lastY - secY == 1 && secX - lastX == 1) {
            return DirectionType.UP_RIGHT;
        } else if (secY - lastY == 1 && lastX - secX == 1) {
            return DirectionType.DOWN_LEFT;
        } else if (secY - lastY == 1 && secX - lastX == 1) {
            return DirectionType.DOWN_RIGHT;
        } 

        return DirectionType.CENTER;
    }

    

    private void HighLightTiles(List<Tile> tiles) {
        tiles.ForEach(t => t.OnTileSelected());
    }

    IEnumerator SlowHighLight(List<Tile> tiles) {
        foreach (Tile tile in tiles) {
            yield return new WaitForSeconds(0.3f);
            tile.OnTileSelected();
        };
    }

    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    
}


