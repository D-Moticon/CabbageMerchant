using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEngine.Serialization;

public class ItemManager : MonoBehaviour
{
    [FormerlySerializedAs("itemManagerMainParent")] public GameObject hideableParent;
    public ItemSlot itemSlotPrefab;
    public ItemWrapper itemWrapperPrefab;
    public List<ItemSlot> itemSlots = new List<ItemSlot>();
    public int startingLockedSlots = 4;
    
    [Header("Grid Dimensions")]
    public int rows = 4;
    public int columns = 2;

    [Header("Layout Settings")]
    public Vector2 spacing = new Vector2(1f, 1f);
    public Vector2 slotSize = new Vector2(1f, 1f);
    
    public PetDefinition startingPet;
    public ItemSlot petSlot;
    
    public delegate void ItemDelegate(Item item);
    public static event ItemDelegate ItemPurchasedEvent;
    public static event ItemDelegate ItemSpawnedEvent;
    public static event ItemDelegate ItemSoldEvent;

    public delegate void ItemSlotDelegate(Item item, ItemSlot slot);

    public static event ItemSlotDelegate ItemAddedToSlotEvent;
    public static ItemDelegate ItemClickedEvent;

    public delegate void TwoItemDelegate(Item itemA, Item itemB);

    public static event TwoItemDelegate ItemsMergedEvent;
    public static event ItemDelegate ItemResultedFromMergeEvent;

    [Header("Sell Settings")]
    public Collider2D sellCollider;
    public ParticleSystem sellVFX;
    public SFXInfo sellSFX;
    public SFXInfo consumeSFX;
    public FloaterReference sellFloater;
    public FloaterReference infoFloater;

    [Header("Merge Settings")]
    public ParticleSystem mergeVFX;
    public SFXInfo mergeSFX;
    
    [Header("Perk Settings")]
    public Transform perkParent;
    public float perkAreaXSize = 5f;

    // Drag-and-drop
    private Item draggingItem = null;
    private Vector3 draggingStartPos;
    private ItemSlot draggingStartSlot;

    public SFXInfo itemDestroySFX;
    public PooledObjectData itemDestroyVFX;
    private bool sellDisabled = false;

    [Header("Keys")] public PooledObjectData keyPooledObject;
    
    private void OnEnable()
    {
        RunManager.RunStartEvent += RunStartListener;
        RunManager.RunEndedEvent += RunEndListener;
        RunManager.SceneChangedEvent += SceneChangedListener;
        BuildManager.FullGameStartedEvent += FullGameStartedListener;
    }

    private void OnDisable()
    {
        RunManager.RunStartEvent -= RunStartListener;
        RunManager.RunEndedEvent -= RunEndListener;
        RunManager.SceneChangedEvent -= SceneChangedListener;
        BuildManager.FullGameStartedEvent -= FullGameStartedListener;
    }

    private void Update()
    {
        HandleItemDragging();
        HandleItemHoverAndMerging();
    }

    public void AddItemToInventoryFromPrefab(Item itemPrefab, bool forceHolofoil = false)
    {
        ItemSlot itemSlot = GetEmptySlot();
        if (itemSlot == null)
        {
            return;
        }
        
        Item item = GenerateItemWithWrapper(itemPrefab);
        AddItemToSlot(item, itemSlot);
        if (forceHolofoil)
        {
            item.SetHolofoil();
        }
        
        ItemPurchasedEvent?.Invoke(item);
    }

    public void AddPerkFromPrefab(Item itemPrefab, bool forceHolofoil = false)
    {
        Item perkItem = GenerateItemWithWrapper(itemPrefab);
        perkItem.itemWrapper.transform.SetParent(perkParent);
        perkItem.itemWrapper.transform.localPosition = Vector3.zero;
        if (forceHolofoil)
        {
            perkItem.SetHolofoil();
        }

        perkItem.purchasable = false;
        ItemPurchasedEvent?.Invoke(perkItem);
    }

    ItemSlot GetEmptySlot()
    {
        for (int i = 0; i < itemSlots.Count(); i++)
        {
            if (itemSlots[i].currentItem == null)
            {
                return itemSlots[i];
            }
        }

        return null;
    }
    
