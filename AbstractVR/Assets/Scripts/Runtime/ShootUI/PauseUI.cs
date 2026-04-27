using UnityEngine;

public class PauseUI: MonoBehaviour
{
    GameObject pauseMenu;
    GameObject feedbackMenu;

    private void Awake()
    {
        pauseMenu = this.gameObject;    // Storing jsut for easy workding.
        feedbackMenu = GetComponentInChildren<FeedbackUI>().gameObject;

        pauseMenu.SetActive(false);
        feedbackMenu.SetActive(false);
    }
    private void OnEnable()
    {
        PauseManager.Instance.OnGamePaused += OnGamePaused;
        PauseManager.Instance.OnGameUnpaused += OnGameResumed;
        FeedbackUI.Instance.OnFeedbackSent += OnFeedbackSent;
    }
    void OnGamePaused()
    {
        pauseMenu?.SetActive(true);
    }
    void OnGameResumed()
    {
        pauseMenu?.SetActive(false);
        feedbackMenu?.SetActive(false);
    }
    void OnFeedbackSent() 
    {
        feedbackMenu?.SetActive(false);
        pauseMenu?.SetActive(true);
    }
    void ShowFeedbackMenu()
    {
        feedbackMenu.SetActive(true);
        pauseMenu.SetActive(false);
    }
    #region UI Events
    public void HitResumeGame()
    {
        PauseManager.Instance.SetPaused(false);
    }
    public void HitGiveFeedback()
    {
        ShowFeedbackMenu();
    }
    #endregion
    
}
