using UnityEngine;
namespace PourCycle {
 public sealed class HandednessController {
  public bool Left {get;private set;}=PlayerPrefs.GetInt("PourCycle.Left",0)==1;
  public void Toggle(){Left=!Left;PlayerPrefs.SetInt("PourCycle.Left",Left?1:0);PlayerPrefs.Save();}
  public void SetLeft(bool left){Left=left;PlayerPrefs.SetInt("PourCycle.Left",Left?1:0);PlayerPrefs.Save();}
  public float X(float x)=>Left ? 1080-x : x;
 }
}
