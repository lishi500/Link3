using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Board : MonoBehaviour
{
    public int width;
    public int height;
    public int widthBorder = 1;


    public int heightBorder = 1;
    public const float MOVE_TIME = 0.3f;


    public GameObject tilePrefab;
    public PieceType[] gamePieceTypes;

    // -------------------- Private members --------------

    Tile[,] m_allTiles;
    GamePiece[,] m_allGamePieces;

    List<Tile> m_selectedTile;
    bool m_isSelecting = false;
    bool m_isReleaseing = false;

    // first tile to Select
    public void ClickTile(Tile tile)
    {
        if (!m_isSelecting && !m_isReleaseing) {
            m_isSelecting = true;
            m_selectedTile = new List<Tile>();
            AddTileToSelected(tile);
        }
    }

    public void DragIntoTile(Tile tile) {
        if (m_isSelecting
            && !m_selectedTile.Contains(tile)
            && TileUtils.Instance.IsNeighbourTile(tile, m_selectedTile[m_selectedTile.Count - 1])
            && TileUtils.Instance.IsSameType(tile, m_selectedTile[0])
            ) {
            AddTileToSelected(tile);
        } else if ( // when roll back to prev tile
            m_isSelecting
            && m_selectedTile.Contains(tile)
            && m_selectedTile.Count > 1
            && m_selectedTile[m_selectedTile.Count - 2] == tile
            ) {
            Tile lastTile = m_selectedTile.LastOrDefault();
            RemoveTileSelected(lastTile);
        }
    }

    public void ReleaseTile() {
        StartCoroutine(ReleaseTileCoroutine());
    }

    IEnumerator ReleaseTileCoroutine() {
        m_isReleaseing = true;
        if (m_isSelecting)
        {
            if (m_selectedTile.Count > 2)
            {

                CalculateScore();
                List<GamePiece> gamePieces = TileUtils.Instance.GetPiecesByTileList(m_selectedTile);
                yield return StartCoroutine(ClearPieceList(gamePieces));
                UnselectList();
            }
            else
            {
                UnselectList();
            }
            m_isSelecting = false;
        }
        m_isReleaseing = false;
    }

    IEnumerator ClearPieceList(List<GamePiece> gamePieces)
    {
        //float waitTime = gamePieces.Count <= 6 ? 0.07f : gamePieces.Count <= 10 ? 0.05f : 0.035f;
        foreach (GamePiece piece in gamePieces) {
            ClearGamePiece(piece);
            yield return new WaitForSeconds(0.2f);
        }
        CollapsePieces();
    }

    void CalculateScore()
    {
        int sum = 0;
        for (int i = 1; i < m_selectedTile.Count + 1; i++) {
            sum += i;
        }
        GameController.Instance.AddScore(sum);

    }

    public void CollapsePieces()
    {
        for (int i = 0; i < width; i++)
        {
            int movingDownStep = 0;
            for (int j = 0; j < height; j++)
            {
                Tile tile = m_allTiles[i, j];
                GamePiece piece = tile.piece;
                if (piece == null)
                {
                    movingDownStep++;
                }
                else if (movingDownStep > 0) {
                    piece.MoveDown(movingDownStep);
                }
            }
            if (movingDownStep > 0) {
                //Debug.Log("Fill " + movingDownStep);
                for (int k = height - movingDownStep; k < height; k++) {
                    //Debug.Log("(" + i + "," + k+") offSet:" + movingDownStep );
                    FillOnePiece(i, k, gamePieceTypes[Random.Range(0, gamePieceTypes.Length)], movingDownStep);
                }
            }
        }
    }

    void LogSelectedList() {
        string logInfo = "";
        foreach (Tile tile in m_selectedTile) {
            logInfo += tile.xIndex + "," + tile.yIndex + "->";
        }
        Debug.Log(m_selectedTile.Count + " Selected: " + logInfo);
    }

    void AddTileToSelected(Tile tile) {
        if (!m_selectedTile.Contains(tile)) {
            m_selectedTile.Add(tile);
            tile.OnTileSelected();
        }
        //LogSelectedList();
    }

    void UnselectList() {
        foreach (Tile tile in m_selectedTile)
        {
            tile.OnTileUnselected();
        }
        m_selectedTile.Clear();
    }
    void RemoveTileSelected(Tile tile) {
        if (m_selectedTile.Contains(tile)) {
            tile.OnTileUnselected();
            m_selectedTile.Remove(tile);
        }
        //LogSelectedList();
    }

    void SetupTiles() {
        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++) {
                GameObject tileObj = Instantiate(tilePrefab, new Vector3(i, j, 0), Quaternion.identity) as GameObject;
                Tile tile = tileObj.GetComponent<Tile>();
                tile.Init(i ,j, this);
                tileObj.name = "Tiles (" + i + "," + j + ")";
                m_allTiles[i, j] = tile;
                tileObj.transform.parent = transform;
            }
        }
    }

    public void PlaceGamePiece(GamePiece gamePiece, int x, int y, int yOffSet = 0) {
        if (gamePiece != null) {
            gamePiece.transform.position = new Vector3(x, y + yOffSet, 0);
            gamePiece.transform.rotation = Quaternion.identity;
            gamePiece.SetCoord(x, y, yOffSet);
            if (IsWithinBounds(x, y)) { 
                m_allGamePieces[x, y] = gamePiece;
                m_allTiles[x, y].piece = gamePiece;
                if (yOffSet > 0) {
                    // clear offset
                    gamePiece.MoveDown(0);
                }
            }
        }
    }

    public void UnsetTile(int x, int y) {
        m_allTiles[x, y].piece = null;
    }

    public Tile[,] GetAllTiles() {
        return m_allTiles;
    }

    void ClearGamePiece(GamePiece piece) {
        if (piece != null)
        {
            m_allGamePieces[piece.xIndex, piece.yIndex] = null;
            m_allTiles[piece.xIndex, piece.yIndex].piece = null;
            StartCoroutine(piece.DestroyPiece());
        }
    }

    bool IsWithinBounds(int x, int y) {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    void FillOnePiece(int x, int y, PieceType type, int yOffset = 0) {
        GamePiece gamePiece = PieceUtil.Instance.GenerateGamePieceByType(type);
        gamePiece.Init(this);
        gamePiece.gameObject.transform.parent = transform;
        PlaceGamePiece(gamePiece, x, y, yOffset);
    }
    void FillAllRandomPieces() {
        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++) {
                PieceType pieceType = gamePieceTypes[Random.Range(0, gamePieceTypes.Length)];
                FillOnePiece(i, j, pieceType);
            }
        }
    }

    void InitialCamera() {
        Vector3 cameraPos = new Vector3((float)(width -1) / 2f, (float)(height -1) / 2f, -10f);
        float aspectRatio = (float)Screen.width / (float)Screen.height;
        float orthographicSize = OrthgraphicSizeHelper.Instance.autoFit(width, height, widthBorder, heightBorder, aspectRatio);
        Camera.main.transform.position = cameraPos;
        Camera.main.orthographicSize = orthographicSize;
    }
    // Start is called before the first frame update
    void Start()
    {
        InitialCamera();
        m_allTiles = new Tile[width, height];
        m_allGamePieces = new GamePiece[width, height];
        SetupTiles();
        FillAllRandomPieces();
    }

   
}
