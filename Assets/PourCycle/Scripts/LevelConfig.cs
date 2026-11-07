using System;
namespace PourCycle {
 [Serializable]
 public sealed class LevelConfig {
  public int LevelNumber=1; public int HoldCapacity=0;
  public int[] ChainBonuses=new[]{0,50,120,220,350,500};
  public int EffectiveHoldCapacity=>HoldCapacity>0?Math.Max(1,Math.Min(6,HoldCapacity)):DefaultHoldCapacity(LevelNumber);
  public static int DefaultHoldCapacity(int level)=>level<=0?6:level==1?2:level==2?3:level==3?4:level==4?5:6;
  public int ChainBonus(int count)=>count<=0?0:Math.Max(0,(ChainBonuses!=null&&ChainBonuses.Length>=Math.Min(6,count))?ChainBonuses[Math.Min(6,count)-1]:new[]{0,50,120,220,350,500}[Math.Min(6,count)-1]);
  public float SessionDuration=45;
  public int TotalCustomers=9;
  public int[] RecipeBaseValues=new[]{100,150,200};
  public int[] ChainSalesBonuses=new[]{0,20,50,90,140,200};
  public int BaseValueForSteps(int steps)=>RecipeBaseValues[Math.Max(0,Math.Min(RecipeBaseValues.Length-1,steps-1))];
  public int ChainSalesBonus(int count)=>count<=0?0:ChainSalesBonuses[Math.Min(ChainSalesBonuses.Length,count)-1];
  public float OrderPatience=20;
  public int TargetFlow=800;
  public int MinRecipeSteps=1;
  public int MaxRecipeSteps=2;
  public int OneStepWeight=70;
  public int TwoStepWeight=30;
  public int ThreeStepWeight=0; public float DemandBias=0;
  public Ingredient[] UnlockedIngredients;
  [UnityEngine.Serialization.FormerlySerializedAs("IncludeEndlessIngredients")] public bool IncludeExtendedIngredients=false;
  public static readonly Ingredient[] StandardIngredients={Ingredient.FOAM,Ingredient.BLUE,Ingredient.RED,Ingredient.PINK,Ingredient.LIME,Ingredient.CUP};
  public static readonly Ingredient[] ExtendedIngredients={Ingredient.FOAM,Ingredient.BLUE,Ingredient.RED,Ingredient.PINK,Ingredient.LIME,Ingredient.CUP,Ingredient.CHERRY};
  public Ingredient[] ActiveIngredients=>UnlockedIngredients!=null&&UnlockedIngredients.Length>0?UnlockedIngredients:IncludeExtendedIngredients?ExtendedIngredients:StandardIngredients;
  public static LevelConfig[] Defaults()=>new[]{
   new LevelConfig{LevelNumber=1,HoldCapacity=2,TotalCustomers=9,SessionDuration=0,OrderPatience=45,TargetFlow=800,MinRecipeSteps=1,MaxRecipeSteps=2,OneStepWeight=65,TwoStepWeight=35,ThreeStepWeight=0,DemandBias=0,UnlockedIngredients=new[]{Ingredient.BLUE,Ingredient.RED}},
   new LevelConfig{LevelNumber=2,HoldCapacity=3,TotalCustomers=12,SessionDuration=0,OrderPatience=45,TargetFlow=1200,MinRecipeSteps=1,MaxRecipeSteps=2,OneStepWeight=45,TwoStepWeight=55,ThreeStepWeight=0,DemandBias=0,UnlockedIngredients=new[]{Ingredient.BLUE,Ingredient.RED,Ingredient.PINK}},
   new LevelConfig{LevelNumber=3,HoldCapacity=4,TotalCustomers=12,SessionDuration=0,OrderPatience=45,TargetFlow=1700,MinRecipeSteps=1,MaxRecipeSteps=2,OneStepWeight=20,TwoStepWeight=80,ThreeStepWeight=0,DemandBias=.2f,UnlockedIngredients=new[]{Ingredient.BLUE,Ingredient.RED,Ingredient.PINK,Ingredient.CUP}},
   new LevelConfig{LevelNumber=4,HoldCapacity=5,TotalCustomers=15,SessionDuration=0,OrderPatience=45,TargetFlow=2300,MinRecipeSteps=2,MaxRecipeSteps=3,OneStepWeight=0,TwoStepWeight=80,ThreeStepWeight=20,DemandBias=.6f,UnlockedIngredients=new[]{Ingredient.BLUE,Ingredient.RED,Ingredient.PINK,Ingredient.CUP,Ingredient.FOAM}},
   new LevelConfig{LevelNumber=5,HoldCapacity=6,TotalCustomers=15,SessionDuration=0,OrderPatience=45,TargetFlow=3000,MinRecipeSteps=2,MaxRecipeSteps=3,OneStepWeight=0,TwoStepWeight=65,ThreeStepWeight=35,DemandBias=1f,IncludeExtendedIngredients=true,UnlockedIngredients=new[]{Ingredient.BLUE,Ingredient.RED,Ingredient.PINK,Ingredient.CUP,Ingredient.FOAM,Ingredient.LIME,Ingredient.CHERRY}}
  };
  public static LevelConfig Practice()=>new LevelConfig{LevelNumber=0,HoldCapacity=6,SessionDuration=0,OrderPatience=60,TargetFlow=0,MinRecipeSteps=1,MaxRecipeSteps=3,OneStepWeight=45,TwoStepWeight=40,ThreeStepWeight=15,DemandBias=0};
  // LevelNumber remains a serialized legacy configuration identifier. Progression uses BigInteger.
  public static LevelConfig Generate(System.Numerics.BigInteger level,LevelConfig basis){
   int step=(int)System.Numerics.BigInteger.Min(120,System.Numerics.BigInteger.Max(1,level-5));
   int difficultyStep=(int)System.Numerics.BigInteger.Min(55,System.Numerics.BigInteger.Max(0,level-6));
   var c=new LevelConfig{LevelNumber=6,HoldCapacity=basis.EffectiveHoldCapacity,
    TotalCustomers=Math.Min(30,Math.Max(3,basis.TotalCustomers)+3*((step+3)/4)),SessionDuration=0,
    OrderPatience=Math.Max(10,32f-.4f*difficultyStep),
    MinRecipeSteps=2,MaxRecipeSteps=3,
    OneStepWeight=0,TwoStepWeight=Math.Max(30,55-difficultyStep),
    ThreeStepWeight=Math.Min(70,45+difficultyStep),DemandBias=Math.Min(1.8f,1.2f+.02f*difficultyStep),
    IncludeExtendedIngredients=true,ChainBonuses=(int[])basis.ChainBonuses.Clone(),
    RecipeBaseValues=(int[])basis.RecipeBaseValues.Clone(),ChainSalesBonuses=(int[])basis.ChainSalesBonuses.Clone(),UnlockedIngredients=(Ingredient[])ExtendedIngredients.Clone()};
   c.TargetFlow=(int)Math.Round((double)basis.TargetFlow*c.TotalCustomers/Math.Max(1,basis.TotalCustomers));
   return c;
  }
 }
}
