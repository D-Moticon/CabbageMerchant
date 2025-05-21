using System;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Keeps track of all destroyed Item instances so they can be re-spawned later.
/// </summary>
public class ItemGraveyard : MonoBehaviour
{
    // All items currently in the graveyard (deactivated)
    private readonly List<Item> graveyard = new List<Item>();

    private void OnEnable()
    {
        RunManager.RunStartEvent += RunStartedListener;
        RunManager.RunEndedEvent += RunEndedListener;
    }

    private void OnDisable()
    {
        RunManager.RunStartEvent -= RunStartedListener;
        RunManager.RunEndedEvent -= RunEndedListener;
    }

    /// <summary>
    /// Adds an item to the graveyard and deactivates its wrapper (or the item if no wrapper).
    /// </summary>
    public void AddToGraveyard(Item item, bool withFX = false)
    {
        if (item == null || graveyard.Contains(item))
            return;

        graveyard.Add(item);
        if (item.itemWrapper != null)
        {
            // Set the wrapper's parent to the graveyard object
            item.itemWrapper.transform.SetParent(this.transform);
            item.itemWrapper.gameObject.SetActive(false);
        }
        else
        {
            item.gameObject.SetActive(false);
            item.transform.SetParent(this.transform);
        }


        if (withFX)
        {
            Singleton.Instance.itemManager.itemDestroyVFX.Spawn(item.gameObject.transform.position);
            Singleton.Instance.itemManager.itemDestroySFX.Play();
        }
    }

    /// <summary>
    /// Removes an item from the graveyard and reactivates its wrapper (or the item if no wrapper).
    /// </summary>
    public void RemoveFromGraveyard(Item item)
    {
        if (item == null)
            return;

        if (graveyard.Remove(item))
        {
            if (item.itemWrapper != null)
            {
                item.itemWrapper.gameObject.SetActive(true);
            }
            else
            {
                item.gameObject.SetActive(true);
            }
        }
    }

    /// <summary>
    /// Destroys all item wrappers (or item GameObjects) in the graveyard and clears the list.
    /// </summary>
    public void ClearGraveyard()
    {
        for (int i = 0; i < graveyard.Count; i++)
        {
            var item = graveyard[i];
            if (item == null)
                continue;

            if (item.itemWrapper != null)
            {
                Destroy(item.itemWrapper.gameObject);
            }
            else
            {
                Destroy(item.gameObject);
            }
        }
        graveyard.Clear();
    }

    /// <summary>
    /// Retrieves a read-only list of all items currently in the graveyard.
    /// </summary>
    public IReadOnlyList<Item> GetGraveyardContents()
    {
        return graveyard.AsReadOnly();
    }

    void RunEndedListener()
    {
        ClearGraveyard();
    }

    void RunStartedListener(RunManager.RunStartParams rsp)
    {
        ClearGraveyard();
    }
    
    public void RemoveNullItems()
    {
        graveyard.RemoveAll(item => item == null || item.itemWrapper == null);
    }
}
