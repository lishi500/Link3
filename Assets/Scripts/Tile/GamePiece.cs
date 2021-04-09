using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePiece : MonoBehaviour
{

    public int xIndex;
    public int yIndex;
    public int yOffSet;
    public PieceType type;

    bool m_isMoving = false;
    bool m_isDestroying = false;
    float m_timeToDestory = 0.15f;
    Board m_board;

    public void SetCoord(int x, int y, int yOffSet)
    {
        xIndex = x;
        yIndex = y;
        this.yOffSet = yOffSet;
    }

    public void Init(Board board) {
        m_board = board;
    }

    public void ChangeColor(Color color) {
        GetComponent<SpriteRenderer>().color = color;
    }

    public void Move(int destX, int dextY, float timeToMove = Board.MOVE_TIME) {
        if (!m_isMoving) { 
            StartCoroutine(MoveRoutine(new Vector3(destX, dextY), timeToMove));
        }
    }

    public void MoveDown(int unit, float timeToMove = Board.MOVE_TIME) {
        if (!m_isMoving) { 
            StartCoroutine(MoveRoutine(new Vector3(xIndex, yIndex-unit, 0), timeToMove));
        }
    }

    public void MoveLeftDown(float timeToMove = Board.MOVE_TIME) {
        if (!m_isMoving) {
            StartCoroutine(MoveRoutine(new Vector3(xIndex - 1, yIndex - 1, 0), timeToMove));
        }
    }

    public void MoveRightDown(float timeToMove = Board.MOVE_TIME) {
        if (!m_isMoving) {
            StartCoroutine(MoveRoutine(new Vector3(xIndex + 1, yIndex - 1, 0), timeToMove));
        }
    }

    public override string ToString() {
        return xIndex + "," + yIndex + " " + type;
    }

    public IEnumerator DestroyPiece() {
        if (!m_isDestroying) {

            Vector3 startScale = transform.localScale;
            Vector3 targetScale = new Vector3(0.1f, 0.1f, 0.1f);
            bool reachTarget = false;
            float elapsedTime = 0f;
            m_isDestroying = true;

            GetTile().TriggerNeighbourTileMask();

            while (!reachTarget)
            {
                if (Vector3.Distance(transform.localScale, targetScale) < 0.01f)
                {
                    reachTarget = true;
                    Destroy(gameObject);
                }

                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp(elapsedTime / m_timeToDestory, 0f, 1f);
                t = CommonUtil.Instance.Smoothstep(t);

                transform.localScale = Vector3.Lerp(startScale, targetScale, t);

                yield return null;
            }
        }
    }

    private void ReserveNextTile(int x, int y) {
        Tile tile = m_board.GetTile(x, y);
        Tile currentTile = m_board.GetTile(xIndex, yIndex);
        if (tile != null) {
            tile.isReserved = true;
        }
        if (currentTile != null) {
            currentTile.piece = null;
        }
    }

    IEnumerator MoveRoutine(Vector3 destination, float timeToMove) {
        ReserveNextTile((int) destination.x, (int) destination.y);
        //Vector3 startPos = new Vector3(transform.position.x, transform.position.y + yOffSet);
        Vector3 startPos = transform.position;
        yOffSet = 0;
        bool reachDest = false;
        float elapsedTime = 0f;
        m_isMoving = true;
        m_board.UnsetTile(xIndex, yIndex);

        while (!reachDest) {
            if (Vector3.Distance(transform.position, destination) < 0.01f) {
                reachDest = true;
                if (m_board != null) { 
                    m_board.PlaceGamePiece(this, (int)destination.x, (int)destination.y);
                }
                break;
            }
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp(elapsedTime / timeToMove, 0f, 1f);
            t = CommonUtil.Instance.Smoothstep(t);

            transform.position = Vector3.Lerp(startPos, destination, t);

            yield return null;
        }
        m_isMoving = false;
    }

    private Tile GetTile() {
        return m_board.GetTile(xIndex, yIndex);
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow)) {
            Move(xIndex, yIndex + 3, 0.5f);
        }
    }
}
