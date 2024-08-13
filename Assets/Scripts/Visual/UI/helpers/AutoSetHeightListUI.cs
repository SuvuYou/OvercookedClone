using UnityEngine;

public class AutoSetHeightListUI : MonoBehaviour
{
    private RectTransform _rect;

    private void Awake()
    {
        _rect = GetComponent<RectTransform>();
    }

    public void SetHeight(RectTransform childElement, int elementsCount, float gapSize)
    {
        if (elementsCount == 0) 
        {
            _rect.sizeDelta = new Vector2(_rect.sizeDelta.x, 0);

            return;
        }

        float height = (childElement.sizeDelta.y * elementsCount) + (gapSize * (elementsCount - 1));

        _rect.sizeDelta = new Vector2(_rect.sizeDelta.x, height);
    }
}
