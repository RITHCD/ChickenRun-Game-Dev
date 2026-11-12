using UnityEngine;

public class GroundTile : MonoBehaviour
{
    public Transform items;   // empty child where obstacles and eggs spawn

    public void ClearItems()
    {
        for (int i = items.childCount - 1; i >= 0; i--)
            Destroy(items.GetChild(i).gameObject);
    }
}