using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace GameStart
{

    public class Army
    {
        public string PlayerID { get; set; }
        public string Location { get; set; }
        public ConcurrentDictionary<String, double> Units { get; set; }
    }

    public class Unit
    {
        public string Name { get; set; }
        public double Attack { get; set; }
        public double Health { get; set; }
        public double Shield { get; set; }
        public double ArmorPen { get; set; }
        public int Command { get; set; }
        public double Movement { get; set; }
        public int Cost { get; set; }
        public string WeaponType { get; set; }
        public double Speed { get; set; }
        public int BarracksRqd { get; set; }
        public int AdvBarracksRqd { get; set; }
       // public double Qty { get; set; }
        public Boolean Partial { get; set; }
                        
    }

    class Program
    {
        static void Main(string[] args)
        {

        
            Army army1 = new Army();
            army1.Units = new ConcurrentDictionary<String, double>();
            army1.PlayerID = "1";
            army1.Location = "Here";

            Army army2 = new Army();
            army2.Units = new ConcurrentDictionary<String, double>();

            //army2.Units.Add(UnitManager.CreateUnit("DragonKnight", 1));
            army2.Units.AddOrUpdate("DragonKnight", 1,(k,v) => v + 1);
            //army2.Units.Add(UnitManager.CreateUnit("Ranger", 1));
            //army2.Units.Add(UnitManager.CreateUnit("SpearThrower", 10));

            army1.Units.AddOrUpdate("MagiKnight", 73,(k,v) => v + 73);
            //army1.Units.Add(UnitManager.CreateUnit("Titan", 1));

            Combat.InitializeAttack(army1, army2);
           /* string input= "";
            while (input != "attack")
            {
                if (input == "attack")
                {

                }
                
                input = Console.ReadLine();
            }*/
        }

            public static class Combat 
            {
                        
            public static void InitializeAttack (Army attackingArmy, Army defendingArmy)
                {
                    Army tempArmy = new Army();
                    tempArmy = defendingArmy;

                   defendingArmy = CalcUnits(attackingArmy, defendingArmy);
                   attackingArmy = CalcUnits(tempArmy, attackingArmy);
                   
                
                Console.WriteLine("Attacking Army");
                   foreach (var x in attackingArmy.Units)
                   {
                       Console.WriteLine("{0}{1}{2}", x.Key, " : ", x.Value);
                    
                   }
                  
                Console.WriteLine(" ");
                Console.WriteLine("Defending Army");
                foreach (var y in defendingArmy.Units)
                   {
                       Console.WriteLine("{0}{1}{2}", y.Key, " : ", y.Value);
                      
                      
                   }
                Console.ReadKey();
                                 
                }

            public static Army CalcUnits (Army army1, Army army2)
            {
                Army tempArmy = new Army();
                tempArmy = army2;
                
                foreach (var x in army1.Units)
                {
                    Unit attackUnit = UnitManager.GetUnitByType(x.Key);
                    double totalDamage = x.Value * attackUnit.Attack; //Calculate how much total damage we need to distribute
                    Army hardTargets = new Army();
                    hardTargets.Units = new ConcurrentDictionary<string, double>();
                    Army softTargets = new Army();
                    softTargets.Units = new ConcurrentDictionary<string, double>();
                    double ratio = 0;
                
                    if (x.Value > 0)
                    { 
                    
                    foreach (var y in tempArmy.Units)
                    {
                        Unit defendingUnit = UnitManager.GetUnitByType(y.Key);
                        if((defendingUnit.Shield * attackUnit.ArmorPen) > attackUnit.Attack) //determine if any "hard" targets are within defending army.  If so, we will need to split the damage.  
                        {
                           hardTargets.Units.AddOrUpdate(y.Key, y.Value, (k,v) => v + y.Value); //create a new temp army called hardTargets, filled with units whose shield is greater then the PerUnitDamage
                        }
                        else
                        {
                            softTargets.Units.AddOrUpdate(y.Key, y.Value, (k, v) => v + y.Value); //create a new temp army called softTargets, filled with remaining units
                        }
                    }

                    if(CalcUniqueUnits(hardTargets) > 0 && CalcUniqueUnits(softTargets) > 0) //determine if any hard targets exist
                    {
                    ratio = CalcUniqueUnits(hardTargets) / CalcUniqueUnits(softTargets);
                    double softDmgQty = ((totalDamage * (1 - (.01 * ratio))) / attackUnit.Attack); //Formula is 100% - (1% * ratio)
                    double hardDmgQty = x.Value - softDmgQty; //rest of damage is applied to hard targets
                    Army tempSoft = AssignDamage(x.Key, softDmgQty, softTargets);
                    Army tempHard = AssignDamage(x.Key, hardDmgQty, hardTargets);
                    tempArmy = CombineArmy(tempSoft, tempHard);
                    }
                        else if(CalcUniqueUnits(hardTargets) == 0) //if there are no hard targets, only attack the soft targets
                           {
                          tempArmy = AssignDamage(x.Key, x.Value, softTargets);
                           }
                           else
                           {
                           tempArmy = AssignDamage(x.Key, x.Value, hardTargets); //if there are no soft targets, only attack the hard targets
                           }
        

                   }
       
                }

                return tempArmy;
           }

            

            public static double CalcUniqueUnits (Army army)
            {
                double count = 0;
                foreach (var x in army.Units)
                {
                    if (x.Value > 0) 
                    {
                        count++;
                    }
                }

                return count;
            }

            public static Army CombineArmy (Army army1, Army army2)
            {
                Army tempArmy = army1;

                foreach (var x in army2.Units)
                {
                    tempArmy.Units.AddOrUpdate(x.Key, x.Value, (k, v) => v + x.Value);
                }
                return tempArmy;
            }
    

            public static Army AssignDamage (string unitName, double qty, Army defend)
            {
                double carryQty = 0;
                double defendCount = 0;
                Unit attackUnit = UnitManager.GetUnitByType(unitName);
                double perUnitDamage = attackUnit.Attack; //+ .05) * attackResearch + Bonuses Add in research and Bonuses here later


                defendCount = CalcUniqueUnits(defend);

                foreach (var x in defend.Units)
                {
                    if (x.Value > 0)
                    {
                        Unit defendUnit = UnitManager.GetUnitByType(x.Key);
                        double newQty = 0;
                        double armorTotal = 0;
                        double totalDamage = 0;
                        double debris = 0;
                        double armorPen = 0;
                        double bleedDamage = 0;

                        double afterArmor = (perUnitDamage - defendUnit.Shield);
                        if (afterArmor < 0)//if damage reduced to less then 0, change it to 0
                        {
                            afterArmor = 0;
                        }
                        
                        if(perUnitDamage > defendUnit.Shield)
                        {
                            armorPen = defendUnit.Shield * attackUnit.ArmorPen;
                        }
                        else
                        {
                            armorPen = perUnitDamage * attackUnit.ArmorPen;
                        }
                        
                        
                        perUnitDamage = afterArmor + armorPen;
                        if(perUnitDamage == 0)
                        {
                            bleedDamage = (perUnitDamage - afterArmor) * (.01); //1% of damage reduced still goes through   
                        }
                        totalDamage = perUnitDamage * (qty / defendCount);
                        armorTotal = x.Value * defendUnit.Health; //How much total health on the unit we are attacking
                        newQty = (armorTotal - totalDamage) / defendUnit.Health; //Qty remaining after taking damage
                        if (newQty < 0)
                        {
                            newQty = 0;
                            carryQty += (totalDamage - armorTotal) / perUnitDamage; //if excess damage was assigned, how many units have to redistribute damage
                        }

                        debris = ((Math.Ceiling(x.Value) - Math.Ceiling(newQty)) * defendUnit.Cost * .35); //For now, 35% of army destroyed is left behind as debris.. this is likekly to change

                        if (defendUnit.Partial == true)
                        {
                            defend.Units.AddOrUpdate(x.Key, x.Value, (k, v) => Math.Ceiling(newQty * 100) / 100); //if units are allowed to be partial, return partial qty to two decimals
                        }
                        else
                        {
                            defend.Units.AddOrUpdate(x.Key, x.Value, (k, v) => Math.Ceiling(newQty)); //if units must be whole units, auto heal them
                        }


                    }

                    if (carryQty > 0)
                    {
                        AssignDamage(unitName, carryQty, defend);
                    }
                }


                return defend;
            }
                


             
            }
    
        
           public static class UnitManager
           {
               private static readonly Dictionary<string, Unit> Defaults = new Dictionary<string, Unit>()
               {
                   {"Footman", new Unit() {Name = "Footman", Health = 2, Attack = 2, Cost = 5, ArmorPen = 0, AdvBarracksRqd = 0, BarracksRqd = 1, Command = -1, Movement = 0, Partial = false, Shield = 0, Speed = 0, WeaponType = "Daggers"}},
                   {"SpearThrower", new Unit() {Name = "SpearThrower", Health = 2, Attack = 4, Cost = 10, ArmorPen = 0, AdvBarracksRqd = 0, BarracksRqd = 2, Command = -1, Movement = 0, Partial = false, Shield = 0, Speed = 0, WeaponType = "Spear"}},
                   {"Brawler", new Unit() {Name = "Brawler", Health = 4, Attack = 4, Cost = 20, ArmorPen = 0, AdvBarracksRqd = 0, BarracksRqd = 4, Command = 0, Movement = 8, Partial = false, Shield = 0, Speed = 8, WeaponType = "Daggers"}},
                   {"Peon", new Unit() {Name = "Peon", Health = 2, Attack = 2, Cost = 30, ArmorPen = 0, AdvBarracksRqd = 0, BarracksRqd = 5, Command = 0, Movement = 5, Partial = false, Shield = 0, Speed = 5, WeaponType = "Daggers"}},
                   {"Bezerker", new Unit() {Name = "Bezerker", Health = 10, Attack = 4, Cost = 30, ArmorPen = 0, AdvBarracksRqd = 3, BarracksRqd = 1, Command = -2, Movement = 0, Partial = false, Shield = 0, Speed = 0, WeaponType = "Sword"}},
                   {"Scout", new Unit() {Name = "Scout", Health = 2, Attack = 1, Cost = 40, ArmorPen = 0, AdvBarracksRqd = 0, BarracksRqd = 4, Command = 0, Movement = 15, Partial = false, Shield = 0, Speed = 15, WeaponType = "Daggers"}},
                   {"Assassin", new Unit() {Name = "Assassin", Health = 8, Attack = 8, Cost = 40, ArmorPen = 0, AdvBarracksRqd = 0, BarracksRqd = 6, Command = 0, Movement = 5, Partial = false, Shield = 0, Speed = 5, WeaponType = "Sword"}},
                   {"Mage", new Unit() {Name = "Mage", Health = 4, Attack = 12, Cost = 60, ArmorPen = 0.5, AdvBarracksRqd = 0, BarracksRqd = 3, Command = -2, Movement = 0, Partial = false, Shield = 1, Speed = 0, WeaponType = "Mystic"}},
                   {"Knight", new Unit() {Name = "Knight", Health = 12, Attack = 12, Cost = 80, ArmorPen = 0, AdvBarracksRqd = 0, BarracksRqd = 8, Command = 4, Movement = 5, Partial = false, Shield = 0, Speed = 5, WeaponType = "Spear"}},
                   {"Settler", new Unit() {Name = "Settler", Health = 4, Attack = 2, Cost = 100, ArmorPen = 0, AdvBarracksRqd = 0, BarracksRqd = 8, Command = 0, Movement = 3, Partial = false, Shield = 0, Speed = 3, WeaponType = "Daggers"}},
                   {"MagiKnight", new Unit() {Name = "MagiKnight", Health = 12, Attack = 14, Cost = 120, ArmorPen = 0.5, AdvBarracksRqd = 0, BarracksRqd = 8, Command = 4, Movement = 5, Partial = false, Shield = 1, Speed = 5, WeaponType = "Mystic"}},
                   {"Crusader", new Unit() {Name = "Crusader", Health = 24, Attack = 24, Cost = 200, ArmorPen = 0, AdvBarracksRqd = 0, BarracksRqd = 10, Command = 4, Movement = 4, Partial = true, Shield = 2, Speed = 4, WeaponType = "Sword"}},
                   {"Captain", new Unit() {Name = "Captain", Health = 24, Attack = 12, Cost = 400, ArmorPen = 0, AdvBarracksRqd = 0, BarracksRqd = 12, Command = 60, Movement = 4, Partial = true, Shield = 2, Speed = 4, WeaponType = "Spear"}},
                   {"Ranger", new Unit() {Name = "Ranger", Health = 48, Attack = 48, Cost = 500, ArmorPen = 0, AdvBarracksRqd = 0, BarracksRqd = 12, Command = 8, Movement = 3, Partial = true, Shield = 4, Speed = 3, WeaponType = "Sword"}},
                   {"Templar", new Unit() {Name = "Templar", Health = 128, Attack = 168, Cost = 2000, ArmorPen = 0, AdvBarracksRqd = 0, BarracksRqd = 16, Command = 40, Movement = 3, Partial = true, Shield = 10, Speed = 3, WeaponType = "Mystic"}},
                   {"General", new Unit() {Name = "General", Health = 96, Attack = 64, Cost = 2500, ArmorPen = 0, AdvBarracksRqd = 0, BarracksRqd = 16, Command = 400, Movement = 3, Partial = true, Shield = 8, Speed = 3, WeaponType = "Mystic"}},
                   {"DragonKnight", new Unit() {Name = "DragonKnight", Health = 512, Attack = 756, Cost = 10000, ArmorPen = 0, AdvBarracksRqd = 1, BarracksRqd = 20, Command = 200, Movement = 2.5, Partial = true, Shield = 20, Speed = 2.5, WeaponType = "Elemental"}},
                   {"Dragon", new Unit() {Name = "Dragon", Health = 2048, Attack = 3500, Cost = 50000, ArmorPen = 0, AdvBarracksRqd = 3, BarracksRqd = 22, Command = 1000, Movement = 2, Partial = true, Shield = 30, Speed = 2, WeaponType = "Elemental"}},
                   {"Titan", new Unit() {Name = "Titan", Health = 6600, Attack = 10000, Cost = 200000, ArmorPen = 0, AdvBarracksRqd = 5, BarracksRqd = 24, Command = 4000, Movement = 1.5, Partial = true, Shield = 40, Speed = 1.5, WeaponType = "Elemental"}}
                   
               };
               public static Unit GetUnitByType(string typeName)
               {
                   if (!Defaults.ContainsKey(typeName))
                       throw new Exception(string.Format("Unit type {0} not found in Default Types.", typeName));

                   return Defaults[typeName];
               }
               public static Unit CreateUnit (string name, double quantity)
               {
                   var defaultUnit = Defaults[name];
                   return new Unit()
                   {
                       Name = defaultUnit.Name,
                       Health = defaultUnit.Health,
                       Attack = defaultUnit.Attack,
                       Cost = defaultUnit.Cost,
                       ArmorPen = defaultUnit.ArmorPen,
                       AdvBarracksRqd = defaultUnit.AdvBarracksRqd,
                       BarracksRqd = defaultUnit.BarracksRqd,
                       Command = defaultUnit.Command,
                       Speed = defaultUnit.Speed,
                       Partial = defaultUnit.Partial,
                       Shield = defaultUnit.Shield,
                       WeaponType = defaultUnit.WeaponType,
                       //Qty = quantity
                   };

               }
           }



    
            
  }

}

 