    public void AddPet(PetDefinition def)
    {
        if (petSlot == null)
            return;

        // destroy existing pet item
        if (petSlot.currentItem != null)
        {
            Destroy(petSlot.currentItem.itemWrapper.gameObject);
            petSlot.currentItem = null;
        }

        if (def == null)
            return;

        // instantiate new pet item
        Item petItem = GenerateItemWithWrapper(def.itemPrefab);
        petSlot.currentItem = petItem;
        petItem.currentItemSlot = petSlot;
        petItem.itemWrapper.transform.SetParent(petSlot.transform);
        petItem.itemWrapper.transform.localPosition = Vector3.zero;
        ItemAddedToSlotEvent?.Invoke(petItem, petSlot);
    }
    
    //==================================
    // DRAG + DROP
    //==================================
    private void HandleItemDragging()
    {
        Vector2 mouseWorldPos = Singleton.Instance.playerInputManager.mousePosWorldSpace;

        if (Singleton.Instance.playerInputManager.fireDown)
        {
            StartDragIfPossible(mouseWorldPos);
            ClickClickableIfPossible(mouseWorldPos);
        }
        else if (Singleton.Instance.playerInputManager.fireHeld && draggingItem != null)
        {
            // Move item with mouse
            draggingItem.itemWrapper.transform.position = mouseWorldPos;
        }
        else if (Singleton.Instance.playerInputManager.fireUp && draggingItem != null)
        {
            AttemptToPlaceItem(mouseWorldPos);

            // re-enable collider
            Collider2D dragColl = draggingItem.itemWrapper.GetComponent<Collider2D>();
            if (dragColl) dragColl.enabled = true;

            // hide override tooltip
            Singleton.Instance.toolTip.HideTooltip();
            draggingItem = null;
        }
        
        UpdateSellButtonLabel();
    }

    void ClickClickableIfPossible(Vector2 mouseWorldPos)
    {
        if (draggingItem != null) return;
        Collider2D col = Physics2D.OverlapPoint(mouseWorldPos);
        if (!col) return;
        ClickableObject clickableObject = col.GetComponentInChildren<ClickableObject>();
        if (clickableObject == null)
        {
            return;
        }
        clickableObject.TryClick();
    }
    
    private void StartDragIfPossible(Vector2 mouseWorldPos)
    {
        if (draggingItem != null) return;

        Collider2D col = Physics2D.OverlapPoint(mouseWorldPos);
        if (!col) return;

        // 1) If they clicked on a locked slot, try to unlock
        var slot = col.GetComponentInParent<ItemSlot>();
        if (slot != null && slot.isLocked)
        {
            if (Singleton.Instance.playerStats.numberKeys > 0)
            {
                Singleton.Instance.playerStats.RemoveKey(1);
                slot.UnLockSlot();
            }
            else
            {
                infoFloater.Spawn("No keys!", slot.transform.position, Color.red);
            }
            return; // block any drag from this click
        }
        
        Item clickedItem = col.GetComponentInChildren<Item>();
        if (clickedItem == null || clickedItem.itemType == Item.ItemType.Pet) return;

        // If perk, single-click buy
        if (clickedItem.itemType == Item.ItemType.Perk)
        {
            HandlePerkClickPurchase(clickedItem);
            return;
        }

        // Otherwise normal item logic:
        if (clickedItem.purchasable)
        {
            ItemClickedEvent?.Invoke(clickedItem);
            double cost = clickedItem.GetItemPrice();
            if (Singleton.Instance.playerStats.coins < cost)
            {
                Debug.Log("Cannot afford this item, cannot drag.");
                return;
            }
        }

        // Start dragging
        draggingItem = clickedItem;
        draggingStartPos = draggingItem.itemWrapper.transform.position;
        draggingStartSlot = draggingItem.currentItemSlot;

        if (draggingStartSlot != null)
        {
            draggingStartSlot.currentItem = null;
            draggingStartSlot.HidePriceText();
            draggingItem.currentItemSlot = null;
        }

        // disable collider
        Collider2D dragColl = draggingItem.itemWrapper.GetComponent<Collider2D>();
        if (dragColl) dragColl.enabled = false;
        
        UpdateSellButtonLabel();
    }

