using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;
 
public class RightClick : MonoBehaviour, IPointerClickHandler {
    public UnityEvent leftClick;//, leftRelease, leftPress;
    public component display;
    public ghostComponent packDisplay;
    [TextArea(3, 5)] public string text;
    public string Name;
    public Sprite icon;
 
    public void OnPointerClick(PointerEventData eventData) {
        if ((eventData.button == PointerEventData.InputButton.Left || eventData.button == PointerEventData.InputButton.Right) && leftClick != null)
            leftClick.Invoke();
    }

    void Update() {
        if (Mouse.current.delta.ReadValue() != Vector2.zero && GetComponent<RectTransform>().rect.Contains(transform.InverseTransformPoint(clicker.mousePos))) {
            Display();

            /*if (leftPress != null && Mouse.current.leftButton.wasPressedThisFrame)
                leftPress.Invoke();

            if (leftRelease != null && Mouse.current.leftButton.wasReleasedThisFrame)
                leftRelease.Invoke();*/
        }
    }

    public void Display() {
        if (display == null && Name != "")
            informationWindow.DisplayString(text, Name, icon);
        else if (Name == "" && display != null)
            informationWindow.Display(display);
        else if (packDisplay != null)
            informationWindow.DisplayString(packDisplay.pack.description, packDisplay.pack.name);
    }
}