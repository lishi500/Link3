using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VerticalLinkSkill : BoardSkill
{
    public override IEnumerator Cast() {
        int x = startTile.xIndex;
        List<Tile> tiles = new List<Tile>();

        for (int i = 0; i < board.height; i++) {
            Tile tile = board.GetTile(x, i);
            if (tile != null && tile.piece != null) {
                tiles.Add(tile);
            }
        }

        yield return board.MatchTilePath(tiles);
    }
}