    private void AttemptToPlaceItem(Vector2 mouseWorldPos)
    {
        // First check if we are dropping on the sellCollider
        if (sellCollider && sellCollider.OverlapPoint(mouseWorldPos))
        {
            // Allow eating if it's a consumable, or selling if it’s already in inventory (non-purchasable)
            if (draggingItem.itemType == Item.ItemType.Consumable
                || !draggingItem.purchasable)
            {
                if (sellDisabled)
                {
                    infoFloater.Spawn("Can't sell right now!", draggingItem.transform.position, Color.red);
                    RevertDraggedItem();
                }
                else
                {
                    // Deduct coins: consumable “cost” is same as its sell value?
                    double value = draggingItem.GetSellValue();
                    if (draggingItem.itemType == Item.ItemType.Consumable && draggingItem.purchasable)
                    {
                        value = draggingItem.GetItemPrice();
                        Singleton.Instance.playerStats.AddCoins(-value);
                    }

                    else
                    {
                        Singleton.Instance.playerStats.AddCoins(value);
                    }
                    

                    // Play VFX/SFX
                    sellVFX.transform.position = draggingItem.transform.position;
                    if (draggingItem.itemType == Item.ItemType.Consumable)
                    {
                        sellVFX.Play();
                        consumeSFX.Play();
                    }
                    else
                    {
                        sellVFX.Play();
                        sellSFX.Play();
                    }

                    // Show float text
                    sellFloater.Spawn(value.ToString(), draggingItem.transform.position, Color.white);
                    ItemSoldEvent?.Invoke(draggingItem);

                    // Destroy it immediately
                    draggingItem.DestroyItem(false, true);
                }
            }
            else
            {
                // Not a consumable and still purchasable → cannot sell
                RevertDraggedItem();
            }

            // Reset label back to “Sell”
            RevertSellButtonLabel();
            return;
        }

        // Otherwise check if we are dropping on an inventory slot
        Collider2D col = Physics2D.OverlapPoint(mouseWorldPos);
        ItemSlot slot = col ? col.GetComponentInParent<ItemSlot>() : null;

        if (slot != null && slot.isLocked)
        {
            infoFloater.Spawn("Slot is locked!", slot.transform.position, Color.red);
            RevertDraggedItem();
            return;
        }
        
        //Check if it's an event slot
        if (slot == null)
        {
            RevertDraggedItem();
            return;
        }

        if (slot.isEventSlot)
        {
            switch (slot.allowedTypes)
            {
                case ItemSlot.AllowedTypes.any:
                    break;
                case ItemSlot.AllowedTypes.itemOnly:
                    if (draggingItem.itemType != Item.ItemType.Item)
                    {
                        RevertDraggedItem();
                        infoFloater.Spawn("Only normal items allowed!", slot.transform.position, Color.red, 1f);
                        return;
                    }
                    break;
                case ItemSlot.AllowedTypes.perkOnly:
                    if (draggingItem.itemType != Item.ItemType.Perk)
                    {
                        RevertDraggedItem();
                        infoFloater.Spawn("Only perks allowed!", slot.transform.position, Color.red, 1f);
                        return;
                    }
                    break;
                case ItemSlot.AllowedTypes.weaponOnly:
                    if (draggingItem.itemType != Item.ItemType.Perk)
                    {
                        RevertDraggedItem();
                        infoFloater.Spawn("Only weapons allowed!", slot.transform.position, Color.red, 1f);
                        return;
                    }
                    break;
                case ItemSlot.AllowedTypes.itemOrConsumable:
                    if (draggingItem.itemType != Item.ItemType.Item && draggingItem.itemType != Item.ItemType.Consumable)
                    {
                        RevertDraggedItem();
                        infoFloater.Spawn("Only normal items and consumables allowed!", slot.transform.position, Color.red, 1f);
                        return;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            AddItemToSlot(draggingItem, slot);
            return;
        }
        
        if (!IsInventorySlot(slot))
        {
            // not a valid slot => revert
            RevertDraggedItem();
            return;
        }

        // Now we know it's an inventory slot
        if (slot.currentItem == null)
        {
            // ——— prevent having two weapons in inventory ———
            if (draggingItem.itemType == Item.ItemType.Weapon)
            {
                bool alreadyHasWeapon = itemSlots.Any(s =>
                    s.currentItem != null &&
                    s.currentItem.itemType == Item.ItemType.Weapon
                );
                if (alreadyHasWeapon)
                {
                    infoFloater.Spawn(
                        "You can only equip one weapon!",
                        slot.transform.position,
                        Color.red
                    );
                    RevertDraggedItem();
                    return;
                }
            }

            AddItemToSlot(draggingItem, slot);
            ItemAddedToSlotEvent?.Invoke(draggingItem, slot);
            
            // ——— normal purchase (if applicable) + place ———
            if (draggingItem.purchasable)
            {
                double cost = draggingItem.GetItemPrice();
                Singleton.Instance.playerStats.AddCoins(-cost);
                draggingItem.purchasable = false;
                ItemPurchasedEvent?.Invoke(draggingItem);
            }
 
        }
        
        else
        {
            // ——— 1) If both items came from inventory AND names differ, swap them ———
            if (draggingStartSlot != null &&
                IsInventorySlot(slot) &&
                IsInventorySlot(draggingStartSlot) &&
                draggingItem.itemName != slot.currentItem.itemName)
            {
                SwapItemsBetweenSlots(draggingStartSlot, slot);
                return;
            }

            // ——— 2) Otherwise, if they’re the same item and upgradable, merge ———
            if (CheckForDuplicateMerge(draggingItem, slot.currentItem, slot))
            {
                mergeVFX.transform.position = slot.transform.position;
                mergeVFX.Play();
                mergeSFX.Play();
            }
            else
            {
                // ——— 3) Fallback: can’t merge or swap, so revert ———
                RevertDraggedItem();
            }
        }
    }

    //===================================
    // MERGING
    //===================================
    private bool CheckForDuplicateMerge(Item dragged, Item inSlot, ItemSlot slot)
    {
        if (dragged.itemName == inSlot.itemName && dragged.upgradedItem != null)
        {
            // 1) figure out holofoil & upgrade‐flag state up front
            bool isHolofoil = inSlot.isHolofoil || dragged.isHolofoil;
            bool keep       = dragged.keepTriggerOnUpgrade || inSlot.keepTriggerOnUpgrade;

            // if we need to keep triggers, pick the one that had the flag
            List<Trigger> triggersToCopy = null;
            if (keep)
            {
                var src = dragged.keepTriggerOnUpgrade ? dragged : inSlot;
                triggersToCopy = src.triggers;
            }

            ItemsMergedEvent?.Invoke(dragged, inSlot);
            
            // 2) destroy the old wrappers/items
            Destroy(inSlot.itemWrapper.gameObject);
            Destroy(dragged.itemWrapper.gameObject);

            // 3) create the new upgraded item
            Item upgraded = GenerateItemWithWrapper(dragged.upgradedItem);
            if (isHolofoil) upgraded.SetHolofoil();
            AddItemToSlot(upgraded, slot);
            ItemAddedToSlotEvent?.Invoke(upgraded, slot);
            ItemResultedFromMergeEvent?.Invoke(upgraded);
            
            // 4) if we’re carrying over triggers, deep‐clone & init them
            if (keep && triggersToCopy != null)
            {
                foreach (Trigger t in upgraded.triggers)
                {
                    t.RemoveTrigger(upgraded);
                }
                upgraded.triggers = new List<Trigger>();
                foreach (var trig in triggersToCopy)
                {
                    // deep‐clone the trigger instance
                    var clone = Helpers.DeepClone(trig);
                    clone.owningItem = upgraded;
                    clone.InitializeTrigger(upgraded);
                    upgraded.triggers.Add(clone);
                }
                // carry the flag forward
                upgraded.keepTriggerOnUpgrade = true;
            }

            // 5) handle shop payment & purchased event
            if (dragged.purchasable)
            {
                double cost = dragged.GetItemPrice();
                Singleton.Instance.playerStats.AddCoins(-cost);
                dragged.purchasable = false;
                ItemPurchasedEvent?.Invoke(upgraded);
            }

            return true;
        }

        return false;
    }


    private void RevertDraggedItem()
    {
        if (draggingItem == null) return;

        if (draggingStartSlot != null)
        {
            AddItemToSlot(draggingItem, draggingStartSlot);
            if (draggingItem.purchasable)
            {
                draggingStartSlot.ShowPriceText();
            }
        }
        else
        {
            // put back in shop or do nothing
            draggingItem.itemWrapper.transform.position = draggingStartPos;
            Debug.Log("Item put back to shop location or left in world space.");
        }
    }

    //===================================
    // PERK LOGIC
    //===================================
    private void HandlePerkClickPurchase(Item perkItem)
    {
        if (perkItem.purchasable)
        {
            double cost = perkItem.GetItemPrice();
            if (Singleton.Instance.playerStats.coins < cost)
            {
                Debug.Log("Cannot afford perk.");
                return;
            }

            Singleton.Instance.playerStats.AddCoins(-cost);
            perkItem.purchasable = false;
            ItemPurchasedEvent?.Invoke(perkItem);
        }

        perkItem.itemWrapper.transform.SetParent(perkParent);
        perkItem.itemWrapper.transform.localPosition = Vector3.zero; 
        if (perkItem.currentItemSlot != null)
        {
            perkItem.currentItemSlot.currentItem = null;
            perkItem.currentItemSlot.HidePriceText();
            perkItem.currentItemSlot = null;
        }

        ItemPurchasedEvent?.Invoke(perkItem);
        DistributePerks();
    }

    //===================================
    // MERGE TOOLTIP
    //===================================
    private void HandleItemHoverAndMerging()
    {
        if (draggingItem == null) return;
        if (draggingItem.upgradedItem == null) return;

        Vector2 mousePos = Singleton.Instance.playerInputManager.mousePosWorldSpace;
        Collider2D col = Physics2D.OverlapPoint(mousePos);
        if (col)
        {
            Item hoveredItem = col.GetComponentInChildren<Item>();
            if (hoveredItem != null)
            {
                if (hoveredItem == draggingItem)
                {
                    Singleton.Instance.toolTip.HideTooltip();
                    return;
                }

                if (hoveredItem.itemName == draggingItem.itemName)
                {
                    // show override
                    HoverableModifier hm = new HoverableModifier();
                    hm.isHolofoil = draggingItem.isHolofoil;
                    Singleton.Instance.toolTip.ShowOverrideTooltip(draggingItem.upgradedItem, hm);
                    return;
                }
            }
        }
        Singleton.Instance.toolTip.HideTooltip();
    }

    //===================================
    // SELLING: HANDLED ABOVE
    //===================================

    //===================================
    // PERK LAYOUT
    //===================================
    private void DistributePerks()
    {
        // gather perk children
        List<Transform> perkChildren = new List<Transform>();
        foreach (Transform child in perkParent)
        {
            if (child.GetComponentInChildren<Item>() != null)
            {
                perkChildren.Add(child);
            }
        }

        if (perkChildren.Count == 0) return;
        float halfWidth = perkAreaXSize * 0.5f;
        float step = perkAreaXSize / (perkChildren.Count + 1);

        for (int i = 0; i < perkChildren.Count; i++)
        {
            float x = -halfWidth + step * (i + 1);
            perkChildren[i].localPosition = new Vector3(x, 0f, 0f);
        }
    }

    //===================================
    // INVENTORY UTILS
    //===================================
    private bool IsInventorySlot(ItemSlot slot)
    {
        return itemSlots.Contains(slot);
    }

    public Item GenerateItemWithWrapper(Item itemPrefab, Vector2 pos = default, Transform parent = null)
    {
        ItemWrapper iw = Instantiate(itemWrapperPrefab, pos, Quaternion.identity);
        Item item = Instantiate(itemPrefab, iw.transform);
        item.transform.localPosition = Vector2.zero;
        iw.transform.SetParent(parent);

        iw.spriteRenderer.sprite = item.icon;
        iw.item = item;
        item.itemWrapper = iw;
        iw.InitializeItemWrapper(item);
        
        item.InitializeItemAfterWrapperCreated();

        if (item.holofoilEffects != null && item.holofoilEffects.Count > 0)
        {
            float holoRand = UnityEngine.Random.Range(0f, 1f);
            if (holoRand <= Singleton.Instance.playerStats.holofoilChance)
            {
                item.SetHolofoil();
            }
        }

        if (item.customMaterial != null)
        {
            iw.spriteRenderer.material = item.customMaterial;
        }

        ItemSpawnedEvent?.Invoke(item);
        
        return item;
    }

    public void AddItemToSlot(Item item, ItemSlot itemSlot)
    {
        itemSlot.currentItem = item;
        item.currentItemSlot = itemSlot;
        item.itemWrapper.transform.position = itemSlot.transform.position;
        item.itemWrapper.transform.SetParent(itemSlot.transform);
        itemSlot.PlayItemAddedToSlotFX();
        if (itemSlot.isFrozen)
        {
            item.FreezeItem();
        }
        else
        {
            item.UnFreezeItem();
        }
        if (Singleton.Instance.buildManager.buildMode == BuildManager.BuildMode.release)
        {
            Debug.Log($"{item.itemName} added to slot {itemSlot.slotNumber}");
        }
    }
    
    private void SwapItemsBetweenSlots(ItemSlot fromSlot, ItemSlot toSlot)
    {
        // remember the items
        Item movedItem    = draggingItem;          // the one you’re dragging
        Item residentItem = toSlot.currentItem;    // the one already in the target slot

        // place the dragged item into the target
        AddItemToSlot(movedItem, toSlot);
        ItemAddedToSlotEvent?.Invoke(movedItem, toSlot);

        // place the resident item back into the original slot
        AddItemToSlot(residentItem, fromSlot);
        ItemAddedToSlotEvent?.Invoke(residentItem, fromSlot);

        // clear our drag state
        //draggingItem = null;
    }

    private void GenerateItemSlots()
    {
        foreach (var oldSlot in itemSlots)
        {
            if (oldSlot != null)
                Destroy(oldSlot.gameObject);
        }
        
        itemSlots.Clear();

        Vector2 gridSize = new Vector2(columns * spacing.x, rows * spacing.y);
        Vector2 startPos = (Vector2)transform.position + new Vector2(
            -gridSize.x * 0.5f + spacing.x * 0.5f,
            gridSize.y * 0.5f - spacing.y * 0.5f
        );

        int slotNumber = 0;
        
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                Vector2 pos = startPos + new Vector2(col * spacing.x, -row * spacing.y);
                ItemSlot newSlot = Instantiate(itemSlotPrefab, pos, Quaternion.identity, transform);
                newSlot.transform.SetParent(hideableParent.transform);
                itemSlots.Add(newSlot);
                newSlot.SetSlotNumber(slotNumber);
                slotNumber++;
            }
        }
    }

