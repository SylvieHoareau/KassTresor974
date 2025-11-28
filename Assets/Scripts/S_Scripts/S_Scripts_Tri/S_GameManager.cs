using UnityEngine;

public class S_GameManager : MonoBehaviour
{
    public S_DropSlot[] slots;

    // Update is called once per frame
    void Update()
    {
        if (AllSlotsFilled())
        {
            //Debug.Log("All slots filled correctly! Level Complete!");
            // Trigger level complete actions here (e.g., load next level, show UI, etc.)
        }
    }

    bool AllSlotsFilled()
    {
        foreach (var slot in slots)
        {
            if (slot.transform.childCount == 0)
            {
                return false; // At least one slot is empty
            }
        }
        return true; // All slots are filled
    }
}
