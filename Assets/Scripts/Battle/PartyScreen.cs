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

    int currentSelection;

    public Pokemon SelectedMember => pokemons[currentSelection];

    /// <summary>
    /// Party screen can be called from different states like ActionSelection, RunningTurn, AboutToUse
    /// </summary>
    public BattleState? CalledFrom { get; set; }

    public void Init()
    {
        memberSlots = GetComponentsInChildren<PartyMemberUI>(true);
    }

    public void SetPartyData(List<Pokemon> pokemons)
    {
        this.pokemons = pokemons;
        
        for (int i = 0; i < memberSlots.Length; i++)
        {
            if (i < pokemons.Count)
            {
                memberSlots[i].gameObject.SetActive(true);
                memberSlots[i].SetData(pokemons[i]);
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

    public void SetMessageText(string message)
    {
        messageText.text = message;
    }
}
