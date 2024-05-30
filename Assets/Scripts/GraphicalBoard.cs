using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GraphicalBoard : MonoBehaviour
{
    public GameObject squarePrefab;

    public GameObject highlightPrefab;

    public GameObject arrowBodyPrefab;
    public GameObject arrowHeadPrefab;

    public Color lightCol;
    public Color darkCol;

    public TMP_Text gameText;

    public GameObject currentPosText;

    private GameObject[] graphicSquares = new GameObject[64];
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

    public TMP_InputField outputFEN;

    private int tileLastSelected = -1;

    private string startPos = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w";

    private bool isDragging = false;

    private int pieceTileBeingDragged = -1;
    private int pieceOnTile = 0;

    private GameObject draggedPiece;

    // Start is called before the first frame update
    void Start()
    {
        CreateGraphicalBoard();
        Board.GenerateSquaresToEdge();

        Board.LoadPositionFromFEN(startPos);

        Board.pastMoves.Clear();
        Board.pastMoves.Add(Board.GenerateFEN());
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Raycast to detect the tile the mouse is currently over
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);

            if (hit.collider != null)
            {
                Tile tileUpsScript = hit.collider.GetComponent<Tile>();

                if (tileUpsScript != null)
                {
                    pieceTileBeingDragged = tileUpsScript.tileIndex;
                    pieceOnTile = Board.Tiles[tileUpsScript.tileIndex];

                    draggedPiece = DecideDraggedPiece(pieceOnTile);
                    if (draggedPiece != null)
                    {
                        draggedPiece.tag = "DontDestroy";

                        isDragging = true;
                        Board.hasChanged = true;
                    }
                    
                }
            }
        }

        if (!justHighlighted && Board.tileSelected != -1)
        {
            justHighlighted = true;

            HighlightMoves();
        }
        else if (Board.tileSelected == -1 || tileLastSelected != Board.tileSelected)
        {
            GameObject[] highlights = GameObject.FindGameObjectsWithTag("Highlight");

            foreach (GameObject highlight in highlights)
            {
                Destroy(highlight);
            }

            justHighlighted = false;
        }

        if (isDragging)
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            draggedPiece.transform.position = new Vector3(mousePosition.x, mousePosition.y, -3f);
        }

        if (Input.GetMouseButtonUp(0))
        {
            Destroy(draggedPiece);
            draggedPiece = null;
            pieceTileBeingDragged = -1;

            isDragging = false;
            Board.hasChanged = true;
        }

        tileLastSelected = Board.tileSelected;
    }

    void LateUpdate()
    {
        if (Input.GetMouseButtonUp(1))
        {
            // User trying to draw arrow
            if (Board.mouseDownHighlight != Board.mouseUpHighlight)
            {
                DrawArrow();

                Board.mouseDownHighlight = -1;
                Board.mouseUpHighlight = -1;
            }
        }

        if (Board.hasChanged)
        {
            Board.hasChanged = false;

            GameObject[] pieces = GameObject.FindGameObjectsWithTag("Piece");

            foreach (GameObject piece in pieces)
            {
                Destroy(piece);
            }

            GameObject[] arrows = GameObject.FindGameObjectsWithTag("Arrow");

            foreach (GameObject arrow in arrows)
            {
                Destroy(arrow);
            }

            if (Board.whitesMove) gameText.text = "White to move";
            else gameText.text = "Black to move";

            for (int i = 0; i < Board.Tiles.Length; i++)
            {
                if (i == pieceTileBeingDragged) continue;
                DisplayPiece(Board.Tiles[i], i);
            }

            Checkmate();

            currentPosText.SetActive(Board.displayPosition == Board.activePosition);
        }
    }

    public void DrawArrow()
    {
        bool exists = false;

        GameObject[] arrows = GameObject.FindGameObjectsWithTag("Arrow");

        foreach (GameObject arrow in arrows)
        {
            ArrowInfo arrowTestInfo = arrow.GetComponent<ArrowInfo>();

            // Arrow is already on these squares
            if (arrowTestInfo.startTile == Board.mouseDownHighlight && arrowTestInfo.endTile == Board.mouseUpHighlight)
            {
                Destroy(arrow);
                exists = true;
            }
        }

        if (exists) return;

        Vector3 downToUpDir = (graphicSquares[Board.mouseUpHighlight].transform.position - graphicSquares[Board.mouseDownHighlight].transform.position).normalized;
        Vector3 offset = downToUpDir * 0.1f;

        var arrowHead = Instantiate(arrowHeadPrefab, null);
        ArrowInfo arrowInfo = arrowHead.GetComponent<ArrowInfo>();
        arrowInfo.startTile = Board.mouseDownHighlight;
        arrowInfo.endTile = Board.mouseUpHighlight;

        arrowHead.transform.position = graphicSquares[Board.mouseUpHighlight].transform.position - offset;

        Vector3 direction = graphicSquares[Board.mouseUpHighlight].transform.position - graphicSquares[Board.mouseDownHighlight].transform.position;
        Quaternion rotation = Quaternion.LookRotation(direction);
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        arrowHead.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));

        arrowHead.transform.position = new Vector3(arrowHead.transform.position.x, arrowHead.transform.position.y, -4f);

        var arrowBody = Instantiate(arrowBodyPrefab, null);
        arrowInfo = arrowBody.GetComponent<ArrowInfo>();
        arrowInfo.startTile = Board.mouseDownHighlight;
        arrowInfo.endTile = Board.mouseUpHighlight;

        arrowBody.transform.position = (graphicSquares[Board.mouseDownHighlight].transform.position + graphicSquares[Board.mouseUpHighlight].transform.position - (downToUpDir * 1f)) / 2;

        direction = graphicSquares[Board.mouseUpHighlight].transform.position - arrowBody.transform.position;
        rotation = Quaternion.LookRotation(direction);
        angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        arrowBody.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));

        float distance = Vector3.Distance(graphicSquares[Board.mouseUpHighlight].transform.position, graphicSquares[Board.mouseDownHighlight].transform.position);
        Vector3 rescale = arrowBody.transform.localScale;
        float newY = distance - 1.19f;
        if (newY < 0.7f) newY = 0f;
        rescale.y = newY;
        
        arrowBody.transform.localScale = rescale;

        arrowBody.transform.position = new Vector3(arrowBody.transform.position.x, arrowBody.transform.position.y, -4f);
    }

    public void BackAMove()
    {
        if (Board.displayPosition <= 0)
        {
            return;
        }

        //Board.pastMoves.RemoveAt(Board.pastMoves.Count - 1);
        Board.displayPosition -= 1;
        Board.LoadPositionFromFEN(Board.pastMoves[Board.displayPosition]);
        Board.hasChanged = true;
    }

    public void ForwardAMove()
    {
        if (Board.pastMoves.Count == Board.displayPosition + 1)
        {
            return;
        }

        Board.displayPosition += 1;
        Board.LoadPositionFromFEN(Board.pastMoves[Board.displayPosition]);
        Board.hasChanged = true;
    }

    public void ResetBoard()
    {
        Board.LoadPositionFromFEN(startPos);

        Board.pastMoves.Clear();
        Board.pastMoves.Add(Board.GenerateFEN());

        Board.activePosition = 0;
        Board.displayPosition = 0;
    }

    public void LoadFENOnBoard()
    {
        Board.pastMoves.Clear();
        Board.LoadPositionFromFEN(inputFEN.text);
        Board.pastMoves.Add(Board.GenerateFEN());

        Board.activePosition = 0;
        Board.displayPosition = 0;
    }

    public void GenerateFENFromBoard()
    {
        string fen = Board.GenerateFEN();
        outputFEN.text = fen;
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

    GameObject DecideDraggedPiece(int pieceInt)
    {
        if (pieceInt == 0) return null;

        GameObject piece = null;

        if ((pieceInt >> 3) == 1)
        {
            if (pieceInt % 8 == 1)
            {
                piece = Instantiate(wk, null);
            }
            else if (pieceInt % 8 == 2)
            {
                piece = Instantiate(wp, null);
            }
            else if (pieceInt % 8 == 3)
            {
                piece = Instantiate(wn, null);
            }
            else if (pieceInt % 8 == 4)
            {
                piece = Instantiate(wb, null);
            }
            else if (pieceInt % 8 == 5)
            {
                piece = Instantiate(wr, null);
            }
            else if (pieceInt % 8 == 6)
            {
                piece = Instantiate(wq, null);
            }
        }
        else
        {
            if (pieceInt % 8 == 1)
            {
                piece = Instantiate(bk, null);
            }
            else if (pieceInt % 8 == 2)
            {
                piece = Instantiate(bp, null);
            }
            else if (pieceInt % 8 == 3)
            {
                piece = Instantiate(bn, null);
            }
            else if (pieceInt % 8 == 4)
            {
                piece = Instantiate(bb, null);
            }
            else if (pieceInt % 8 == 5)
            {
                piece = Instantiate(br, null);
            }
            else if (pieceInt % 8 == 6)
            {
                piece = Instantiate(bq, null);
            }
        }

        return piece;
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
