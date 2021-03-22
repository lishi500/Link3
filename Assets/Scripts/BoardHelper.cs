using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardHelper : MonoBehaviour
{
    Board m_board;
    Tile[,] m_allTiles;

    public void ChangePieceType(int x, int y, PieceType type) {
        GamePiece piece = m_allTiles[x, y].piece;
        if (piece != null) {
            piece.type = type;
            Color color = PieceUtil.Instance.GetTypeColor(type);
            piece.ChangeColor(color);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        m_board = GetComponent<Board>();
        m_allTiles = m_board.GetAllTiles();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
