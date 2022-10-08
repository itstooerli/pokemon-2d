using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyMemberUI : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] HPBar hpBar;

    Pokemon _pokemon;

    public void Init(Pokemon pokemon)
    {
        _pokemon = pokemon;
        UpdateData();
        
        _pokemon.OnHPChanged += UpdateData;
    }

    void UpdateData()
    {
        nameText.text = _pokemon.Base.Name;
        levelText.text = "Lvl " + _pokemon.Level;
        hpBar.SetHP((float)_pokemon.HP / _pokemon.MaxHp);
    }

    public void SetSelected(bool selected)
    {
        if (selected)
        {
            nameText.color = GlobalSettings.i.HighlightedColor;
        }
        else
        {
            nameText.color = Color.black;
        }
    }
}
