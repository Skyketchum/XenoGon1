using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PartyScreen : MonoBehaviour
{
    [SerializeField] Text messageText;
    [SerializeField] GameObject selectPanel;

    [SerializeField] XenogonParty xenogonParty; //Swapping Xenogon

    PartyMemberUI [] memberSlots;
    List<Xenogon> xenogonList;
    List<Text> panelItems;
    XenogonParty party;

    public bool openPanel = false;
    public bool panelIsOpened = false;
    bool swapping = false;
    int swapSelection = -1;

    int selection = 0;
    int panelSelection = 0;

    public Xenogon SelectedMember => xenogonList[selection];


    //Notes **** 07/25/22 rememebr to ask jason about heal and swap inside of party screen
    /// <summary>
    /// Party screen can be called from different states like ActionSelection, RunningTurn, AboutToUse
    /// </summary>
    public BattleState? CalledFrom { get; set; }

    private void Awake()
    {
        panelItems = selectPanel.GetComponentsInChildren<Text>().ToList();
    }

    public void Init()
    {
        memberSlots = GetComponentsInChildren<PartyMemberUI>(true);

        party = XenogonParty.GetPlayerParty();
        SetPartyData();

        party.OnUpdated += SetPartyData;
    }

    public void OpenSelectPanel()
    {
        selectPanel.SetActive(true);
    }

    public void CloseSelectPanel()
    {
        selectPanel.SetActive(false);
    }

    public void SetPartyData()    //loops through current xenogon party and fills party member slots and displays active xenogon
    {

        xenogonList = party.XenogonList;

        for (int i = 0; i < memberSlots.Length; i++)
        {
            if (i < xenogonList.Count)
            {
                memberSlots[i].gameObject.SetActive(true);
                memberSlots[i].Init(xenogonList[i]);
            }
            else
                memberSlots[i].gameObject.SetActive(false);
        }

        UpdateMemberSelection(selection);

        SetMessageText("Select a Xenogon");
        //messageText.text = "Select a Xenogon";
    }

    public void HandleUpdate(Action onSelected, Action onBack)
    {
        var prevSelection = selection;
        var prevPanelSelection = panelSelection; //Panel

        if (!panelIsOpened) //Move through Xenogon party
        {
            if ((Input.GetKeyDown(KeyCode.RightArrow)) || (Input.GetKeyDown(KeyCode.D)))
                ++selection;
            else if ((Input.GetKeyDown(KeyCode.LeftArrow)) || (Input.GetKeyDown(KeyCode.A)))
                --selection;
            else if ((Input.GetKeyDown(KeyCode.DownArrow)) || (Input.GetKeyDown(KeyCode.S)))
                selection += 2;
            else if ((Input.GetKeyDown(KeyCode.UpArrow)) || (Input.GetKeyDown(KeyCode.W)))
                selection -= 2;
        }
        else if (panelIsOpened) //Move through panel options
        {
            if ((Input.GetKeyDown(KeyCode.DownArrow)) || (Input.GetKeyDown(KeyCode.S)))
                ++panelSelection;
            else if ((Input.GetKeyDown(KeyCode.UpArrow)) || (Input.GetKeyDown(KeyCode.W)))
                --panelSelection;
        }
        

        selection = Mathf.Clamp(selection, 0, xenogonList.Count - 1);

        panelSelection = Mathf.Clamp(panelSelection, 0, panelItems.Count - 1); //Panel

        if (selection != prevSelection)
            UpdateMemberSelection(selection);

        if (panelSelection != prevPanelSelection) //Panel
            UpdatePanelSelection(panelSelection);


        //  Inputs
        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (openPanel)
            {
                OpenSelectPanel();
                openPanel = false;
                panelIsOpened = true;
                panelSelection = 0;
                UpdatePanelSelection(panelSelection);
                SetMessageText("Select an option");
            }
            else if (panelIsOpened)
            {
                if (panelSelection == 0)
                {
                    //Swap
                    CloseSelectPanel();
                    panelIsOpened = false;
                    swapping = true;
                    ShowSwappingXenogon(selection);
                    swapSelection = selection;
                    SetMessageText("Select a Xenogon to swap");
                }
                else if (panelSelection == 1)
                {
                    //Heal ---> originally 'Details' but this is heal for now for testing purposes
                    //SetMessageText("Showing details..."); --> part of 'Details'
                    HealXenogon(selection);
                    CloseSelectPanel();
                    panelIsOpened = false;
                    openPanel = true;
                    SetMessageText("Select a Xenogon");
                }
            }
            else if (swapping)
            {
                if (swapSelection == selection)
                {
                    SetMessageText("Choose a different Xenogon to swap to.");
                }
                else
                {
                    ShowSwappingXenogon(-1);
                    SwapXenogon(swapSelection, selection);
                    swapping = false;
                    openPanel = true;
                    SetMessageText("Select a Xenogon");
                }
            }
            else
                onSelected?.Invoke();
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            if (panelIsOpened)
            {
                //Back
                CloseSelectPanel();
                panelIsOpened = false;
                openPanel = true;
                SetMessageText("Select a Xenogon");
            }
            else if (swapping)
            {
                ShowSwappingXenogon(-1);
                swapping = false;
                openPanel = true;
                SetMessageText("Select a Xenogon");
            }
            else
                onBack?.Invoke();
        }
    }
    
    public void UpdateMemberSelection(int selectedMember)
    {
        for(int i = 0; i < xenogonList.Count; i++)
        {
            if (i == selectedMember)
                memberSlots[i].SetSelected(true);
            else
                memberSlots[i].SetSelected(false);
        }
    }

    public void HealXenogon(int selectedMember)
    {
        xenogonParty.XenogonList[selectedMember].FullyRestoreXenogon();
        SetPartyData();
    }

    public void SwapXenogon(int xenogon1, int xenogon2)
    {
        var tempXenogon = xenogonParty.XenogonList[xenogon1];
        xenogonParty.XenogonList[xenogon1] = xenogonParty.XenogonList[xenogon2];
        xenogonParty.XenogonList[xenogon2] = tempXenogon;
        SetPartyData();
    }

    public void ShowSwappingXenogon(int selectedMember)
    {
        for (int i = 0; i < xenogonList.Count; i++)
        {
            if (i == selectedMember)
                memberSlots[i].ShowSelectedMember(true);
            else
                memberSlots[i].ShowSelectedMember(false);
        }
    }

    public void UpdatePanelSelection(int selectedOption)
    {
        for (int i = 0; i < panelItems.Count; i++)
        {
            if (i == selectedOption)
                panelItems[i].color = GlobalSettings.i.HighlightedColor;
            else
                panelItems[i].color = Color.black;
        }
    }

    public void SetMessageText(string message)
    {
        messageText.text = message;
    }
}
