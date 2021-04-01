using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BoardSkill : MonoBehaviour
{
    public Tile startTile;
    public DirectionType direction;
    public Board board;
    public BoardSkill Init(Tile tile, DirectionType direction = DirectionType.CENTER) {
        this.startTile = tile;
        this.direction = direction;

        return this;
    }

    public abstract IEnumerator Cast();

    public void Awake() {
       board = GameManager.Instance.GetGameBoard();
    }
}
