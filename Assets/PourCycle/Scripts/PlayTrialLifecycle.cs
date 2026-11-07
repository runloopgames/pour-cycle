#if UNITY_ANDROID || UNITY_EDITOR
using UnityEngine;
namespace PourCycle {
 // Google Play owns expiration and the purchase paywall. A status is not a license verdict.
 // In particular NOT_UNDER_TRIAL and UNKNOWN must never disable a purchased game.
 public sealed class PlayTrialLifecycle : MonoBehaviour {
  public PlayTrialStatus Status {get;private set;}=PlayTrialStatus.Unknown;
  public IPlayTrialService Service {private get;set;}=new PlayTrialService();
  void Awake(){RefreshStatus();}
  public void RefreshStatus(){
   try{Status=Service!=null?Service.CheckStatus():PlayTrialStatus.Unknown;}
   catch(System.Exception e){Status=PlayTrialStatus.Unknown;PlayTrialService.Log("[PlayTrial] Status check failed: "+e.Message);}
  }
  void OnApplicationPause(bool paused){if(paused)PlayerPrefs.Save();else RefreshStatus();}
  void OnApplicationFocus(bool focused){if(focused)RefreshStatus();}
 }
}
#endif