    public void MoveItemsToEmptyInventorySlots(List<Item> items, float moveDuration = 0.5f)
    {
        // 1) Only consider items NOT already in an inventory slot
        var itemsToMove = items
            .Where(i => i.currentItemSlot == null || !itemSlots.Contains(i.currentItemSlot))
            .ToList();

        // 2) Find all free inventory slots
        var emptySlots = itemSlots.Where(s => s.currentItem == null && !s.isLocked).ToList();

        // 3) Kick off the sequenced mover
        StartCoroutine(MoveItemsSequentially(itemsToMove, emptySlots, moveDuration));
    }
    
    public void MoveItemToEmptyInventorySlot(Item item, float moveDuration = 0.5f)
    {
        if (item.currentItemSlot != null && itemSlots.Contains(item.currentItemSlot))
        {
            return;
        }

        // 2) Find all free inventory slots
        var emptySlots = itemSlots.Where(s => s.currentItem == null).ToList();

        // 3) Kick off the sequenced mover
        StartCoroutine(MoveItemToSlotRoutine(item, emptySlots[0], moveDuration));
    }

    private IEnumerator MoveItemsSequentially(List<Item> items, List<ItemSlot> slots, float duration)
    {
        // A) Check if there's already a weapon in your current inventory
        bool hasWeapon = itemSlots.Any(s =>
            s.currentItem != null &&
            s.currentItem.itemType == Item.ItemType.Weapon
        );

        int slotIndex = 0;

        // B) Walk through each item, filling slots in order
        foreach (var item in items)
        {
            if (slotIndex >= slots.Count)
                yield break; // no more room

            // If it's a weapon and we already have one, skip it
            if (item.itemType == Item.ItemType.Weapon)
            {
                if (hasWeapon)
                    continue;
                // first weapon we move becomes our one allowed weapon
                hasWeapon = true;
            }

            // animate into the next free slot
            yield return StartCoroutine(MoveItemToSlotRoutine(item, slots[slotIndex], duration));
            slotIndex++;
        }
    }

