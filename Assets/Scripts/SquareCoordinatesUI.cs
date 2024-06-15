using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SquareCoordinatesUI : MonoBehaviour {
    public GameObject textPrefab;
    public Canvas canvas;
    private readonly List<GameObject> files = new(), ranks = new();
    private readonly char[] fileChars = { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h' };
    private readonly char[] rankChars = { '1', '2', '3', '4', '5', '6', '7', '8' };
    public string[] colors = { "#2E1B11", "#FFFA91" }; // brown and yellow

    public void GenerateFilesAndRanks(string playerPerspective) {
        int startIdx = playerPerspective.Equals("white") ? 0 : 7, increment = playerPerspective.Equals("white") ? 1 : -1;

        // Generate files (a-h)
        for (int i = 0, idx = startIdx; i < 8; ++i, idx += increment) {
            GameObject fileText = Instantiate(textPrefab, canvas.transform);
            fileText.name = "File " + i;
            TextMeshProUGUI textComponent = fileText.GetComponent<TextMeshProUGUI>();
            textComponent.text = fileChars[idx].ToString();
            ColorUtility.TryParseHtmlString(colors[(i + 1) % 2], out Color neededColor);
            textComponent.color = neededColor;
            fileText.transform.position = new Vector3(-3.108f + i, -3.84f, -0.02f);
            files.Add(fileText);
        }

        // Generate ranks (1-8)
        for (int i = 0, idx = startIdx; i < 8; ++i, idx += increment) {
            GameObject rankText = Instantiate(textPrefab, canvas.transform);
            rankText.name = "Rank " + i;
            TextMeshProUGUI textComponent = rankText.GetComponent<TextMeshProUGUI>();
            textComponent.text = rankChars[idx].ToString();
            ColorUtility.TryParseHtmlString(colors[(i + 1) % 2], out Color neededColor);
            textComponent.color = neededColor;
            rankText.transform.position = new Vector3(-3.901f, -3.083f + i, -0.02f);
            ranks.Add(rankText);
        }
    }

    public void SwapPerspectivesForPieceCoordinates() {
        foreach (GameObject fileText in files) {
            TextMeshProUGUI textComponent = fileText.GetComponent<TextMeshProUGUI>();
            textComponent.text = ((char)('h' - textComponent.text[0] + 'a')).ToString();
        }
        foreach (GameObject rankText in ranks) {
            TextMeshProUGUI textComponent = rankText.GetComponent<TextMeshProUGUI>();
            textComponent.text = ((char)('8' - textComponent.text[0] + '1')).ToString();
        }
    }

    public void DestroyFilesAndRanks() {
        for (int i = 0; i < files.Count; ++i) {
            Destroy(files[i]);
            Destroy(ranks[i]);
        }
        files.Clear();
        ranks.Clear();
    }
}
