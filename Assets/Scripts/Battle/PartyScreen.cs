using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PartyScreen : MonoBehaviour
{
    [SerializeField] GameObject menu;
    [SerializeField] Text messageText;
    
    PartyMemberUI[] memberSlots;
    List<Pokemon> pokemons;
    PokemonParty playerParty;

    public event Action<int> onMiniMenuSelected;

    List<Text> menuItems;

    int currentAction = 0;
    int currentSelection;
    int swappedSelection;

    public GameObject Menu => menu;
    public Pokemon SelectedMember => pokemons[currentSelection];

    /// <summary>
    /// Party screen can be called from different states like ActionSelection, RunningTurn, AboutToUse
    /// </summary>
    public BattleState? CalledFrom { get; set; }

    private void Awake()
    {
        menuItems = menu.GetComponentsInChildren<Text>().ToList();
    }

    public void Init()
    {
        memberSlots = GetComponentsInChildren<PartyMemberUI>(true);
        playerParty = PokemonParty.GetPlayerParty();
        SetPartyData();
        playerParty.OnUpdated += SetPartyData;
    }

    public void SetPartyData()
    {
        pokemons = playerParty.Pokemons;
        
        for (int i = 0; i < memberSlots.Length; i++)
        {
            if (i < pokemons.Count)
            {
                memberSlots[i].gameObject.SetActive(true);
                memberSlots[i].Init(pokemons[i]);
            }
            else
            {
                memberSlots[i].gameObject.SetActive(false);
            }
        }

        UpdateMemberSelection(currentSelection);

        ResetMessageText();
    }

    public void HandleUpdate(Action onSelected, Action onBack)
    {
        var prevSelection = currentSelection;
        
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ++currentSelection;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            --currentSelection;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentSelection += 2;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentSelection -= 2;
        }

        currentSelection = Mathf.Clamp(currentSelection, 0, pokemons.Count - 1);

        if (currentSelection != prevSelection)
            UpdateMemberSelection(currentSelection);

        if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Space) || (Input.GetKeyDown(KeyCode.Return)))
        {
            onSelected?.Invoke();
        }
        else if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.Escape))
        {
            onBack?.Invoke();
        }
    }

    public void UpdateMemberSelection(int selectedMember)
    {
        for (int i = 0; i < pokemons.Count; i++)
        {
            if (i == selectedMember)
            {
                memberSlots[i].SetSelected(true);
            }
            else
            {
                memberSlots[i].SetSelected(false);
            }
        }
    }

    /// <summary>
    /// Below is all custom code to handle party swapping
    /// </summary>
    public void OpenMiniMenu()
    {
        menu.SetActive(true);
        UpdateActionSelection(currentAction);
    }

    public void CloseMiniMenu()
    {
        menu.SetActive(false);
    }


    public void HandlePokemonSelection(Action onBack)
    {
        Menu.SetActive(true);
        var prevAction = currentAction;

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            ++currentAction;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            --currentAction;
        }

        currentAction = Mathf.Clamp(currentAction, 0, menuItems.Count - 1);

        if (currentAction != prevAction)
            UpdateActionSelection(currentAction);


        if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            swappedSelection = currentSelection; // Save the pokemon that could be swapped
            onMiniMenuSelected?.Invoke(currentAction);

        }
        else if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.Escape))
        {
            onBack?.Invoke();
        }
    }

    public void UpdateActionSelection(int currentAction)
    {
        for (int i = 0; i < menuItems.Count; i++)
        {
            if (i == currentAction)
            {
                menuItems[i].color = GlobalSettings.i.HighlightedColor;
            }
            else
            {
                menuItems[i].color = Color.black;
            }
        }
    }

    public void UpdateMessageTextUponSwapping()
    {
        messageText.text = "Choose a pokemon to swap with.";
    }

    public void ResetMessageText()
    {
        messageText.text = "Choose a Pokemon";
    }

    public void HandleSwapPartyPokemon(Action onBack)
    {
        var prevSelection = currentSelection;

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ++currentSelection;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            --currentSelection;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentSelection += 2;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentSelection -= 2;
        }

        currentSelection = Mathf.Clamp(currentSelection, 0, pokemons.Count - 1);

        if (currentSelection != prevSelection)
            UpdateMemberSelection(currentSelection);

        if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Space) || (Input.GetKeyDown(KeyCode.Return)))
        {
            playerParty.SwapPokemonInParty(swappedSelection, currentSelection);
            onBack?.Invoke();
        }
        else if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.Escape))
        {
            onBack?.Invoke();
        }
    }

    // End mini menu sections

    public void ShowIfTmIsUsable(TmItem tmItem)
    {
        for (int i = 0; i < pokemons.Count; i++)
        {
            string message = tmItem.CanBeTaught(pokemons[i]) ? "CAN BE TAUGHT" : "CANNOT BE TAUGHT";
            memberSlots[i].SetMessage(message);
        }
    }

    public void ClearMemberSlotMessages()
    {
        for (int i = 0; i < pokemons.Count; i++)
        {
            memberSlots[i].SetMessage("");
        }
    }

    public void SetMessageText(string message)
    {
        messageText.text = message;
    }
}
