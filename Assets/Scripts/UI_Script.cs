using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_Script : MonoBehaviour {

    public Slider blood_slider;
    public GameObject wizardGO;
    public GameObject menu;
    public GameObject gameOverScreen;
    public GameObject customLEvelEditor;
    public GameObject levelLength;
    public GameObject levelDifficulty;
    public TMP_InputField CustomLevelLength;
    public TMP_Dropdown CustomLevelDifficulty;

    Wizard wizard;
    TerrainGenerator generator;

    private void Awake()
    {
        wizard = wizardGO.GetComponent<Wizard>();
        generator = GameObject.Find("TerrainGenerator").GetComponent<TerrainGenerator>();
    }

    public void Refresh_menu()
    {
        var info = generator.InfoNextLevel();
        levelLength.GetComponent<TextMeshProUGUI>().text = info.Item1;
        levelDifficulty.GetComponent<TextMeshProUGUI>().text = info.Item2;
    }

    private void Start()
    {
        Refresh_menu();
    }

    // Update is called once per frame
    void Update () {
        blood_slider.value = wizard.health;
        if (wizard.health <= 0)
            GameOverSeqence();
	}

    public void StartLevel()
    {
        menu.SetActive(false);
        customLEvelEditor.SetActive(false);
        wizardGO.transform.position = new Vector3(1, 2, 0);
        wizardGO.SetActive(true);
        generator.GenerateNextLevel();
    }

    public void GameOverSeqence(bool won = false)
    {
        TerrainGenerator.Spawner.RemoveAllSpawners();
        wizard.gameObject.SetActive(false);
        int treasures = generator.GetCurrentLevel();
        gameOverScreen.SetActive(true);
        if (won)
            gameOverScreen.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "You managed to find all " + (treasures + 1) + " Treasures. You have won!";
        else
            if (treasures == 0)
            gameOverScreen.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "You managed to find no Treasures. Unfortunatly...";
        else if (treasures == 1)
            gameOverScreen.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "You managed to find just 1 Treasure";
        else
            gameOverScreen.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "You managed to find " + treasures + " Treasures. Congratulations!";
    }

    public void RestartGame()
    {
        wizard.health = 100;
        generator.ResetLevel();
        gameOverScreen.SetActive(false);
        menu.SetActive(true);
        Refresh_menu();
        GameObject.Find("EnemyPool").GetComponent<ObjectPool>().DeactivateAllObjects();
        GameObject.Find("DropsSpawner").GetComponent<ObjectPool>().DeactivateAllObjects();
        GameObject.Find("EnemyProjectileSpawner").GetComponent<ObjectPool>().DeactivateAllObjects();
        GameObject.Find("ProjectileSpawner").GetComponent<ObjectPool>().DeactivateAllObjects();
    }

    public void SetSeed(string arg0)
    {
        Debug.Log("Seed modefied: " + arg0);
        Random.InitState(int.Parse(arg0));
    }

    public void OpenCusomLevelEditor()
    {
        customLEvelEditor.SetActive(!customLEvelEditor.activeSelf);
    }

    public void StartCustomLevel()
    {
        menu.SetActive(false);
        customLEvelEditor.SetActive(false);
        wizardGO.transform.position = new Vector3(1, 2, 0);
        wizardGO.SetActive(true);
        int length = int.Parse(CustomLevelLength.text);
        int diff = CustomLevelDifficulty.value;
        generator.GenerateCustomLevel(length, diff);
    }

    public void ToggleSound()
    {
        if (AudioListener.volume == 1)
            AudioListener.volume = 0;
        else
            AudioListener.volume = 1;
    }
}