    // your existing MoveItemToSlotRoutine stays unchanged
    private IEnumerator MoveItemToSlotRoutine(Item item, ItemSlot slot, float duration)
    {
        var col = item.itemWrapper.GetComponent<Collider2D>();
        if (col) col.enabled = false;

        Vector3 startPos = item.itemWrapper.transform.position;
        Vector3 endPos   = slot.transform.position;
        float elapsed    = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            item.itemWrapper.transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        // finalize
        AddItemToSlot(item, slot);
        ItemAddedToSlotEvent?.Invoke(item, slot);

        if (col) col.enabled = true;
    }

    public int GetNumberOfItemsInInventory()
    {
        int num = 0;
        for (int i = 0; i < itemSlots.Count; i++)
        {
            if (itemSlots[i].currentItem != null)
            {
                num++;
            }
        }

        return num;
    }
    
    /// <summary>
    /// Returns all Items currently sitting in the inventory slots (ignores empty slots).
    /// </summary>
    public List<Item> GetItemsInInventory()
    {
        return itemSlots
            .Where(slot => slot.currentItem != null)
            .Select(slot => slot.currentItem)
            .ToList();
    }

    /// <summary>
    /// Returns all Items in inventory slots AND all perk Items (children of perkParent).
    /// </summary>
    public List<Item> GetItemsAndPerks()
    {
        // Inventory items
        var items = GetItemsInInventory();

        // Perk items: any Item under perkParent that's not in an inventory slot
        items.AddRange(
            perkParent
                .GetComponentsInChildren<Item>()
                .Where(perk => perk.currentItemSlot == null)
        );

        return items;
    }
    
