using UnityEngine;
using UnityEngine.EventSystems;

public class FocusScale : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private Outline outline;

    private bool isSelected = false;

    private void Awake()
    {
        outline = GetComponent<Outline>();
    }

    private void Start()
    {
        outline.OutlineColor = Color.white;
        outline.enabled = false;
    }

    public void OnPointerEnter(PointerEventData _)
    {
        FocusingOutline(true);
    }

    public void OnPointerExit(PointerEventData _)
    {
        if (!isSelected) 
        {
            FocusingOutline(false);
        }
    }

    public void OnPointerClick(PointerEventData _)
    {
        isSelected = !isSelected;
        if (isSelected)
        {
            FocusingOutline(true);
            outline.OutlineColor = Color.red;
        }
        else
        {
            outline.OutlineColor = Color.white;
        }
        Debug.Log($"{gameObject.name} is Clicked");
    }

    private void FocusingOutline(bool isFocused)
    {
        outline.enabled = isFocused;
    }

    public void ResetOutline()
    {
        isSelected = false;
        if(outline != null)
        {
            outline.OutlineColor = Color.white;
            FocusingOutline(false);
        }
    }
}
