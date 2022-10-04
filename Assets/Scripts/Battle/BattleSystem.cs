using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public enum BattleState { Start, ActionSelection, MoveSelection, RunningTurn, Busy, PartyScreen, AboutToUse, BattleOver }
public enum BattleAction { Move, SwitchPokemon, UseItem, Run }

public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleDialogBox dialogBox;
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] Image playerImage;
    [SerializeField] Image trainerImage;
    [SerializeField] GameObject pokeballPrefab;

    public event Action<bool> OnBattleOver;

    BattleState state;
    BattleState? prevState;
    int currentAction;
    int currentMove;
    int currentMember;
    bool aboutToUseChoice = false;

    PokemonParty playerParty;
    PokemonParty trainerParty;
    Pokemon wildPokemon;

    bool isTrainerBattle = false;
    PlayerController player;
    TrainerController trainer;

    // Start is called before the first frame update
    public void StartBattle(PokemonParty playerParty, Pokemon wildPokemon)
    {
        isTrainerBattle = false;
        this.playerParty = playerParty;
        this.wildPokemon = wildPokemon;
        player = playerParty.GetComponent<PlayerController>();
        StartCoroutine(SetupBattle());
    }

    public void StartTrainerBattle(PokemonParty playerParty, PokemonParty trainerParty)
    {
        this.playerParty = playerParty;
        this.trainerParty = trainerParty;
        
        isTrainerBattle = true;
        player = playerParty.GetComponent<PlayerController>();
        trainer = trainerParty.GetComponent<TrainerController>();
        
        StartCoroutine(SetupBattle());
    }

    public IEnumerator SetupBattle()
    {
        playerUnit.Clear();
        enemyUnit.Clear();

        if (!isTrainerBattle)
        {
            // Wild Pokemon Battle
            playerUnit.Setup(playerParty.GetHealthyPokemon());
            enemyUnit.Setup(wildPokemon);

            dialogBox.SetMoveNames(playerUnit.Pokemon.Moves);
            yield return StartCoroutine(dialogBox.TypeDialog($"A wild {enemyUnit.Pokemon.Base.Name} appeared!"));
        }
        else
        {
            // Trainer Battle

            // Show player and trainer sprites
            playerUnit.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(false);

            playerImage.gameObject.SetActive(true);
            trainerImage.gameObject.SetActive(true);
            playerImage.sprite = player.Sprite;
            trainerImage.sprite = trainer.Sprite;
            
            // Show dialog
            yield return dialogBox.TypeDialog($"{trainer.Name} wants to battle!");

            // Send out first pokemon of the trainer
            trainerImage.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(true);
            var enemyPokemon = trainerParty.GetHealthyPokemon();
            enemyUnit.Setup(enemyPokemon);
            yield return dialogBox.TypeDialog($"{trainer.Name} sent out {enemyPokemon.Base.Name}!");

            // Send out first pokemon of the player
            playerImage.gameObject.SetActive(false);
            playerUnit.gameObject.SetActive(true);
            var playerPokmeon = playerParty.GetHealthyPokemon();
            playerUnit.Setup(playerPokmeon);
            yield return dialogBox.TypeDialog($"Go {playerPokmeon.Base.Name}!");
            dialogBox.SetMoveNames(playerUnit.Pokemon.Moves);
        }
        
        partyScreen.Init();
        ActionSelection();
    }

    void BattleOver(bool won)
    {
        state = BattleState.BattleOver;
        playerParty.Pokemons.ForEach(p => p.OnBattleOver());
        OnBattleOver(won); // Notifies GameController Battle is Over
    }

    void ActionSelection()
    {
        state = BattleState.ActionSelection;
        dialogBox.SetDialog("Choose an action");
        // StartCoroutine(dialogBox.TypeDialog("Choose an action")); // If desire to be animated
        dialogBox.EnableActionSelector(true);
    }

    void OpenPartyScreen()
    {
        state = BattleState.PartyScreen;
        partyScreen.SetPartyData(playerParty.Pokemons);
        partyScreen.gameObject.SetActive(true);
    }

    void MoveSelection()
    {
        state = BattleState.MoveSelection;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableMoveSelector(true);
    }

    IEnumerator AboutToUse(Pokemon newPokemon)
    {
        state = BattleState.Busy;
        yield return dialogBox.TypeDialog($"{trainer.Name} is about to use {newPokemon.Base.Name}. Do you want to change your pokemon?");
        
        state = BattleState.AboutToUse;
        dialogBox.EnableChoiceSelector(true);
    }

    IEnumerator RunTurns(BattleAction playerAction)
    {
        state = BattleState.RunningTurn;

        if (playerAction == BattleAction.Move)
        {
            playerUnit.Pokemon.CurrentMove = playerUnit.Pokemon.Moves[currentMove];
            enemyUnit.Pokemon.CurrentMove = enemyUnit.Pokemon.GetRandomMove();

            int playerMovePriority = playerUnit.Pokemon.CurrentMove.Base.Priority;
            int enemyMovePriority = enemyUnit.Pokemon.CurrentMove.Base.Priority;

            // Check which pokemon gets first move
            bool playerGoesFirst = true;

            if (enemyMovePriority > playerMovePriority)
            {
                playerGoesFirst = false;
            }
            else if (enemyMovePriority == playerMovePriority)
            {
                playerGoesFirst = playerUnit.Pokemon.Speed >= enemyUnit.Pokemon.Speed;
            }

            var firstUnit = (playerGoesFirst) ? playerUnit : enemyUnit;
            var secondUnit = (playerGoesFirst) ? enemyUnit : playerUnit;

            var secondPokemon = secondUnit.Pokemon;

            // First Move
            yield return RunMove(firstUnit, secondUnit, firstUnit.Pokemon.CurrentMove);
            yield return RunAfterTurn(firstUnit);
            if (state == BattleState.BattleOver) yield break;

            if (secondPokemon.HP > 0)
            {
                // Second Turn
                yield return RunMove(secondUnit, firstUnit, secondUnit.Pokemon.CurrentMove);
                yield return RunAfterTurn(secondUnit);
                if (state == BattleState.BattleOver) yield break;
            }
        }
        else
        {
            if (playerAction == BattleAction.SwitchPokemon)
            {
                var selectedPokemon = playerParty.Pokemons[currentMember];
                state = BattleState.Busy;
                yield return SwitchPokemon(selectedPokemon);
            }
            else if (playerAction == BattleAction.UseItem)
            {
                dialogBox.EnableActionSelector(false);
                yield return ThrowPokeball();
            }

            // Enemy Turn
            var enemyMove = enemyUnit.Pokemon.GetRandomMove();
            yield return RunMove(enemyUnit, playerUnit, enemyMove);
            yield return RunAfterTurn(enemyUnit);
            if (state == BattleState.BattleOver) yield break;
        }

        if (state != BattleState.BattleOver)
        {
            ActionSelection();
        }
    }

    IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Move move)
    {
        bool canRunMove = sourceUnit.Pokemon.OnBeforeMove();

        if (!canRunMove)
        {
            yield return ShowStatusChanges(sourceUnit.Pokemon);
            yield return sourceUnit.Hud.UpdateHP();
            yield break;
        }
        yield return ShowStatusChanges(sourceUnit.Pokemon);

        move.Pp--;
        yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name} used {move.Base.Name}!");

        if (CheckIfMoveHits(move, sourceUnit.Pokemon, targetUnit.Pokemon))
        {
            sourceUnit.PlayAttackAnimation();
            yield return new WaitForSeconds(1f);

            if (move.Base.Target != MoveTarget.Self)
                targetUnit.PlayHitAnimation();

            if (move.Base.Category == MoveCategory.Status)
            {
                yield return RunMoveEffects(move.Base.Effects, sourceUnit.Pokemon, targetUnit.Pokemon, move.Base.Target);
            }
            else
            {
                var damageDetails = targetUnit.Pokemon.TakeDamage(move, sourceUnit.Pokemon);
                yield return targetUnit.Hud.UpdateHP();
                yield return ShowDamageDetails(damageDetails);
            }

            if (move.Base.Secondaries != null && move.Base.Secondaries.Count > 0 && targetUnit.Pokemon.HP > 0)
            {
                foreach (var secondary in move.Base.Secondaries)
                {
                    var rand = UnityEngine.Random.Range(1, 101);

                    if (rand <= secondary.Chance)
                    {
                        yield return RunMoveEffects(secondary, sourceUnit.Pokemon, targetUnit.Pokemon, secondary.Target);
                    }
                }
            }

            if (targetUnit.Pokemon.HP <= 0)
            {
                yield return dialogBox.TypeDialog($"{targetUnit.Pokemon.Base.Name} fainted!");
                targetUnit.PlayFaintAnimation();
                yield return new WaitForSeconds(1.5f);

                CheckForBattleOver(targetUnit);
            }
        }
        else
        {
            yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name}'s attacked missed!");
        }
    }

    IEnumerator RunMoveEffects(MoveEffects effects, Pokemon source, Pokemon target, MoveTarget moveTarget)
    {
        // Stat Boosting
        if (effects.Boosts != null)
        {
            if (moveTarget == MoveTarget.Self)
            {
                source.ApplyBoosts(effects.Boosts);
            }
            else
            {
                target.ApplyBoosts(effects.Boosts);
            }
        }

        // Status Condition
        if (effects.Status != ConditionID.none)
        {
            target.SetStatus(effects.Status);
        }

        // Volatile Status Condition
        if (effects.VolatileStatus != ConditionID.none)
        {
            target.SetVolatileStatus(effects.VolatileStatus);
        }

        yield return ShowStatusChanges(source);
        yield return ShowStatusChanges(target);
    }

    IEnumerator RunAfterTurn(BattleUnit sourceUnit)
    {
        if (state == BattleState.BattleOver) yield break;
        yield return new WaitUntil(() => state == BattleState.RunningTurn);
        
        // Status effects like burn or poison will hurt the pokemon after the turn and could faint
        sourceUnit.Pokemon.OnAfterTurn();
        yield return ShowStatusChanges(sourceUnit.Pokemon);
        yield return sourceUnit.Hud.UpdateHP();

        if (sourceUnit.Pokemon.HP <= 0)
        {
            yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name} fainted!");
            sourceUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(1.5f);

            CheckForBattleOver(sourceUnit);
            yield return new WaitUntil(() => state == BattleState.RunningTurn);
        }
    }

    bool CheckIfMoveHits(Move move, Pokemon source, Pokemon target)
    {
        if (move.Base.AlwaysHits)
        {
            return true;
        }
        
        float moveAccuracy = move.Base.Accuracy;

        int accuracy = source.StatBoosts[Stat.Accuracy];
        int evasion = source.StatBoosts[Stat.Evasion];

        var boostValues = new float[] { 1f, 4f / 3f, 5f / 3f, 2f, 7f / 3f, 8f / 3f, 3f };

        if (accuracy > 0)
        {
            moveAccuracy *= boostValues[accuracy];
        }
        else
        {
            moveAccuracy /= boostValues[-accuracy];
        }

        if (evasion > 0)
        {
            moveAccuracy /= boostValues[evasion];
        }
        else
        {
            moveAccuracy *= boostValues[-evasion];
        }
        
        return UnityEngine.Random.Range(1, 101) <= moveAccuracy;
    }

    IEnumerator ShowStatusChanges(Pokemon pokemon)
    {
        while (pokemon.StatusChanges.Count > 0)
        {
            var message = pokemon.StatusChanges.Dequeue();
            yield return dialogBox.TypeDialog(message);
        }
    }

    void CheckForBattleOver(BattleUnit faintedUnit)
    {
        if (faintedUnit.IsPlayerUnit)
        {
            var nextPokemon = playerParty.GetHealthyPokemon();

            if (nextPokemon != null)
            {
                OpenPartyScreen();
            }
            else
            {
                BattleOver(false);
            }
        }
        else
        {
            if (!isTrainerBattle)
            {
                // Wild pokemon battle over
                BattleOver(true);
            }
            else
            {
                var nextPokemon = trainerParty.GetHealthyPokemon();
                if (nextPokemon != null)
                {
                    // Send out trainer's next pokemon
                    StartCoroutine(AboutToUse(nextPokemon));
                }
                else
                {
                    // Trainer lost
                    BattleOver(true);
                }
            }
        }
    }

    IEnumerator ShowDamageDetails(DamageDetails damageDetails)
    {
        if (damageDetails.Critical > 1f)
        {
            yield return dialogBox.TypeDialog("A critical hit!");
        }

        if (damageDetails.TypeEffectiveness > 1f)
        {
            yield return dialogBox.TypeDialog("It's super effective!");
        }
        else if (damageDetails.TypeEffectiveness < 1f)
        {
            yield return dialogBox.TypeDialog("It's not very effective...");
        }
    }

    // Update is called once per frame
    public void HandleUpdate()
    {
        if (state == BattleState.ActionSelection)
        {
            HandleActionSelection();
        }
        else if (state == BattleState.MoveSelection)
        {
            HandleMoveSelection();
        }
        else if (state == BattleState.PartyScreen)
        {
            HandlePartySelection();
        }
        else if (state == BattleState.AboutToUse)
        {
            HandleAboutToUse();
        }
    }

    void HandleActionSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ++currentAction;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            --currentAction;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentAction += 2;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentAction -= 2;
        }

        currentAction = Mathf.Clamp(currentAction, 0, 3);

        dialogBox.UpdateActionSelection(currentAction);

        if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            if (currentAction == 0)
            {
                // Fight
                MoveSelection();
            }
            else if (currentAction == 1) {
                // Bag
                StartCoroutine(RunTurns(BattleAction.UseItem));
            }
            else if (currentAction == 2)
            {
                // Pokemon
                prevState = state;
                OpenPartyScreen();
            }
            else if (currentAction == 3)
            {
                // Run
            }
        }
    }

    void HandleMoveSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ++currentMove;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            --currentMove;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentMove += 2;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentMove -= 2;
        }

        currentMove = Mathf.Clamp(currentMove, 0, playerUnit.Pokemon.Moves.Count - 1);

        dialogBox.UpdateMoveSelection(currentMove, playerUnit.Pokemon.Moves[currentMove]);

        if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Space) || (Input.GetKeyDown(KeyCode.Return)))
        {
            var move = playerUnit.Pokemon.Moves[currentMove];
            if (move.Pp == 0) return;

            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            StartCoroutine(RunTurns(BattleAction.Move));
        }
        else if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.Escape))
        {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            ActionSelection();
        }
    }

    void HandlePartySelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ++currentMember;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            --currentMember;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentMember += 2;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentMember -= 2;
        }

        currentMember = Mathf.Clamp(currentMember, 0, playerParty.Pokemons.Count - 1);

        partyScreen.UpdateMemberSelection(currentMember);

        if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Space) || (Input.GetKeyDown(KeyCode.Return)))
        {
            var selectedMember = playerParty.Pokemons[currentMember];
            if (selectedMember.HP <= 0)
            {
                partyScreen.SetMessageText("You cannot send out a fainted pokemon!");
                return;
            }

            if (selectedMember == playerUnit.Pokemon)
            {
                partyScreen.SetMessageText("You cannot switch to the same pokemon!");
                return;
            }

            partyScreen.gameObject.SetActive(false);

            if (prevState == BattleState.ActionSelection)
            {
                prevState = null;
                StartCoroutine(RunTurns(BattleAction.SwitchPokemon));
            }
            else
            {
                state = BattleState.Busy;
                StartCoroutine(SwitchPokemon(selectedMember));
            }
        }
        else if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.Escape))
        {
            if (playerUnit.Pokemon.HP <= 0)
            {
                partyScreen.SetMessageText("You have to choose a pokemon to continue!");
                return;
            }
            
            partyScreen.gameObject.SetActive(false);
            
            if (prevState == BattleState.AboutToUse)
            {
                prevState = null;
                StartCoroutine(SendNextTrainerPokmeon());
            }
            else
            {
                ActionSelection();
            }
        }
    }

    void HandleAboutToUse()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            aboutToUseChoice = !aboutToUseChoice;
        }

        dialogBox.UpdateChoiceSelection(aboutToUseChoice);

        if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            dialogBox.EnableChoiceSelector(false);
            if (aboutToUseChoice == true)
            {
                // Yes Option
                prevState = BattleState.AboutToUse;
                OpenPartyScreen();
            }
            else
            {
                // No Option
                StartCoroutine(SendNextTrainerPokmeon());
            }
        }
        else if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.Escape))
        {
            dialogBox.EnableChoiceSelector(false);
            StartCoroutine(SendNextTrainerPokmeon());
        }
    }

    IEnumerator SwitchPokemon(Pokemon newPokemon)
    {
        if (playerUnit.Pokemon.HP > 0)
        {
            yield return dialogBox.TypeDialog($"Come back {playerUnit.Pokemon.Base.Name}!");
            playerUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(2f);
        }

        playerUnit.Setup(newPokemon);
        dialogBox.SetMoveNames(newPokemon.Moves);
        yield return StartCoroutine(dialogBox.TypeDialog($"Go {newPokemon.Base.Name}!"));

        if (prevState == null)
        {
            state = BattleState.RunningTurn;
        }
        else if (prevState == BattleState.AboutToUse)
        {
            prevState = null;
            StartCoroutine(SendNextTrainerPokmeon());
        }
    }

    IEnumerator SendNextTrainerPokmeon()
    {
        state = BattleState.Busy;

        var nextPokemon = trainerParty.GetHealthyPokemon();
        enemyUnit.Setup(nextPokemon);
        yield return dialogBox.TypeDialog($"{trainer.Name} sent out {nextPokemon.Base.Name}!");
        state = BattleState.RunningTurn;
    }

    IEnumerator ThrowPokeball()
    {
        state = BattleState.Busy;

        if (isTrainerBattle)
        {
            yield return dialogBox.TypeDialog($"You can't steal the trainer's pokemon!");
            state = BattleState.RunningTurn;
            yield break;
        }

        yield return dialogBox.TypeDialog($"{player.Name} used POKEBALL!");

        // Instantiate
        var pokeballObj = Instantiate(pokeballPrefab, playerUnit.transform.position - new Vector3(2, 0), Quaternion.identity);
        var pokeball = pokeballObj.GetComponent<SpriteRenderer>();

        // Animations
        // Throw Ball
        yield return pokeball.transform.DOJump(enemyUnit.transform.position + new Vector3(0, 2), 2f, 1, 1f).WaitForCompletion();
        // Capture
        yield return enemyUnit.PlayCaptureAnimation();
        // Drop Ball
        yield return pokeball.transform.DOMoveY(enemyUnit.transform.position.y - 1.3f, 0.5f).WaitForCompletion();

        int shakeCount = TryToCatchPokemon(enemyUnit.Pokemon);
        
        // Shake
        for (int i = 0; i < Mathf.Min(3, shakeCount); ++i)
        {
            yield return new WaitForSeconds(0.5f);
            yield return pokeball.transform.DOPunchRotation(new Vector3(0, 0, 10f), 0.8f).WaitForCompletion();
        }

        if (shakeCount == 4)
        {
            // Pokemon is caught
            yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Base.Name} was caught!");
            yield return pokeball.DOFade(0, 1.5f).WaitForCompletion();

            playerParty.AddPokemon(enemyUnit.Pokemon);
            yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Base.Name} has been added to your party.");

            Destroy(pokeballObj);
            BattleOver(true);
        }
        else
        {
            // Pokemon broke out
            yield return new WaitForSeconds(1f);
            pokeball.DOFade(0, 0.2f);
            yield return enemyUnit.PlayBreakoutAnimation();
            
            if (shakeCount < 2)
            {
                yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Base.Name} broke free!");
            }
            else
            {
                yield return dialogBox.TypeDialog($"Almost caught it!");
            }

            Destroy(pokeballObj);
            state = BattleState.RunningTurn;
        }

    }

    int TryToCatchPokemon(Pokemon pokemon)
    {
        float a = (3 * pokemon.MaxHp - 2 * pokemon.HP) * pokemon.Base.CatchRate * ConditionsDB.GetStatusBonus(pokemon.Status) / (3 * pokemon.HP);

        if (a >= 255)
            return 4;

        float b = 1048560 / Mathf.Sqrt(Mathf.Sqrt(16711680 / a));

        int shakeCount = 0;

        while (shakeCount < 4)
        {
            if (UnityEngine.Random.Range(0, 65535) >= b)
                break;

            ++shakeCount;
        }

        return shakeCount;
    }
}
