using UnityEngine;
using UnityEngine.UI;

public class PlateIconTemplateUI : MonoBehaviour
{
    [SerializeField] private Image _iconSprite;

    public void SetIconSprite(Sprite sprite)
    {
        _iconSprite.sprite = sprite;
    }
}
