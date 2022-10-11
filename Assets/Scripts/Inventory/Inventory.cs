using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum ItemCategory { Items, Pokeballs, Tms }

public class Inventory : MonoBehaviour
{
    [SerializeField] List<ItemSlot> slots;
    [SerializeField] List<ItemSlot> pokeballSlots;
    [SerializeField] List<ItemSlot> tmSlots;

    List<List<ItemSlot>> allSlots;

    public event Action OnUpdated;

    private void Awake()
    {
        allSlots = new List<List<ItemSlot>>() { slots, pokeballSlots, tmSlots };
    }

    /// <summary>
    /// This should match the public enum for ItemCategory
    /// </summary>
    public static List<string> ItemCategories { get; set; } = new List<string>()
    {
        "ITEMS", "POKEBALLS", "TMs"
    };

    public List<ItemSlot> GetSlotsByCategory(int categoryIndex)
    {
        return allSlots[categoryIndex];
    }

    public ItemBase GetItem(int itemIndex, int categoryIndex)
    {
        var currSlots = GetSlotsByCategory(categoryIndex);
        return currSlots[itemIndex].Item;
    }

    public static Inventory GetInventory()
    {
        return FindObjectOfType<PlayerController>().GetComponent<Inventory>();
    }

    public ItemBase UseItem(int itemIndex, Pokemon selectedPokemon, int selectedCategory)
    {
        var item = GetItem(itemIndex, selectedCategory);
        bool itemUsed = item.Use(selectedPokemon);

        if (itemUsed)
        {
            if (!item.IsReusable)
                RemoveItem(item, selectedCategory);
            return item;
        }

        return null;
    }

    public void AddItem(ItemBase item, int count=1)
    {
        int category = (int)GetCategoryFromItem(item);
        var currentSlots = GetSlotsByCategory(category);

        var itemSlot = currentSlots.FirstOrDefault(slot => slot.Item == item);

        if (itemSlot != null)
        {
            itemSlot.Count += count;
        }
        else
        {
            currentSlots.Add(new ItemSlot()
            {
                Item = item,
                Count = count
            });
        }

        OnUpdated?.Invoke();
    }

    public void RemoveItem(ItemBase item, int selectedCategory)
    {
        var currSlots = GetSlotsByCategory(selectedCategory);
        
        var itemSlot = currSlots.First(slot => slot.Item == item);
        itemSlot.Count--;

        if (itemSlot.Count == 0)
        {
            currSlots.Remove(itemSlot);
        }

        OnUpdated?.Invoke();
    }

    ItemCategory GetCategoryFromItem(ItemBase item)
    {
        if (item is RecoveryItem)
            return ItemCategory.Items;
        else if (item is PokeballItem)
            return ItemCategory.Pokeballs;
        else if (item is TmItem)
            return ItemCategory.Tms;

        return ItemCategory.Items;
    }
}

[System.Serializable]
public class ItemSlot
{
    [SerializeField] ItemBase item;
    [SerializeField] int count;

    public ItemBase Item
    {
        get => item;
        set => item = value;
    }
    public int Count {
        get => count;
        set => count = value;
    }
}
