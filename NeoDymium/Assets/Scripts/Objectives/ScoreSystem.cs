using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreSystem : MonoBehaviour
{
    public int totalScore;
    public TextMeshProUGUI displayScore;

    void Update() {
        displayScore.text = "Score: " + totalScore.ToString();
    }
}
