using UnityEngine;
using System.Collections.Generic;
namespace PourCycle {
 public sealed class AudioController : MonoBehaviour {
  AudioSource source; readonly Dictionary<string,AudioClip> clips=new Dictionary<string,AudioClip>();
  void Awake(){source=gameObject.AddComponent<AudioSource>();source.playOnAwake=false;}
  public void Play(string cue,int level=0){
   string key=cue+level;
   if(!clips.TryGetValue(key,out var clip)) {
    float duration=cue=="complete"?.28f:cue=="select"?.055f:cue=="release"||cue=="all"?.38f:.17f;
    int n=(int)(44100*duration);var data=new float[n];
    if(cue=="complete"){
     float pitch=Mathf.Pow(1.025f,Mathf.Clamp(level,0,2));
     for(int i=0;i<n;i++){
      float t=i/44100f;
      float bodyAttack=Mathf.SmoothStep(0,1,Mathf.Clamp01(t/.02f));
      float shimmerAttack=Mathf.SmoothStep(0,1,Mathf.Clamp01(t/.014f));
      float bodyEnvelope=bodyAttack*Mathf.Exp(-t*9f);
      float shimmerEnvelope=shimmerAttack*Mathf.Exp(-t*25f);
      float tailFade=1-Mathf.SmoothStep(0,1,Mathf.Clamp01((t-.22f)/.06f));
      float chime=(Mathf.Sin(2*Mathf.PI*1108.73f*pitch*t)+.18f*Mathf.Sin(2*Mathf.PI*1121.2f*pitch*t))*bodyEnvelope
       +(.065f*Mathf.Sin(2*Mathf.PI*1632.4f*pitch*t)+.018f*Mathf.Sin(2*Mathf.PI*2381.7f*pitch*t))*shimmerEnvelope;
      data[i]=chime*tailFade*.15f;
     }
    }else{
     float frequency=cue=="error"?130:cue=="select"?620:cue=="hold"?440*Mathf.Pow(1.2599f,level):cue=="pour"?520:cue=="release"?660:990;
     for(int i=0;i<n;i++){float t=i/44100f;float env=Mathf.Sin(Mathf.PI*i/n)*Mathf.Exp(-t*5);float v=Mathf.Sin(2*Mathf.PI*frequency*t);if(cue=="release"||cue=="all")v=(v+Mathf.Sin(2*Mathf.PI*frequency*1.25f*t)+Mathf.Sin(2*Mathf.PI*frequency*1.5f*t))/3;data[i]=v*env*.24f;}
    }
    clip=AudioClip.Create(key,n,1,44100,false);clip.SetData(data,0);clips.Add(key,clip);
   }
   source.PlayOneShot(clip);
  }
  void OnDestroy(){foreach(var c in clips.Values)Destroy(c);}
 }
}
