using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public enum InventoryUIState { ItemSelection, PartySelection, MoveToForget, Busy}
public class InventoryUI : MonoBehaviour
{
    [SerializeField] GameObject itemList;
    [SerializeField] ItemSlotUI itemSlotUI;

    [SerializeField] Text categoryText;
    [SerializeField] Image itemIcon;
    [SerializeField] Text itemDescription;

    [SerializeField] Image upArrow;
    [SerializeField] Image downArrow;
    [SerializeField] Image leftArrow;
    [SerializeField] Image rightArrow;

    [SerializeField] PartyScreen partyScreen;
    [SerializeField] MoveSelectionUI moveSelectionUI;

    Action<ItemBase> onItemUsed; //added item base to allow access for more item types to use this action

    int selectedItem = 0;
    int selectedCategory = 0;

    MoveBase moveToLearn;

    InventoryUIState state;

    const int itemsInViewport = 8;

    List<ItemSlotUI> slotUIList;

    Inventory inventory;
    RectTransform itemListRect;

    private void Awake()
    {
        inventory = Inventory.GetInventory();
        itemListRect = itemList.GetComponent<RectTransform>();
    }

    private void Start()
    {
        UpdateItemList();

        inventory.OnUpdated += UpdateItemList;
    }

    void UpdateItemList()
    {
        //Clear all the existing items
        foreach (Transform child in itemList.transform)
            Destroy(child.gameObject);

        //Sort Inventory (default is alphabetially by name)
        SortInventoryList();

        //Initialize list
        slotUIList = new List<ItemSlotUI>();


        inventory.GetSlotsByCategory(selectedCategory).Sort(delegate (ItemSlot slot1, ItemSlot slot2) { return slot1.Item.Name.CompareTo(slot2.Item.Name); });

        foreach (var itemSlot in inventory.GetSlotsByCategory(selectedCategory))
        {
            var slotUIObj = Instantiate(itemSlotUI, itemList.transform);
            slotUIObj.SetData(itemSlot);

            slotUIList.Add(slotUIObj);
        }

        SortInventoryList(true);

        UpdateItemSelection();
    }


    public void HandleUpdate(Action onBack, Action<ItemBase> onItemUsed=null)
    {
        this.onItemUsed = onItemUsed;

        if (state == InventoryUIState.ItemSelection)
        {
            int prevSelection = selectedItem;
            int prevCategory = selectedCategory;

            if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
                ++selectedItem;
            else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
                --selectedItem;
            else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
                ++selectedCategory;
            else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
                --selectedCategory;
            //THIS if-else if-  block below must happen before the selectedItem clamp to help user functionality
            if (selectedCategory > Inventory.ItemCategories.Count - 1)
                selectedCategory = 0;
            else if (selectedCategory < 0)
                selectedCategory = Inventory.ItemCategories.Count - 1;

            selectedItem = Mathf.Clamp(selectedItem, 0, inventory.GetSlotsByCategory(selectedCategory).Count - 1);

            if(prevCategory != selectedCategory)
            {
                ResetSelection();
                categoryText.text = Inventory.ItemCategories[selectedCategory];
                UpdateItemList();
            }
          else  if (prevSelection != selectedItem)
            {
                UpdateItemSelection();
            }

            if (Input.GetKeyDown(KeyCode.Z))
            {
                //Debug.Log("Use Item");
               partyScreen.panelIsOpened = false;
               partyScreen.openPanel = false;
                StartCoroutine(ItemSelected());
            }
            else if (Input.GetKeyDown(KeyCode.X))
                onBack?.Invoke();
        }
        else if (state == InventoryUIState.PartySelection)
        {
            //Handle party selection
            Action onSelected = () =>
            {
                StartCoroutine(UseItem());
            };

            Action onBackPartyScreen = () =>
            {
                ClosePartyScreen();
            };

            partyScreen.HandleUpdate(onSelected, onBackPartyScreen);

        }
        else if (state == InventoryUIState.MoveToForget)
        {
            Action<int> onMoveSelected = (int moveIndex) =>
            {
                StartCoroutine(OnMoveToForgetSelected(moveIndex));
            };

            moveSelectionUI.HandleMoveSelection(onMoveSelected);
        }


    }

    IEnumerator ItemSelected() //changed to coroutine to run yeild return logic

    {

        state = InventoryUIState.Busy;

        var item = inventory.GetItem(selectedItem, selectedCategory);

        if(GameController.Instance.State == GameState.Battle)
        {
            //in battle

            if (!item.CanUseInBattle)
            {
                yield return DialogManager.Instance.ShowDialogText($"This item cannot be used in battle!");
                state = InventoryUIState.ItemSelection;
                yield break;
            }
            
        }
        else
        {
            //outside battle
            if (!item.CanUseOutsideBattle)
            {
                yield return DialogManager.Instance.ShowDialogText($"This item cannot be used here");
                state = InventoryUIState.ItemSelection;
                yield break;
            }

        }


        if (selectedCategory == (int)ItemCategory.XBait)
        {

            StartCoroutine(UseItem());
        }
        else
        {
            OpenPartyScreen();
        }
    }

    IEnumerator UseItem()
    {
        state = InventoryUIState.Busy;

        yield return HandleSecretMoveItems();

        var usedItem = inventory.UseItem(selectedItem, partyScreen.SelectedMember, selectedCategory);

        if (usedItem != null)
        {

            if ((usedItem is RecoveryItem ))

                yield return DialogManager.Instance.ShowDialogText($"Used item.");
            onItemUsed?.Invoke(usedItem);
        }
        else
        {
            yield return DialogManager.Instance.ShowDialogText($"The item won't have any affect.");
        }

        ClosePartyScreen();
    }

