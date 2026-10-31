using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip itemPickupSound;

    [Header("Generic Components")]
    public int fuseCount;
    public int coolantCanisterCount;

    [Header("Specific Items")]
    public List<string> specificItemIDs =
        new List<string>();

    [Header("Access Cards")]
    public List<string> accessCardIDs =
        new List<string>();

    [Header("Keys")]
    public List<string> keyIDs =
        new List<string>();

    [Header("Authorized Gates")]
    public List<string> authorizedGateIDs =
        new List<string>();

    [Header("Generator Tasks")]
    public bool maintenanceTaskComplete;
    public bool controlTaskComplete;

    public bool hasFuse
    {
        get
        {
            return fuseCount > 0;
        }

        set
        {
            fuseCount = value ? 1 : 0;
        }
    }

    public bool hasCoolantCanister
    {
        get
        {
            return coolantCanisterCount > 0;
        }

        set
        {
            coolantCanisterCount = value ? 1 : 0;
        }
    }

    public void AddFuse()
    {
        fuseCount++;
        PlayItemPickupSound();
    }

    public bool ConsumeFuse()
    {
        if (fuseCount <= 0)
            return false;

        fuseCount--;
        return true;
    }

    public void AddCoolantCanister()
    {
        coolantCanisterCount++;
        PlayItemPickupSound();
    }

    public bool ConsumeCoolantCanister()
    {
        if (coolantCanisterCount <= 0)
            return false;

        coolantCanisterCount--;
        return true;
    }

    public void AddSpecificItem(string itemID)
    {
        if (string.IsNullOrWhiteSpace(itemID))
            return;

        if (!specificItemIDs.Contains(itemID))
        {
            specificItemIDs.Add(itemID);
            PlayItemPickupSound();
        }
    }

    public bool HasSpecificItem(string itemID)
    {
        return specificItemIDs.Contains(itemID);
    }

    public bool ConsumeSpecificItem(string itemID)
    {
        if (!specificItemIDs.Contains(itemID))
            return false;

        specificItemIDs.Remove(itemID);
        return true;
    }

    public void AddAccessCard(string cardID)
    {
        if (string.IsNullOrWhiteSpace(cardID))
            return;

        if (!accessCardIDs.Contains(cardID))
        {
            accessCardIDs.Add(cardID);
            PlayItemPickupSound();
        }
    }

    public bool HasAccessCard(string cardID)
    {
        return accessCardIDs.Contains(cardID);
    }

    public bool ConsumeAccessCard(string cardID)
    {
        if (!accessCardIDs.Contains(cardID))
            return false;

        accessCardIDs.Remove(cardID);
        return true;
    }

    public void AddKey(string keyID)
    {
        if (string.IsNullOrWhiteSpace(keyID))
            return;

        if (!keyIDs.Contains(keyID))
        {
            keyIDs.Add(keyID);
            PlayItemPickupSound();
        }
    }

    public bool HasKey(string keyID)
    {
        return keyIDs.Contains(keyID);
    }

    public bool ConsumeKey(string keyID)
    {
        if (!keyIDs.Contains(keyID))
            return false;

        keyIDs.Remove(keyID);
        return true;
    }

    public bool IsGateAuthorized(string gateID)
    {
        return authorizedGateIDs.Contains(gateID);
    }

    public void AuthorizeGate(string gateID)
    {
        if (string.IsNullOrWhiteSpace(gateID))
            return;

        if (!authorizedGateIDs.Contains(gateID))
            authorizedGateIDs.Add(gateID);
    }

    private void PlayItemPickupSound()
    {
        if (audioSource != null &&
            itemPickupSound != null)
        {
            audioSource.PlayOneShot(itemPickupSound);
        }
    }
}