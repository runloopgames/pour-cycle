using System; using System.Linq; namespace PourCycle.Editor { public static class CoreChecks {
  static int checks;
  static void Check(bool ok,string name){if(!ok)throw new Exception("FAIL: "+name);checks++;}
  
  public static int Run(){
   checks=0;var p=new PourController(true,42);
   Check(p.Buffer.Count==1,"demo preload");
   Check(!p.Press(Ingredient.CUP,false).Success,"wrong ingredient");
   Check(p.Orders.Slots[0].Step==0,"wrong pour unchanged");
   Check(p.Press(Ingredient.BLUE,true).Success && p.Buffer.Count==2,"mixed hold allowed");
   p.Press(Ingredient.RED,false);Check(p.Buffer.Count==2 && p.Orders.Slots[1].Next==Ingredient.FOAM,"pour preserves buffer");
   p.Press(Ingredient.BLUE,false);var lime=p.Press(Ingredient.LIME,false);Check(lime.Completed==1,"order 3 complete");
   p.Orders.Replace(2);Check(p.Orders.Slots[2].Id==4 && p.Orders.Slots[2].Next==Ingredient.BLUE,"preview order 4");
   p.Press(Ingredient.FOAM,true);p.Press(Ingredient.BLUE,false);
   while(p.Buffer.Count<p.Buffer.Capacity)Check(p.Press(Ingredient.FOAM,true).Success,"fill capacity");Check(!p.Press(Ingredient.FOAM,true).Success,"capacity");
   ChainStep step;int released=0,completed=0;while(p.TryReleaseNext(out step)){released++;if(step.Completed)completed++;}var release=new PourResult{Release=true};release.Targets.Add(new OrderData(90,Ingredient.FOAM){Step=1});release.Targets.Add(new OrderData(91,Ingredient.FOAM){Step=1});release.Targets.Add(new OrderData(92,Ingredient.FOAM){Step=1});Check(released>=1 && completed>=1,"chain release resolves held ingredients");
   var session=new SessionController(60,false);session.Record(release);Check(session.Cleared==3 && session.BestRelease==3 && session.AllClears==1,"statistics");session.Tick(61);Check(session.Remaining==0,"timer");
   p=new PourController(false,42);
   for(int i=0;i<3;i++)p.Orders.Slots[i]=new OrderData(i+1,Ingredient.FOAM,Ingredient.FOAM);
   var one=p.Press(Ingredient.FOAM,false);Check(one.Targets.Count==1 && one.Targets[0].Id==1,"oldest single target");
   for(int i=0;i<3;i++)p.Press(Ingredient.FOAM,true);
   p.Orders.Slots[2]=new OrderData(3,Ingredient.BLUE);
   ChainStep chain;Check(p.TryReleaseNext(out chain)&&chain.Target.Id==1,"chain uses oldest matching order");
   Check(!p.Press(Ingredient.RED,false).Success,"no match fails without changing orders");
   var releaseGesture=new PourGesture();releaseGesture.Begin(false,.35f);Check(releaseGesture.Tick(.34f,false)==PourAction.None,"long press waits for threshold");Check(releaseGesture.Tick(.02f,false)==PourAction.Release,"long press fires release");Check(releaseGesture.End()==PourAction.None,"long press does not also pour on release");
   releaseGesture.Begin(true,.35f);Check(releaseGesture.Tick(.5f,true)==PourAction.None&&releaseGesture.End()==PourAction.Hold,"tilted press remains hold");
   var staged=new PourController(false,new LevelConfig{LevelNumber=5,HoldCapacity=6},7);
   staged.Orders.Slots[0]=new OrderData(1,Ingredient.RED,Ingredient.FOAM);
   staged.Orders.Slots[1]=new OrderData(2,Ingredient.BLUE,Ingredient.RED);
   staged.Orders.Slots[2]=new OrderData(3,Ingredient.RED,Ingredient.LIME);
   staged.Press(Ingredient.RED,true);staged.Press(Ingredient.FOAM,true);staged.Press(Ingredient.LIME,true);staged.Press(Ingredient.RED,true);
   int stagedReleased=0;while(staged.TryReleaseNext(out chain))stagedReleased++;
   Check(stagedReleased==4&&staged.Buffer.Count==0&&staged.Orders.Slots[0].Complete&&staged.Orders.Slots[2].Complete,"buffer release chains usable held ingredients without selected input");
   Check(session.FlowScore==450 && session.BestFlow==450,"triple flow reward");
   var scores=new SessionController(60,false);
   var reward=new PourResult();reward.Targets.Add(new OrderData(10,Ingredient.FOAM){Step=1});scores.Record(reward);
   Check(scores.FlowScore==100 && scores.BestFlow==100,"single flow reward");
   reward.Targets.Add(new OrderData(11,Ingredient.FOAM){Step=1});scores.Record(reward);
   Check(scores.FlowScore==350 && scores.BestFlow==250,"double flow total and best");
   reward.Targets.Clear();scores.Record(reward);Check(scores.FlowScore==350,"no score for incomplete pour or hold");
   var patience=new OrderPatienceController(15);patience.Tick(p.Orders,10,false);
   Check(patience.Seconds(2)==5,"order patience counts down");
   patience.Tick(p.Orders,10,true);Check(patience.Seconds(2)==5,"preview patience frozen");
   patience.Tick(p.Orders,5,false);Check(patience.Expired(2),"patience expires");
   int oldId=p.Orders.Slots[2].Id;p.Orders.ReplaceExpired(2);patience.Sync(p.Orders);
   Check(p.Orders.Slots[2].Id>oldId && patience.Seconds(2)==15,"expired order replaced and patience reset");
   Check(scores.FlowScore==350,"expiry does not award flow");
   var levels=LevelConfig.Defaults();
   Ingredient[][] expectedIngredients={new[]{Ingredient.BLUE,Ingredient.RED},new[]{Ingredient.BLUE,Ingredient.RED,Ingredient.PINK},new[]{Ingredient.BLUE,Ingredient.RED,Ingredient.PINK,Ingredient.CUP},new[]{Ingredient.BLUE,Ingredient.RED,Ingredient.PINK,Ingredient.CUP,Ingredient.FOAM},new[]{Ingredient.BLUE,Ingredient.RED,Ingredient.PINK,Ingredient.CUP,Ingredient.FOAM,Ingredient.LIME,Ingredient.CHERRY}};
   Check(levels.Length==5,"five authored levels");
   for(int level=1;level<=5;level++){
    var config=levels[level-1];Check(config.OrderPatience==45,"fixed patience LEVEL "+level);Check(config.EffectiveHoldCapacity==level+1,"staged hold LEVEL "+level);Check(config.ActiveIngredients.SequenceEqual(expectedIngredients[level-1]),"staged ingredients LEVEL "+level);
    var stagedLevel=new PourController(false,config,42+level);
    for(int sample=0;sample<120;sample++){var order=stagedLevel.Orders.Slots[0];Check(order.Recipe.Length>=config.MinRecipeSteps&&order.Recipe.Length<=config.MaxRecipeSteps,"recipe length LEVEL "+level);Check(order.Recipe.All(config.ActiveIngredients.Contains),"recipe uses unlocked ingredients LEVEL "+level);while(!order.Complete)stagedLevel.Press(order.Next,false);stagedLevel.Orders.Replace(0);}
   }
   bool emptyCandidatesDetected=false;try{new PourController(false,new LevelConfig{LevelNumber=99,UnlockedIngredients=new[]{Ingredient.CHERRY},MinRecipeSteps=3,MaxRecipeSteps=3,OneStepWeight=0,TwoStepWeight=0,ThreeStepWeight=100},99);}catch(InvalidOperationException){emptyCandidatesDetected=true;}Check(emptyCandidatesDetected,"empty unlocked template set is rejected");
   bool biased=false;for(int seed=1;seed<30&&!biased;seed++){var biasedOrders=new OrderManager(false,levels[4],seed);var counts=new int[System.Enum.GetValues(typeof(Ingredient)).Length];for(int s=0;s<3;s++)for(int j=0;j<biasedOrders.Slots[s].Recipe.Length;j++)counts[(int)biasedOrders.Slots[s].Recipe[j]]++;for(int c=0;c<counts.Length;c++)if(counts[c]>=3)biased=true;}Check(biased,"strong bias can create visible demand clusters");
   Check((int)Ingredient.CHERRY>(int)Ingredient.CUP && levels[4].IncludeExtendedIngredients && levels[4].ActiveIngredients.Length==7,"LEVEL 5 unlocks Lime and Cherry");
   var endless=LevelConfig.Generate(6,levels[4]);Check(endless.IncludeExtendedIngredients && endless.ActiveIngredients.Length==7 && endless.TotalCustomers==18&&Math.Abs(endless.OrderPatience-32)<.001f,"LEVEL 6 full unlock and patience formula");
   Check(Math.Abs(LevelConfig.Generate(7,levels[4]).OrderPatience-31.6f)<.001f&&Math.Abs(LevelConfig.Generate(8,levels[4]).OrderPatience-31.2f)<.001f&&LevelConfig.Generate(System.Numerics.BigInteger.Parse("999999999999999999"),levels[4]).OrderPatience==10,"endless patience curve and floor");
   bool cherrySeen=false;for(int seed=1;seed<80&&!cherrySeen;seed++){var endlessOrders=new OrderManager(false,endless,seed);for(int s=0;s<3;s++)for(int j=0;j<endlessOrders.Slots[s].Recipe.Length;j++)if(endlessOrders.Slots[s].Recipe[j]==Ingredient.CHERRY)cherrySeen=true;}Check(cherrySeen,"endless orders can include cherry");
   var timed=new SessionController(2,false,0);timed.Tick(1);Check(!timed.Ended,"endless timer runs before timeout");timed.Tick(2);Check(timed.Ended,"endless timer ends session");
   return checks;
  }
 }
}












