using System.Collections.Generic;
using UnityEngine;

public class PieceUtil : Singleton<PieceUtil>
{
    private Dictionary<PieceType, Color> m_pieceTypeColor = new Dictionary<PieceType, Color> {
        { PieceType.Blue, new Color(107f/255f, 137f/255f, 210f/255f)},
        { PieceType.Cyan, new Color(143f/255f, 212f/255f, 217f/255f)},
        { PieceType.Green, new Color(15f/255f, 89f/255f, 89f/255f)},
        { PieceType.Indigo, new Color(44f/255f, 22f/255f, 71f/255f)},
        { PieceType.Pink, new Color(181f/255f, 55f/255f, 137f/255f)},
        { PieceType.Red, new Color(217f/255f, 50f/255f, 64f/255f)},
        { PieceType.Teal, new Color(23f/255f, 166f/255f, 151f/255f)},
        { PieceType.Yellow, new Color(248f/255f, 212f/255f, 27f/255f)}
    };

    public GamePiece GenerateGamePieceByType(PieceType type) {
        GameObject gamePiecePrafab = GetGamePiecePrafab();
        GameObject gamePieceObj = Instantiate(gamePiecePrafab, Vector3.zero, Quaternion.identity);
        SpriteRenderer spr = gamePieceObj.GetComponent<SpriteRenderer>();
        spr.color = GetTypeColor(type);

        GamePiece piece = gamePieceObj.GetComponent<GamePiece>();
        piece.type = type;

        return piece;
    }

    public Color GetTypeColor(PieceType type) {
        Color typeColor = Color.white;
        m_pieceTypeColor.TryGetValue(type, out typeColor);

        return typeColor;
    }

    private GameObject GetGamePiecePrafab() {
        return (GameObject) Resources.Load("Prefab/PieceDot");
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
