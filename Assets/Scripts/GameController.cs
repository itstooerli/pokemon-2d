using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState { FreeRoam, Battle, Dialog, Cutscene, Paused }

public class GameController : MonoBehaviour
{
    [SerializeField] PlayerController playerController;
    [SerializeField] BattleSystem battleSystem;
    [SerializeField] Camera worldCamera;
    
    GameState state;
    GameState stateBeforePause;

    public SceneDetails CurrentScene { get; private set; }
    public SceneDetails PreviousScene { get; private set; }

    public static GameController Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        ConditionsDB.Init();
    }

    // Start is called before the first frame update
    void Start()
    {
        battleSystem.OnBattleOver += EndBattle;

        DialogManager.Instance.OnShowDialog += () =>
        {
            state = GameState.Dialog;
        };

        DialogManager.Instance.OnCloseDialog += () =>
        {
            if (state == GameState.Dialog)
                state = GameState.FreeRoam;
        };
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
        }
        else if (state == GameState.Battle)
        {
            battleSystem.HandleUpdate();
        }
        else if (state == GameState.Dialog)
        {
            DialogManager.Instance.HandleUpdate();
        }
    }

    public void SetCurrentScene(SceneDetails currScene)
    {
        PreviousScene = CurrentScene;
        CurrentScene = currScene;
    }
}
