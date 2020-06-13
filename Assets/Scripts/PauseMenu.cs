﻿using System.IO;
using Assets.Scripts.Common;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts
{
  public class PauseMenu : MonoBehaviour
  {
    GraphService graphService;
    public GameObject pauseMenu;
    public GameObject saveDialog;

    bool _isSaved = false;

    public void Start()
    {
      pauseMenu.SetActive(false);
      saveDialog.SetActive(false);
      graphService = FindObjectOfType<GraphService>();
    }

    public void SaveButton_OnClick()
    {
      Save();
    }

    public void ResumeButton_OnClick() => GameService.Instance.GlobalUnPauseGame();

    public void QuitButton_OnClick()
    {
      if (_isSaved)
        Quit();
      else
      {
        pauseMenu.SetActive(false);
        saveDialog.SetActive(true);
      }
    }

    public void SaveDialogYes_OnClick()
    {
      Save();
      Quit();
    }

    public void SaveDialogNo_OnClick()
    {
      Quit();
    }

    public void SaveDialogCancel_OnClick()
    {
      pauseMenu.SetActive(true);
      saveDialog.SetActive(false);
    }

    public void ShowMenu()
    {
      _isSaved = false;
      pauseMenu.SetActive(true);
    }

    public void HideMenu()
    {
      pauseMenu.SetActive(false);
      saveDialog.SetActive(false);
    }

    void Quit()
    {
      SceneManager.LoadScene(Constants.MainMenuScene);
    }

    void Save()
    {
      var filePath = PlayerPrefs.GetString(Constants.GraphFilePathKey);
      var graph = graphService.Graph;
      var json = JsonUtility.ToJson(graph, true);
      File.WriteAllText(filePath, json);
      Toast.Instance.Show("Graph saved successfully!", 4f, Toast.ToastColor.Green);
      _isSaved = true;
    }
  }
}
