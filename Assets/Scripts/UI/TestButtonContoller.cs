using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestButtonContoller : MonoBehaviour
{
    public Board board;
    public int x;
    public int y;

    public void CollapsePieces() {
        board.CollapsePieces();
    }

    public void TestFindAllLink() {
        //TileUtils.Instance.FindLongestMatchPath(board.GetTile(x, y));
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
