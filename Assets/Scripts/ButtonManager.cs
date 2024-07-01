using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using static PositionCounter;
using System;
using TMPro;

public class ButtonManager : MonoBehaviour {
    public Transform canvasTransform;
    public GameObject swapPerspectivePrefab, showHintPrefab, getSizeOfGameTreePrefab, startTutorialPrefab,
        evaluateEnginePrefab, getStaticPositionEvalPrefab, runTestsPrefab, getPositionEvalPrefab, 
        generatePositionPrefab, inputFieldPrefab, textPromptPrefab, mainMenuPrefab, soundPrefab, nextTutorialPrefab;
    public GameObject swapPerspectiveObject, showHintObject, getSizeOfGameTreeObject, startTutorialObject,
        evaluateEngineObject, getStaticPositionEvalObject, runTestsObject, getPositionEvalObject,
        generatePositionObject, inputFieldObject, textPromptObject, mainMenuObject, soundObject, nextTutorialObject;
    public int FENskipChunk;

    void Start() {
        CreateSwapPerspectiveButton();
        CreateShowHintButton();
        CreateGetSizeOfGameTreeButton();
        CreateStartTutorialButton();
        CreateEvaluateEngineButton();
        CreateGetStaticPositionEvalButton();
        CreateRunTestsButton();
        CreateGetPositionEvalButton();
        CreateGeneratePositionButton();
        CreateInputField();
        CreateTextPrompt();
        CreateMainMenuButton();
        CreateSoundButton();
        CreateNextTutorialButton();
        Game.Instance.salvageMove = true;
    }

    void CreateSwapPerspectiveButton() {
        swapPerspectiveObject = Instantiate(swapPerspectivePrefab, canvasTransform);

        RectTransform prefabRectTransform = swapPerspectivePrefab.GetComponent<RectTransform>();
        RectTransform newButtonRectTransform = swapPerspectiveObject.GetComponent<RectTransform>();

        if (prefabRectTransform != null && newButtonRectTransform != null) {
            newButtonRectTransform.localPosition = prefabRectTransform.localPosition;
            newButtonRectTransform.localRotation = prefabRectTransform.localRotation;
            newButtonRectTransform.localScale = prefabRectTransform.localScale;
        }
        swapPerspectiveObject.GetComponent<Button>().onClick.AddListener(OnSwapPerspectiveClicked);
    }

    void CreateShowHintButton() {
        showHintObject = Instantiate(showHintPrefab, canvasTransform);

        RectTransform prefabRectTransform = showHintPrefab.GetComponent<RectTransform>();
        RectTransform newButtonRectTransform = showHintObject.GetComponent<RectTransform>();

        if (prefabRectTransform != null && newButtonRectTransform != null) {
            newButtonRectTransform.localPosition = prefabRectTransform.localPosition;
            newButtonRectTransform.localRotation = prefabRectTransform.localRotation;
            newButtonRectTransform.localScale = prefabRectTransform.localScale;
        }
        showHintObject.GetComponent<Button>().onClick.AddListener(OnShowHintButtonClicked);
    }

    void CreateGetSizeOfGameTreeButton() {
        getSizeOfGameTreeObject = Instantiate(getSizeOfGameTreePrefab, canvasTransform);

        RectTransform prefabRectTransform = getSizeOfGameTreePrefab.GetComponent<RectTransform>();
        RectTransform newButtonRectTransform = getSizeOfGameTreeObject.GetComponent<RectTransform>();

        if (prefabRectTransform != null && newButtonRectTransform != null) {
            newButtonRectTransform.localPosition = prefabRectTransform.localPosition;
            newButtonRectTransform.localRotation = prefabRectTransform.localRotation;
            newButtonRectTransform.localScale = prefabRectTransform.localScale;
        }
        getSizeOfGameTreeObject.GetComponent<Button>().onClick.AddListener(OnGetSizeOfGameTreeButtonClicked);
    }

