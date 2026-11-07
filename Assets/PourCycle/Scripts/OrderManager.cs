using System;
using System.Collections.Generic;
using System.Linq;
namespace PourCycle {
 public enum Ingredient { FOAM, BLUE, RED, PINK, LIME, CUP, CHERRY }
 public sealed class OrderData {
  public int Id, Step; public Ingredient[] Recipe;
  public bool Complete => Step >= Recipe.Length;
  public Ingredient Next => Recipe[Step];
  public OrderData(int id, params Ingredient[] recipe) { Id=id; Recipe=recipe; }
 }
 public sealed class OrderManager {
  public readonly OrderData[] Slots = new OrderData[3];
  int serial; bool preview; readonly Random random; readonly LevelConfig config;
  static readonly Ingredient[][] StandardTemplates = {
   new[]{Ingredient.FOAM}, new[]{Ingredient.RED,Ingredient.FOAM},
   new[]{Ingredient.BLUE,Ingredient.LIME}, new[]{Ingredient.BLUE,Ingredient.FOAM},
   new[]{Ingredient.CUP,Ingredient.PINK,Ingredient.FOAM},
   new[]{Ingredient.LIME,Ingredient.FOAM}, new[]{Ingredient.PINK,Ingredient.LIME},
   new[]{Ingredient.CUP,Ingredient.BLUE,Ingredient.FOAM},
   new[]{Ingredient.RED}, new[]{Ingredient.BLUE}, new[]{Ingredient.PINK}, new[]{Ingredient.LIME},
   new[]{Ingredient.PINK,Ingredient.FOAM}, new[]{Ingredient.RED,Ingredient.LIME},
   new[]{Ingredient.BLUE,Ingredient.RED,Ingredient.FOAM}, new[]{Ingredient.PINK,Ingredient.LIME,Ingredient.FOAM},
   new[]{Ingredient.CUP,Ingredient.LIME,Ingredient.FOAM},
   new[]{Ingredient.BLUE,Ingredient.RED}, new[]{Ingredient.RED,Ingredient.BLUE},
   new[]{Ingredient.BLUE,Ingredient.PINK}, new[]{Ingredient.PINK,Ingredient.RED},
   new[]{Ingredient.RED,Ingredient.PINK}, new[]{Ingredient.PINK,Ingredient.BLUE},
   new[]{Ingredient.CUP,Ingredient.BLUE}, new[]{Ingredient.CUP,Ingredient.RED}, new[]{Ingredient.CUP,Ingredient.PINK}
  };
  static readonly Ingredient[][] ExtendedExtraTemplates = {
   new[]{Ingredient.CHERRY,Ingredient.FOAM}, new[]{Ingredient.BLUE,Ingredient.CHERRY},
   new[]{Ingredient.LIME,Ingredient.CHERRY}, new[]{Ingredient.RED,Ingredient.CHERRY,Ingredient.FOAM},
   new[]{Ingredient.CUP,Ingredient.CHERRY,Ingredient.LIME}, new[]{Ingredient.CHERRY,Ingredient.LIME,Ingredient.FOAM},
   new[]{Ingredient.BLUE,Ingredient.LIME,Ingredient.CHERRY}, new[]{Ingredient.CUP,Ingredient.CHERRY,Ingredient.FOAM}
  };
  Ingredient[][] Templates {
   get {var active=config.ActiveIngredients;return StandardTemplates.Concat(ExtendedExtraTemplates).Where(recipe=>recipe.All(active.Contains)).ToArray();}
  }
  public OrderManager(bool demo, int seed=0):this(demo,demo?LevelConfig.Practice():LevelConfig.Defaults()[0],seed){}
  public OrderManager(bool demo, LevelConfig level, int seed=0) {
   preview=demo; config=level??LevelConfig.Defaults()[0]; random=seed==0 ? new Random() : new Random(seed);
   if(preview){Slots[0]=new OrderData(++serial,StandardTemplates[0]);Slots[1]=new OrderData(++serial,StandardTemplates[1]);Slots[2]=new OrderData(++serial,StandardTemplates[2]);}
   else for(int i=0;i<3;i++)Slots[i]=NewOrder(-1);
  }
  public List<OrderData> Apply(Ingredient item,int quantity) {
   var targets=Slots.Where(o=>!o.Complete && o.Next==item).OrderBy(o=>o.Id).Take(quantity).ToList();
   foreach(var order in targets) order.Step++;
   return targets;
  }
  public void Replace(int slot) {if(!Slots[slot].Complete) return;ReplaceExpired(slot);}
  public void ReplaceExpired(int slot) { Slots[slot]=NewOrder(slot); }
  OrderData NewOrder(int replacingSlot) {
   var templates=Templates;if(templates.Length==0)throw new InvalidOperationException("No order templates use only the unlocked ingredients for LEVEL "+config.LevelNumber);
   int template=preview && serial==3 ? Array.FindIndex(templates,r=>r.SequenceEqual(StandardTemplates[3])) : PickTemplate(replacingSlot,templates);
   if(template<0)template=PickTemplate(replacingSlot,templates);
   return new OrderData(++serial,templates[template]);
  }
  int PickTemplate(int replacingSlot,Ingredient[][] templates) {
   var candidates=templates.Select((recipe,index)=>new{recipe,index}).Where(t=>t.recipe.Length>=Math.Max(1,config.MinRecipeSteps) && t.recipe.Length<=Math.Max(config.MinRecipeSteps,config.MaxRecipeSteps)).ToList();
   if(candidates.Count==0)throw new InvalidOperationException("No unlocked order templates match the configured recipe length for LEVEL "+config.LevelNumber);
   var demand=DemandCounts(replacingSlot);
   int total=0;var weights=new int[candidates.Count];
   for(int i=0;i<candidates.Count;i++){weights[i]=TemplateWeight(candidates[i].recipe,demand);total+=weights[i];}
   if(total<=0){for(int i=0;i<candidates.Count;i++){weights[i]=1;total++;}}
   int roll=random.Next(total),cursor=0;
   for(int i=0;i<candidates.Count;i++){cursor+=weights[i];if(roll<cursor)return candidates[i].index;}
   return candidates[candidates.Count-1].index;
  }
  int[] DemandCounts(int replacingSlot){
   var counts=new int[Enum.GetValues(typeof(Ingredient)).Length];
   for(int s=0;s<Slots.Length;s++){
    if(s==replacingSlot||Slots[s]==null||Slots[s].Complete)continue;
    for(int i=Slots[s].Step;i<Slots[s].Recipe.Length;i++)counts[(int)Slots[s].Recipe[i]]++;
   }
   return counts;
  }
  int TemplateWeight(Ingredient[] recipe,int[] demand){
   int baseWeight=Weight(recipe.Length);if(baseWeight<=0)return 0;
   if(config.DemandBias<=0)return baseWeight;
   int demandScore=0;for(int i=0;i<recipe.Length;i++)demandScore+=demand[(int)recipe[i]];
   float multiplier=1f+config.DemandBias*demandScore;
   return Math.Max(1,(int)Math.Round(baseWeight*multiplier));
  }
  int Weight(int steps){return Math.Max(0,steps==1?config.OneStepWeight:steps==2?config.TwoStepWeight:config.ThreeStepWeight);}
 }
}