    IEnumerator HandleSecretMoveItems()
    {
      var secretMoveItem = inventory.GetItem(selectedItem, selectedCategory) as SecretMoveItem;
        if (secretMoveItem == null)
            yield break;

        var xenogon = partyScreen.SelectedMember; //reference for xengon in the party screen

        if (xenogon.HasMove(secretMoveItem.Move))
        {
            yield return DialogManager.Instance.ShowDialogText($"{xenogon.Base.Name} has already learned{secretMoveItem.Move.name}.");
            yield break;
        }

        if(xenogon.Moves.Count < XenogonBase.MaxNumOfMoves)
        {
            xenogon.LearnMove(secretMoveItem.Move);
            yield return DialogManager.Instance.ShowDialogText($"{xenogon.Base.Name} learned {secretMoveItem.Move.name}.");
        }
        else
        {
            yield return DialogManager.Instance.ShowDialogText($"{xenogon.Base.Name} is trying to learn{secretMoveItem.Move.name}.");
            yield return DialogManager.Instance.ShowDialogText($"But it cannot learn more than {XenogonBase.MaxNumOfMoves} moves.");
            yield return ChooseMoveToForget(xenogon, secretMoveItem.Move);
            yield return new WaitUntil(() => state != InventoryUIState.MoveToForget); //waits until stat process finishes
        }
    }

    IEnumerator ChooseMoveToForget(Xenogon xenogon, MoveBase newMove)
    {
        state = InventoryUIState.Busy;
        yield return DialogManager.Instance.ShowDialogText($"Choose a move you want to forget.", true, false);
        moveSelectionUI.gameObject.SetActive(true);
        moveSelectionUI.SetMoveData(xenogon.Moves.Select(x => x.Base).ToList(), newMove);
        moveToLearn = newMove;

        state = InventoryUIState.MoveToForget;
    }

    void UpdateItemSelection()
    {
        var slots = inventory.GetSlotsByCategory(selectedCategory);

        selectedItem = Mathf.Clamp(selectedItem, 0, slots.Count - 1); // stopes the game from throwing an error after using the last item

        for (int i = 0; i < slotUIList.Count; i++)
        {
            if (i == selectedItem)
            {
                slotUIList[i].NameText.color = GlobalSettings.i.HighlightedColor;
                slotUIList[i].CountText.color = GlobalSettings.i.HighlightedColor;
            }
            else
            {
                slotUIList[i].NameText.color = Color.black;
                slotUIList[i].CountText.color = Color.black;
            }
        }

        
        if (slots.Count > 0) //conditions when category has no slots assigned to not through an exception
        {
            var slot = slots[selectedItem].Item;
            itemIcon.sprite = slot.Icon;
            itemDescription.text = slot.Description;
        }

        HandleScrolling();
    }

    void HandleScrolling()
    {
        if (slotUIList.Count <= itemsInViewport) return;

        float scrollPos = Mathf.Clamp(selectedItem - (itemsInViewport/2), 0, selectedItem) * slotUIList[0].Height;
        itemListRect.localPosition = new Vector2(itemListRect.localPosition.x, scrollPos);

        bool showUpArrow = selectedItem > itemsInViewport/2;
        upArrow.gameObject.SetActive(showUpArrow);

        bool showDownArrow = selectedItem + itemsInViewport/2 < slotUIList.Count;
        downArrow.gameObject.SetActive(showDownArrow);
    }

    void SortInventoryList(bool byName=true, bool byCount=false, string type=null)
    {
        if (byName)
        {
            inventory.GetSlotsByCategory(selectedCategory).Sort(delegate (ItemSlot slot1, ItemSlot slot2) { return slot1.Item.Name.CompareTo(slot2.Item.Name); });
        }
        else if (byCount)
        {
            inventory.GetSlotsByCategory(selectedCategory).Sort(delegate (ItemSlot slot1, ItemSlot slot2) { return slot1.Count.CompareTo(slot2.Count); });
        }
        else
        {
            switch (type)
            {
                case "a":
                    break;
                case "b":
                    break;
                case "c":
                    break;
                case "d":
                    break;
                case "e":
                    break;
                default:
                    break;
            }
        }
    }

    void ResetSelection()
    {
        selectedItem = 0;
        upArrow.gameObject.SetActive(false);
        downArrow.gameObject.SetActive(false);

        itemIcon.sprite = null;
        itemDescription.text = "";

    }


    void OpenPartyScreen()
    {
        state = InventoryUIState.PartySelection;
        partyScreen.gameObject.SetActive(true);
    }
    void ClosePartyScreen()
    {
        state = InventoryUIState.ItemSelection;
        partyScreen.gameObject.SetActive(false);
    }
    IEnumerator OnMoveToForgetSelected(int moveIndex)
    {
      

            var xenogon = partyScreen.SelectedMember;

            DialogManager.Instance.CloseDialog();
            moveSelectionUI.gameObject.SetActive(false);
            if (moveIndex == XenogonBase.MaxNumOfMoves)
            {
                //Don't learn the new move
                yield return DialogManager.Instance.ShowDialogText($"{xenogon.Base.Name} did not learn {moveToLearn.name}.");
            }
            else
            {
                //Forget the selected move and learn the new move
                var selectedMove = xenogon.Moves[moveIndex].Base;
                yield return DialogManager.Instance.ShowDialogText($"{xenogon.Base.Name} forgot {selectedMove.name} and learned {moveToLearn.name}.");
                xenogon.Moves[moveIndex] = new Moves(moveToLearn);
            }

            // use   DialogManager.Instance.ShowDialogText for outisde of battle system UI

            moveToLearn = null;
            state = InventoryUIState.ItemSelection;
        

      
    }


}


