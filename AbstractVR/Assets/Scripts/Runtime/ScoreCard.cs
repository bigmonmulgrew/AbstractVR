using TMPro;
using UnityEngine;

public class ScoreCard : MonoBehaviour
{
    public static ScoreCard Instance {  get; private set; }
    [SerializeField] TextMeshProUGUI scoreText;
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }
    public void ShowGameScore(int score)
    {
        scoreText.text = score.ToString();
    }
}
