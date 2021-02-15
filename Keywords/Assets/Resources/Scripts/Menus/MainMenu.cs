using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour {
    private GameObject MainMenuUI;
    private GameObject CreditsUI;
    private bool creditsActive = false;
    private Button[] mainMenuButtons;

    private void Start() {
        MainMenuUI = transform.Find("MainMenu").gameObject;
        CreditsUI = transform.Find("CreditsMenu").gameObject;
        mainMenuButtons = MainMenuUI.GetComponentsInChildren<Button>();
    }

    public void ShowCredits() {
        MainMenuUI.SetActive(false);
        CreditsUI.SetActive(true);
        creditsActive = true;
    }

    public void ShowMainMenu() {
        CreditsUI.SetActive(false);
        MainMenuUI.SetActive(true);
        creditsActive = false;
    }

    public void ToggleCredits() {
        if (creditsActive) { ShowMainMenu(); } else { ShowCredits(); }
    }

    public void Play() {
        SceneManager.LoadScene("Versus");
    }

    public void TwoVTwo() {
        SceneManager.LoadScene("2v2");
    }

    public void Quit() {
        Application.Quit();
    }
}
