using System;
namespace PourCycle {
 public sealed class SessionController {
  public float Remaining,Elapsed; public bool Demo;
  public int TotalCustomers,ResolvedCustomers,PlayerSales,RivalSales,OrdersServed,CustomersLost;
  public int Cleared,BestRelease,AllClears;
  public int FlowScore,BestFlow;
  public bool Timed=>!Demo&&TotalCustomers<=0;
  public bool Ended => !Demo && (CustomersFinished || (Timed&&Remaining<=0));
  public SessionController(float seconds,bool demo) {Remaining=seconds;Demo=demo;}
  public SessionController(float seconds,bool demo,int customers) {Remaining=seconds;Demo=demo;TotalCustomers=customers;}
  public bool CustomersFinished=>!Demo&&TotalCustomers>0&&ResolvedCustomers>=TotalCustomers;
  public void Tick(float dt) {if(Demo)return;Elapsed+=dt;if(Remaining>0||Timed) Remaining=System.Math.Max(0,Remaining-dt);}
  public static int FlowFor(int completed)=>completed==3?450:completed==2?250:completed==1?100:0;
  public void RecordChain(int length,int completed,int bonus){
   Cleared+=completed;BestRelease=System.Math.Max(BestRelease,length);if(completed==3)AllClears++;
   int flow=FlowFor(completed)+System.Math.Max(0,bonus);FlowScore+=flow;BestFlow=System.Math.Max(BestFlow,flow);
  }
  public void RecordSales(int baseValue,int chainBonus){PlayerSales+=baseValue+Math.Max(0,chainBonus);OrdersServed++;ResolvedCustomers++;}
  public void RecordMiss(int baseValue){RivalSales+=baseValue;CustomersLost++;ResolvedCustomers++;}
  public void Record(PourResult r) { Cleared+=r.Completed; if(r.Release) BestRelease=System.Math.Max(BestRelease,r.Targets.Count); if(r.Completed==3) AllClears++;int flow=FlowFor(r.Completed);FlowScore+=flow;BestFlow=System.Math.Max(BestFlow,flow); }
 }
}
