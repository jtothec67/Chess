using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    private new SpriteRenderer renderer;
    public Color defaultCol;
    public int tileIndex;

    private bool once = true;

    // Start is called before the first frame update
    void Start()
    {

    }

    void Awake()
    {
        renderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (once)
        {
            defaultCol = renderer.color;
            once = false;
        }

        if (Board.tileSelected == tileIndex)
        {
            Vector4 newCol = new Vector4(defaultCol.r + 0.1f, defaultCol.g + 0.1f, defaultCol.b + 0.1f, 255f);
            renderer.color = newCol;
        }
        else
        {
            renderer.color = defaultCol;
        }
    }

    void OnMouseDown()
    {
        if (transform.childCount > 0 && Board.pieceSelected == false)
        {
            Board.tileSelected = tileIndex;
            Board.pieceSelected = true;
        }
        else if(Board.pieceSelected == true && Board.tileSelected == tileIndex)
        {
            Board.pieceSelected = false;

            Board.tileSelected = -1;
        }
        else if (Board.pieceSelected == true && Board.tileSelected != tileIndex)
        {
            Board.MovePiece(Board.tileSelected, tileIndex);
            Board.pieceSelected = false;
            Board.tileSelected = -1;
        }
    }
}
