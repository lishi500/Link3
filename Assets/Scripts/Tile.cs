using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public int xIndex;
    public int yIndex;
    public bool visited = false;
    private Color m_originColor;
    private Color m_highLightColor = Color.yellow;
    Board m_board;

    public GamePiece piece;

    public void OnTileSelected()
    {
        GetComponent<SpriteRenderer>().color = m_highLightColor;
    }

    public void OnTileUnselected() {
        GetComponent<SpriteRenderer>().color = m_originColor;
    }

    private void OnMouseDown()
    {
        //m_board.ClickTile(this);
    }

    private void OnMouseUp()
    {
        //m_board.ReleaseTile();
    }

    private void OnMouseUpAsButton()
    {
        //m_board.ClearPieceByTile(this);
       List<Tile> bestPath = TileUtils.Instance.FindLongestMatchPath(this);
        StartCoroutine(m_board.MatchTilePath(bestPath));
    }

    // Start is called before the first frame update
    void Start()
    {
        m_originColor = GetComponent<SpriteRenderer>().color;
    }

    public void Init(int x, int y, Board board)
    {
        xIndex = x;
        yIndex = y;
        m_board = board;
    }

    private void LateUpdate()
    {
        
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
