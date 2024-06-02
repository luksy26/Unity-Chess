using System;

public class GameState {
    public char[,] boardConfiguration;
    public int noBlackPieces;
    public int noWhitePieces;
    public char whoMoves;
    public int moveCounter50Move;
    public int moveCounterFull;
    public char enPassantFile;
    public int enPassantRank;
    public int blackKingRow, blackKingColumn, whiteKingRow, whiteKingColumn;
    public bool white_O_O, white_O_O_O, black_O_O, black_O_O_O;

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
        white_O_O = other.white_O_O;
        white_O_O_O = other.white_O_O_O;
        black_O_O = other.black_O_O;
        black_O_O_O = other.black_O_O_O;
    }


    // we need to define the hashing function in order to store the gameState in a hashtable
    public override int GetHashCode() {
        int hash = whoMoves.GetHashCode();
        hash = (hash * 37) ^ enPassantFile.GetHashCode();
        hash = (hash * 37) ^ enPassantRank;
        hash = (hash * 37) ^ white_O_O.GetHashCode();
        hash = (hash * 37) ^ white_O_O_O.GetHashCode();
        hash = (hash * 37) ^ black_O_O.GetHashCode();
        hash = (hash * 37) ^ black_O_O_O.GetHashCode();

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
                   white_O_O == other.white_O_O &&
                   white_O_O_O == other.white_O_O_O &&
                   black_O_O == other.black_O_O &&
                   black_O_O_O == other.black_O_O_O &&
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
    public void MovePiece(IndexMove indexMove) {
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
                    black_O_O_O = false;
                } else if (new_j == 7) { // possibly took black's rook on h8
                    black_O_O = false;
                }
            } else if (new_i == 7) { // on white's backrank
                // possibly took white's rook on a1
                if (new_j == 0) {
                    white_O_O_O = false;
                } else if (new_j == 7) { // possibly took white's rook on h1
                    white_O_O = false;
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
                    white_O_O_O = false;
                } else if (old_j == 7) { // white's rook on h1
                    white_O_O = false;
                }
            } else if (whoMoves == 'b' && old_i == 0) {
                // black's rook on a8
                if (old_j == 0) {
                    black_O_O_O = false;
                } else if (old_j == 7) { // black's rook on h8
                    black_O_O = false;
                }
            }
        }

        // king moved
        if (kingMoved) {
            // need to update king's position and remove castling rights for the current player
            if (whoMoves == 'w') {
                whiteKingRow = new_i;
                whiteKingColumn = new_j;
                white_O_O = false;
                white_O_O_O = false;
            } else {
                blackKingRow = new_i;
                blackKingColumn = new_j;
                black_O_O = false;
                black_O_O_O = false;
            }
            // the king just castled, so we need to move the corresponding rook as well
            if (Math.Abs(new_j - old_j) == 2) {
                // short castle
                if (new_j > old_j) {
                    boardConfiguration[new_i, new_j - 1] = boardConfiguration[new_i, new_j + 1];
                    boardConfiguration[new_i, new_j + 1] = '-';
                } else { // long castle
                    boardConfiguration[new_i, new_j + 1] = boardConfiguration[new_i, new_j - 2];
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
        sb.AppendLine("Number of white pieces: " + noWhitePieces);
        sb.AppendLine("Current player: " + whoMoves);
        sb.AppendLine("White can" + (white_O_O ? " " : "\'t ") + "short castle");
        sb.AppendLine("White can" + (white_O_O_O ? " " : "\'t ") + "long castle");
        sb.AppendLine("Black can" + (black_O_O ? " " : "\'t ") + "short castle");
        sb.AppendLine("Black can" + (black_O_O_O ? " " : "\'t ") + "long castle");
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
