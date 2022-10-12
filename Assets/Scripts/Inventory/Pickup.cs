using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour, Interactable, ISavable
{
    [SerializeField] ItemBase item;
    [SerializeField] int count = 1;

    public bool Used { get; set; } = false;

    public IEnumerator Interact(Transform initiator)
    {
        if (!Used)
        {
            initiator.GetComponent<Inventory>().AddItem(item, count);
            Used = true;
            DisablePickup();

            string playerName = initiator.GetComponent<PlayerController>().Name;

            yield return DialogManager.Instance.ShowDialogText($"{playerName} found {count}x {item.Name}(s)!");
        }
    }

    void DisablePickup()
    {
        GetComponent<SpriteRenderer>().enabled = false; // Disable instead of destroy to allow it to be saved
        GetComponent<BoxCollider2D>().enabled = false;
    }

    public object CaptureState()
    {
        return Used;
    }


    public void RestoreState(object state)
    {
        Used = (bool)state;

        if (Used)
        {
            DisablePickup();
        }
    }
}