    void CreateStartTutorialButton() {
        startTutorialObject = Instantiate(startTutorialPrefab, canvasTransform);

        RectTransform prefabRectTransform = startTutorialPrefab.GetComponent<RectTransform>();
        RectTransform newButtonRectTransform = startTutorialObject.GetComponent<RectTransform>();

        if (prefabRectTransform != null && newButtonRectTransform != null) {
            newButtonRectTransform.localPosition = prefabRectTransform.localPosition;
            newButtonRectTransform.localRotation = prefabRectTransform.localRotation;
            newButtonRectTransform.localScale = prefabRectTransform.localScale;
        }
        startTutorialObject.GetComponent<Button>().onClick.AddListener(OnStartTutorialButtonClicked);
    }
    void CreateEvaluateEngineButton() {
        evaluateEngineObject = Instantiate(evaluateEnginePrefab, canvasTransform);

        RectTransform prefabRectTransform = evaluateEnginePrefab.GetComponent<RectTransform>();
        RectTransform newButtonRectTransform = evaluateEngineObject.GetComponent<RectTransform>();

        if (prefabRectTransform != null && newButtonRectTransform != null) {
            newButtonRectTransform.localPosition = prefabRectTransform.localPosition;
            newButtonRectTransform.localRotation = prefabRectTransform.localRotation;
            newButtonRectTransform.localScale = prefabRectTransform.localScale;
        }
        evaluateEngineObject.GetComponent<Button>().onClick.AddListener(OnEvaluateEngineButtonClicked);
    }

    void CreateGetStaticPositionEvalButton() {
        getStaticPositionEvalObject = Instantiate(getStaticPositionEvalPrefab, canvasTransform);

        RectTransform prefabRectTransform = getStaticPositionEvalPrefab.GetComponent<RectTransform>();
        RectTransform newButtonRectTransform = getStaticPositionEvalObject.GetComponent<RectTransform>();

        if (prefabRectTransform != null && newButtonRectTransform != null) {
            newButtonRectTransform.localPosition = prefabRectTransform.localPosition;
            newButtonRectTransform.localRotation = prefabRectTransform.localRotation;
            newButtonRectTransform.localScale = prefabRectTransform.localScale;
        }
        getStaticPositionEvalObject.GetComponent<Button>().onClick.AddListener(OnGetStaticPositionEvalClicked);
    }

    void CreateRunTestsButton() {
        runTestsObject = Instantiate(runTestsPrefab, canvasTransform);

        RectTransform prefabRectTransform = runTestsPrefab.GetComponent<RectTransform>();
        RectTransform newButtonRectTransform = runTestsObject.GetComponent<RectTransform>();

        if (prefabRectTransform != null && newButtonRectTransform != null) {
            newButtonRectTransform.localPosition = prefabRectTransform.localPosition;
            newButtonRectTransform.localRotation = prefabRectTransform.localRotation;
            newButtonRectTransform.localScale = prefabRectTransform.localScale;
        }
        runTestsObject.GetComponent<Button>().onClick.AddListener(OnRunTestsButtonClicked);
    }

    void CreateGetPositionEvalButton() {
        getPositionEvalObject = Instantiate(getPositionEvalPrefab, canvasTransform);

        RectTransform prefabRectTransform = getPositionEvalPrefab.GetComponent<RectTransform>();
        RectTransform newButtonRectTransform = getPositionEvalObject.GetComponent<RectTransform>();

        if (prefabRectTransform != null && newButtonRectTransform != null) {
            newButtonRectTransform.localPosition = prefabRectTransform.localPosition;
            newButtonRectTransform.localRotation = prefabRectTransform.localRotation;
            newButtonRectTransform.localScale = prefabRectTransform.localScale;
        }
        getPositionEvalObject.GetComponent<Button>().onClick.AddListener(OnGetPositionEvalClicked);
    }

