using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GraphicalBoard : MonoBehaviour
{
    public GameObject squarePrefab;

    public GameObject highlightPrefab;

    public Color lightCol;
    public Color darkCol;

    public TMP_Text gameText;

    private GameObject[] graphicSquares;
    private int currentSquare = 0;

    private bool justHighlighted = false;


    public GameObject wk;
    public GameObject wp;
    public GameObject wn;
    public GameObject wb;
    public GameObject wr;
    public GameObject wq;

    public GameObject bk;
    public GameObject bp;
    public GameObject bn;
    public GameObject bb;
    public GameObject br;
    public GameObject bq;


    public TMP_InputField inputFEN;
    public TMP_Dropdown whosMove;


    // Start is called before the first frame update
    void Start()
    {
        graphicSquares = new GameObject[64];
        CreateGraphicalBoard();
        Board.GenerateSquaresToEdge();

        Board.LoadPositionFromFen("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR", true);
    }

    // Update is called once per frame
    void Update()
    {
        if (Board.hasChanged)
        {
            Board.hasChanged = false;

            GameObject[] pieces = GameObject.FindGameObjectsWithTag("Piece");

            foreach (GameObject piece in pieces)
            {
                Destroy(piece);
            }

            if (Board.whitesMove) gameText.text = "White to move";
            else gameText.text = "Black to move";

            for (int i = 0; i < Board.Tiles.Length; i++)
            {
                DisplayPiece(Board.Tiles[i], i);
            }

            Checkmate();
        }

        if (!justHighlighted && Board.tileSelected != -1)
        {
            justHighlighted = true;

            HighlightMoves();
        }
        else if (!Board.pieceSelected)
        {
            GameObject[] highlights = GameObject.FindGameObjectsWithTag("Highlight");

            foreach (GameObject highlight in highlights)
            {
                Destroy(highlight);
            }

            justHighlighted = false;
        }
    }

    public void ResetBoard()
    {
        Board.LoadPositionFromFen("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR", true);
    }

    public void LoadFENOnBoard()
    {
        bool personToMove = true;
        
        if (whosMove.value == 1) personToMove = false;

        Board.LoadPositionFromFen(inputFEN.text, personToMove);
    }

    void HighlightMoves()
    {
        int[] legalMoves = Board.GetLegalMoves(Board.tileSelected);

        for (int i = 0; i < legalMoves.Length; i++)
        {
            if (legalMoves[i] >= 0 && legalMoves[i] <= 63)
            {
                var highlight = Instantiate(highlightPrefab, graphicSquares[legalMoves[i]].transform);
                highlight.transform.position = new Vector3(highlight.transform.position.x, highlight.transform.position.y, -2f);
            }
        }
    }

    void Checkmate()
    {
        bool isCheckmate = true;

        if (!Board.whitesMove)
        {
            for (int i = 0; i < 64; i++)
            {
                int[] legalMoves = Board.GetLegalMoves(i);

                if (legalMoves[0] != -1) isCheckmate = false;
            }

            if (isCheckmate)
            {
                for (int y = 0; y < 64; y++)
                {
                    int[] squaresAttacked = Board.GetSquaresAttackedForStalemate(y, true);

                    for (int j = 0; j < squaresAttacked.Length; j++)
                    {
                        if (squaresAttacked[j] == -1)
                        {

                        }
                        else if (Board.Tiles[squaresAttacked[j]] >> 3 == 2 && Board.Tiles[squaresAttacked[j]] % 8 == 1)
                        {
                            gameText.text = "White wins by checkmate";
                            return;
                        }
                    }
                }
                gameText.text = "Draw by stalemate";
            }
        }
        else
        {
            for (int i = 0; i < 64; i++)
            {
                int[] legalMoves = Board.GetLegalMoves(i);

                if (legalMoves[0] != -1) isCheckmate = false;
            }

            if(isCheckmate)
            {
                for (int y = 0; y < 64; y++)
                {
                    int[] squaresAttacked = Board.GetSquaresAttackedForStalemate(y, false);

                    for (int j = 0; j < squaresAttacked.Length; j++)
                    {
                        if (squaresAttacked[j] == -1)
                        {

                        }
                        else if (Board.Tiles[squaresAttacked[j]] >> 3 == 1 && Board.Tiles[squaresAttacked[j]] % 8 == 1)
                        {
                            gameText.text = "Black wins by checkmate";
                            return;
                        }
                    }
                }
                gameText.text = "Draw by stalemate";
            }
        }
    }

    void DisplayPiece(int pieceInt, int tileIndex)
    {
        if (pieceInt == 0) return;


        if ((pieceInt >> 3) == 1)
        {
            if(pieceInt % 8 == 1)
            {
                var piece = Instantiate(wk, graphicSquares[tileIndex].transform);
                piece.transform.position = new Vector3(piece.transform.position.x, piece.transform.position.y, -1f);
            }
            else if (pieceInt % 8 == 2)
            {
                var piece = Instantiate(wp, graphicSquares[tileIndex].transform);
                piece.transform.position = new Vector3(piece.transform.position.x, piece.transform.position.y, -1f);
            }
            else if (pieceInt % 8 == 3)
            {
                var piece = Instantiate(wn, graphicSquares[tileIndex].transform);
                piece.transform.position = new Vector3(piece.transform.position.x, piece.transform.position.y, -1f);
            }
            else if (pieceInt % 8 == 4)
            {
                var piece = Instantiate(wb, graphicSquares[tileIndex].transform);
                piece.transform.position = new Vector3(piece.transform.position.x, piece.transform.position.y, -1f);
            }
            else if (pieceInt % 8 == 5)
            {
                var piece = Instantiate(wr, graphicSquares[tileIndex].transform);
                piece.transform.position = new Vector3(piece.transform.position.x, piece.transform.position.y, -1f);
            }
            else if (pieceInt % 8 == 6)
            {
                var piece = Instantiate(wq, graphicSquares[tileIndex].transform);
                piece.transform.position = new Vector3(piece.transform.position.x, piece.transform.position.y, -1f);
            }
        }
        else
        {
            if (pieceInt % 8 == 1)
            {
                var piece = Instantiate(bk, graphicSquares[tileIndex].transform);
                piece.transform.position = new Vector3(piece.transform.position.x, piece.transform.position.y, -1f);
            }
            else if (pieceInt % 8 == 2)
            {
                var piece = Instantiate(bp, graphicSquares[tileIndex].transform);
                piece.transform.position = new Vector3(piece.transform.position.x, piece.transform.position.y, -1f);
            }
            else if (pieceInt % 8 == 3)
            {
                var piece = Instantiate(bn, graphicSquares[tileIndex].transform);
                piece.transform.position = new Vector3(piece.transform.position.x, piece.transform.position.y, -1f);
            }
            else if (pieceInt % 8 == 4)
            {
                var piece = Instantiate(bb, graphicSquares[tileIndex].transform);
                piece.transform.position = new Vector3(piece.transform.position.x, piece.transform.position.y, -1f);
            }
            else if (pieceInt % 8 == 5)
            {
                var piece = Instantiate(br, graphicSquares[tileIndex].transform);
                piece.transform.position = new Vector3(piece.transform.position.x, piece.transform.position.y, -1f);
            }
            else if (pieceInt % 8 == 6)
            {
                var piece = Instantiate(bq, graphicSquares[tileIndex].transform);
                piece.transform.position = new Vector3(piece.transform.position.x, piece.transform.position.y, -1f);
            }
        }
    }

    void CreateGraphicalBoard()
    {
        for (int rank = 0; rank < 8; rank++)
        {
            for (int file = 0; file < 8; file ++)
            {
                bool isLightSquare = (file + rank) % 2 != 0;

                var squareColour = (isLightSquare) ? lightCol : darkCol;
                var position = new Vector3(-3.5f + file, -3.5f + rank, 0f);

                DrawSquare(squareColour, position);
            }
        }
    }

    void DrawSquare(Color squareColour, Vector3 position)
    {
        var square = Instantiate(squarePrefab, transform);
        square.transform.position = position;

        SpriteRenderer rend = square.GetComponent<SpriteRenderer>();
        rend.color = squareColour;

        var Tile = square.GetComponent<Tile>();
        Tile.tileIndex = currentSquare;

        if(currentSquare < 8)
        {
            var rankText = square.GetComponentInChildren<TextMeshPro>();

            int num = (currentSquare % 8) + 97;

            char letter = (char)num;

            rankText.text = "" + letter;
        }

        if(currentSquare % 8 == 0)
        {
            Transform child = square.transform.Find("Rank Number");

            var textObject = child.gameObject;

            var fileText = textObject.GetComponentInChildren<TextMeshPro>();

            int num = (currentSquare / 8) + 1;

            fileText.text = "" + num;
        }

        graphicSquares[currentSquare] = square;
        currentSquare++;
    }
}