    public List<Item> GetNormalItems()
    {
        // Inventory items
        var items = GetItemsInInventory().Where(x=>x.itemType==Item.ItemType.Item).ToList();

        // Perk items: any Item under perkParent that's not in an inventory slot
        items.AddRange(
            perkParent
                .GetComponentsInChildren<Item>()
                .Where(item => item.itemType == Item.ItemType.Item)
        );

        return items;
    }
    
    public List<Item> GetNormalItemsAndConsumables()
    {
        // Inventory items
        var items = GetItemsInInventory().Where(x=>(x.itemType==Item.ItemType.Item || x.itemType==Item.ItemType.Consumable)).ToList();

        return items;
    }

    public List<Item> GetItemsConsumablesAndPerks()
    {
        var items = GetItemsInInventory().Where(x=>(x.itemType==Item.ItemType.Item || x.itemType==Item.ItemType.Consumable)).ToList();
        items.AddRange(
            perkParent
                .GetComponentsInChildren<Item>()
                .Where(perk => perk.currentItemSlot == null)
        );
        return items;
    }

    public List<Item> GetWeapons()
    {
        var items = GetItemsInInventory().Where(x=>(x.itemType==Item.ItemType.Weapon)).ToList();
        return items;
    }

    /// <summary>
    /// Completely removes an Item from the game: unhooks it from any slot or perk,
    /// destroys its wrapper (and thus the Item component), and clears references.
    /// </summary>
    public void DestroyItem(Item item, bool sendToGraveyard = false)
    {
        if (item == null)
            return;

        // 1) Remove from its inventory slot (if any)
        if (item.currentItemSlot != null)
        {
            item.currentItemSlot.currentItem = null;
            item.currentItemSlot.HidePriceText();
            item.currentItemSlot = null;
        }

        // 2) Detach from perkParent if it is a perk
        if (item.itemWrapper != null && item.itemWrapper.transform.parent == perkParent)
        {
            item.itemWrapper.transform.SetParent(null);
        }


        if (itemDestroyVFX != null)
        {
            itemDestroyVFX.Spawn(item.transform.position);
        }

        itemDestroySFX.Play();
        
        // 3) Destroy the visual wrapper (this also destroys the Item component)
        if (item.itemWrapper != null)
        {
            item.DestroyItem(false,sendToGraveyard);
        }
    }
    
