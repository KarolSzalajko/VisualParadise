﻿using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.Scripts
{
  public class NewGraphMenu : MonoBehaviour
  {
    public InputField GraphNameInput;
    public Button CreateButton;

    public void Start()
    {
      CreateButton.interactable = false;
    }

    public void Update()
    {
      //TODO: Consider better validation
      var isNameValid = !string.IsNullOrEmpty(GraphNameInput.text);
      CreateButton.interactable = isNameValid;
    }

    public void StartNew()
    {
      var graphName = GraphNameInput.text;
      var filePath = $"{Constants.GraphFolder}{graphName}.json";
      File.Create(filePath);
      PlayerPrefs.SetString(Constants.GraphFilePathKey, filePath);
      SceneManager.LoadScene(Constants.GameScene);
    }
  }
}