    void CreateGeneratePositionButton() {
        generatePositionObject = Instantiate(generatePositionPrefab, canvasTransform);

        RectTransform prefabRectTransform = generatePositionPrefab.GetComponent<RectTransform>();
        RectTransform newButtonRectTransform = generatePositionObject.GetComponent<RectTransform>();

        if (prefabRectTransform != null && newButtonRectTransform != null) {
            newButtonRectTransform.localPosition = prefabRectTransform.localPosition;
            newButtonRectTransform.localRotation = prefabRectTransform.localRotation;
            newButtonRectTransform.localScale = prefabRectTransform.localScale;
        }
        generatePositionObject.GetComponent<Button>().onClick.AddListener(OnGeneratePositionButtonClicked);
    }

    void CreateInputField() {
        inputFieldObject = Instantiate(inputFieldPrefab, canvasTransform);

        RectTransform prefabRectTransform = inputFieldPrefab.GetComponent<RectTransform>();
        RectTransform newRectTransform = inputFieldObject.GetComponent<RectTransform>();

        if (prefabRectTransform != null && newRectTransform != null) {
            newRectTransform.localPosition = prefabRectTransform.localPosition;
            newRectTransform.localRotation = prefabRectTransform.localRotation;
            newRectTransform.localScale = prefabRectTransform.localScale;
        }
    }

    void CreateTextPrompt() {
        textPromptObject = Instantiate(textPromptPrefab, canvasTransform);

        RectTransform prefabRectTransform = textPromptPrefab.GetComponent<RectTransform>();
        RectTransform newRectTransform = textPromptObject.GetComponent<RectTransform>();

        if (prefabRectTransform != null && newRectTransform != null) {
            newRectTransform.localPosition = prefabRectTransform.localPosition;
            newRectTransform.localRotation = prefabRectTransform.localRotation;
            newRectTransform.localScale = prefabRectTransform.localScale;
        }
        Game.Instance.prompt = textPromptObject.GetComponent<TMP_Text>();
    }

    void CreateMainMenuButton() {
        mainMenuObject = Instantiate(mainMenuPrefab, canvasTransform);

        RectTransform prefabRectTransform = mainMenuPrefab.GetComponent<RectTransform>();
        RectTransform newRectTransform = mainMenuObject.GetComponent<RectTransform>();

        if (prefabRectTransform != null && newRectTransform != null) {
            newRectTransform.localPosition = prefabRectTransform.localPosition;
            newRectTransform.localRotation = prefabRectTransform.localRotation;
            newRectTransform.localScale = prefabRectTransform.localScale;
        }
        // TODO Add Listener
    }

    void CreateSoundButton() {
        soundObject = Instantiate(soundPrefab, canvasTransform);

        RectTransform prefabRectTransform = soundPrefab.GetComponent<RectTransform>();
        RectTransform newRectTransform = soundObject.GetComponent<RectTransform>();

        if (prefabRectTransform != null && newRectTransform != null) {
            newRectTransform.localPosition = prefabRectTransform.localPosition;
            newRectTransform.localRotation = prefabRectTransform.localRotation;
            newRectTransform.localScale = prefabRectTransform.localScale;
        }
        // TODO Add Listener
    }

    void CreateNextTutorialButton() {
        nextTutorialObject = Instantiate(nextTutorialPrefab, canvasTransform);

        RectTransform prefabRectTransform = nextTutorialPrefab.GetComponent<RectTransform>();
        RectTransform newRectTransform = nextTutorialObject.GetComponent<RectTransform>();

        if (prefabRectTransform != null && newRectTransform != null) {
            newRectTransform.localPosition = prefabRectTransform.localPosition;
            newRectTransform.localRotation = prefabRectTransform.localRotation;
            newRectTransform.localScale = prefabRectTransform.localScale;
        }
        // TODO Add Listener
    }

    void OnGeneratePositionButtonClicked() {
        string inputFEN = inputFieldObject.GetComponent<InputField>().text;
        if (!GameStateManager.Instance.IsEngineRunning) {
            Game.Instance.AIPlayer = '-';
            Game.Instance.playerPerspective = "white";
            Game.Instance.gameTreeMaxDepth = 5;
            Game.Instance.timeToMove = 5f;
            Game.Instance.timeNotExpired = true;
            Game.Instance.CancelMovePiece();
            GameStateManager.Instance.GenerateGameState(inputFEN);
            Game.Instance.DestroyPosition();
            Game.Instance.GeneratePosition();
        }
    }

