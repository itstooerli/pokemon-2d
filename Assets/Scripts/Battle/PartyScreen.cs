using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyScreen : MonoBehaviour
{
    [SerializeField] Text messageText;
    
    PartyMemberUI[] memberSlots;
    List<Pokemon> pokemons;
    PokemonParty playerParty;

    int currentSelection;

    public Pokemon SelectedMember => pokemons[currentSelection];

    /// <summary>
    /// Party screen can be called from different states like ActionSelection, RunningTurn, AboutToUse
    /// </summary>
    public BattleState? CalledFrom { get; set; }

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

        messageText.text = "Choose a Pokemon";
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
