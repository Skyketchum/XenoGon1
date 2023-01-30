using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum ItemCategory { Items, XBait, SecretMoves}// item catgeory here, new categories must be in the same order in list below

public class Inventory : MonoBehaviour
{
    [SerializeField] List<ItemSlot> slots;
    [SerializeField] List<ItemSlot> xBaitSlots;
    [SerializeField] List<ItemSlot> secretMoveSlots;

    List<List<ItemSlot>> allSlots; //list that holds a list of item slots " allows to create an index/list for lists

    public event Action OnUpdated;

    //  public List<ItemSlot> Slots => slots;    "outdated property, was deleted in the video but i kept here for logging purposes ***can delete once reviewed


    private void Awake()
    {
        allSlots = new List<List<ItemSlot>>() { slots, xBaitSlots, secretMoveSlots }; //assigns the item slots to the list
    }
    public static List<string> ItemCategories { get; set; } = new List<string>() // creation of list to store category types
    {
        "ITEMS", "XBAIT", "SECRET MOVES"
    };

    public List<ItemSlot> GetSlotsByCategory(int categoryIndex) //gets the corrct item slot to display when switching item categories
    {
        return allSlots[categoryIndex];
    }

    public ItemBase GetItem(int itemIndex, int categoryIndex) //finds current slot amount and category of item
    {
        var currentSlots = GetSlotsByCategory(categoryIndex);
        return currentSlots[itemIndex].Item;
    }

    public ItemBase UseItem(int itemIndex, Xenogon selectedXenogon, int selectedCategory)
    {
        var item = GetItem(itemIndex, selectedCategory); //calls and sets the item by index and category
        
        bool itemUsed = item.Use(selectedXenogon);
        if (itemUsed)
        {
            if (!item.IsReusable)
            {
                RemoveItem(item, selectedCategory);
            }
             

            return item;
        }

        return null;
    }

    public void RemoveItem(ItemBase item, int category)
    {

        var currentSlots = GetSlotsByCategory(category);


        var itemSlot = currentSlots.First(slot => slot.Item == item);
        itemSlot.Count--;

        if (itemSlot.Count <= 0)
            currentSlots.Remove(itemSlot);

        OnUpdated?.Invoke();
    }

    public static Inventory GetInventory()
    {
        return FindObjectOfType<PlayerController>().GetComponent<Inventory>();
    }
}

[Serializable]
public class ItemSlot
{
    [SerializeField] ItemBase item;
    [SerializeField] int count;

    public ItemBase Item => item;
    public int Count {
        get => count;
        set => count = value;
    }
}


