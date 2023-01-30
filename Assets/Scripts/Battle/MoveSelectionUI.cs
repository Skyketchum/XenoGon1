using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class MoveSelectionUI : MonoBehaviour
{
    [SerializeField] List<Text> moveTexts;

    int currentSelection = 0;

    public void SetMoveData(List<MoveBase> currentMoves, MoveBase newMove)
    {
        for (int i=0; i<currentMoves.Count; ++i)
        {
            moveTexts[i].text = currentMoves[i].name;
        }

        moveTexts[currentMoves.Count].text = newMove.name;
    }

    public void HandleMoveSelection(Action<int> onSelected)
    {
        if ((Input.GetKeyDown(KeyCode.DownArrow)) || (Input.GetKeyDown(KeyCode.S)))
            ++currentSelection;
        else if ((Input.GetKeyDown(KeyCode.UpArrow)) || (Input.GetKeyDown(KeyCode.W)))
            --currentSelection;

        currentSelection = Mathf.Clamp(currentSelection, 0, XenogonBase.MaxNumOfMoves);

        UpdateMoveSelection(currentSelection);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            onSelected?.Invoke(currentSelection);
        }
    }

    public void UpdateMoveSelection(int selection)
    {
        for (int i = 0; i < XenogonBase.MaxNumOfMoves+1; i++)
        {
            if (i == selection)
                moveTexts[i].color = GlobalSettings.i.HighlightedColor;
            else
                moveTexts[i].color = Color.black;
        }
    }
}