    void OnGetSizeOfGameTreeButtonClicked() {
        Game.Instance.GetSizeOfGameTree();
    }

    async void OnEvaluateEngineButtonClicked() {
        FENskipChunk = 1;
        for (int i = 1; i <= 7; i += 6) {
            await EvaluateEngine("engineEvaluation" + i + ".txt", i);
        }
    }

    async Task EvaluateEngine(string outputFile, int AI) {
        Game.Instance.timeToMove = 5f;
        string filePath = Path.Combine(Application.streamingAssetsPath, "REF_Values.txt");
        string outPath = Path.Combine(Application.streamingAssetsPath, outputFile);
        if (File.Exists(filePath)) {
            using StreamReader reader = new(filePath);
            using StreamWriter writer = new(outPath, false);
            string line;
            int idx = 0;
            line = reader.ReadLine(); // skip the header
            float sum = 0;
            int nr = 0;
            float maxDiff = 0;
            while ((line = reader.ReadLine()) != null) {
                if (idx % FENskipChunk != 0) {
                    ++idx;
                    continue; // only process one portion of the data
                }
                string[] parts = line.Split(',');
                string fen = parts[0];
                string bestMove = parts[1];
                string evaluation = parts[2];

                int mateValue = 0;
                float evaluationScore = 0;

                string[] partsEvaluation = evaluation.Split(" ");
                if (partsEvaluation.Length > 1) {
                    mateValue = int.Parse(partsEvaluation[1]);
                } else {
                    evaluationScore = int.Parse(evaluation);
                }

                UnityEngine.Debug.Log("processing FEN " + fen);
                GameStateManager.Instance.GenerateGameState(fen);
                GameStateManager.Instance.IsEngineRunning = true;
                StartCoroutine(Game.Instance.MoveTimerCoroutine(Game.Instance.timeToMove));
                Game.Instance.timeNotExpired = true;
                MoveEval moveToMakeFound = null;
                MoveEval mandatoryMoveFound = null;
                int searchDepth = 1;
                while (true) {
                    MoveEval moveToMake = new();
                    MoveEval mandatoryMove = new() { move = new IndexMove(new Move(bestMove)), score = 10000 };
                    switch (AI) {
                        case 1: await Task.Run(() => moveToMake = AIv1.GetBestMove(GameStateManager.Instance.globalGameState, searchDepth, mandatoryMove)); break;
                        case 2: await Task.Run(() => moveToMake = AIv2.GetBestMove(GameStateManager.Instance.globalGameState, searchDepth, mandatoryMove)); break;
                        case 3: await Task.Run(() => moveToMake = AIv3.GetBestMove(GameStateManager.Instance.globalGameState, searchDepth, mandatoryMove, moveToMakeFound, Game.Instance.gameStates)); break;
                        case 4: await Task.Run(() => moveToMake = AIv4.GetBestMove(GameStateManager.Instance.globalGameState, searchDepth, mandatoryMove, moveToMakeFound, Game.Instance.gameStates)); break;
                        case 5: await Task.Run(() => moveToMake = AIv5.GetBestMove(GameStateManager.Instance.globalGameState, searchDepth, mandatoryMove, moveToMakeFound, Game.Instance.gameStates)); break;
                        case 6: await Task.Run(() => moveToMake = AIv6.GetBestMove(GameStateManager.Instance.globalGameState, searchDepth, mandatoryMove, moveToMakeFound, Game.Instance.gameStates)); break;
                        case 7: await Task.Run(() => moveToMake = AIv7.GetBestMove(GameStateManager.Instance.globalGameState, searchDepth, mandatoryMove, moveToMakeFound, Game.Instance.gameStates)); break;
                        default: break;
                    }
                    if (Game.Instance.timeNotExpired || (Game.Instance.salvageMove && Math.Abs(mandatoryMove.score) != 10000)) {
                        mandatoryMoveFound = mandatoryMove;
                        if (Game.Instance.timeNotExpired || (AI >= 3 && new Move(moveToMake.move).ToString().Equals(bestMove))) {
                            moveToMakeFound = moveToMake; // our new guaranteed best move 
                            /* 
                            either time is not expired (whole tree was generated), 
                            or it's the same as the mandatory move
                            (only for AIv3+, since AIv1 and AIv2 can't salvage moves for partial searches)
                            */
                        }
                        UnityEngine.Debug.Log("best move at depth " + searchDepth + " " + new Move(moveToMakeFound.move) +
                        " score: " + (Math.Abs(moveToMakeFound.score) > 950 ? "Mate in " +
                        (Math.Abs(Math.Abs(moveToMakeFound.score) - 1000) + Math.Abs(moveToMakeFound.score) % 2) / 2 : moveToMakeFound.score));

                        if (!Game.Instance.timeNotExpired) {
                            UnityEngine.Debug.Log("time expired while searching at depth" + searchDepth + ", but we salvaged the mandatory move");
                            break;
                        }
                    } else {
                        UnityEngine.Debug.Log("time expired while searching at depth" + searchDepth + ", and can't salvage a mandatory move");
                        break;
                    }
                    // now search deeper
                    ++searchDepth;
                }
                UnityEngine.Debug.Log("best move found in " + Game.Instance.timeToMove + "s " + new Move(moveToMakeFound.move) +
                    " score: " + (Math.Abs(moveToMakeFound.score) > 950 ? "Mate in " +
                    (Math.Abs(Math.Abs(moveToMakeFound.score) - 1000) + Math.Abs(moveToMakeFound.score) % 2) / 2 : moveToMakeFound.score));
                int foundMateValue = 0;
                float foundEvaluationScore = mandatoryMoveFound.score;
                if (Math.Abs(mandatoryMoveFound.score) > 950) {
                    foundMateValue = (int)((Math.Abs(Math.Abs(mandatoryMoveFound.score) - 1000) + Math.Abs(mandatoryMoveFound.score) % 2) / 2);
                }
                if (foundMateValue != 0) {
                    if (mandatoryMoveFound.score < 0) {
                        foundMateValue *= -1;
                    }
                }

                UnityEngine.Debug.Log("mandatory move found in " + Game.Instance.timeToMove + "s " + new Move(mandatoryMoveFound.move) +
                    " score: " + (Math.Abs(mandatoryMoveFound.score) > 950 ? "Mate in " +
                    (Math.Abs(Math.Abs(mandatoryMoveFound.score) - 1000) + Math.Abs(mandatoryMoveFound.score) % 2) / 2 : mandatoryMoveFound.score));
                GameStateManager.Instance.IsEngineRunning = false;
                if (GameStateManager.Instance.globalGameState.whoMoves == 'b') {
                    if (mateValue != 0) {
                        mateValue *= -1;
                    } else {
                        evaluationScore *= -1;
                    }
                }
                float trueDiff;
                if (mateValue != 0 && foundMateValue != 0) { // found the mate, it's fine if it's longer
                    trueDiff = 0;
                } else {
                    if (mateValue != 0) { // didn't find the mate, just assign a large enough advantage to diff with
                        if (mateValue < 0) {
                            evaluationScore = -15;
                        } else {
                            evaluationScore = 15;
                        }
                    } else {
                        evaluationScore /= 100f;
                    }
                    float diff = Math.Abs(foundEvaluationScore - evaluationScore);
                    if (Math.Abs(evaluationScore) <= 1 || Math.Abs(foundEvaluationScore) <= 1 || evaluationScore * foundEvaluationScore < 0) {
                        // one of the values is in [-1, 1] or they are on opposite sides (eg. -2, 4)
                        trueDiff = diff;
                    } else { // same side, need to scale the diff down depending on how far the smaller value is from 0
                        trueDiff = diff / Math.Min(Math.Abs(evaluationScore), Math.Abs(foundEvaluationScore));
                        // when the value closer to -1 or 1 approaches them, we are basically dividing by 1, which makes our function continuous
                    }
                }
                sum += trueDiff * trueDiff;
                ++nr;
                if (trueDiff > maxDiff) {
                    maxDiff = trueDiff;
                }
                writer.WriteLine("got " + new Move(mandatoryMoveFound.move) + " " + (foundMateValue != 0 ? "mate in " + foundMateValue : foundEvaluationScore) +
                    ", expected " + bestMove + " " + (mateValue != 0 ? "mate in " + mateValue : evaluationScore) +
                    " Diff is " + trueDiff);
                ++idx;
            }
            writer.WriteLine("Engine deviation: " + Math.Sqrt(sum / nr).ToString("F2"));
            writer.WriteLine("Max Diff " + maxDiff);
            UnityEngine.Debug.Log("Engine deviation: " + Math.Sqrt(sum / nr).ToString("F2"));
        } else {
            UnityEngine.Debug.Log("file not found");
        }
    }

