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
    public const float DIAGONAL_MOVE_TIME = 0.15f;

    public GameObject tilePrefab;
    public PieceType[] gamePieceTypes;

    // -------------------- public method members --------------
    public bool hasMasks {
        get { return m_liveMasks != null && m_liveMasks.Count > 0; }
    }

    // -------------------- Private members --------------
    Tile[,] m_allTiles;
    GamePiece[,] m_allGamePieces;

    List<Tile> m_selectedTile;
    public List<TileMask> m_liveMasks;
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

    public IEnumerator MatchTilePath(List<Tile> tilePath) {
        CalculateScore(tilePath);
        List<GamePiece> gamePieces = TileUtils.Instance.GetPiecesByTileList(tilePath);
        yield return StartCoroutine(ClearPieceList(gamePieces));
        UnselectList(tilePath);
    }

    IEnumerator ReleaseTileCoroutine() {
        m_isReleaseing = true;
        if (m_isSelecting)
        {
            if (m_selectedTile.Count > 2)
            {
                yield return StartCoroutine(MatchTilePath(m_selectedTile));
                m_selectedTile.Clear();
            }
            else
            {
                UnselectList(m_selectedTile);
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
        StartCoroutine(CollapseAndFillPieces());
    }

    void CalculateScore(List<Tile> tiles) {
        int sum = 0;
        for (int i = 1; i < tiles.Count + 1; i++) {
            sum += i;
        }
        GameManager.Instance.AddScore(sum);
    }

    public IEnumerator CollapseAndFillPieces() {
        bool completed = false;
        while (!completed) {
            StraightCollapsePieces();
            yield return new WaitForSeconds(MOVE_TIME);
            DiagonalCollapsePieces();
            yield return new WaitForSeconds(DIAGONAL_MOVE_TIME + 0.05f);

            completed = IsCollapseCompleted();
            if (!completed) {
                Debug.Log("not completed");
            }
        }
        

    }

    void StraightCollapsePieces()
    {
        for (int i = 0; i < width; i++)
        {
            CollapseColumn(i);
        }
    }

    void DiagonalCollapsePieces() {
        for (int i = 0; i < width; i++) {
            CollapseEmptyTileBelowMaskInColumn(i);
        }
    }

    void CollapseEmptyTileBelowMaskInColumn(int x) {
        for (int i = 0; i < height; i++) {
            Tile nextTile = GetTile(x, i);
            if (nextTile.piece == null && !nextTile.isReserved && IsColumnRangeHasMask(x, i + 1, height)) {
                Tile leftTopTile = GetTile(x - 1, i + 1);
                Tile rightTopTile = GetTile(x + 1, i + 1);
                if (leftTopTile.CanLink() && !leftTopTile.isReserved) {
                    BorrowPiecesDiagonally(nextTile, leftTopTile);
                } else if (rightTopTile.CanLink() && !rightTopTile.isReserved) { 
                    BorrowPiecesDiagonally(nextTile, rightTopTile);
                }
            }
        }
    }

    void BorrowPiecesDiagonally(Tile original, Tile lender) {
        if (original.piece == null && lender.CanLink()) {
            lender.piece.Move(original.xIndex, original.yIndex, DIAGONAL_MOVE_TIME);
            ChainMoveDown(lender.xIndex, lender.yIndex);
        }
    }

    void ChainMoveDown(int startX, int startY) {
        for (int i = startY; i < height - 1; i++) {
            Tile currentTile = GetTile(startX, i);
            if (currentTile.piece == null && !currentTile.isReserved) {
                Tile nextTile = GetTile(startX, i + 1);
                if (nextTile.CanLink()) {
                    nextTile.piece.MoveDown(1);
                }
            }

            if (i + 2 == height) { 
                Tile ceillingTile = GetTile(startX, height - 1);
                if (ceillingTile.piece == null) {
                    FillOnePiece(startX, height - 1, gamePieceTypes[Random.Range(0, gamePieceTypes.Length)], 1);
                }
            }
        }
    }

    private bool IsCollapseCompleted() {
        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++) {
                if (GetTile(i, j).piece == null) {
                    // check top tile
                    if (GetTile(i, j + 1) != null && GetTile(i, j + 1).CanLink()) {
                        return false;
                    }

                    if (IsColumnRangeHasMask(i, j + 1, height)) {
                        // check top left tile
                        if (GetTile(i - 1, j + 1) != null && GetTile(i - 1, j + 1).CanLink()) {
                            return false;
                        }
                        // check top right tile
                        if (GetTile(i + 1, j + 1) != null && GetTile(i + 1, j + 1).CanLink()) {
                            return false;
                        }
                    }
                }
            }
        }

        return true;
    }
    private void CollapseColumn(int x) {
        if (IsColumnHasMask(x)) {
            CollapseColumnWithMask(x);
        } else {
            CollapseColumnWithRange(x, 0, height, true);
        }
    }

    private void CollapseColumnWithMask(int x) {
        int start = 0;
        int end = 0;

        while (end < height) {
            if (GetTile(x, start).HasMask()) {
                start += 1;
                end = start;
            } else {
                end += 1;
                if (end == height) {
                    CollapseColumnWithRange(x, start, end, true);
                    break;
                }
                if (GetTile(x, end).HasMask()) { 
                    CollapseColumnWithRange(x, start, end, false);
                    start = end;
                }
            }
        }
    }

    private void CollapseColumnWithRange(int x, int yFrom, int yTo, bool shouldFill) {
        int movingDownStep = 0;
        for (int y = yFrom; y < yTo; y++) {
            Tile tile = GetTile(x, y);
            GamePiece piece = tile.piece;
            if (piece == null) {
                movingDownStep++;
            } else if (movingDownStep > 0) {
                piece.MoveDown(movingDownStep);
            }
        }
        // TODO logic error, need fix
        if (movingDownStep > 0 && shouldFill) {
            for (int k = height - movingDownStep; k < height; k++) {
                FillOnePiece(x, k, gamePieceTypes[Random.Range(0, gamePieceTypes.Length)], movingDownStep);
            }
        }
    }

    private bool IsColumnHasMask(int x) {
        return IsColumnRangeHasMask(x, 0, height);
    }

    private bool IsColumnRangeHasMask(int x, int yFrom, int yTo) {
        for (int i = yFrom; i < yTo; i++) {
            if (m_allTiles[x, i].HasMask()) {
                return true;
            }
        }
        return false;
    }

    private int FindNextMaskInColumn(int x, int y) {
        for (int i = y; i < height; i++) {
            if (m_allTiles[x, i].HasMask()) {
                return i;
            }
        }
        return -1;
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

    void UnselectList(List<Tile> tilePath) {
        if (tilePath != null) {
            foreach (Tile tile in tilePath)
            {
                tile.OnTileUnselected();
            }
        }
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
                m_allTiles[x, y].isReserved = false;
                if (yOffSet > 0) {
                    // clear offset
                    gamePiece.MoveDown(0);
                }
            }
        }
    }

    public void ApplyTileMask(Tile tile, TileMaskType maskType) {
        TileMask tileMask = TileMaskUtil.Instance.CreateTileMask(maskType);
        tile.ApplyMask(tileMask);
        m_liveMasks.Add(tileMask);
        tileMask.notifyTileMaskDestroy += OnTileMaskDestory;
    }

    public void OnTileMaskDestory(TileMask tileMask) {
        tileMask.maskedTile.tileMask = null;
        m_liveMasks.Remove(tileMask);
    }

    public void UnsetTile(int x, int y) {
        m_allTiles[x, y].piece = null;
    }

    public Tile[,] GetAllTiles() {
        return m_allTiles;
    }

    public Tile GetTile(int x, int y) {
        if (IsWithinBounds(x, y)) {
            return m_allTiles[x, y];
        }

        return null;
    }

    public void ClearGamePiece(GamePiece piece) {
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
        m_liveMasks = new List<TileMask>();
        SetupTiles();
        FillAllRandomPieces();
        ApplyTileMask(GetTile(1, 3), TileMaskType.Ice);
        ApplyTileMask(GetTile(2, 3), TileMaskType.Ice);
        ApplyTileMask(GetTile(3, 3), TileMaskType.Ice);
        ApplyTileMask(GetTile(4, 5), TileMaskType.Ice);
    }

    private void TestFunction() {
        //GetTile(5, 6).piece.MoveRightDown();
        //GetTile(3, 1).piece.MoveRightDown();
    }

    int delayAction = 5;
    float delayTimer = 0;
    bool isDelayActionTriggered = false;
    void Update() {
        delayTimer += Time.deltaTime;
        if (delayTimer >= delayAction && !isDelayActionTriggered) {
            isDelayActionTriggered = true;
            TestFunction();
        }
    }
}
