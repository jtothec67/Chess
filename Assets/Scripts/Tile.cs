using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    private new SpriteRenderer renderer;
    public Color defaultCol;
    public int tileIndex;

    // Start is called before the first frame update
    void Start()
    {
        renderer = GetComponent<SpriteRenderer>();
        defaultCol = renderer.color;
    }

    void Awake()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //if (Board.tileSelected == tileIndex)
        //{
        //    Vector4 newCol = new Vector4(defaultCol.r + 0.1f, defaultCol.g + 0.1f, defaultCol.b + 0.1f, 255f);
        //    renderer.color = newCol;
        //}
        //else
        //{
        //    renderer.color = defaultCol;
        //}
    }

    void OnMouseDown()
    {
        Board.mouseDownTile = tileIndex;

        // If there is already a tile selected
        if (Board.tileSelected != -1)
        {
            // If we can't move, select this tile as new tile
            if (!Board.MovePiece(Board.tileSelected, Board.mouseDownTile))
            {
                Board.tileSelected = tileIndex;
            }
            else
            {
                // If we could move we reset selected tile
                Board.tileSelected = -1;
                Board.mouseDownTile = -1;
                Board.mouseUpTile = -1;
            }
        }
        // If no tile selected we select this tile
        else
        {
            Board.tileSelected = tileIndex;
        }
    }

    void OnMouseUp()
    {
        // Don't do any logic if there's no tile selected
        if (Board.tileSelected == -1) return;

        // Raycast to detect the tile the mouse is currently over
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);

        if (hit.collider != null)
        {
            Tile tileUpsScript = hit.collider.GetComponent<Tile>();

            if (tileUpsScript != null)
            {
                Board.mouseUpTile = tileUpsScript.tileIndex;
            }
            else
            {
                Board.tileSelected = -1;
                Board.mouseDownTile = -1;
                Board.mouseUpTile = -1;
            }
        }
        else
        {
            Board.tileSelected = -1;
            Board.mouseDownTile = -1;
            Board.mouseUpTile = -1;
        }

        // If dragged from one tile to another, try moving and then reset selected tile
        if (Board.mouseDownTile != Board.mouseUpTile)
        {
            Board.MovePiece(Board.tileSelected, Board.mouseUpTile);

            Board.tileSelected = -1;
            Board.mouseDownTile = -1;
            Board.mouseUpTile = -1;
        }
        // If clicked and released on the same tile
        else if (Board.mouseDownTile == Board.mouseUpTile)
        {
            // If we can move we reset selected tile
            if (Board.MovePiece(Board.tileSelected, Board.mouseUpTile))
            {
                Board.tileSelected = -1;
                Board.mouseDownTile = -1;
                Board.mouseUpTile = -1;
            }
        }
    }
}
