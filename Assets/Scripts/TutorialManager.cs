using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour {
    public int currentTutorial, currentPromptNumber, currentMoveNumber, currentLoopNumber;
    public int maxLoop;
    public List<int> loopTutorials;
    public List<int> multipleMovesResetTutorials;
    public Hashtable resetTutorials;
    public List<int> oneMoveTutorials;
    public List<int> opponentMoveTutorials;
    public List<int> skipOpponentPromptTutorials;
    public string[] tutorialNames;
    public string[] tutorialFENs;
    public string[][] tutorialMoves;
    public string[][] tutorialPrompts;

    public void Start() {
        currentTutorial = -1;
        maxLoop = 4;
        loopTutorials = new() { 0, 1, 2, 3, 4 };
        resetTutorials = new() {
            { 7, 2 },
            { 9, 4 }
        };
        multipleMovesResetTutorials = new() { 7 };
        oneMoveTutorials = new() { 5, 6, 8, 10, 12, 13, 17 };
        opponentMoveTutorials = new() { 11, 14, 15, 16, 18 };
        skipOpponentPromptTutorials = new() { 14, 15, 16, 18 };
        tutorialNames = new string[]{
            "Rook Movement",
            "Bishop Movement",
            "Knight Movement",
            "Queen Movement",
            "King Movement",
            "Pawn Movement",
            "Pawn Capture",
            "Castling",
            "Checking the King",
            "Promotion",
            "Checkmate",
            "Queen is not always best",
            "Stalemate: Be careful!",
            "En-passant",
            "Good development: Pawns",
            "Good development: Knights",
            "Good development: Bishops",
            "King Safety",
            "Good development: Rooks"
        };
        tutorialFENs = new string[]{
            "8/8/8/8/4R3/8/8/8 w - - 0 1", // rook tutorial
            "8/8/8/8/4B3/8/8/8 w - - 0 1", // bishop tutorial
            "8/8/8/8/4N3/8/8/8 w - - 0 1", // knight tutorial
            "8/8/8/8/4Q3/8/8/8 w - - 0 1", // queen tutorial
            "8/8/8/8/4K3/8/8/8 w - - 0 1", // king tutorial
            "8/8/8/8/8/8/4P3/8 w - - 0 1", // pawn movement tutorial
            "8/8/8/3p4/4P3/8/8/8 w - - 0 1", // pawn capture tutorial
            "4k3/pppppppp/8/8/8/3P4/PPPQPPPP/R3K2R w KQ - 0 1", // castling tutorial
            "3kr3/1p6/2p5/8/2Q4P/3P4/8/K7 w - - 0 1", // check king with queen tutorial
            "8/3P4/8/8/8/8/8/8 w - - 0 1", // promotion tutorial
            "3k4/3p4/8/3Q4/8/8/8/K2R4 w - - 0 1", // checkmate king tutorial
            "8/1P1q4/2k5/8/8/8/2P5/RK6 w - - 0 1", // advanced promotion tutorial
            "k7/8/1q6/8/8/1Q6/8/3KR3 w - - 0 1", // stalemate tutorial
            "8/8/8/3pP3/8/8/8/8 w - d6 0 1", // en-passant tutorial
            "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1", // push 2 central pawns tutorial
            "rnbqkbnr/1pppppp1/8/p6p/3PP3/8/PPP2PPP/RNBQKBNR w KQkq - 0 1", // develop knights tutorial
            "r1bqkb1r/1pppppp1/n6n/p6p/3PP3/2N2N2/PPP2PPP/R1BQKB1R w KQkq - 0 1", // develop bishops tutorial
            "r1bqk2r/1pppppb1/n5pn/p6p/2BPPB2/2N2N2/PPP2PPP/R2QK2R w KQkq - 0 1", // castle king
            "r2qk2r/1bppppb1/np4pn/p6p/2BPPB2/2NQ1N2/PPP2PPP/R4RK1 w - - 0 1", // develop rooks
        };
        tutorialMoves = new string[][]{
            new string[0] {}, // just play around with the rook
            new string[0] {}, // just play around with the bishop
            new string[0] {}, // just play around with the knight
            new string[0] {}, // just play around with the queen
            new string[0] {}, // just play around with the king
            new string[0] {}, // just play around with the pawn
            new string[1] {"e4d5"}, // capture enemy pawn
            new string[2] {"e1g1", "e1c1"}, // castle short and long
            new string[1] {"c4d4"}, // check king in safe spot
            new string[0] {}, // just play around with promotion
            new string[1] {"d5d7"}, // checkmate king
            new string[3] {"b7b8N", "c6c7", "b8d7"}, // fork promotion
            new string[1] {"b3b6"}, // stalemate capture
            new string[1] {"e5d6"}, // en-passant capture
            new string[4] {"e2e4", "a7a5", "d2d4", "h7h5"}, // develop central pawns
            new string[4] {"g1f3", "g8h6", "b1c3", "b8a6"}, // develop knights
            new string[4] {"f1c4", "g7g6", "c1f4", "f8g7"}, // develop bishops
            new string[1] {"e1g1"}, // king to safety
            new string[4] {"f1e1", "a8a7", "a1d1", "h8h7"}, // develop rooks
        };
        tutorialPrompts = new string[][]{
            new string[3] {
                        "This is a rook, a powerful piece that moves in a straight line.\n\nGive it a try!",
                        "Good job, now try moving the rook vertically or horizontally to various spots!",
                        "Well done, you have learned how rooks move!"
                        },
            new string[3] {
                        "This is a bishop, a piece that is usually weaker than the rook.\n\nIt moves diagonally.\n\nGive it a try!",
                        "Good job, try moving the bishop on different diagonals!",
                        "Well done, you have learned how bishops move!"
                        },
            new string[3] {
                        "This is a knight, a piece that is usually weaker than the rook, but equally as valuable as a bishop.\n\nIt moves in an L shape, and can even jump over other pieces.\n\nGive it a try!",
                        "Good job, try jumping to different spots!",
                        "Well done, you have learned how knights move!"
                        },
            new string[3] {
                        "This is a queen, the most powerful piece.\n\nIt combines the movements of the rook and bishop, moving all over the board.\n\nGive it a try!",
                        "Good job, try moving the queen to different spots!",
                        "Well done, you have learned how queens move!"
                        },
            new string[3] {
                        "This is a king, the most important piece.\n\nSimilarly to the queen, it can move in any direction, but only one square.\n\nGive it a try!",
                        "Good job, try moving the king to different spots!",
                        "Well done, you have learned how the king moves!"
                        },
            new string[2] {
                        "This is a pawn, the weakest piece, but also the most versatile.\n\nIt can only move forward one square, but when it is on its starting square, it can move two.\n\nGive it a try!",
                        "Well done, you have learned how pawns move!"
                        },
            new string[2] {
                        "Pawns capture pieces in a different way, by moving diagonally!\n\nTry capturing the enemy pawn!",
                        "Well done, you have learned how pawns capture pieces!"
                        },
            new string[3] {
                        "Kings can also do a special move called castling.\n\nThis can be done by moving the king two squares, towards one of the rooks.\n\nRemember, there have to be no pieces in between AND the rook and king must not have moved at all in the curent game.\n\nGive it a try and castle your king towards the closer rook, a kingside castle!",
                        "Let's also try a queenside castle!",
                        "Well done, you have learned how to castle your King!"
                        },
            new string[2] {
                        "If a king is attacked by a piece, it is called a Check!\n\nThe king has to move to a safe spot if checked, or the attacking piece needs to be captured.\n\nTry checking the enemy king with your queen, in a way that your queen won't get captured by the black pawn.",
                        "Well done, you have learned how to check a King!"
                        },
            new string[3] {
                        "When pawns are one square away from the backrank, they can promote!\n\nPromotion means the pawn is replaced by a more powerful piece.\n\nGive it a try!",
                        "Try promoting into different pieces!",
                        "Well done, you have learned how to promote your pawns!"

                    },
            new string[2] {
                        "When a king is attacked and there is no safe spot to move, or a way to capture the attacking pieces, it is called Checkmate!\n\nTry checkmating the enemy king with your queen in one move, with the support of the rook!",
                        "Well done, you have learned how to Checkmate!"
                        },
            new string[4] {
                        "Promoting into a queen is not always the best move!\n\nIn this situation, what can the pawn promote into to attack both the black king and queen at the same time?\n\nHint: L shape",
                        "Good choice, the knight forks the king and queen!",
                        "Now take that queen!",
                        "Well done, you have learned when to underpromote!"
                        },
            new string[3] {
                        "Look! An undefended queen. Let's capture it!",
                        "But wait, look at the black king, it has no moves!\n\nThis is called Stalemate. When a player has no legal moves, but is not in check, the game ends in a draw.",
                        "Well done, you have learned about Stalemate!"
                        },
            new string[2] {
                        "Our opponent has just pushed their pawn two squares.\n\nYou can capture this pawn using your own pawn in a special way.\n\nBecause it moved two squares, landing right next to your pawn, you can pretend it only moved one square and capture it!\n\nThis is called en-passant.\n\nKeep in mind, this can be done only on this very turn, if the pawns remain unmoved in a future position, en-passant wouldn't be possible.\n\nGive it a try!",
                        "Well done, you have learned about En-passant!"
                        },
            new string[3] {
                        "Let's take some space in the center by pushing the pawn in front of the king two squares!",
                        "Looks like our opponent doesn't know fundamentals, let's push the pawn in front of our queen as well!",
                        "Well done, you control much more space and you can develop your pieces more easily!"
                        },
            new string[3] {
                        "Let's control more of the center by developing our kingside knight forward!",
                        "Looks like our opponent doesn't know fundamentals, let's develop our other knight as well!",
                        "Well done, the knights are nicely placed on f3 and c3, their best squares!"
                        },
            new string[3] {
                        "Let's develop our last minor pieces.\n\nMove that kingside bishop to the active c4 square!",
                        "Looks like our opponent doesn't know fundamentals, let's develop our other bishop as well!",
                        "Well done, the bishops are nicely placed on c4 and f4, controlling important diagonals!"
                        },
            new string[2] {
                        "It is important to keep our king safe.\n\nNow that we've developed our kingside pieces, let's castle short (kingside)",
                        "Well done, the king is now safely tucked in the corner and we can push our center pawns without fear!"
                        },
            new string[3] {
                        "Last but not least, the rooks need to be developed as well.\n\nLet's move our recently castled kingside rook behind the center pawn on e4!",
                        "Let's bring our other rook in the center as well, behind the pawn on d4!",
                        "Well done, the rooks nicely control the center and those 'e' and 'd' pawns can be pushed in the future!"
                        },
        };
    }
    public void StartTutorial(int tutorial) {
        GameStateManager.Instance.GenerateGameState(tutorialFENs[tutorial]);
        Game.Instance.CancelMovePiece();
        Game.Instance.DestroyPosition();
        Game.Instance.playerPerspective = "white";
        Game.Instance.GeneratePosition();
        Game.Instance.activeTutorial = true;
        Game.Instance.tutorialMoving = false;
        currentTutorial = tutorial;
        currentPromptNumber = 0;
        currentMoveNumber = 0;
        currentLoopNumber = 1;
        Game.Instance.prompt.text = "Tutorial: " + tutorialNames[tutorial] + "\n\n" + tutorialPrompts[tutorial][currentPromptNumber++];
        if (tutorialMoves[tutorial].Length > 0) {
            Game.Instance.tutorialMove = new Move(tutorialMoves[tutorial][currentMoveNumber++]);
        } else {
            Game.Instance.tutorialMove = null;
        }
    }
    public void ContinueTutorial() {
        if (loopTutorials.Contains(currentTutorial)) {
            if (currentLoopNumber == 1) {
                Game.Instance.prompt.text += "\n\n" + tutorialPrompts[currentTutorial][currentPromptNumber++];
            } else if (currentLoopNumber > maxLoop) {
                Game.Instance.prompt.text += "\n\n" + tutorialPrompts[currentTutorial][currentPromptNumber];
                Game.Instance.activeTutorial = false;
                Game.Instance.currentPlayer = '-';
                return;
            }
            ++currentLoopNumber;
            Game.Instance.currentPlayer = 'w';
            GameStateManager.Instance.globalGameState.whoMoves = 'w';
            Game.Instance.HandleGameState(GameStateManager.Instance.globalGameState, Game.Instance.gameStates);
        } else if (resetTutorials.Contains(currentTutorial)) {
            if (currentLoopNumber == 1) {
                Game.Instance.prompt.text += "\n\n" + tutorialPrompts[currentTutorial][currentPromptNumber++];
            } else if (currentLoopNumber >= (int)resetTutorials[currentTutorial]) {
                Game.Instance.prompt.text += "\n\n" + tutorialPrompts[currentTutorial][currentPromptNumber];
                Game.Instance.activeTutorial = false;
                Game.Instance.currentPlayer = '-';
                return;
            }
            if (multipleMovesResetTutorials.Contains(currentTutorial)) {
                Game.Instance.tutorialMove = new Move(tutorialMoves[currentTutorial][currentMoveNumber]);
                if (currentLoopNumber != 1) {
                    Game.Instance.prompt.text += "\n\n" + tutorialPrompts[currentTutorial][currentPromptNumber++];
                }
            }
            ++currentLoopNumber;
            StartCoroutine(ResetTutorialPosition(1f));
        } else if (oneMoveTutorials.Contains(currentTutorial)) {
            Game.Instance.prompt.text += "\n\n" + tutorialPrompts[currentTutorial][currentPromptNumber++];
            Game.Instance.activeTutorial = false;
            Game.Instance.currentPlayer = '-';
            return;
        } else if (opponentMoveTutorials.Contains(currentTutorial)) {
            if (!skipOpponentPromptTutorials.Contains(currentTutorial)) {
                Game.Instance.prompt.text += "\n\n" + tutorialPrompts[currentTutorial][currentPromptNumber++];
            }
            if (currentPromptNumber != tutorialPrompts[currentTutorial].Length) {
                Game.Instance.tutorialMoving = true;
                StartCoroutine(ContinueOpponentTutorial(2f));
            } else {
                Game.Instance.activeTutorial = false;
                Game.Instance.currentPlayer = '-';
                if (currentTutorial == tutorialNames.Length - 1) {
                    Game.Instance.prompt.text += "\n\nYou have completed all tutorials and are now ready to take on the AI opponents!\nFind them from the Main Menu!";
                }
                return;
            }
        }
    }
    private IEnumerator ResetTutorialPosition(float delay) {
        // Wait for the delay (in seconds)
        int localCurrentTutorial = currentTutorial;
        yield return new WaitForSeconds(delay);
        if (currentTutorial == localCurrentTutorial) { // still in the same tutorial
            GameStateManager.Instance.GenerateGameState(tutorialFENs[currentTutorial]);
            Game.Instance.CancelMovePiece();
            Game.Instance.DestroyPosition();
            Game.Instance.playerPerspective = "white";
            Game.Instance.GeneratePosition();
            Game.Instance.activeTutorial = true;
        }
    }
    private IEnumerator ContinueOpponentTutorial(float delay) {
        // Wait for the delay (in seconds)
        int localCurrentTutorial = currentTutorial;
        yield return new WaitForSeconds(delay);
        if (localCurrentTutorial == currentTutorial) { // tutorial was not changed
            Game.Instance.tutorialMove = new Move(tutorialMoves[currentTutorial][currentMoveNumber]);
            Game.Instance.MovePiece(new Move(tutorialMoves[currentTutorial][currentMoveNumber++]));
            if (currentMoveNumber < tutorialMoves[currentTutorial].Length) {
                Game.Instance.tutorialMove = new Move(tutorialMoves[currentTutorial][currentMoveNumber++]);
            }
            Game.Instance.prompt.text += "\n\n" + tutorialPrompts[currentTutorial][currentPromptNumber++];
            Game.Instance.tutorialMoving = false;
            if (currentPromptNumber == tutorialPrompts[currentTutorial].Length) {
                Game.Instance.activeTutorial = false;
                Game.Instance.currentPlayer = '-';
                if (currentTutorial == tutorialNames.Length - 1) {
                    yield return new WaitForSeconds(1f);
                    Game.Instance.prompt.text += "\n\nYou have completed all tutorials and are now ready to take on the AI opponents!\nFind them from the Main Menu!";
                }
            }
        }
    }
}
