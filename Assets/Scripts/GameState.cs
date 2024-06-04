using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameState {
    public char[,] boardConfiguration;
    public Hashtable whitePiecesPositions, blackPiecesPositions;
    public int noBlackPieces;
    public int noWhitePieces;
    public char whoMoves;
    public int moveCounter50Move;
    public int moveCounterFull;
    public char enPassantFile;
    public int enPassantRank;
    public int blackKingRow, blackKingColumn, whiteKingRow, whiteKingColumn;
    public bool canWhite_O_O, canWhite_O_O_O, canBlack_O_O, canBlack_O_O_O;

    // Default constructor
    public GameState() { }

    public GameState(GameState other) {
        boardConfiguration = new char[8, 8];
        Array.Copy(other.boardConfiguration, boardConfiguration, 64);
        noBlackPieces = other.noBlackPieces;
        noWhitePieces = other.noWhitePieces;
        whoMoves = other.whoMoves;
        moveCounter50Move = other.moveCounter50Move;
        moveCounterFull = other.moveCounterFull;
        enPassantFile = other.enPassantFile;
        enPassantRank = other.enPassantRank;
        blackKingRow = other.blackKingRow;
        blackKingColumn = other.blackKingColumn;
        whiteKingRow = other.whiteKingRow;
        whiteKingColumn = other.whiteKingColumn;
        canWhite_O_O = other.canWhite_O_O;
        canWhite_O_O_O = other.canWhite_O_O_O;
        canBlack_O_O = other.canBlack_O_O;
        canBlack_O_O_O = other.canBlack_O_O_O;
    }


    // we need to define the hashing function in order to store the gameState in a hashtable
    public override int GetHashCode() {
        int hash = whoMoves.GetHashCode();
        hash = (hash * 37) ^ enPassantFile.GetHashCode();
        hash = (hash * 37) ^ enPassantRank;
        hash = (hash * 37) ^ canWhite_O_O.GetHashCode();
        hash = (hash * 37) ^ canWhite_O_O_O.GetHashCode();
        hash = (hash * 37) ^ canBlack_O_O.GetHashCode();
        hash = (hash * 37) ^ canBlack_O_O_O.GetHashCode();

        // Efficiently hash the board configuration
        for (int i = 0; i < boardConfiguration.GetLength(0); i++) {
            for (int j = 0; j < boardConfiguration.GetLength(1); j++) {
                hash = (hash * 37) ^ boardConfiguration[i, j].GetHashCode();
            }
        }
        return hash;
    }

    // also need to define the "Equals" method in case of (hopefully not) hash collisions
    public override bool Equals(object obj) {
        if (obj is GameState other) {
            // Compare fields needed for equality: player to move, en-passant rights, castling rights, piece placement
            return whoMoves == other.whoMoves &&
                   enPassantFile == other.enPassantFile &&
                   enPassantRank == other.enPassantRank &&
                   canWhite_O_O == other.canWhite_O_O &&
                   canWhite_O_O_O == other.canWhite_O_O_O &&
                   canBlack_O_O == other.canBlack_O_O &&
                   canBlack_O_O_O == other.canBlack_O_O_O &&
                   AreBoardsEqual(boardConfiguration, other.boardConfiguration);
        }
        return false;
    }

    // check if two boards have the same piece placement
    public static bool AreBoardsEqual(char[,] board1, char[,] board2) {
        // Check if the dimensions are the same
        if (board1.GetLength(0) != board2.GetLength(0) || board1.GetLength(1) != board2.GetLength(1)) {
            return false;
        }

        // Compare each square
        for (int i = 0; i < board1.GetLength(0); i++) {
            for (int j = 0; j < board1.GetLength(1); j++) {
                if (board1[i, j] != board2[i, j]) {
                    return false;
                }
            }
        }

        // If all squares are the same, the boards are equal
        return true;
    }

    // update the game state after a piece has moved
    public void MakeMove(IndexMove indexMove) {

        // save the gameState data to help if we want to unmake the move
        indexMove.oldCanBlack_O_O = canBlack_O_O;
        indexMove.oldCanWhite_O_O = canWhite_O_O;
        indexMove.oldCanBlack_O_O_O = canBlack_O_O_O;
        indexMove.oldCanWhite_O_O_O = canWhite_O_O_O;
        indexMove.oldEnPassantFile = enPassantFile;
        indexMove.oldEnPassantRank = enPassantRank;
        indexMove.oldMoveCounter50Move = moveCounter50Move;
        indexMove.oldMoveCounterFull = moveCounterFull;

        int old_i, old_j, new_i, new_j;

        old_i = indexMove.oldRow;
        old_j = indexMove.oldColumn;
        new_i = indexMove.newRow;
        new_j = indexMove.newColumn;

        indexMove.oldSquareState = boardConfiguration[new_i, new_j];
        // we don't know yet if we will do an en-passant, castle, or move the king
        indexMove.oldIntermediarySquareState1 = 'x';
        indexMove.oldIntermediarySquareState2 = 'x';
        indexMove.kingMoved = false;

        if (whoMoves == 'w') {
            whitePiecesPositions.Remove(old_i * 8 + old_j);
            whitePiecesPositions.Add(new_i * 8 + new_j, 1);
        } else {
            blackPiecesPositions.Remove(old_i * 8 + old_j);
            blackPiecesPositions.Add(new_i * 8 + new_j, 1);
        }

        bool movingToEmptySquare = boardConfiguration[new_i, new_j] == '-';
        bool pawnMoved = char.ToLower(boardConfiguration[old_i, old_j]) == 'p';
        bool rookMoved = char.ToLower(boardConfiguration[old_i, old_j]) == 'r';
        bool kingMoved = char.ToLower(boardConfiguration[old_i, old_j]) == 'k';

        // capturing a piece or advancing a pawn
        if (!movingToEmptySquare || pawnMoved) {
            moveCounter50Move = 0;
        } else {
            ++moveCounter50Move;
        }

        // a piece was captured
        if (!movingToEmptySquare) {
            if (whoMoves == 'w') {
                --noBlackPieces;
                blackPiecesPositions.Remove(new_i * 8 + new_j);
            } else {
                --noWhitePieces;
                whitePiecesPositions.Remove(new_i * 8 + new_j);
            }
            // on black's backrank
            if (new_i == 0) {
                // possibly took black's rook on a8
                if (new_j == 0) {
                    canBlack_O_O_O = false;
                } else if (new_j == 7) { // possibly took black's rook on h8
                    canBlack_O_O = false;
                }
            } else if (new_i == 7) { // on white's backrank
                // possibly took white's rook on a1
                if (new_j == 0) {
                    canWhite_O_O_O = false;
                } else if (new_j == 7) { // possibly took white's rook on h1
                    canWhite_O_O = false;
                }
            }
        }

        bool moved2Ranks = Math.Abs(new_i - old_i) == 2;
        // we have a new en-passant target
        if (pawnMoved && moved2Ranks) {
            // first get the file and rank of the pawn
            enPassantFile = (char)(new_j + 'a');
            enPassantRank = 8 - new_i;
            // now change actual en-passant rank to be behind the pawn (depends on the pawn's color)
            if (whoMoves == 'w') {
                --enPassantRank;
            } else {
                ++enPassantRank;
            }
        } else { // previous en-passant is no longer valid and we don't have a new one
            enPassantFile = '*';
            enPassantRank = 0;
        }

        // move the piece on the new square
        boardConfiguration[new_i, new_j] = boardConfiguration[old_i, old_j];
        boardConfiguration[old_i, old_j] = '-';

        // a pawn was moved
        if (pawnMoved) {
            if (new_i == 0 || new_i == 7) { // pawn is promoting
                boardConfiguration[new_i, new_j] = indexMove.promotesInto;
            } else if (Math.Abs(new_j - old_j) == 1 && movingToEmptySquare) { // changed files and moved to an empty square
                // remove the pawn that was captured en-passant (its rank depends on who moves)
                if (whoMoves == 'w') {
                    indexMove.oldIntermediarySquareState1 = boardConfiguration[new_i + 1, new_j];
                    indexMove.intermediaryRow1 = new_i + 1;
                    indexMove.intermediaryColumn1 = new_j;
                    boardConfiguration[new_i + 1, new_j] = '-';
                    --noBlackPieces;
                    blackPiecesPositions.Remove((new_i + 1) * 8 + new_j);
                } else {
                    indexMove.oldIntermediarySquareState1 = boardConfiguration[new_i - 1, new_j];
                    indexMove.intermediaryRow1 = new_i - 1;
                    indexMove.intermediaryColumn1 = new_j;
                    boardConfiguration[new_i - 1, new_j] = '-';
                    --noWhitePieces;
                    whitePiecesPositions.Remove((new_i - 1) * 8 + new_j);
                }
            }
        }
        // a rook was moved, we may need to change castling rights
        if (rookMoved) {
            // from the backrank
            if (whoMoves == 'w' && old_i == 7) {
                // white's rook on a1
                if (old_j == 0) {
                    canWhite_O_O_O = false;
                } else if (old_j == 7) { // white's rook on h1
                    canWhite_O_O = false;
                }
            } else if (whoMoves == 'b' && old_i == 0) {
                // black's rook on a8
                if (old_j == 0) {
                    canBlack_O_O_O = false;
                } else if (old_j == 7) { // black's rook on h8
                    canBlack_O_O = false;
                }
            }
        }

        // king moved
        if (kingMoved) {
            indexMove.kingMoved = true;
            indexMove.oldKingRow = old_i;
            indexMove.oldKingColumn = old_j;
            // need to update king's position and remove castling rights for the current player
            if (whoMoves == 'w') {
                whiteKingRow = new_i;
                whiteKingColumn = new_j;
                canWhite_O_O = false;
                canWhite_O_O_O = false;
            } else {
                blackKingRow = new_i;
                blackKingColumn = new_j;
                canBlack_O_O = false;
                canBlack_O_O_O = false;
            }
            // the king just castled, so we need to move the corresponding rook as well
            if (Math.Abs(new_j - old_j) == 2) {
                // short castle
                if (new_j > old_j) {
                    // square where the rook will move
                    indexMove.oldIntermediarySquareState1 = '-';
                    indexMove.intermediaryRow1 = new_i;
                    indexMove.intermediaryColumn1 = new_j - 1;
                    boardConfiguration[new_i, new_j - 1] = boardConfiguration[new_i, new_j + 1];
                    // square where the rook originally was
                    indexMove.oldIntermediarySquareState2 = boardConfiguration[new_i, new_j + 1];
                    indexMove.intermediaryRow2 = new_i;
                    indexMove.intermediaryColumn2 = new_j + 1;
                    boardConfiguration[new_i, new_j + 1] = '-';
                    if (whoMoves == 'w') {
                        whitePiecesPositions.Remove(new_i * 8 + new_j + 1);
                        whitePiecesPositions.Add(new_i * 8 + new_j - 1, 1);
                    } else {
                        blackPiecesPositions.Remove(new_i * 8 + new_j + 1);
                        blackPiecesPositions.Add(new_i * 8 + new_j - 1, 1);
                    }
                } else { // long castle
                    // square where the rook will move
                    indexMove.oldIntermediarySquareState1 = '-';
                    indexMove.intermediaryRow1 = new_i;
                    indexMove.intermediaryColumn1 = new_j + 1;
                    boardConfiguration[new_i, new_j + 1] = boardConfiguration[new_i, new_j - 2];
                    // square where the rook originally was
                    indexMove.oldIntermediarySquareState2 = boardConfiguration[new_i, new_j - 2];
                    indexMove.intermediaryRow2 = new_i;
                    indexMove.intermediaryColumn2 = new_j - 2;
                    boardConfiguration[new_i, new_j - 2] = '-';
                    if (whoMoves == 'w') {
                        whitePiecesPositions.Remove(new_i * 8 + new_j - 2);
                        whitePiecesPositions.Add(new_i * 8 + new_j + 1, 1);
                    } else {
                        blackPiecesPositions.Remove(new_i * 8 + new_j - 2);
                        blackPiecesPositions.Add(new_i * 8 + new_j + 1, 1);
                    }
                }
            }
        }

        // update fullmove counter and swap players
        if (whoMoves == 'b') {
            ++moveCounterFull;
            whoMoves = 'w';
        } else {
            whoMoves = 'b';
        }
    }

    public void MakeMoveNoHashtable(IndexMove indexMove) {

        // save the gameState data to help if we want to unmake the move
        indexMove.oldCanBlack_O_O = canBlack_O_O;
        indexMove.oldCanWhite_O_O = canWhite_O_O;
        indexMove.oldCanBlack_O_O_O = canBlack_O_O_O;
        indexMove.oldCanWhite_O_O_O = canWhite_O_O_O;
        indexMove.oldEnPassantFile = enPassantFile;
        indexMove.oldEnPassantRank = enPassantRank;
        indexMove.oldMoveCounter50Move = moveCounter50Move;
        indexMove.oldMoveCounterFull = moveCounterFull;

        int old_i, old_j, new_i, new_j;

        old_i = indexMove.oldRow;
        old_j = indexMove.oldColumn;
        new_i = indexMove.newRow;
        new_j = indexMove.newColumn;

        indexMove.oldSquareState = boardConfiguration[new_i, new_j];
        // we don't know yet if we will do an en-passant, castle, or move the king
        indexMove.oldIntermediarySquareState1 = 'x';
        indexMove.oldIntermediarySquareState2 = 'x';
        indexMove.kingMoved = false;

        bool movingToEmptySquare = boardConfiguration[new_i, new_j] == '-';
        bool pawnMoved = char.ToLower(boardConfiguration[old_i, old_j]) == 'p';
        bool rookMoved = char.ToLower(boardConfiguration[old_i, old_j]) == 'r';
        bool kingMoved = char.ToLower(boardConfiguration[old_i, old_j]) == 'k';

        // capturing a piece or advancing a pawn
        if (!movingToEmptySquare || pawnMoved) {
            moveCounter50Move = 0;
        } else {
            ++moveCounter50Move;
        }

        // a piece was captured
        if (!movingToEmptySquare) {
            if (whoMoves == 'w') {
                --noBlackPieces;
            } else {
                --noWhitePieces;
            }
            // on black's backrank
            if (new_i == 0) {
                // possibly took black's rook on a8
                if (new_j == 0) {
                    canBlack_O_O_O = false;
                } else if (new_j == 7) { // possibly took black's rook on h8
                    canBlack_O_O = false;
                }
            } else if (new_i == 7) { // on white's backrank
                // possibly took white's rook on a1
                if (new_j == 0) {
                    canWhite_O_O_O = false;
                } else if (new_j == 7) { // possibly took white's rook on h1
                    canWhite_O_O = false;
                }
            }
        }

        bool moved2Ranks = Math.Abs(new_i - old_i) == 2;
        // we have a new en-passant target
        if (pawnMoved && moved2Ranks) {
            // first get the file and rank of the pawn
            enPassantFile = (char)(new_j + 'a');
            enPassantRank = 8 - new_i;
            // now change actual en-passant rank to be behind the pawn (depends on the pawn's color)
            if (whoMoves == 'w') {
                --enPassantRank;
            } else {
                ++enPassantRank;
            }
        } else { // previous en-passant is no longer valid and we don't have a new one
            enPassantFile = '*';
            enPassantRank = 0;
        }

        // move the piece on the new square
        boardConfiguration[new_i, new_j] = boardConfiguration[old_i, old_j];
        boardConfiguration[old_i, old_j] = '-';

        // a pawn was moved
        if (pawnMoved) {
            if (new_i == 0 || new_i == 7) { // pawn is promoting
                boardConfiguration[new_i, new_j] = indexMove.promotesInto;
            } else if (Math.Abs(new_j - old_j) == 1 && movingToEmptySquare) { // changed files and moved to an empty square
                // remove the pawn that was captured en-passant (its rank depends on who moves)
                if (whoMoves == 'w') {
                    indexMove.oldIntermediarySquareState1 = boardConfiguration[new_i + 1, new_j];
                    indexMove.intermediaryRow1 = new_i + 1;
                    indexMove.intermediaryColumn1 = new_j;
                    boardConfiguration[new_i + 1, new_j] = '-';
                    --noBlackPieces;
                } else {
                    indexMove.oldIntermediarySquareState1 = boardConfiguration[new_i - 1, new_j];
                    indexMove.intermediaryRow1 = new_i - 1;
                    indexMove.intermediaryColumn1 = new_j;
                    boardConfiguration[new_i - 1, new_j] = '-';
                    --noWhitePieces;
                }
            }
        }
        // a rook was moved, we may need to change castling rights
        if (rookMoved) {
            // from the backrank
            if (whoMoves == 'w' && old_i == 7) {
                // white's rook on a1
                if (old_j == 0) {
                    canWhite_O_O_O = false;
                } else if (old_j == 7) { // white's rook on h1
                    canWhite_O_O = false;
                }
            } else if (whoMoves == 'b' && old_i == 0) {
                // black's rook on a8
                if (old_j == 0) {
                    canBlack_O_O_O = false;
                } else if (old_j == 7) { // black's rook on h8
                    canBlack_O_O = false;
                }
            }
        }

        // king moved
        if (kingMoved) {
            indexMove.kingMoved = true;
            indexMove.oldKingRow = old_i;
            indexMove.oldKingColumn = old_j;
            // need to update king's position and remove castling rights for the current player
            if (whoMoves == 'w') {
                whiteKingRow = new_i;
                whiteKingColumn = new_j;
                canWhite_O_O = false;
                canWhite_O_O_O = false;
            } else {
                blackKingRow = new_i;
                blackKingColumn = new_j;
                canBlack_O_O = false;
                canBlack_O_O_O = false;
            }
            // the king just castled, so we need to move the corresponding rook as well
            if (Math.Abs(new_j - old_j) == 2) {
                // short castle
                if (new_j > old_j) {
                    // square where the rook will move
                    indexMove.oldIntermediarySquareState1 = '-';
                    indexMove.intermediaryRow1 = new_i;
                    indexMove.intermediaryColumn1 = new_j - 1;
                    boardConfiguration[new_i, new_j - 1] = boardConfiguration[new_i, new_j + 1];
                    // square where the rook originally was
                    indexMove.oldIntermediarySquareState2 = boardConfiguration[new_i, new_j + 1];
                    indexMove.intermediaryRow2 = new_i;
                    indexMove.intermediaryColumn2 = new_j + 1;
                    boardConfiguration[new_i, new_j + 1] = '-';
                } else { // long castle
                    // square where the rook will move
                    indexMove.oldIntermediarySquareState1 = '-';
                    indexMove.intermediaryRow1 = new_i;
                    indexMove.intermediaryColumn1 = new_j + 1;
                    boardConfiguration[new_i, new_j + 1] = boardConfiguration[new_i, new_j - 2];
                    // square where the rook originally was
                    indexMove.oldIntermediarySquareState2 = boardConfiguration[new_i, new_j - 2];
                    indexMove.intermediaryRow2 = new_i;
                    indexMove.intermediaryColumn2 = new_j - 2;
                    boardConfiguration[new_i, new_j - 2] = '-';
                }
            }
        }

        // update fullmove counter and swap players
        if (whoMoves == 'b') {
            ++moveCounterFull;
            whoMoves = 'w';
        } else {
            whoMoves = 'b';
        }
    }

    // revert the gameState to before the move was made
    public void UnmakeMove(IndexMove indexMove) {
        // restore data about move counters, castling rights, and en-passant
        moveCounter50Move = indexMove.oldMoveCounter50Move;
        moveCounterFull = indexMove.oldMoveCounterFull;
        canBlack_O_O = indexMove.oldCanBlack_O_O;
        canBlack_O_O_O = indexMove.oldCanBlack_O_O_O;
        canWhite_O_O = indexMove.oldCanWhite_O_O;
        canWhite_O_O_O = indexMove.oldCanWhite_O_O_O;
        enPassantFile = indexMove.oldEnPassantFile;
        enPassantRank = indexMove.oldEnPassantRank;

        // swap back players
        if (whoMoves == 'w') {
            whoMoves = 'b';
        } else {
            whoMoves = 'w';
        }

        // put the piece back to where it was moved from
        if (indexMove.promotesInto != '-') {
            // if it was a promotion turn it back into a pawn
            if (whoMoves == 'w') {
                boardConfiguration[indexMove.oldRow, indexMove.oldColumn] = 'P';
            } else {
                boardConfiguration[indexMove.oldRow, indexMove.oldColumn] = 'p';
            }
        } else {
            boardConfiguration[indexMove.oldRow, indexMove.oldColumn] = boardConfiguration[indexMove.newRow, indexMove.newColumn];
        }

        // restore hashtable of piece positions
        if (whoMoves == 'w') {
            whitePiecesPositions.Add(indexMove.oldRow * 8 + indexMove.oldColumn, 1);
            whitePiecesPositions.Remove(indexMove.newRow * 8 + indexMove.newColumn);
        } else {
            blackPiecesPositions.Add(indexMove.oldRow * 8 + indexMove.oldColumn, 1);
            blackPiecesPositions.Remove(indexMove.newRow * 8 + indexMove.newColumn);
        }
        // restore the square the piece moved to
        boardConfiguration[indexMove.newRow, indexMove.newColumn] = indexMove.oldSquareState;

        // we brought back a piece
        if (indexMove.oldSquareState != '-') {
            if (whoMoves == 'w') {
                ++noBlackPieces;
                blackPiecesPositions.Add(indexMove.newRow * 8 + indexMove.newColumn, 1);
            } else {
                ++noWhitePieces;
                whitePiecesPositions.Add(indexMove.newRow * 8 + indexMove.newColumn, 1);
            }
        }

        // restore the position of the king
        if (indexMove.kingMoved) {
            if (whoMoves == 'w') {
                whiteKingRow = indexMove.oldKingRow;
                whiteKingColumn = indexMove.oldKingColumn;
            } else {
                blackKingRow = indexMove.oldKingRow;
                blackKingColumn = indexMove.oldKingColumn;
            }
        }
        // restore 2 potential intermediate squares (for en-passant or castling moves)
        if (indexMove.oldIntermediarySquareState1 != 'x') {
            // restore the first intermediate square
            boardConfiguration[indexMove.intermediaryRow1, indexMove.intermediaryColumn1] = indexMove.oldIntermediarySquareState1;
            // this square was an enemy pawn captured by en-passant
            if (indexMove.oldIntermediarySquareState1 != '-') {
                // add the previously captured pawn back
                if (whoMoves == 'w') {
                    ++noBlackPieces;
                    blackPiecesPositions.Add(indexMove.intermediaryRow1 * 8 + indexMove.intermediaryColumn1, 1);
                } else {
                    ++noWhitePieces;
                    whitePiecesPositions.Add(indexMove.intermediaryRow1 * 8 + indexMove.intermediaryColumn1, 1);
                }
            } else { // we are dealing with castling
                // restore the second intermediate square (where the rook was placed)
                boardConfiguration[indexMove.intermediaryRow2, indexMove.intermediaryColumn2] = indexMove.oldIntermediarySquareState2;
                // restore the piece positions hashtable (for the position of castled rook)
                if (whoMoves == 'w') {
                    whitePiecesPositions.Add(indexMove.intermediaryRow2 * 8 + indexMove.intermediaryColumn2, 1);
                    whitePiecesPositions.Remove(indexMove.intermediaryRow1 * 8 + indexMove.intermediaryColumn1);
                } else {
                    blackPiecesPositions.Add(indexMove.intermediaryRow2 * 8 + indexMove.intermediaryColumn2, 1);
                    blackPiecesPositions.Remove(indexMove.intermediaryRow1 * 8 + indexMove.intermediaryColumn1);
                }
            }
        }
    }

    public void UnmakeMoveNoHashtable(IndexMove indexMove) {
        // restore data about move counters, castling rights, and en-passant
        moveCounter50Move = indexMove.oldMoveCounter50Move;
        moveCounterFull = indexMove.oldMoveCounterFull;
        canBlack_O_O = indexMove.oldCanBlack_O_O;
        canBlack_O_O_O = indexMove.oldCanBlack_O_O_O;
        canWhite_O_O = indexMove.oldCanWhite_O_O;
        canWhite_O_O_O = indexMove.oldCanWhite_O_O_O;
        enPassantFile = indexMove.oldEnPassantFile;
        enPassantRank = indexMove.oldEnPassantRank;

        // swap back players
        if (whoMoves == 'w') {
            whoMoves = 'b';
        } else {
            whoMoves = 'w';
        }

        // put the piece back to where it was moved from
        if (indexMove.promotesInto != '-') {
            // if it was a promotion turn it back into a pawn
            if (whoMoves == 'w') {
                boardConfiguration[indexMove.oldRow, indexMove.oldColumn] = 'P';
            } else {
                boardConfiguration[indexMove.oldRow, indexMove.oldColumn] = 'p';
            }
        } else {
            boardConfiguration[indexMove.oldRow, indexMove.oldColumn] = boardConfiguration[indexMove.newRow, indexMove.newColumn];
        }

        // restore the square the piece moved to
        boardConfiguration[indexMove.newRow, indexMove.newColumn] = indexMove.oldSquareState;

        // we brought back a piece
        if (indexMove.oldSquareState != '-') {
            if (whoMoves == 'w') {
                ++noBlackPieces;
            } else {
                ++noWhitePieces;
            }
        }

        // restore the position of the king
        if (indexMove.kingMoved) {
            if (whoMoves == 'w') {
                whiteKingRow = indexMove.oldKingRow;
                whiteKingColumn = indexMove.oldKingColumn;
            } else {
                blackKingRow = indexMove.oldKingRow;
                blackKingColumn = indexMove.oldKingColumn;
            }
        }
        // restore 2 potential intermediate squares (for en-passant or castling moves)
        if (indexMove.oldIntermediarySquareState1 != 'x') {
            // restore the first intermediate square
            boardConfiguration[indexMove.intermediaryRow1, indexMove.intermediaryColumn1] = indexMove.oldIntermediarySquareState1;
            // this square was an enemy pawn captured by en-passant
            if (indexMove.oldIntermediarySquareState1 != '-') {
                // add the previously captured pawn back
                if (whoMoves == 'w') {
                    ++noBlackPieces;
                } else {
                    ++noWhitePieces;
                }
            } else { // we are dealing with castling
                // restore the second intermediate square (where the rook was placed)
                boardConfiguration[indexMove.intermediaryRow2, indexMove.intermediaryColumn2] = indexMove.oldIntermediarySquareState2;
            }
        }
    }

    public void MakeMoveClean(IndexMove indexMove) {
        int old_i, old_j, new_i, new_j;

        old_i = indexMove.oldRow;
        old_j = indexMove.oldColumn;
        new_i = indexMove.newRow;
        new_j = indexMove.newColumn;

        bool movingToEmptySquare = boardConfiguration[new_i, new_j] == '-';
        bool pawnMoved = char.ToLower(boardConfiguration[old_i, old_j]) == 'p';
        bool rookMoved = char.ToLower(boardConfiguration[old_i, old_j]) == 'r';
        bool kingMoved = char.ToLower(boardConfiguration[old_i, old_j]) == 'k';

        // capturing a piece or advancing a pawn
        if (!movingToEmptySquare || pawnMoved) {
            moveCounter50Move = 0;
        } else {
            ++moveCounter50Move;
        }

        // a piece was captured
        if (!movingToEmptySquare) {
            if (whoMoves == 'w') {
                --noBlackPieces;
            } else {
                --noWhitePieces;
            }
            // on black's backrank
            if (new_i == 0) {
                // possibly took black's rook on a8
                if (new_j == 0) {
                    canBlack_O_O_O = false;
                } else if (new_j == 7) { // possibly took black's rook on h8
                    canBlack_O_O = false;
                }
            } else if (new_i == 7) { // on white's backrank
                // possibly took white's rook on a1
                if (new_j == 0) {
                    canWhite_O_O_O = false;
                } else if (new_j == 7) { // possibly took white's rook on h1
                    canWhite_O_O = false;
                }
            }
        }

        bool moved2Ranks = Math.Abs(new_i - old_i) == 2;
        // we have a new en-passant target
        if (pawnMoved && moved2Ranks) {
            // first get the file and rank of the pawn
            enPassantFile = (char)(new_j + 'a');
            enPassantRank = 8 - new_i;
            // now change actual en-passant rank to be behind the pawn (depends on the pawn's color)
            if (whoMoves == 'w') {
                --enPassantRank;
            } else {
                ++enPassantRank;
            }
        } else { // previous en-passant is no longer valid and we don't have a new one
            enPassantFile = '*';
            enPassantRank = 0;
        }

        // move the piece on the new square
        boardConfiguration[new_i, new_j] = boardConfiguration[old_i, old_j];
        boardConfiguration[old_i, old_j] = '-';

        // a pawn was moved
        if (pawnMoved) {
            if (new_i == 0 || new_i == 7) { // pawn is promoting
                boardConfiguration[new_i, new_j] = indexMove.promotesInto;
            } else if (Math.Abs(new_j - old_j) == 1 && movingToEmptySquare) { // changed files and moved to an empty square
                // remove the pawn that was captured en-passant (its rank depends on who moves)
                if (whoMoves == 'w') {
                    boardConfiguration[new_i + 1, new_j] = '-';
                    --noBlackPieces;
                } else {
                    boardConfiguration[new_i - 1, new_j] = '-';
                    --noWhitePieces;
                }
            }
        }
        // a rook was moved, we may need to change castling rights
        if (rookMoved) {
            // from the backrank
            if (whoMoves == 'w' && old_i == 7) {
                // white's rook on a1
                if (old_j == 0) {
                    canWhite_O_O_O = false;
                } else if (old_j == 7) { // white's rook on h1
                    canWhite_O_O = false;
                }
            } else if (whoMoves == 'b' && old_i == 0) {
                // black's rook on a8
                if (old_j == 0) {
                    canBlack_O_O_O = false;
                } else if (old_j == 7) { // black's rook on h8
                    canBlack_O_O = false;
                }
            }
        }

        // king moved
        if (kingMoved) {
            // need to update king's position and remove castling rights for the current player
            if (whoMoves == 'w') {
                whiteKingRow = new_i;
                whiteKingColumn = new_j;
                canWhite_O_O = false;
                canWhite_O_O_O = false;
            } else {
                blackKingRow = new_i;
                blackKingColumn = new_j;
                canBlack_O_O = false;
                canBlack_O_O_O = false;
            }
            // the king just castled, so we need to move the corresponding rook as well
            if (Math.Abs(new_j - old_j) == 2) {
                // short castle
                if (new_j > old_j) {
                    // square where the rook will move
                    boardConfiguration[new_i, new_j - 1] = boardConfiguration[new_i, new_j + 1];
                    // square where the rook originally was
                    boardConfiguration[new_i, new_j + 1] = '-';
                } else { // long castle
                    // square where the rook will move
                    boardConfiguration[new_i, new_j + 1] = boardConfiguration[new_i, new_j - 2];
                    // square where the rook originally was
                    boardConfiguration[new_i, new_j - 2] = '-';
                }
            }
        }

        // update fullmove counter and swap players
        if (whoMoves == 'b') {
            ++moveCounterFull;
            whoMoves = 'w';
        } else {
            whoMoves = 'b';
        }
    }

    public override string ToString() {
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < 8; i++) {
            string row = "";
            for (int j = 0; j < 8; j++) {
                char element = boardConfiguration[i, j];
                row += element + "  "; // Add two spaces after each character
            }
            sb.AppendLine(row.TrimEnd());
        }
        sb.AppendLine("Number of black pieces: " + noBlackPieces);

        var sortedKeys = new List<int>(blackPiecesPositions.Keys.Cast<int>());
        sortedKeys.Sort();
        foreach (int key in sortedKeys) {
            sb.Append(key + " ");
        }
        sb.AppendLine();
        sb.AppendLine("Number of white pieces: " + noWhitePieces);

        sortedKeys = new List<int>(whitePiecesPositions.Keys.Cast<int>());
        sortedKeys.Sort();
        foreach (int key in sortedKeys) {
            sb.Append(key + " ");
        }
        sb.AppendLine();
        sb.AppendLine("Current player: " + whoMoves);
        sb.AppendLine("White can" + (canWhite_O_O ? " " : "\'t ") + "short castle");
        sb.AppendLine("White can" + (canWhite_O_O_O ? " " : "\'t ") + "long castle");
        sb.AppendLine("Black can" + (canBlack_O_O ? " " : "\'t ") + "short castle");
        sb.AppendLine("Black can" + (canBlack_O_O_O ? " " : "\'t ") + "long castle");
        sb.AppendLine(enPassantRank == 0 ? "no en-passant available" : "en-passant at " + enPassantFile + enPassantRank);
        sb.AppendLine("50 move counter " + moveCounter50Move + " fullmove counter " + moveCounterFull);

        char whiteKingFile = (char)(whiteKingColumn + 'a');
        char blackKingFile = (char)(blackKingColumn + 'a');
        int whiteKingRank = 8 - whiteKingRow;
        int blackKingRank = 8 - blackKingRow;

        sb.AppendLine("white king at " + whiteKingFile + whiteKingRank + ", black king at " + blackKingFile + blackKingRank);

        return sb.ToString();
    }
}
