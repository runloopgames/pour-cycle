using UnityEngine;
namespace PourCycle {
 public sealed class TiltInputController : MonoBehaviour {
  [Range(.05f,.8f)] public float TiltThreshold=.25f;
  [Range(.01f,.3f)] public float Hysteresis=.08f;
  public float FilterSpeed=10; public int Direction=1; public bool LeftHand;
  float neutral, filtered, sum, elapsed; int samples; bool active;
  public bool Calibrating => elapsed<.6f;
  public bool Modifier => Application.isMobilePlatform ? active : Input.GetKey(KeyCode.LeftShift);
  public void Calibrate() {elapsed=0;sum=0;samples=0;active=false;filtered=0;}
  void OnEnable(){Calibrate();}
  void OnApplicationFocus(bool focus){if(focus) Calibrate();}
  void Update() {
   float rawTiltX=Input.acceleration.x;
   if(Calibrating) {elapsed+=Time.unscaledDeltaTime;sum+=rawTiltX;samples++;neutral=sum/samples;return;}
   float tiltDelta=rawTiltX-neutral;
   float effectiveTilt=LeftHand?-tiltDelta:tiltDelta;
   filtered=Mathf.Lerp(filtered,effectiveTilt,1-Mathf.Exp(-FilterSpeed*Time.unscaledDeltaTime));
   float amount=filtered*Mathf.Sign(Direction==0?1:Direction);
   active=amount>(active ? Mathf.Max(.01f,TiltThreshold-Hysteresis) : TiltThreshold);
  }
 }
}
