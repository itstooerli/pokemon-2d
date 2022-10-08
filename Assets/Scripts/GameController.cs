using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState { FreeRoam, Battle, Dialog, Menu, PartyScreen, Bag, Cutscene, Paused }

public class GameController : MonoBehaviour
{
    [SerializeField] PlayerController playerController;
    [SerializeField] BattleSystem battleSystem;
    [SerializeField] Camera worldCamera;
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] InventoryUI inventoryUI;
    
    GameState state;
    GameState stateBeforePause;

    MenuController menuController;

    public SceneDetails CurrentScene { get; private set; }
    public SceneDetails PreviousScene { get; private set; }

    public static GameController Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        
        menuController = GetComponent<MenuController>();

        // Disabling mouse in game
        // Cursor.lockState = CursorLockMode.Locked;
        // Cursor.visible = false;

        ConditionsDB.Init();
        PokemonDB.Init();
        MoveDB.Init();
    }

    // Start is called before the first frame update
    void Start()
    {
        battleSystem.OnBattleOver += EndBattle;

        partyScreen.Init();

        DialogManager.Instance.OnShowDialog += () =>
        {
            state = GameState.Dialog;
        };

        DialogManager.Instance.OnCloseDialog += () =>
        {
            if (state == GameState.Dialog)
                state = GameState.FreeRoam;
        };

        menuController.onBack += () =>
        {
            state = GameState.FreeRoam;
        };

        menuController.onMenuSelected += OnMenuSelected;
    }
    
    public void PauseGame(bool pause)
    {
        if (pause == true)
        {
            stateBeforePause = state;
            state = GameState.Paused;
        }
        else
        {
            state = stateBeforePause;
        }
    }

    public void StartBattle()
    {
        state = GameState.Battle;
        battleSystem.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);

        var playerParty = playerController.GetComponent<PokemonParty>();
        var wildPokemon = CurrentScene.GetComponent<MapArea>().GetRandomWildPokemon();
        
        var wildPokemonCopy = new Pokemon(wildPokemon.Base, wildPokemon.Level);
        battleSystem.StartBattle(playerParty, wildPokemonCopy);
    }

    TrainerController trainer;
    public void OnEnterTrainersView(TrainerController trainer)
    {
        state = GameState.Cutscene;
        StartCoroutine(trainer.TriggerTrainerBattle(playerController));
    }

    public void StartTrainerBattle(TrainerController trainer)
    {
        state = GameState.Battle;
        battleSystem.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);

        this.trainer = trainer;
        var playerParty = playerController.GetComponent<PokemonParty>();
        var trainerParty = trainer.GetComponent<PokemonParty>();

        battleSystem.StartTrainerBattle(playerParty, trainerParty);
    }

    void EndBattle(bool won)
    {
        if (trainer != null && won == true)
        {
            trainer.BattleLost();
            trainer = null;
        }

        state = GameState.FreeRoam;
        battleSystem.gameObject.SetActive(false);
        worldCamera.gameObject.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        if (state == GameState.FreeRoam)
        {
            playerController.HandleUpdate();

            if (Input.GetKeyDown(KeyCode.Return))
            {
                menuController.OpenMenu();
                state = GameState.Menu;
            }
        }
        else if (state == GameState.Battle)
        {
            battleSystem.HandleUpdate();
        }
        else if (state == GameState.Dialog)
        {
            DialogManager.Instance.HandleUpdate();
        }
        else if (state == GameState.Menu)
        {
            playerController.Character.Animator.IsMoving = false; // CUSTOM: Stop character animation when entering menu screen
            menuController.HandleUpdate();
        }
        else if (state == GameState.PartyScreen)
        {
            Action onSelected = () =>
            {
                // Go to Summary Screen?
            };

            Action onBack = () =>
            {
                /*
                partyScreen.gameObject.SetActive(false);
                // state = GameState.FreeRoam; // CUSTOM EXCLUSION: Allow to go back to menu selection after party screen
                menuController.OpenMenu(); // CUSTOM: Allow to go back to menu selection after party screen
                state = GameState.Menu; // CUSTOM: Allow to go back to menu selection after party screen
                */
                ExitFromSecondaryMenu(partyScreen.gameObject);
            };
            
            partyScreen.HandleUpdate(onSelected, onBack);
        }
        else if (state == GameState.Bag)
        {
            Action onBack = () =>
            {
                /*
                inventoryUI.gameObject.SetActive(false);
                // state = GameState.FreeRoam; // CUSTOM EXCLUSION: Allow to go back to menu selection after party screen
                menuController.OpenMenu(); // CUSTOM: Allow to go back to menu selection after party screen
                state = GameState.Menu; // CUSTOM: Allow to go back to menu selection after party screen
                */
                ExitFromSecondaryMenu(inventoryUI.gameObject);
            };

            inventoryUI.HandleUpdate(onBack);
        }
    }

    public void ExitFromSecondaryMenu(GameObject activeGameObject)
    {
        activeGameObject.SetActive(false);
        menuController.OpenMenu(); // CUSTOM: Allow to go back to menu selection after secondary menu
        state = GameState.Menu; // CUSTOM: Allow to go back to menu selection after secondary menu
    }

    public void SetCurrentScene(SceneDetails currScene)
    {
        PreviousScene = CurrentScene;
        CurrentScene = currScene;
    }

    /// <summary>
    /// Determines what should be done based on the item selected in the menu window
    /// </summary>
    /// <param name="selectedItem"></param>
    void OnMenuSelected(int selectedItem)
    {
        if (selectedItem == 0)
        {
            // Pokemon
            partyScreen.gameObject.SetActive(true);
            state = GameState.PartyScreen;
            // menuController.CloseMenu(); // CUSTOM EXCLUSION: Allow to go back to menu selection after party screen
        }
        else if (selectedItem == 1)
        {
            // Bag
            inventoryUI.gameObject.SetActive(true);
            state = GameState.Bag;
        }
        else if (selectedItem == 2)
        {
            // Save
            SavingSystem.i.Save("SaveSlot1");
            state = GameState.FreeRoam;
            menuController.CloseMenu(); // CUSTOM: Allow to go back to menu selection after party screen
        }
        else if (selectedItem == 3)
        {
            // Load
            SavingSystem.i.Load("SaveSlot1");
            state = GameState.FreeRoam;
            menuController.CloseMenu(); // CUSTOM: Allow to go back to menu selection after party screen
        }
        else if (selectedItem == 4)
        {
            // Exit : CUSTOM: Allow user to close menu without keyboard with this item
            state = GameState.FreeRoam;
            menuController.CloseMenu();
        }
    }
}