    void RunStartListener(RunManager.RunStartParams rsp)
    {
        GenerateItemSlots();
        DestroyAllItemsAndLockSlots();
    }

    void RunEndListener()
    {
        GenerateItemSlots();
        DestroyAllItemsAndLockSlots();
    }

    void DestroyAllItemsAndLockSlots()
    {
        // 1) Destroy and clear all inventory items
        var inventoryItems = GetItemsInInventory().ToList();
        foreach (var item in inventoryItems)
        {
            DestroyItem(item);
        }

        // 2) Destroy and clear all perk items
        //    (GetItemsAndPerks includes inventory; so exclude those already removed)
        var perkItems = GetItemsAndPerks()
            .Except(inventoryItems)
            .ToList();
        foreach (var perk in perkItems)
        {
            DestroyItem(perk);
        }
        
        LockInventorySlots(startingLockedSlots);
    }

    public int GetWeaponCount()
    {
        int count = 0;
        for (int i = 0; i < itemSlots.Count; i++)
        {
            if (itemSlots[i].currentItem == null)
            {
                continue;
            }
            if (itemSlots[i].currentItem.itemType == Item.ItemType.Weapon)
            {
                count++;
            }
        }

        return count;
    }

    public void DisableSell()
    {
        sellDisabled = true;
    }

