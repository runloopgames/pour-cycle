using UnityEngine;
using UnityEngine.EventSystems;
namespace PourCycle {
 public sealed class PourPressButton:MonoBehaviour,IPointerDownHandler,IPointerUpHandler,IPointerExitHandler,ICancelHandler {
  public System.Action Begin,End,Cancel;int pointer;bool down;
  public void OnPointerDown(PointerEventData e){if(down||e.button!=PointerEventData.InputButton.Left)return;down=true;pointer=e.pointerId;Begin?.Invoke();}
  public void OnPointerUp(PointerEventData e){if(!down||e.pointerId!=pointer)return;down=false;End?.Invoke();}
  public void OnPointerExit(PointerEventData e){if(down&&e.pointerId==pointer)Abort();}
  public void OnCancel(BaseEventData e){Abort();}
  void OnDisable(){Abort();}
  void Abort(){if(!down)return;down=false;Cancel?.Invoke();}
 }
}
