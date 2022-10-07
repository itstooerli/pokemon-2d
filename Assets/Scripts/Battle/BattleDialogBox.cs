using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleDialogBox : MonoBehaviour
{
    [SerializeField] int lettersPerSecond;

    [SerializeField] Text dialogText;
    [SerializeField] GameObject actionSelector;
    [SerializeField] GameObject moveSelector;
    [SerializeField] GameObject moveDetails;
    [SerializeField] GameObject choiceBox;

    [SerializeField] List<Text> actionTexts;
    [SerializeField] List<Text> moveTexts;

    [SerializeField] Text ppText;
    [SerializeField] Text typeText;

    [SerializeField] Text yesText;
    [SerializeField] Text noText;

    Color highlightedColor;

    private void Start()
    {
        highlightedColor = GlobalSettings.i.HighlightedColor;
    }

    public void SetDialog(string dialog)
    {
        dialogText.text = dialog;
    }
    
    public IEnumerator TypeDialog(string dialog)
    {
        dialogText.text = "";
        foreach (var letter in dialog.ToCharArray())
        {
            dialogText.text += letter;
            yield return new WaitForSeconds(1f / lettersPerSecond);
        }

        yield return new WaitForSeconds(1f);
    }

    public void EnableDialogText(bool enabled)
    {
        dialogText.enabled = enabled;
    }

    public void EnableActionSelector(bool enabled)
    {
        actionSelector.SetActive(enabled);
    }

    public void EnableMoveSelector(bool enabled)
    {
        moveSelector.SetActive(enabled);
        moveDetails.SetActive(enabled);
    }

    public void EnableChoiceSelector(bool enabled)
    {
        choiceBox.SetActive(enabled);
    }

    public void UpdateActionSelection(int selectedAction)
    {
        for (int i=0; i < actionTexts.Count; ++i)
        {
            if (i == selectedAction)
            {
                actionTexts[i].color = highlightedColor;
            }
            else
            {
                actionTexts[i].color = Color.black;
            }
        }
    }

    public void UpdateMoveSelection(int selectedMove, Move move)
    {
        for (int i = 0; i < moveTexts.Count; ++i)
        {
            if (i == selectedMove)
            {
                moveTexts[i].color = highlightedColor;
            }
            else if (move.Pp <= move.Base.Pp / 2)
            {
                ppText.color = new Color(1f, 0.647f, 0f);
            }
            else
            {
                moveTexts[i].color = Color.black;
            }
        }

        ppText.text = $"PP {move.Pp}/{move.Base.Pp}";
        typeText.text = move.Base.Type.ToString();

        if (move.Pp == 0)
        {
            ppText.color = Color.red;
        }
        else
        {
            ppText.color = Color.black;
        }
    }

    public void SetMoveNames(List<Move> moves)
    {
        for (int i = 0; i < moveTexts.Count; ++i)
        {
            if (i < moves.Count)
            {
                moveTexts[i].text = moves[i].Base.Name;
            }
            else
            {
                moveTexts[i].text = "-";
            }
        }
    }

    public void UpdateChoiceSelection(bool yesSelected)
    {
        if (yesSelected)
        {
            yesText.color = highlightedColor;
            noText.color = Color.black;
        }
        else
        {
            noText.color = highlightedColor;
            yesText.color = Color.black;
        }
    }
}
