using UnityEngine;
using System.Collections.Generic;
namespace PourCycle {
 public sealed class AudioController : MonoBehaviour {
  AudioSource source; readonly Dictionary<string,AudioClip> clips=new Dictionary<string,AudioClip>();
  void Awake(){source=gameObject.AddComponent<AudioSource>();source.playOnAwake=false;}
  public void Play(string cue,int level=0){
   string key=cue+level;
   if(!clips.TryGetValue(key,out var clip)) {
    float duration=cue=="complete"?.30f:cue=="select"?.055f:cue=="release"||cue=="all"?.38f:.17f;
    int n=(int)(44100*duration);var data=new float[n];
    if(cue=="complete"){
     float pitch=Mathf.Pow(1.025f,Mathf.Clamp(level,0,2));
     for(int i=0;i<n;i++){
      float t=i/44100f;
      float popTime=Mathf.Min(t,.14f),popEnvelope=Mathf.Sin(Mathf.PI*popTime/.14f)*Mathf.Exp(-popTime*7f);
      float popPhase=2*Mathf.PI*(330f*popTime-105f*popTime*popTime);
      float pop=(Mathf.Sin(popPhase)+.22f*Mathf.Sin(popPhase*2f))*popEnvelope*.18f;
      float chimeTime=t-.105f,chime=0;
      if(chimeTime>0){float chimeEnvelope=(1-Mathf.Exp(-chimeTime*90f))*Mathf.Exp(-chimeTime*13f);chime=(Mathf.Sin(2*Mathf.PI*1046.5f*pitch*chimeTime)+.38f*Mathf.Sin(2*Mathf.PI*1568f*pitch*chimeTime))*chimeEnvelope*.105f;}
      data[i]=pop+chime;
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