    public void EnableSell()
    {
        sellDisabled = false;
    }
    
    public void LockInventorySlots(int number)
    {
        int totalSlots = itemSlots.Count;
        // clamp to [0, totalSlots]
        int toLock = Mathf.Clamp(number, 0, totalSlots);
        for (int i = 0; i < totalSlots; i++)
        {
            // if this index is in the “last toLock” range → lock, otherwise unlock
            if (i >= totalSlots - toLock)
            {
                itemSlots[i].LockSlot();
            }
            else
            {
                itemSlots[i].UnLockSlot(false);
            }
        }
    }

    public void UnLockAllInventorySlots()
    {
        int totalSlots = itemSlots.Count;
        for (int i = 0; i < totalSlots; i++)
        {
            itemSlots[i].UnLockSlot(true);
        }
    }

    void SceneChangedListener(string sceneName)
    {
        if (sceneName == "Overworld")
        {
            HideItemManager();
        }

        else
        {
            ShowItemManager();
        }
    }

    public void HideItemManager()
    {
        hideableParent.SetActive(false);
    }

    public void ShowItemManager()
    {
        hideableParent.SetActive(true);
    }

    void FullGameStartedListener()
    {
        HideItemManager();
    }
    
    private void UpdateSellButtonLabel()
    {
        if (draggingItem != null 
            && draggingItem.itemType == Item.ItemType.Consumable)
        {
            sellCollider.GetComponentInChildren<TMPro.TMP_Text>().text = "Eat";
        }
        else
        {
            sellCollider.GetComponentInChildren<TMPro.TMP_Text>().text = "Sell";
        }
    }

    void RevertSellButtonLabel()
    {
        sellCollider.GetComponentInChildren<TMPro.TMP_Text>().text = "Sell";
    }

    public Item UpgradeItem(Item item)
    {
        if (item.upgradedItem == null)
        {
            return null;
        }
        
        ItemSlot slot = item.currentItemSlot;
        
        bool keep = item.keepTriggerOnUpgrade;
        List<Trigger> triggersToCopy = null;
        if (keep)
        {
            triggersToCopy = item.triggers;
        }
        
        Item upgraded = GenerateItemWithWrapper(item.upgradedItem);
        if (item.isHolofoil) upgraded.SetHolofoil();
        if (item.purchasable) upgraded.purchasable = true;
        if(item.isMysterious) upgraded.MakeItemMysterious();
        
        item.DestroyItem(false,false);
        
        if (slot != null)
        {
            AddItemToSlot(upgraded, slot);
            ItemAddedToSlotEvent?.Invoke(upgraded, slot);
            slot.SetPriceText();
            
        }

        // 4) if we’re carrying over triggers, deep‐clone & init them
        if (keep && triggersToCopy != null)
        {
            foreach (Trigger t in upgraded.triggers)
            {
                t.RemoveTrigger(upgraded);
            }
            upgraded.triggers = new List<Trigger>();
            foreach (var trig in triggersToCopy)
            {
                // deep‐clone the trigger instance
                var clone = Helpers.DeepClone(trig);
                clone.owningItem = upgraded;
                clone.InitializeTrigger(upgraded);
                upgraded.triggers.Add(clone);
            }
            // carry the flag forward
            upgraded.keepTriggerOnUpgrade = true;
        }

        if (Singleton.Instance.buildManager.buildMode == BuildManager.BuildMode.release)
        {
            Debug.Log($"{item.itemName} upgraded to {upgraded.itemName}");
        }
        
        return upgraded;
    }
}
