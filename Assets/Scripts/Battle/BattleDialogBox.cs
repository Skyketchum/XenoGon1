using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleDialogBox : MonoBehaviour
{
    [SerializeField] int lettersPerSecond;

    [SerializeField] Text dialogText;
    [SerializeField] GameObject actionSelector;
    [SerializeField] GameObject moveDetails;
    [SerializeField] GameObject choiceBox;
    [SerializeField] List<GameObject> moveSelectors;

    [SerializeField] List<Text> actionTexts;
    [SerializeField] List<Text> moveTexts;
    [SerializeField] List<Text> pageTexts;

    [SerializeField] Text mpText;
    [SerializeField] Text typeText;

    [SerializeField] Text yesText;
    [SerializeField] Text noText;

    Color highlightedColor;

    private void Start()
    {
        highlightedColor = GlobalSettings.i.HighlightedColor;
    }

    /*public void SetDialog(string dialog)
    {
        dialogText.text = dialog;
    }*/

    public IEnumerator TypeDialog(string dialog, float letterSpeed = 1f, bool waitForInput = true) //type the dialog for the dialog box
    {
        dialogText.text = "";
        foreach (var letter in dialog.ToCharArray())
        {
            dialogText.text += letter;
            yield return new WaitForSeconds(letterSpeed/lettersPerSecond);
        }

      

        yield return new WaitForSeconds(1f);
    }

    public void EnableDialogText(bool enabled) // enable/disable dialog text
    {
        dialogText.enabled = enabled;
    }

    public void EnableActionSelector(bool enabled) // enable/disable action selector (Fight or Run)
    {
        actionSelector.SetActive(enabled);
    }

    public void EnableMoveSelector(bool enabled)// enable/disable move selector and move details
    {
        SetActiveMovePage(enabled, 0);
        moveDetails.SetActive(enabled);
    }


    public void EnableChoiceBox(bool enabled) // enable/disable action selector (Fight or Run)
    {
        choiceBox.SetActive(enabled);
    }

    public void UpdateActionSelection(int selectedAction) //sets which action is selected to the highlighted color
    {
        for (int i = 0; i < actionTexts.Count; ++i)
        {
            if (i == selectedAction)
                actionTexts[i].color = highlightedColor;
            else
                actionTexts[i].color = Color.black;
        }
    }

    public void UpdateMoveSelection(bool nextPage, int currentPage, Moves move, int selectedMove = -1, Xenogon xenogon = null) //sets which move is selected to the highlighted color and displays the details
    {
        if (nextPage)
        {
            for (int i = 0; i < moveTexts.Count; ++i)
            {
                moveTexts[i].color = Color.black;
            }

            for (int i = 0; i < pageTexts.Count; ++i)
            {
                if (i == currentPage)
                    pageTexts[i].color = highlightedColor;
                else
                    pageTexts[i].color = Color.black;
            }

            mpText.text = "";
            typeText.text = "Next Page";
        }
        else
        { 
            for (int i = 0; i < pageTexts.Count; ++i)
            {
                pageTexts[i].color = Color.black;
            }

            for (int i = 0; i < moveTexts.Count; ++i)
            {
                if (i == selectedMove)
                    moveTexts[i].color = highlightedColor;
                else
                    moveTexts[i].color = Color.black;
            }

            typeText.text = move.Base.Type.ToString();

            if (xenogon.Status == null)
            {
                mpText.color = Color.black;
                mpText.text = $"MP {move.MPCOST}";
            }
            else if (xenogon.Status.Id == ConditionID.crs)
            {
                mpText.color = Color.red;
                mpText.text = $"MP {move.MPCOST * 2}";
            }
            
        }
    }
    public void UpdateChoiceBox(bool yesSelected) //sets which action is selected to the highlighted color
    {
        if (yesSelected)
        {
            yesText.color = highlightedColor;
            noText.color = Color.black;
        }
        else
        {
            yesText.color = Color.black;
            noText.color = highlightedColor;
        }
    }

    public void SetMoveNames(List<Moves> moves) //Sets up the move names for the player's current pokemon
    {
        for (int i = 0; i < moveTexts.Count; ++i)
        {
            if (i < moves.Count)
                moveTexts[i].text = moves[i].Base.name;
            else
                moveTexts[i].text = "-";
        }
    }

    public void SetActiveMovePage(bool enabled, int page = 0) //page number passed needs to be the number for the element in the array (p1 = 0, p2 = 1, p3 = 2)
    {
        if (enabled)
        {
            for (int i = 0; i < moveSelectors.Count; ++i)
            {
                if (i == page)
                    moveSelectors[i].SetActive(true);
                else
                    moveSelectors[i].SetActive(false);
            }
        }
        else
        {
            for (int i = 0; i < moveSelectors.Count; ++i)
            {
                    moveSelectors[i].SetActive(false);
            }
        }
    }

   /* public void HandleUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Z) && !isTyping)
        {
            ++currentLine;

            if (currentLine < dialog.Lines.Count)
            {
                StartCoroutine(TypeDialog(dialog.Lines[currentLine]));
            }
        
        }
    }

  */
}
