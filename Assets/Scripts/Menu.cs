﻿using UnityEngine;

namespace Assets.Scripts
{
  public class Menu : MonoBehaviour
  {
    public GameObject mainMenu;
    public GameObject loadMenu;
    public GameObject newGraphMenu;

    // Start is called before the first frame update
    void Start()
    {
      LoadMainMenu();
    }

    public void LoadMainMenu()
    {
      mainMenu.SetActive(true);
      loadMenu.SetActive(false);
      newGraphMenu.SetActive(false);
    }

    public void NewGraphButton_OnClick()
    {
      newGraphMenu.SetActive(true);
      mainMenu.SetActive(false);
    }

    public void LoadButton_OnClick()
    {
      mainMenu.SetActive(false);
      loadMenu.SetActive(true);
    }

    public void QuitButton_OnClick()
    {
      Application.Quit();
    }
  }
}
