using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum InventoryUIState { ItemSelection, PartySelection, Busy }

public class InventoryUI : MonoBehaviour
{
    [SerializeField] GameObject itemList;
    [SerializeField] ItemSlotUI itemSlotUIPrefab;

    [SerializeField] Image itemIcon;
    [SerializeField] Text itemDescription;

    [SerializeField] Image upArrow;
    [SerializeField] Image downArrow;

    [SerializeField] PartyScreen partyScreen;

    Inventory inventory;
    List<ItemSlotUI> slotUIList;
    RectTransform itemListRect;

    Action onItemUsed;

    int selectedItem = 0;

    InventoryUIState state;

    const int itemsInViewport = 8;

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
        // Clear all existing items
        foreach (Transform child in itemList.transform)
        {
            Destroy(child.gameObject);
        }

        slotUIList = new List<ItemSlotUI>();
        foreach (var itemSlot in inventory.Slots)
        {
            var slotUIObj = Instantiate(itemSlotUIPrefab, itemList.transform);
            slotUIObj.SetData(itemSlot);
            slotUIList.Add(slotUIObj);
        }

        UpdateItemSelection();
    }

    public void HandleUpdate(Action onBack, Action onItemUsed=null)
    {
        this.onItemUsed = onItemUsed;
        
        if (state == InventoryUIState.ItemSelection)
        {
            int prevSelection = selectedItem;

            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                ++selectedItem;
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                --selectedItem;
            }

            selectedItem = Mathf.Clamp(selectedItem, 0, inventory.Slots.Count - 1);

            if (prevSelection != selectedItem)
                UpdateItemSelection();

            
            if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            {

                OpenPartyScreen();
            }
            else if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.Escape))
            {
                onBack?.Invoke();
            }
        }
        else if (state == InventoryUIState.PartySelection)
        {
            // Handle Party Selection

            Action onSelectedPartyScreen = () =>
            {
                // Use the item on the selected pokemon
                StartCoroutine(UseItem());
            };

            Action onBackPartyScreen = () =>
            {
                ClosePartyScreen();
            };

            partyScreen.HandleUpdate(onSelectedPartyScreen, onBackPartyScreen);
        }
    }

    IEnumerator UseItem()
    {
        state = InventoryUIState.Busy;
        
        var usedItem = inventory.UseItem(selectedItem, partyScreen.SelectedMember);
        if (usedItem != null)
        {
            yield return DialogManager.Instance.ShowDialogText($"The player used {usedItem.Name} on {partyScreen.SelectedMember.Base.Name}.");
            onItemUsed?.Invoke();
        }
        else
        {
            yield return DialogManager.Instance.ShowDialogText($"It won't have any effect!");
        }

        ClosePartyScreen();
    }

    void UpdateItemSelection()
    {
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

        var item = inventory.Slots[selectedItem].Item;
        itemIcon.sprite = item.Icon;
        itemDescription.text = item.Description;

        HandleScrolling();
    }

    void HandleScrolling()
    {
        if (slotUIList.Count <= itemsInViewport) return;

        float scrollPos = Mathf.Clamp(selectedItem - itemsInViewport/2, 0, selectedItem) * slotUIList[0].Height;
        itemListRect.localPosition = new Vector2(itemListRect.localPosition.x, scrollPos);


        bool showUpArrow = selectedItem > itemsInViewport/2;
        bool showDownArrow = selectedItem + itemsInViewport/2 < slotUIList.Count;
        upArrow.gameObject.SetActive(showUpArrow);
        downArrow.gameObject.SetActive(showDownArrow);

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
}
