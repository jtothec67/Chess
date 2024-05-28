using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board
{
    public static List<string> pastMoves = new List<string>();
    public static int displayPosition = 0;
    public static int activePosition = 0;

    public static bool hasChanged = true;
    public static int[] Tiles = new int[64];
    public static int[] tempTiles = new int[64];

    public static bool pieceSelected = false;
    public static int tileSelected = -1;
    public static int mouseDownTile = -1;
    public static int mouseUpTile = -1;

    public static bool whitesMove = true;

    public static bool whiteCanCastleShort = true;
    public static bool blackCanCastleShort = true;
    public static bool whiteCanCastleLong = true;
    public static bool blackCanCastleLong = true;

    public static int[] directionOffsets = { 8, -8, -1, 1, 7, -7, 9, -9 };

    public static int[] whitePawnDirectionOffsets = { 8 };
    public static int[] whiteStartPawnDirectionOffsets = { 8 , 16 };
    public static int[] whitePawnTakesDirectionOffsets = { 7 , 9 };
    public static int[] blackPawnDirectionOffsets = { -8 };
    public static int[] blackStartPawnDirectionOffsets = { -8 , -16 };
    public static int[] blackPawnTakesDirectionOffsets = { -7, -9 };

    public static int[] knightDirectionOffsets = { 17, 10, -6, -15, -17, -10, 6, 15 };

    public static int enPassentable = -1;

    public static bool whiteShortCastle = true;

    public static int[,] numSquaresToEdge = new int[64, 8];

    public static void GenerateSquaresToEdge()
    {
        for (int rank = 0; rank < 8; rank++) 
        {
            for (int file = 0; file < 8; file++)
            {
                int squareIndex = rank * 8 + file;

                int numNorth = 7 - rank;
                int numSouth = rank;
                int numWest = file;
                int numEast = 7 - file;


                numSquaresToEdge[squareIndex,0] = numNorth;
                numSquaresToEdge[squareIndex, 1] = numSouth;
                numSquaresToEdge[squareIndex, 2] = numWest;
                numSquaresToEdge[squareIndex, 3] = numEast;
                numSquaresToEdge[squareIndex, 4] = Mathf.Min(numNorth, numWest);
                numSquaresToEdge[squareIndex, 5] = Mathf.Min(numSouth, numEast);
                numSquaresToEdge[squareIndex, 6] = Mathf.Min(numNorth, numEast);
                numSquaresToEdge[squareIndex, 7] = Mathf.Min(numSouth, numWest);
            }
        }
    }

    public static void LoadPositionFromFEN(string fen)
    {
        for (int i = 0; i < 64; i++)
        {
            Tiles[i] = -1;
        }

        var pieceTypeFromSymbol = new Dictionary<char, int>()
        {
            ['k'] = Piece.King,
            ['p'] = Piece.Pawn,
            ['n'] = Piece.Knight,
            ['b'] = Piece.Bishop,
            ['r'] = Piece.Rook,
            ['q'] = Piece.Queen
        };

        string fenBoard = fen.Split(' ')[0];
        bool toMoveIncluded = true;

        try
        {
            string playerMove = fen.Split(' ')[1];

            if (playerMove[0] == 'w')
            {
                whitesMove = true;
            }
            else if (playerMove[0] == 'b')
            {
                whitesMove = false;
            }
        }
        catch (Exception e)
        {
            toMoveIncluded = false;
        }

        int file = 0, rank = 7;

        foreach (char symbol in fenBoard)
        {
            if (symbol == '/')
            {
                file = 0;
                rank--;
            }
            else
            {
                if (char.IsDigit(symbol))
                {
                    file += (int)char.GetNumericValue(symbol);
                }
                else
                {
                    int pieceColour = (char.IsUpper(symbol)) ? Piece.White : Piece.Black;
                    int pieceType = pieceTypeFromSymbol[char.ToLower(symbol)];
                    Tiles[rank * 8 + file] = pieceType | pieceColour;
                    file++;
                }
            }
        }

        if (Tiles[4] % 8 != 1 || (Tiles[4] >> 3) != 1) { whiteCanCastleShort = false; whiteCanCastleLong = false; }
        else { whiteCanCastleShort = true; whiteCanCastleLong = true; }
        if (Tiles[60] % 8 != 1 || (Tiles[60] >> 3) != 2) { blackCanCastleShort = false; blackCanCastleLong = false; }
        else { blackCanCastleShort = true; blackCanCastleLong = true; }
        if (Tiles[0] % 8 != 5 || (Tiles[0] >> 3) != 1) { whiteCanCastleLong = false; }
        else { whiteCanCastleLong = true; }
        if (Tiles[7] % 8 != 5 || (Tiles[7] >> 3) != 1) { whiteCanCastleShort = false; }
        else { whiteCanCastleShort = true; }
        if (Tiles[56] % 8 != 5 || (Tiles[56] >> 3) != 2) { blackCanCastleLong = false; }
        else { blackCanCastleLong = true; }
        if (Tiles[63] % 8 != 5 || (Tiles[63] >> 3) != 1) { blackCanCastleShort = false; }
        else { blackCanCastleShort = true; }

        hasChanged = true;
    }

    public static string GenerateFEN()
    {
        StringBuilder fenBuilder = new StringBuilder();

        for (int rank = 7; rank >= 0; rank--)
        {
            int emptyCount = 0;
            for (int file = 0; file < 8; file++)
            {
                int index = rank * 8 + file;
                int piece = Tiles[index];
                if (piece == -1)
                {
                    emptyCount++;
                }
                else
                {
                    if (emptyCount > 0)
                    {
                        fenBuilder.Append(emptyCount);
                        emptyCount = 0;
                    }
                    char pieceSymbol = PieceToSymbol(piece);
                    fenBuilder.Append(pieceSymbol);
                }
            }
            if (emptyCount > 0)
            {
                fenBuilder.Append(emptyCount);
            }
            if (rank > 0)
            {
                fenBuilder.Append('/');
            }
        }

        fenBuilder.Append(' ');
        fenBuilder.Append(whitesMove ? 'w' : 'b');

        return fenBuilder.ToString();
    }

    private static char PieceToSymbol(int piece)
    {
        char symbol;

        switch (piece & 0b111)
        {
            case Piece.King:
                symbol = 'K';
                break;
            case Piece.Queen:
                symbol = 'Q';
                break;
            case Piece.Rook:
                symbol = 'R';
                break;
            case Piece.Bishop:
                symbol = 'B';
                break;
            case Piece.Knight:
                symbol = 'N';
                break;
            case Piece.Pawn:
                symbol = 'P';
                break;
            default:
                throw new ArgumentException("Invalid piece type");
        }

        if ((piece & Piece.Black) != 0)
        {
            symbol = char.ToLower(symbol);
        }

        return symbol;
    }

    public static int[] GetLegalMoves(int position)
    {
        int currentArrayPos = 0;
        int[] possibleMoves = new int[27];
        for (int i = 0; i < possibleMoves.Length; i++)
        {
            possibleMoves[i] = -1;
        }
        if (whitesMove && (Tiles[position] >> 3) == 1) // Is white
        {
            if (Tiles[position] % 8 == 1) // Is white king
            {
                for (int directionIndex = 0; directionIndex < 8; directionIndex++)
                {
                    int targetSquare = position + directionOffsets[directionIndex];

                    if (numSquaresToEdge[position, directionIndex] != 0 && (Tiles[targetSquare] >> 3) != 1)
                    {
                        if(KingIsSafe(position, targetSquare))
                        {
                            possibleMoves[currentArrayPos] = targetSquare;
                            currentArrayPos++;
                        }
                    }
                }

                if (whiteCanCastleShort && Tiles[position + 1] % 8 == -1 && Tiles[position + 2] % 8 == -1)
                {
                    if (KingIsSafe(position, position + 2))
                    {
                        possibleMoves[currentArrayPos] = position + 2;
                        currentArrayPos++;
                    }
                }

                if (whiteCanCastleLong && Tiles[position - 1] % 8 == -1 && Tiles[position - 2] % 8 == -1 && Tiles[position - 3] % 8 == -1)
                {
                    if (KingIsSafe(position, position - 2))
                    {
                        possibleMoves[currentArrayPos] = position - 2;
                        currentArrayPos++;
                    }
                }

                return possibleMoves;
            }
            else if (Tiles[position] % 8 == 2) // Is white pawn
            {
                if (position < 55 && Tiles[position + whitePawnTakesDirectionOffsets[1]] != -1 && (Tiles[position + whitePawnTakesDirectionOffsets[1]] >> 3) != 1 && numSquaresToEdge[position,3] != 0)
                {
                    if (KingIsSafe(position, position + whitePawnTakesDirectionOffsets[1]))
                    {
                        possibleMoves[currentArrayPos] = position + whitePawnTakesDirectionOffsets[1];
                        currentArrayPos++;
                    }
                }

                if (position < 56 && Tiles[position + whitePawnTakesDirectionOffsets[0]] != -1 && (Tiles[position + whitePawnTakesDirectionOffsets[0]] >> 3) != 1 && numSquaresToEdge[position, 2] != 0)
                {
                    if (KingIsSafe(position, position + whitePawnTakesDirectionOffsets[0]))
                    {
                        possibleMoves[currentArrayPos] = position + whitePawnTakesDirectionOffsets[0];
                        currentArrayPos++;
                    }
                }

                if (position >= 32 && position <= 39 && Tiles[position + 1] % 8 == 2 && numSquaresToEdge[position, 3] != 0 && enPassentable == position + 1) // Is en passent
                {
                    if (KingIsSafe(position, position + whitePawnTakesDirectionOffsets[1]))
                    {
                        possibleMoves[currentArrayPos] = position + whitePawnTakesDirectionOffsets[1];
                        currentArrayPos++;
                    }
                }

                if (position >= 32 && position <= 39 && Tiles[position - 1] % 8 == 2 && numSquaresToEdge[position, 2] != 0 && enPassentable == position - 1) // Is en passent
                {
                    if (KingIsSafe(position, position + whitePawnTakesDirectionOffsets[0]))
                    {
                        possibleMoves[currentArrayPos] = position + whitePawnTakesDirectionOffsets[0];
                        currentArrayPos++;
                    }
                }

                if (position >= 8 && position <= 15) // Is on starting rank
                {
                    for (int i = 0; i < whiteStartPawnDirectionOffsets.Length; i++)
                    {
                        if (Tiles[position + whiteStartPawnDirectionOffsets[i]] != -1)
                        {
                            break;
                        }
                        else
                        {
                            if (KingIsSafe(position, position + whiteStartPawnDirectionOffsets[i]))
                            {
                                possibleMoves[currentArrayPos] = position + whiteStartPawnDirectionOffsets[i];
                                currentArrayPos++;
                            }
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < whitePawnDirectionOffsets.Length; i++)
                    {
                        if (Tiles[position + whitePawnDirectionOffsets[i]] == -1)
                        {
                            if (KingIsSafe(position, position + whitePawnDirectionOffsets[i]))
                            {
                                possibleMoves[currentArrayPos] = position + whitePawnDirectionOffsets[i];
                                currentArrayPos++;
                            }
                        }

                    }
                }

                return possibleMoves;
            }
            else if (Tiles[position] % 8 == 3) // Is white knight
            {
                int y = position / 8;
                int x = position - y * 8;
                for (int i = 0; i < knightDirectionOffsets.Length; i++)
                {
                    int targetSquare = position + knightDirectionOffsets[i];

                    int knightSquareY = targetSquare / 8;
                    int knightSquareX = targetSquare - knightSquareY * 8;

                    if (targetSquare >= 0 && targetSquare <= 63 && (Tiles[position + knightDirectionOffsets[i]] >> 3) != 1) // On board and isnt white
                    {
                        int maxCoordMoveDst = System.Math.Max(System.Math.Abs(x - knightSquareX), System.Math.Abs(y - knightSquareY));
                        if (maxCoordMoveDst == 2)
                        {
                            if (KingIsSafe(position, targetSquare))
                            {
                                possibleMoves[currentArrayPos] = targetSquare;
                                currentArrayPos++;
                            }
                        }
                    }
                }

                return possibleMoves;
            }
            else if (Tiles[position] % 8 == 4) // Is white bishop
            {
                for (int directionIndex = 4; directionIndex < 8; directionIndex++)
                {
                    for (int n = 0; n < numSquaresToEdge[position, directionIndex]; n++)
                    {
                        int targetSquare = position + directionOffsets[directionIndex] * (n + 1);

                        if ((Tiles[targetSquare] >> 3) == 1) break; // Blocked by white piece

                        if (KingIsSafe(position, targetSquare))
                        {
                            possibleMoves[currentArrayPos] = targetSquare;
                            currentArrayPos++;
                        }

                        if ((Tiles[targetSquare] >> 3) == 2) break; // Capture black piece
                    }
                }

                return possibleMoves;
            }
            else if (Tiles[position] % 8 == 5) // Is white rook
            {
                for (int directionIndex = 0; directionIndex < 4; directionIndex++)
                {
                    for (int n = 0; n < numSquaresToEdge[position, directionIndex]; n++)
                    {
                        int targetSquare = position + directionOffsets[directionIndex] * (n + 1);
                        int pieceOnTargetSquare = Tiles[targetSquare];

                        if ((Tiles[targetSquare] >> 3) == 1) break; // Blocked by white piece

                        if (KingIsSafe(position, targetSquare))
                        {
                            possibleMoves[currentArrayPos] = targetSquare;
                            currentArrayPos++;
                        }

                        if ((Tiles[targetSquare] >> 3) == 2) break; // Capture black piece
                    }
                }

                return possibleMoves;
            }
            else if (Tiles[position] % 8 == 6) // Is white queen
            {
                for (int directionIndex = 0; directionIndex < 8; directionIndex++)
                {
                    for (int n = 0; n < numSquaresToEdge[position, directionIndex]; n++)
                    {
                        int targetSquare = position + directionOffsets[directionIndex] * (n + 1);
                        int pieceOnTargetSquare = Tiles[targetSquare];

                        if ((Tiles[targetSquare] >> 3) == 1) break; // Blocked by white piece

                        if (KingIsSafe(position, targetSquare))
                        {
                            possibleMoves[currentArrayPos] = targetSquare;
                            currentArrayPos++;
                        }

                        if ((Tiles[targetSquare] >> 3) == 2) break; // Capture black piece
                    }
                }

                return possibleMoves;
            }
        }
        else if (!whitesMove && (Tiles[position] >> 3) == 2) // Is black
        {
            if (Tiles[position] % 8 == 1) // Is black king
            {
                for (int directionIndex = 0; directionIndex < 8; directionIndex++)
                {
                    int targetSquare = position + directionOffsets[directionIndex];

                    if (numSquaresToEdge[position, directionIndex] != 0 && (Tiles[targetSquare] >> 3) != 2)
                    {
                        if (KingIsSafe(position, targetSquare))
                        {
                            possibleMoves[currentArrayPos] = targetSquare;
                            currentArrayPos++;
                        }
                    }
                }

                if (blackCanCastleShort && Tiles[position + 1] % 8 == -1 && Tiles[position + 2] % 8 == -1)
                {
                    if (KingIsSafe(position, position + 2))
                    {
                        possibleMoves[currentArrayPos] = position + 2;
                        currentArrayPos++;
                    }
                }

                if (blackCanCastleLong && Tiles[position - 1] % 8 == -1 && Tiles[position - 2] % 8 == -1 && Tiles[position - 3] % 8 == -1)
                {
                    if (KingIsSafe(position, position - 2))
                    {
                        possibleMoves[currentArrayPos] = position - 2;
                        currentArrayPos++;
                    }
                }

                return possibleMoves;
            }
            else if (Tiles[position] % 8 == 2) // Is black pawn
            {
                if (position > 8 && Tiles[position + blackPawnTakesDirectionOffsets[0]] != -1 && (Tiles[position + blackPawnTakesDirectionOffsets[0]] >> 3) != 2 && numSquaresToEdge[position, 3] != 0)
                {
                    if (KingIsSafe(position, position + blackPawnTakesDirectionOffsets[0]))
                    {
                        possibleMoves[currentArrayPos] = position + blackPawnTakesDirectionOffsets[0];
                        currentArrayPos++;
                    }
                }

                if (position > 8 && Tiles[position + blackPawnTakesDirectionOffsets[1]] != -1 && (Tiles[position + blackPawnTakesDirectionOffsets[1]] >> 3) != 2 && numSquaresToEdge[position, 2] != 0)
                {
                    if (KingIsSafe(position, position + blackPawnTakesDirectionOffsets[1]))
                    {
                        possibleMoves[currentArrayPos] = position + blackPawnTakesDirectionOffsets[1];
                        currentArrayPos++;
                    }
                }

                if (position >= 24 && position <= 31 && Tiles[position - 1] % 8 == 2 && numSquaresToEdge[position, 3] != 0 && enPassentable == position - 1) // Is en passent
                {
                    if (KingIsSafe(position, position + blackPawnTakesDirectionOffsets[1]))
                    {
                        possibleMoves[currentArrayPos] = position + blackPawnTakesDirectionOffsets[1];
                        currentArrayPos++;
                    }
                }

                if (position >= 24 && position <= 31 && Tiles[position + 1] % 8 == 2 && numSquaresToEdge[position, 2] != 0 && enPassentable == position + 1) // Is en passent
                {
                    if (KingIsSafe(position, position + blackPawnTakesDirectionOffsets[0]))
                    {
                        possibleMoves[currentArrayPos] = position + blackPawnTakesDirectionOffsets[0];
                        currentArrayPos++;
                    }
                }

                if (position >= 48 && position <= 55) // Is on starting rank
                {
                    for (int i = 0; i < blackStartPawnDirectionOffsets.Length; i++)
                    {
                        if (Tiles[position + blackStartPawnDirectionOffsets[i]] != -1)
                        {
                            break;
                        }
                        else
                        {
                            if (KingIsSafe(position, position + blackStartPawnDirectionOffsets[i]))
                            {
                                possibleMoves[currentArrayPos] = position + blackStartPawnDirectionOffsets[i];
                                currentArrayPos++;
                            }
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < blackPawnDirectionOffsets.Length; i++)
                    {
                        if (position > 7 && Tiles[position + blackPawnDirectionOffsets[i]] == -1)
                        {
                            if (KingIsSafe(position, position + blackPawnDirectionOffsets[i]))
                            {
                                possibleMoves[currentArrayPos] = position + blackPawnDirectionOffsets[i];
                                currentArrayPos++;
                            }
                        }
                    }
                }

                return possibleMoves;
            }
            else if (Tiles[position] % 8 == 3) // Is black knight
            {
                int y = position / 8;
                int x = position - y * 8;
                for (int i = 0; i < knightDirectionOffsets.Length; i++)
                {
                    int targetSquare = position + knightDirectionOffsets[i];

                    int knightSquareY = targetSquare / 8;
                    int knightSquareX = targetSquare - knightSquareY * 8;

                    if (targetSquare >= 0 && targetSquare <= 63 && (Tiles[position + knightDirectionOffsets[i]] >> 3) != 2) // On board and isnt black
                    {
                        int maxCoordMoveDst = System.Math.Max(System.Math.Abs(x - knightSquareX), System.Math.Abs(y - knightSquareY));
                        if (maxCoordMoveDst == 2)
                        {
                            if (KingIsSafe(position, targetSquare))
                            {
                                possibleMoves[currentArrayPos] = targetSquare;
                                currentArrayPos++;
                            }
                        }

                    }
                }

                return possibleMoves;
            }
            else if (Tiles[position] % 8 == 4) // Is black bishop
            {
                for (int directionIndex = 4; directionIndex < 8; directionIndex++)
                {
                    for (int n = 0; n < numSquaresToEdge[position, directionIndex]; n++)
                    {
                        int targetSquare = position + directionOffsets[directionIndex] * (n + 1);
                        int pieceOnTargetSquare = Tiles[targetSquare];

                        if ((Tiles[targetSquare] >> 3) == 2) break; // Blocked by black piece

                        if (KingIsSafe(position, targetSquare))
                        {
                            possibleMoves[currentArrayPos] = targetSquare;
                            currentArrayPos++;
                        }

                        if ((Tiles[targetSquare] >> 3) == 1) break; // Capture white piece
                    }
                }

                return possibleMoves;
            }
            else if (Tiles[position] % 8 == 5) // Is black rook
            {
                for (int directionIndex = 0; directionIndex < 4; directionIndex++)
                {
                    for (int n = 0; n < numSquaresToEdge[position, directionIndex]; n++)
                    {
                        int targetSquare = position + directionOffsets[directionIndex] * (n + 1);
                        int pieceOnTargetSquare = Tiles[targetSquare];

                        if ((Tiles[targetSquare] >> 3) == 2) break; // Blocked by black piece

                        if (KingIsSafe(position, targetSquare))
                        {
                            possibleMoves[currentArrayPos] = targetSquare;
                            currentArrayPos++;
                        }

                        if ((Tiles[targetSquare] >> 3) == 1) break; // Capture white piece
                    }
                }

                return possibleMoves;
            }
            else if (Tiles[position] % 8 == 6) // Is black queen
            {
                for (int directionIndex = 0; directionIndex < 8; directionIndex++)
                {
                    for (int n = 0; n < numSquaresToEdge[position, directionIndex]; n++)
                    {
                        int targetSquare = position + directionOffsets[directionIndex] * (n + 1);
                        int pieceOnTargetSquare = Tiles[targetSquare];

                        if ((Tiles[targetSquare] >> 3) == 2) break; // Blocked by black piece

                        if (KingIsSafe(position, targetSquare))
                        {
                            possibleMoves[currentArrayPos] = targetSquare;
                            currentArrayPos++;
                        }

                        if ((Tiles[targetSquare] >> 3) == 1) break; // Capture white piece
                    }
                }

                return possibleMoves;
            }
        }
            possibleMoves[currentArrayPos] = -1;
            return possibleMoves;
    }

    public static int[] GetSquaresAttacked(int position, bool whiteToAttack)
    {
        int currentArrayPos = 0;
        int[] possibleMoves = new int[27];
        for (int i = 0; i < possibleMoves.Length; i++)
        {
            possibleMoves[i] = -1;
        }

        if (whiteToAttack) // Is white
        {
            if (tempTiles[position] % 8 == 1) // Is white king
            {
                for (int directionIndex = 0; directionIndex < 8; directionIndex++)
                {
                    int targetSquare = position + directionOffsets[directionIndex];

                    if (numSquaresToEdge[position, directionIndex] != 0 && (tempTiles[targetSquare] >> 3) != 1)
                    {
                        possibleMoves[currentArrayPos] = targetSquare;
                        currentArrayPos++;
                    }
                }

                if (whiteCanCastleShort && tempTiles[position + 1] % 8 == -1 && tempTiles[position + 2] % 8 == -1)
                {
                    possibleMoves[currentArrayPos] = position + 2;
                    currentArrayPos++;
                }

                if (whiteCanCastleLong && tempTiles[position - 1] % 8 == -1 && tempTiles[position - 2] % 8 == -1 && tempTiles[position - 3] % 8 == -1)
                {
                    possibleMoves[currentArrayPos] = position - 2;
                    currentArrayPos++;
                }

                return possibleMoves;
            }
            else if (tempTiles[position] % 8 == 2) // Is white pawn
            {
                if (position < 55 && tempTiles[position + whitePawnTakesDirectionOffsets[1]] != -1 && (tempTiles[position + whitePawnTakesDirectionOffsets[1]] >> 3) != 1 && numSquaresToEdge[position, 3] != 0)
                {
                    possibleMoves[currentArrayPos] = position + whitePawnTakesDirectionOffsets[1];
                    currentArrayPos++;
                }

                if (position < 56 && tempTiles[position + whitePawnTakesDirectionOffsets[0]] != -1 && (tempTiles[position + whitePawnTakesDirectionOffsets[0]] >> 3) != 1 && numSquaresToEdge[position, 2] != 0)
                {
                    possibleMoves[currentArrayPos] = position + whitePawnTakesDirectionOffsets[0];
                    currentArrayPos++;
                }

                else
                {
                    for (int i = 0; i < whitePawnDirectionOffsets.Length; i++)
                    {
                        if (position < 56 && tempTiles[position + whitePawnDirectionOffsets[i]] == -1)
                        {
                            possibleMoves[currentArrayPos] = position + whitePawnDirectionOffsets[i];
                            currentArrayPos++;
                        }
                    }
                }

                return possibleMoves;
            }
            else if (tempTiles[position] % 8 == 3) // Is white knight
            {
                int y = position / 8;
                int x = position - y * 8;
                for (int i = 0; i < knightDirectionOffsets.Length; i++)
                {
                    int targetSquare = position + knightDirectionOffsets[i];

                    int knightSquareY = targetSquare / 8;
                    int knightSquareX = targetSquare - knightSquareY * 8;

                    if (targetSquare >= 0 && targetSquare <= 63 && (tempTiles[position + knightDirectionOffsets[i]] >> 3) != 1) // On board and isnt white
                    {
                        int maxCoordMoveDst = System.Math.Max(System.Math.Abs(x - knightSquareX), System.Math.Abs(y - knightSquareY));
                        if (maxCoordMoveDst == 2)
                        {
                            possibleMoves[currentArrayPos] = targetSquare;
                            currentArrayPos++;
                        }

                    }
                }

                return possibleMoves;
            }
            else if (tempTiles[position] % 8 == 4) // Is white bishop
            {
                for (int directionIndex = 4; directionIndex < 8; directionIndex++)
                {
                    for (int n = 0; n < numSquaresToEdge[position, directionIndex]; n++)
                    {
                        int targetSquare = position + directionOffsets[directionIndex] * (n + 1);

                        if ((tempTiles[targetSquare] >> 3) == 1) break; // Blocked by white piece

                        possibleMoves[currentArrayPos] = targetSquare; currentArrayPos++;

                        if ((tempTiles[targetSquare] >> 3) == 2) break; // Capture black piece
                    }
                }

                return possibleMoves;
            }
            else if (tempTiles[position] % 8 == 5) // Is white rook
            {
                for (int directionIndex = 0; directionIndex < 4; directionIndex++)
                {
                    for (int n = 0; n < numSquaresToEdge[position, directionIndex]; n++)
                    {
                        int targetSquare = position + directionOffsets[directionIndex] * (n + 1);
                        int pieceOnTargetSquare = tempTiles[targetSquare];

                        if ((tempTiles[targetSquare] >> 3) == 1) break; // Blocked by white piece

                        possibleMoves[currentArrayPos] = targetSquare; currentArrayPos++;

                        if ((tempTiles[targetSquare] >> 3) == 2) break; // Capture black piece
                    }
                }

                return possibleMoves;
            }
            else if (tempTiles[position] % 8 == 6) // Is white queen
            {
                for (int directionIndex = 0; directionIndex < 8; directionIndex++)
                {
                    for (int n = 0; n < numSquaresToEdge[position, directionIndex]; n++)
                    {
                        int targetSquare = position + directionOffsets[directionIndex] * (n + 1);
                        int pieceOnTargetSquare = tempTiles[targetSquare];

                        if ((tempTiles[targetSquare] >> 3) == 1) break; // Blocked by white piece

                        possibleMoves[currentArrayPos] = targetSquare; currentArrayPos++;

                        if ((tempTiles[targetSquare] >> 3) == 2) break; // Capture black piece
                    }
                }

                return possibleMoves;
            }
        }
        else if (!whiteToAttack) // Is black
        {
            if (tempTiles[position] % 8 == 1) // Is black king
            {
                for (int directionIndex = 0; directionIndex < 8; directionIndex++)
                {
                    int targetSquare = position + directionOffsets[directionIndex];

                    if (numSquaresToEdge[position, directionIndex] != 0 && (tempTiles[targetSquare] >> 3) != 2)
                    {
                        possibleMoves[currentArrayPos] = targetSquare;
                        currentArrayPos++;
                    }
                }

                if (blackCanCastleShort && tempTiles[position + 1] % 8 == -1 && tempTiles[position + 2] % 8 == -1)
                {
                    possibleMoves[currentArrayPos] = position + 2;
                    currentArrayPos++;
                }

                if (blackCanCastleLong && tempTiles[position - 1] % 8 == -1 && tempTiles[position - 2] % 8 == -1 && tempTiles[position - 3] % 8 == -1)
                {
                    possibleMoves[currentArrayPos] = position - 2;
                    currentArrayPos++;
                }

                return possibleMoves;
            }
            else if (tempTiles[position] % 8 == 2) // Is black pawn
            {
                if (position > 8 && tempTiles[position + blackPawnTakesDirectionOffsets[0]] != -1 && (tempTiles[position + blackPawnTakesDirectionOffsets[0]] >> 3) != 2 && numSquaresToEdge[position, 3] != 0)
                {
                    possibleMoves[currentArrayPos] = position + blackPawnTakesDirectionOffsets[0];
                    currentArrayPos++;
                }

                if (position > 8 && tempTiles[position + blackPawnTakesDirectionOffsets[1]] != -1 && (tempTiles[position + blackPawnTakesDirectionOffsets[1]] >> 3) != 2 && numSquaresToEdge[position, 2] != 0)
                {
                    possibleMoves[currentArrayPos] = position + blackPawnTakesDirectionOffsets[1];
                    currentArrayPos++;
                }

                else
                {
                    for (int i = 0; i < blackPawnDirectionOffsets.Length; i++)
                    {
                        if (position > 7 && tempTiles[position + blackPawnDirectionOffsets[i]] == -1)
                        {
                            possibleMoves[currentArrayPos] = position + blackPawnDirectionOffsets[i];
                            currentArrayPos++;
                        }
                    }
                }

                return possibleMoves;
            }
            else if (tempTiles[position] % 8 == 3) // Is black knight
            {
                int y = position / 8;
                int x = position - y * 8;
                for (int i = 0; i < knightDirectionOffsets.Length; i++)
                {
                    int targetSquare = position + knightDirectionOffsets[i];

                    int knightSquareY = targetSquare / 8;
                    int knightSquareX = targetSquare - knightSquareY * 8;

                    if (targetSquare >= 0 && targetSquare <= 63 && (tempTiles[position + knightDirectionOffsets[i]] >> 3) != 2) // On board and isnt black
                    {
                        int maxCoordMoveDst = System.Math.Max(System.Math.Abs(x - knightSquareX), System.Math.Abs(y - knightSquareY));
                        if (maxCoordMoveDst == 2)
                        {
                            possibleMoves[currentArrayPos] = targetSquare;
                            currentArrayPos++;
                        }

                    }
                }

                return possibleMoves;
            }
            else if (tempTiles[position] % 8 == 4) // Is black bishop
            {
                for (int directionIndex = 4; directionIndex < 8; directionIndex++)
                {
                    for (int n = 0; n < numSquaresToEdge[position, directionIndex]; n++)
                    {
                        int targetSquare = position + directionOffsets[directionIndex] * (n + 1);
                        int pieceOnTargetSquare = tempTiles[targetSquare];

                        if ((tempTiles[targetSquare] >> 3) == 2) break; // Blocked by black piece

                        possibleMoves[currentArrayPos] = targetSquare; currentArrayPos++;

                        if ((tempTiles[targetSquare] >> 3) == 1) break; // Capture white piece
                    }
                }

                return possibleMoves;
            }
            else if (tempTiles[position] % 8 == 5) // Is black rook
            {
                for (int directionIndex = 0; directionIndex < 4; directionIndex++)
                {
                    for (int n = 0; n < numSquaresToEdge[position, directionIndex]; n++)
                    {
                        int targetSquare = position + directionOffsets[directionIndex] * (n + 1);
                        int pieceOnTargetSquare = tempTiles[targetSquare];

                        if ((tempTiles[targetSquare] >> 3) == 2) break; // Blocked by black piece

                        possibleMoves[currentArrayPos] = targetSquare; currentArrayPos++;

                        if ((tempTiles[targetSquare] >> 3) == 1) break; // Capture white piece
                    }
                }

                return possibleMoves;
            }
            else if (tempTiles[position] % 8 == 6) // Is black queen
            {
                for (int directionIndex = 0; directionIndex < 8; directionIndex++)
                {
                    for (int n = 0; n < numSquaresToEdge[position, directionIndex]; n++)
                    {
                        int targetSquare = position + directionOffsets[directionIndex] * (n + 1);
                        int pieceOnTargetSquare = tempTiles[targetSquare];

                        if ((tempTiles[targetSquare] >> 3) == 2) break; // Blocked by black piece

                        possibleMoves[currentArrayPos] = targetSquare; currentArrayPos++;

                        if ((tempTiles[targetSquare] >> 3) == 1) break; // Capture white piece
                    }
                }

                return possibleMoves;
            }
        }
        possibleMoves[currentArrayPos] = -1;
        return possibleMoves;
    }

    public static int[] GetSquaresAttackedForStalemate(int position, bool whiteToAttack)
    {
        int currentArrayPos = 0;
        int[] possibleMoves = new int[27];
        for (int i = 0; i < possibleMoves.Length; i++)
        {
            possibleMoves[i] = -1;
        }

        if (whiteToAttack) // Is white
        {
            if (Tiles[position] % 8 == 1) // Is white king
            {
                for (int directionIndex = 0; directionIndex < 8; directionIndex++)
                {
                    int targetSquare = position + directionOffsets[directionIndex];

                    if (numSquaresToEdge[position, directionIndex] != 0 && (Tiles[targetSquare] >> 3) != 1)
                    {
                        possibleMoves[currentArrayPos] = targetSquare;
                        currentArrayPos++;
                    }
                }

                if (whiteCanCastleShort && Tiles[position + 1] % 8 == -1 && Tiles[position + 2] % 8 == -1)
                {
                    possibleMoves[currentArrayPos] = position + 2;
                    currentArrayPos++;
                }

                if (whiteCanCastleLong && Tiles[position - 1] % 8 == -1 && Tiles[position - 2] % 8 == -1 && Tiles[position - 3] % 8 == -1)
                {
                    possibleMoves[currentArrayPos] = position - 2;
                    currentArrayPos++;
                }

                return possibleMoves;
            }
            else if (Tiles[position] % 8 == 2) // Is white pawn
            {
                if (position < 55 && Tiles[position + whitePawnTakesDirectionOffsets[1]] != -1 && (Tiles[position + whitePawnTakesDirectionOffsets[1]] >> 3) != 1 && numSquaresToEdge[position, 3] != 0)
                {
                    possibleMoves[currentArrayPos] = position + whitePawnTakesDirectionOffsets[1];
                    currentArrayPos++;
                }

                if (position < 56 && Tiles[position + whitePawnTakesDirectionOffsets[0]] != -1 && (Tiles[position + whitePawnTakesDirectionOffsets[0]] >> 3) != 1 && numSquaresToEdge[position, 2] != 0)
                {
                    possibleMoves[currentArrayPos] = position + whitePawnTakesDirectionOffsets[0];
                    currentArrayPos++;
                }

                else
                {
                    for (int i = 0; i < whitePawnDirectionOffsets.Length; i++)
                    {
                        if (position < 56 && Tiles[position + whitePawnDirectionOffsets[i]] == -1)
                        {
                            possibleMoves[currentArrayPos] = position + whitePawnDirectionOffsets[i];
                            currentArrayPos++;
                        }
                    }
                }

                return possibleMoves;
            }
            else if (Tiles[position] % 8 == 3) // Is white knight
            {
                int y = position / 8;
                int x = position - y * 8;
                for (int i = 0; i < knightDirectionOffsets.Length; i++)
                {
                    int targetSquare = position + knightDirectionOffsets[i];

                    int knightSquareY = targetSquare / 8;
                    int knightSquareX = targetSquare - knightSquareY * 8;

                    if (targetSquare >= 0 && targetSquare <= 63 && (Tiles[position + knightDirectionOffsets[i]] >> 3) != 1) // On board and isnt white
                    {
                        int maxCoordMoveDst = System.Math.Max(System.Math.Abs(x - knightSquareX), System.Math.Abs(y - knightSquareY));
                        if (maxCoordMoveDst == 2)
                        {
                            possibleMoves[currentArrayPos] = targetSquare;
                            currentArrayPos++;
                        }

                    }
                }

                return possibleMoves;
            }
            else if (Tiles[position] % 8 == 4) // Is white bishop
            {
                for (int directionIndex = 4; directionIndex < 8; directionIndex++)
                {
                    for (int n = 0; n < numSquaresToEdge[position, directionIndex]; n++)
                    {
                        int targetSquare = position + directionOffsets[directionIndex] * (n + 1);

                        if ((Tiles[targetSquare] >> 3) == 1) break; // Blocked by white piece

                        possibleMoves[currentArrayPos] = targetSquare; currentArrayPos++;

                        if ((Tiles[targetSquare] >> 3) == 2) break; // Capture black piece
                    }
                }

                return possibleMoves;
            }
            else if (Tiles[position] % 8 == 5) // Is white rook
            {
                for (int directionIndex = 0; directionIndex < 4; directionIndex++)
                {
                    for (int n = 0; n < numSquaresToEdge[position, directionIndex]; n++)
                    {
                        int targetSquare = position + directionOffsets[directionIndex] * (n + 1);
                        int pieceOnTargetSquare = Tiles[targetSquare];

                        if ((Tiles[targetSquare] >> 3) == 1) break; // Blocked by white piece

                        possibleMoves[currentArrayPos] = targetSquare; currentArrayPos++;

                        if ((Tiles[targetSquare] >> 3) == 2) break; // Capture black piece
                    }
                }

                return possibleMoves;
            }
            else if (Tiles[position] % 8 == 6) // Is white queen
            {
                for (int directionIndex = 0; directionIndex < 8; directionIndex++)
                {
                    for (int n = 0; n < numSquaresToEdge[position, directionIndex]; n++)
                    {
                        int targetSquare = position + directionOffsets[directionIndex] * (n + 1);
                        int pieceOnTargetSquare = Tiles[targetSquare];

                        if ((Tiles[targetSquare] >> 3) == 1) break; // Blocked by white piece

                        possibleMoves[currentArrayPos] = targetSquare; currentArrayPos++;

                        if ((Tiles[targetSquare] >> 3) == 2) break; // Capture black piece
                    }
                }

                return possibleMoves;
            }
        }
        else if (!whiteToAttack) // Is black
        {
            if (Tiles[position] % 8 == 1) // Is black king
            {
                for (int directionIndex = 0; directionIndex < 8; directionIndex++)
                {
                    int targetSquare = position + directionOffsets[directionIndex];

                    if (numSquaresToEdge[position, directionIndex] != 0 && (Tiles[targetSquare] >> 3) != 2)
                    {
                        possibleMoves[currentArrayPos] = targetSquare;
                        currentArrayPos++;
                    }
                }

                if (blackCanCastleShort && Tiles[position + 1] % 8 == -1 && Tiles[position + 2] % 8 == -1)
                {
                    possibleMoves[currentArrayPos] = position + 2;
                    currentArrayPos++;
                }

                if (blackCanCastleLong && Tiles[position - 1] % 8 == -1 && Tiles[position - 2] % 8 == -1 && Tiles[position - 3] % 8 == -1)
                {
                    possibleMoves[currentArrayPos] = position - 2;
                    currentArrayPos++;
                }

                return possibleMoves;
            }
            else if (Tiles[position] % 8 == 2) // Is black pawn
            {
                if (position > 8 && Tiles[position + blackPawnTakesDirectionOffsets[0]] != -1 && (Tiles[position + blackPawnTakesDirectionOffsets[0]] >> 3) != 2 && numSquaresToEdge[position, 3] != 0)
                {
                    possibleMoves[currentArrayPos] = position + blackPawnTakesDirectionOffsets[0];
                    currentArrayPos++;
                }

                if (position > 8 && Tiles[position + blackPawnTakesDirectionOffsets[1]] != -1 && (Tiles[position + blackPawnTakesDirectionOffsets[1]] >> 3) != 2 && numSquaresToEdge[position, 2] != 0)
                {
                    possibleMoves[currentArrayPos] = position + blackPawnTakesDirectionOffsets[1];
                    currentArrayPos++;
                }

                else
                {
                    for (int i = 0; i < blackPawnDirectionOffsets.Length; i++)
                    {
                        if (position > 7 && Tiles[position + blackPawnDirectionOffsets[i]] == -1)
                        {
                            possibleMoves[currentArrayPos] = position + blackPawnDirectionOffsets[i];
                            currentArrayPos++;
                        }
                    }
                }

                return possibleMoves;
            }
            else if (Tiles[position] % 8 == 3) // Is black knight
            {
                int y = position / 8;
                int x = position - y * 8;
                for (int i = 0; i < knightDirectionOffsets.Length; i++)
                {
                    int targetSquare = position + knightDirectionOffsets[i];

                    int knightSquareY = targetSquare / 8;
                    int knightSquareX = targetSquare - knightSquareY * 8;

                    if (targetSquare >= 0 && targetSquare <= 63 && (Tiles[position + knightDirectionOffsets[i]] >> 3) != 2) // On board and isnt black
                    {
                        int maxCoordMoveDst = System.Math.Max(System.Math.Abs(x - knightSquareX), System.Math.Abs(y - knightSquareY));
                        if (maxCoordMoveDst == 2)
                        {
                            possibleMoves[currentArrayPos] = targetSquare;
                            currentArrayPos++;
                        }

                    }
                }

                return possibleMoves;
            }
            else if (Tiles[position] % 8 == 4) // Is black bishop
            {
                for (int directionIndex = 4; directionIndex < 8; directionIndex++)
                {
                    for (int n = 0; n < numSquaresToEdge[position, directionIndex]; n++)
                    {
                        int targetSquare = position + directionOffsets[directionIndex] * (n + 1);
                        int pieceOnTargetSquare = Tiles[targetSquare];

                        if ((Tiles[targetSquare] >> 3) == 2) break; // Blocked by black piece

                        possibleMoves[currentArrayPos] = targetSquare; currentArrayPos++;

                        if ((Tiles[targetSquare] >> 3) == 1) break; // Capture white piece
                    }
                }

                return possibleMoves;
            }
            else if (Tiles[position] % 8 == 5) // Is black rook
            {
                for (int directionIndex = 0; directionIndex < 4; directionIndex++)
                {
                    for (int n = 0; n < numSquaresToEdge[position, directionIndex]; n++)
                    {
                        int targetSquare = position + directionOffsets[directionIndex] * (n + 1);
                        int pieceOnTargetSquare = Tiles[targetSquare];

                        if ((Tiles[targetSquare] >> 3) == 2) break; // Blocked by black piece

                        possibleMoves[currentArrayPos] = targetSquare; currentArrayPos++;

                        if ((Tiles[targetSquare] >> 3) == 1) break; // Capture white piece
                    }
                }

                return possibleMoves;
            }
            else if (Tiles[position] % 8 == 6) // Is black queen
            {
                for (int directionIndex = 0; directionIndex < 8; directionIndex++)
                {
                    for (int n = 0; n < numSquaresToEdge[position, directionIndex]; n++)
                    {
                        int targetSquare = position + directionOffsets[directionIndex] * (n + 1);
                        int pieceOnTargetSquare = Tiles[targetSquare];

                        if ((Tiles[targetSquare] >> 3) == 2) break; // Blocked by black piece

                        possibleMoves[currentArrayPos] = targetSquare; currentArrayPos++;

                        if ((Tiles[targetSquare] >> 3) == 1) break; // Capture white piece
                    }
                }

                return possibleMoves;
            }
        }
        possibleMoves[currentArrayPos] = -1;
        return possibleMoves;
    }

    public static bool KingIsSafe(int startingIndex, int targetIndex)
    {
        Tiles.CopyTo(tempTiles, 0);

        if (tempTiles[startingIndex] % 8 == 2 && tempTiles[targetIndex] == -1 && whitesMove)
        {
            var movingPiece = tempTiles[startingIndex];

            tempTiles[startingIndex] = -1;

            tempTiles[targetIndex] = movingPiece;

            tempTiles[targetIndex - 8] = -1;
        }
        else if (tempTiles[startingIndex] % 8 == 2 && tempTiles[targetIndex] == -1 && !whitesMove)
        {
            var movingPiece = tempTiles[startingIndex];

            tempTiles[startingIndex] = -1;

            tempTiles[targetIndex] = movingPiece;

            tempTiles[targetIndex + 8] = -1;
        }
        else if (whiteCanCastleShort && tempTiles[startingIndex] % 8 == 1 && targetIndex == startingIndex + 2 && whitesMove)
        {
            var movingPiece = tempTiles[startingIndex];

            tempTiles[startingIndex] = -1;

            tempTiles[targetIndex] = movingPiece;


            var movingRook = tempTiles[startingIndex + 3];

            tempTiles[startingIndex + 3] = -1;

            tempTiles[startingIndex + 1] = movingRook;
        }
        else if (whiteCanCastleLong && tempTiles[startingIndex] % 8 == 1 && targetIndex == startingIndex - 2 && whitesMove)
        {
            var movingPiece = tempTiles[startingIndex];

            tempTiles[startingIndex] = -1;

            tempTiles[targetIndex] = movingPiece;


            var movingRook = tempTiles[startingIndex - 4];

            tempTiles[startingIndex - 4] = -1;

            tempTiles[startingIndex - 1] = movingRook;
        }
        else if (blackCanCastleShort && tempTiles[startingIndex] % 8 == 1 && targetIndex == startingIndex + 2 && !whitesMove)
        {
            var movingPiece = tempTiles[startingIndex];

            tempTiles[startingIndex] = -1;

            tempTiles[targetIndex] = movingPiece;


            var movingRook = tempTiles[startingIndex + 3];

            tempTiles[startingIndex + 3] = -1;

            tempTiles[startingIndex + 1] = movingRook;
        }
        else if (blackCanCastleLong && tempTiles[startingIndex] % 8 == 1 && targetIndex == startingIndex - 2 && !whitesMove)
        {
            var movingPiece = tempTiles[startingIndex];

            tempTiles[startingIndex] = -1;

            tempTiles[targetIndex] = movingPiece;


            var movingRook = tempTiles[startingIndex - 4];

            tempTiles[startingIndex - 4] = -1;

            tempTiles[startingIndex - 1] = movingRook;
        }
        else
        {
            var movingPiece = tempTiles[startingIndex];

            tempTiles[startingIndex] = -1;

            tempTiles[targetIndex] = movingPiece;
        }

        for (int i = 0; i < 64; i++)
        {
            if (whitesMove) // Check if black is attacking whites king
            {
                if ((tempTiles[i] >> 3) == 1 || tempTiles[i] == -1) // Piece we're looking at is white or blank
                {
                    continue;
                }
                else
                {
                    int[] squaresAttacked = GetSquaresAttacked(i, false);

                    for (int j = 0; j < squaresAttacked.Length; j++)
                    {
                        if (squaresAttacked[j] == -1)
                        {

                        }
                        else if (tempTiles[squaresAttacked[j]] >> 3 == 1 && tempTiles[squaresAttacked[j]] % 8 == 1)
                        {
                            return false;
                        }
                    }
                }
            }
            else if (!whitesMove) // Check if white is attacking blacks king
            {
                if ((tempTiles[i] >> 3) == 2 || tempTiles[i] == -1) // Piece we're looking at is white
                {
                    continue;
                }
                else
                {
                    int[] squaresAttacked = GetSquaresAttacked(i, true);

                    for (int j = 0; j < squaresAttacked.Length; j++)
                    {
                        if (squaresAttacked[j] == -1)
                        {

                        }
                        else if (tempTiles[squaresAttacked[j]] >> 3 == 2 && tempTiles[squaresAttacked[j]] % 8 == 1)
                        {
                            return false;
                        }
                    }
                }
            }
        }

        return true;
    }

    public static bool MovePiece(int startingIndex, int targetIndex)
    {
        if (startingIndex == -1) return false;

        int[] legalMoves = GetLegalMoves(startingIndex);

        bool valid = false;
        for (int i = 0; i < legalMoves.Length; i++)
        {
            if (legalMoves[i] == targetIndex) valid = true;
        }

        if (!valid) return false;


        if (Tiles[startingIndex] % 8 == 2 && targetIndex == startingIndex + 16 && whitesMove)
        {
            enPassentable = targetIndex;
        }
        else if (Tiles[startingIndex] % 8 == 2 && targetIndex == startingIndex - 16 && !whitesMove)
        {
            enPassentable = targetIndex;
        }
        else
        {
            enPassentable = -1;
        }

        if (targetIndex < 8 && Tiles[startingIndex] % 8 == 2)
        {
            var movingPiece = 22;

            Tiles[startingIndex] = -1;

            Tiles[targetIndex] = movingPiece;

            hasChanged = true;

            if (whitesMove) { whitesMove = false; }
            else { whitesMove = true; }
        }
        else if (targetIndex > 55 && Tiles[startingIndex] % 8 == 2)
        {
            var movingPiece = 14;

            Tiles[startingIndex] = -1;

            Tiles[targetIndex] = movingPiece;

            hasChanged = true;

            if (whitesMove) { whitesMove = false; }
            else { whitesMove = true; }
        }
        else if (Tiles[startingIndex] % 8 == 2 && Tiles[targetIndex] == -1 && whitesMove)
        {
            var movingPiece = Tiles[startingIndex];

            Tiles[startingIndex] = -1;

            Tiles[targetIndex] = movingPiece;

            Tiles[targetIndex - 8] = -1;

            hasChanged = true;

            if (whitesMove) { whitesMove = false; }
            else { whitesMove = true; }
        }
        else if (Tiles[startingIndex] % 8 == 2 && Tiles[targetIndex] == -1 && !whitesMove)
        {
            var movingPiece = Tiles[startingIndex];

            Tiles[startingIndex] = -1;

            Tiles[targetIndex] = movingPiece;

            Tiles[targetIndex + 8] = -1;

            hasChanged = true;

            if (whitesMove) { whitesMove = false; }
            else { whitesMove = true; }
        }
        else if (whiteCanCastleShort && Tiles[startingIndex] % 8 == 1 && targetIndex == startingIndex + 2 && whitesMove)
        {
            var movingPiece = Tiles[startingIndex];

            Tiles[startingIndex] = -1;

            Tiles[targetIndex] = movingPiece;


            var movingRook = Tiles[startingIndex +3];

            Tiles[startingIndex + 3] = -1;

            Tiles[startingIndex + 1] = movingRook;

            hasChanged = true;

            whiteCanCastleShort = false;
            whiteCanCastleLong = false;

            if (whitesMove) { whitesMove = false; }
            else { whitesMove = true; }
        }
        else if (whiteCanCastleLong && Tiles[startingIndex] % 8 == 1 && targetIndex == startingIndex - 2 && whitesMove)
        {
            var movingPiece = Tiles[startingIndex];

            Tiles[startingIndex] = -1;

            Tiles[targetIndex] = movingPiece;


            var movingRook = Tiles[startingIndex - 4];

            Tiles[startingIndex - 4] = -1;

            Tiles[startingIndex - 1] = movingRook;

            hasChanged = true;

            whiteCanCastleShort = false;
            whiteCanCastleLong = false;

            if (whitesMove) { whitesMove = false; }
            else { whitesMove = true; }
        }
        else if (blackCanCastleShort && Tiles[startingIndex] % 8 == 1 && targetIndex == startingIndex + 2 && !whitesMove)
        {
            var movingPiece = Tiles[startingIndex];

            Tiles[startingIndex] = -1;

            Tiles[targetIndex] = movingPiece;


            var movingRook = Tiles[startingIndex + 3];

            Tiles[startingIndex + 3] = -1;

            Tiles[startingIndex + 1] = movingRook;

            hasChanged = true;

            blackCanCastleShort = false;
            blackCanCastleLong = false;

            if (whitesMove) { whitesMove = false; }
            else { whitesMove = true; }
        }
        else if (blackCanCastleLong && Tiles[startingIndex] % 8 == 1 && targetIndex == startingIndex - 2 && !whitesMove)
        {
            var movingPiece = Tiles[startingIndex];

            Tiles[startingIndex] = -1;

            Tiles[targetIndex] = movingPiece;


            var movingRook = Tiles[startingIndex - 4];

            Tiles[startingIndex - 4] = -1;

            Tiles[startingIndex - 1] = movingRook;

            hasChanged = true;

            blackCanCastleShort = false;
            blackCanCastleLong = false;

            if (whitesMove) { whitesMove = false; }
            else { whitesMove = true; }
        }
        else
        {
            if (whitesMove && Tiles[startingIndex] % 8 == 1) whiteCanCastleShort = false; whiteCanCastleLong = false;
            if (!whitesMove && Tiles[startingIndex] % 8 == 1) blackCanCastleShort = false; blackCanCastleLong = false;
            if (whitesMove && Tiles[startingIndex] % 8 == 5 && startingIndex == 0) whiteCanCastleLong = false;
            if (whitesMove && Tiles[startingIndex] % 8 == 5 && startingIndex == 7) whiteCanCastleShort = false;
            if (!whitesMove && Tiles[startingIndex] % 8 == 5 && startingIndex == 56) blackCanCastleLong = false;
            if (!whitesMove && Tiles[startingIndex] % 8 == 5 && startingIndex == 63) blackCanCastleShort = false;
            if (targetIndex == 0) whiteCanCastleLong = false;
            if (targetIndex == 7) whiteCanCastleShort = false;
            if (targetIndex == 56) blackCanCastleLong = false;
            if (targetIndex == 63) blackCanCastleShort = false;

            var movingPiece = Tiles[startingIndex];

            Tiles[startingIndex] = -1;

            Tiles[targetIndex] = movingPiece;

            hasChanged = true;

            if (whitesMove) { whitesMove = false; }
            else { whitesMove = true; }
        }

        if (displayPosition != activePosition)
        {
            pastMoves.RemoveRange(displayPosition + 1, pastMoves.Count - displayPosition - 1);

            displayPosition += 1;
            activePosition = displayPosition;
        }
        else
        {
            activePosition += 1;
            displayPosition += 1;
        }

        pastMoves.Add(GenerateFEN());
        return true;
    }
}