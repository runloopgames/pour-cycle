using UnityEngine;
using UnityEngine.EventSystems;
namespace PourCycle {
 public sealed class PointerDownButton : MonoBehaviour,IPointerDownHandler {
  public System.Action Press;
  public void OnPointerDown(PointerEventData e){if(e.button==PointerEventData.InputButton.Left)Press?.Invoke();}
 }
}
