using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public int xIndex;
    public int yIndex;
    public bool visited = false;
    public bool isReserved;

    // ----- restriction

    private Color m_originColor;
    private Color m_highLightColor = Color.yellow;
    Board m_board;

    public GamePiece piece;
    public TileMask tileMask;

    public void OnTileSelected()
    {
        GetComponent<SpriteRenderer>().color = m_highLightColor;
    }

    public void OnTileUnselected() {
        GetComponent<SpriteRenderer>().color = m_originColor;
    }

    public bool CanLink() {
        if (tileMask != null && !tileMask.CanMoveTile()) {
            return false;
        }

        return true;
    }

    public void ApplyMask(TileMask mask) {
        this.tileMask = mask;
        mask.transform.position = transform.position;
        mask.transform.parent = transform.parent;
        mask.maskedTile = this;
    }

    public void TriggerTileMask() {
        if (tileMask != null && tileMask.canErase) {
            tileMask.EraseOnce();
        }
    }

    public void TriggerNeighbourTileMask() {
        if (m_board.hasMasks) {
            List<Tile> neighbours = TileUtils.Instance.GetSurroundingTiles(this);
            neighbours.ForEach(tile => tile.TriggerTileMask());
        }
    }

    public bool HasMask() {
        return tileMask != null;
    }

    private void OnMouseDown() {
        m_board.ClickTile(this);
    }

    private void OnMouseUp()
    {
        m_board.ReleaseTile();
    }

    private void OnMouseEnter() {
        m_board.DragIntoTile(this);
    }

    private void OnMouseUpAsButton()
    {
        m_board.ClearGamePiece(piece);
        //AutoLinkSkill autoLink = new AutoLinkSkill();
        //autoLink.Init(this);
        //autoLink.Cast();
        //BoardSkill verticalSkill = BoardSkillManager.Instance.GetBoardSkill(typeof(VerticalLinkSkill));
        //verticalSkill.Init(this);
        //StartCoroutine(verticalSkill.Cast());
        //m_board.ApplyTileMask(this, TileMaskType.Ice);
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
