using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HorizontalLinkSkill : BoardSkill {
    public override IEnumerator Cast() {
        int y = startTile.yIndex;
        List<Tile> tiles = new List<Tile>();

        for (int i = 0; i < board.width; i++) {
            Tile tile = board.GetTile(i, y);
            if (tile != null && tile.piece != null) {
                tiles.Add(tile);
            }
        }

        yield return board.MatchTilePath(tiles);
    }
}
