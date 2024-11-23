using UnityEditor;
using UnityEngine;

public class ConvertToRecttTransform : MonoBehaviour
{
    // This will add a custom option to the Unity Editor under the "Tools" menu
    [MenuItem("Tools/Convert to RectTransform")]
    static void ConvertSelectedToRectTransform()
    {
        // Iterate through all the selected GameObjects in the Project view
        foreach (GameObject obj in Selection.gameObjects)
        {
            // Check if the GameObject is not null and contains a Transform component
            if (obj != null && obj.GetComponent<Transform>() != null)
            {
                // Check if the object has a RectTransform already (skip if it does)
                if (obj.GetComponent<RectTransform>() == null)
                {
                    // Add RectTransform (this replaces the Transform)
                    RectTransform rectTransform = obj.AddComponent<RectTransform>();

                    // Optional: Adjust RectTransform properties if needed, e.g., setting anchors/pivot
                    // rectTransform.anchorMin = Vector2.zero;
                    // rectTransform.anchorMax = Vector2.one;
                    // rectTransform.pivot = new Vector2(0.5f, 0.5f);

                    // Log to confirm the conversion
                    Debug.Log($"Converted {obj.name} to RectTransform.");
                }
                else
                {
                    Debug.Log($"{obj.name} already has a RectTransform.");
                }
            }
        }
    }
}