    async void OnRunTestsButtonClicked() {
        string filePath = Path.Combine(Application.streamingAssetsPath, "perft.txt");
        string outPath = Path.Combine(Application.streamingAssetsPath, "outputChess.txt");
        if (File.Exists(filePath)) {
            using StreamReader reader = new(filePath);
            using StreamWriter writer = new(outPath, false);
            Stopwatch stopwatch = new();
            stopwatch.Start();
            string line;
            int idx = 0;
            FENskipChunk = 1; // make this larger to process less data (2 is 50% of the data, 10 is 10% of the data etc.)
            while ((line = reader.ReadLine()) != null) {
                if (idx % FENskipChunk != 0) {
                    ++idx;
                    continue; // only process one portion of the data
                }
                string[] parts = line.Split(',');
                string fen = parts[0];
                GameStateManager.Instance.GenerateGameState(fen);
                long[] legalMovesDepth = new long[7];
                for (int i = 1; i <= 6; ++i) {
                    legalMovesDepth[i] = long.Parse(parts[i]);
                }
                bool ok = true;
                for (int perftDepth = 1; perftDepth <= 4; ++perftDepth) { // increase upper limit for deeper searches
                    maxDepth = perftDepth;
                    long result = 0;
                    await Task.Run(() => result = SearchPositions(GameStateManager.Instance.globalGameState, 0));
                    if (result != legalMovesDepth[perftDepth]) {
                        ok = false;
                        writer.WriteLine("Incorrect results for depth 1: FEN: " + fen +
                            " ; expected " + legalMovesDepth[perftDepth] + " got " + result);
                        UnityEngine.Debug.Log("Incorrect results for depth 1: FEN: " + fen +
                            " ; expected " + legalMovesDepth[perftDepth] + " got " + result);
                    }
                }
                UnityEngine.Debug.Log(fen + "was checked, it is" + (ok ? "" : " not") + " ok");
                ++idx;
            }
            stopwatch.Stop();
            UnityEngine.Debug.Log("Ran tests in " + stopwatch.ElapsedMilliseconds + "ms");
        } else {
            UnityEngine.Debug.Log("file not found");
        }
    }

    void OnSwapPerspectiveClicked() {
        Game.Instance.SwapPerspectives();
    }

    void OnGetPositionEvalClicked() {
        Game.Instance.GetPositionEval();
    }

    void OnGetStaticPositionEvalClicked() {
        Game.Instance.GetStaticPositionEval();
    }
    void OnShowHintButtonClicked() {
        Game.Instance.ShowHint();
    }
    void OnStartTutorialButtonClicked() {
        int tutorial;
        if (inputFieldObject.GetComponent<InputField>().text.Equals("")) {
            tutorial = 0;
        } else {
            tutorial = int.Parse(inputFieldObject.GetComponent<InputField>().text);
        }
        Game.Instance.StartTutorial(tutorial);
    }
}
