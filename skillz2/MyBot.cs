using System.Collections.Generic;
using System.Linq;
using PenguinGame;

namespace MyBot
{
    class MyBot :ISkillzBot
    {
        public void DoTurn(Game game)
        {
            utils.game = game;
            game.DoSimpleStrategy();
        }
    }
    public static class utils
    {
        public static Game game;
        /*public static int IcebergEndangermentLevel(this Iceberg berg,bool ByEnemy)
        {
            Player attacker;
            if (ByEnemy)
            {
                attacker = game.GetEnemy();
            }
            else
            {
                attacker = game.GetMyself();
            }
            List<Iceberg> EnemyAndMyself = game.GetEnemyIcebergs().Concat(game.GetMyIcebergs()).ToList();
            EnemyAndMyself =
            (from Iceberg i in EnemyAndMyself
             where !i.Equals(berg)
             orderby i.GetTurnsTillArrival(berg)
             select i).ToList();
            int endangerment = 0;
            foreach(Iceberg i in EnemyAndMyself)
            {

            }
        }*/
        public static double Eval(this Iceberg berg, Player PrespectivePlayer)
        {
            Iceberg[] bergs = 
                (from Iceberg i in PrespectivePlayer.Icebergs 
                 where !i.Equals(berg) 
                 select i).ToArray();
            int[] penguinsToSend = 
                (from Iceberg i in bergs 
                 let send= berg.SimplifiedPenguinsToSend(i)
                 where send<i.PenguinAmount
                 select send).ToArray();
            if (penguinsToSend.Length == 0) return int.MaxValue;
            int min = penguinsToSend.Min();
            return (double)min / berg.PenguinsPerTurn;
        }
        public static int BestValue(this Player player)
        {
            List<Iceberg> bergs = (from Iceberg i in game.GetAllIcebergs() where !i.Owner.Equals(player) orderby i.Eval(player) select i).ToList();
        }
        public static int PredictIcebergStateAfterAll(this Iceberg berg)
        {
            List<PenguinGroup> groups =
                (from PenguinGroup a in game.GetAllPenguinGroups()
                 where a.Destination == berg
                 select a).ToList();
            groups.Sort((x, y) =>
            {
                return x.TurnsTillArrival - y.TurnsTillArrival;
            });

            int BergSign = berg.BergSign();
            int CurrentTurn = 0;
            int PenguinAmount = berg.PenguinAmount * BergSign;

            foreach (PenguinGroup penguins in groups)
            {
                int amount = penguins.GroupAmountAfterClash();
                PenguinAmount += berg.PenguinsPerTurn * (penguins.TurnsTillArrival - CurrentTurn) * BergSign;
                PenguinAmount += amount * penguins.GroupSign();
                BergSign = System.Math.Sign(PenguinAmount);
                CurrentTurn = penguins.TurnsTillArrival;
            }
            return PenguinAmount;
        }
        public static bool CanAttackSimplified(this Iceberg From, Iceberg To)
        {
            int amount = To.SimplifiedPenguinsToSend(From);
            return From.PenguinAmount > amount + 1;
        }
        public static int SimplifiedPenguinsToSend(this Iceberg berg, Iceberg sender)
        {
            List<PenguinGroup> groups =
                (from PenguinGroup a in game.GetAllPenguinGroups()
                 where a.Destination == berg
                 select a).ToList();
            groups.Sort((x, y) =>
            {
                return x.TurnsTillArrival - y.TurnsTillArrival;
            });

            int BergSign = berg.BergSign();
            int CurrentTurn = 0;
            int PenguinAmount = berg.PenguinAmount * BergSign;

            foreach (PenguinGroup penguins in groups)
            {
                int amount = penguins.GroupAmountAfterClash();
                PenguinAmount += berg.PenguinsPerTurn * (penguins.TurnsTillArrival - CurrentTurn) * BergSign;
                PenguinAmount += amount * penguins.GroupSign();
                BergSign = System.Math.Sign(PenguinAmount);
                CurrentTurn = penguins.TurnsTillArrival;
            }
            if (CurrentTurn < sender.GetTurnsTillArrival(berg))
            {
                PenguinAmount += berg.PenguinsPerTurn * (sender.GetTurnsTillArrival(berg) - CurrentTurn) * BergSign;
            }
            if (BergSign == sender.BergSign()) return 0;
            return PenguinAmount*sender.BergSign()*-1;
        }
        public static int PredictIcebergState(this Iceberg berg, int turns)
        {
            List<PenguinGroup> groups =
                (from PenguinGroup a in game.GetAllPenguinGroups()
                 where a.Destination == berg && a.TurnsTillArrival <= turns
                 select a).ToList();
            groups.Sort((x, y) =>
            {
                return x.TurnsTillArrival - y.TurnsTillArrival;
            });
            int BergSign=berg.BergSign();
            int CurrentTurn = 0;
            int PenguinAmount = berg.PenguinAmount*BergSign;
            
            foreach (PenguinGroup penguins in groups)
            {
                int amount = penguins.GroupAmountAfterClash();
                PenguinAmount += berg.PenguinsPerTurn * (penguins.TurnsTillArrival - CurrentTurn)*BergSign;
                PenguinAmount += amount * penguins.GroupSign();
                BergSign = System.Math.Sign(PenguinAmount);
                CurrentTurn = penguins.TurnsTillArrival;
            }
            if (CurrentTurn < turns)
            {
                PenguinAmount += berg.PenguinsPerTurn * (turns - CurrentTurn) * BergSign;
            }
            return PenguinAmount;

        }
        public static int GroupAmountAfterClash(this PenguinGroup a)
        {
            List<PenguinGroup> Groups =
                (from PenguinGroup i in game.GetAllPenguinGroups()
                 where !i.Owner.Equals(a.Owner) && i.Source.Equals(a.Destination) && a.Source.Equals(i.Destination)
                 select i
                 ).ToList();
            int Amount = a.PenguinAmount;
            if (Groups.Count == 0) return Amount;
            int JourneyLength = a.Source.GetTurnsTillArrival(a.Destination);
            foreach(PenguinGroup g in Groups)
            {
                if (g.TurnsTillArrival + a.TurnsTillArrival >= JourneyLength) Amount -= g.PenguinAmount;
            }
            return System.Math.Max(0, Amount);
        }
        public static int BergSign(this Iceberg a)
        {
            if (a.Owner.Equals(game.GetEnemy())) return -1;
            if (a.Owner.Equals(game.GetNeutral())) return 0;
            return 1;
        }
        public static int GroupSign(this PenguinGroup a)
        {
            if (a.Owner.Equals(game.GetEnemy())) return -1;
            return 1;
        }
        public static Iceberg Best(this Iceberg[] bergs)
        {
            return bergs.Aggregate((Iceberg a, Iceberg b) =>
            {
                if (a.PenguinAmount > b.PenguinAmount) return a;
                return b;
            });
        }
        public static Iceberg Worst(this Iceberg[] bergs)
        {
            return bergs.Aggregate((Iceberg a, Iceberg b) =>
            {
                if (a.PenguinAmount < b.PenguinAmount) return a;
                return b;
            });
        }
        public static Iceberg SecondBest(this Iceberg[] bergs)
        {
            Iceberg a = bergs.Best();
            Iceberg[] temp = (from Iceberg item in bergs where item != a select item).ToArray();
            return temp.Best();
        }
        public static int PenguinsInNTurns(this Iceberg berg,int turn)
        {
            return berg.PenguinAmount + berg.PenguinsPerTurn * turn;
        }
        public static int PenguinsToSendto(this Iceberg from, Iceberg to)
        {
            return to.PenguinsInNTurns(from.GetTurnsTillArrival(to)) + 1;
        }
        public static bool CanAttack(this Iceberg a, Iceberg b)
        {
            return (a.PenguinsToSendto(b) < a.PenguinAmount&& ! a.AlreadyActed);
        }
        public static int SumPenguins(this Iceberg[] Bergs)
        {
            return Bergs.Sum((Iceberg berg) =>
            {
                return berg.PenguinAmount;
            });
        }
        
        public static void DoSimpleStrategy(this Game game)
        {
            try
            {
                Iceberg[] MyBergs = game.GetMyIcebergs();
                Iceberg[] EnemyBergs = game.GetEnemyIcebergs();
                Iceberg MyBest = MyBergs.Best();
                Iceberg EnemyWorst = EnemyBergs.Worst();
                Iceberg[] NeutralBergs = game.GetNeutralIcebergs();
                if(NeutralBergs.Length==0&& !MyBest.CanAttack(EnemyWorst))
                {
                    if (MyBergs.SumPenguins() > EnemyWorst.PenguinAmount*1.3)
                    {
                        foreach(Iceberg i in MyBergs)
                        {
                            i.SendPenguins(EnemyWorst, (int) (i.PenguinAmount / 1.2) );
                        }
                    }
                }
                if (MyBergs.Length > 1)
                {
                    Iceberg MySecondBest = MyBergs.SecondBest();
                    if (MySecondBest.CanAttack(EnemyWorst))
                    {
                        MySecondBest.SendPenguins(EnemyWorst, MySecondBest.PenguinsToSendto(EnemyWorst));
                    }
                }
                if (MyBest.CanUpgrade()&& !MyBest.AlreadyActed)
                {
                    MyBest.Upgrade();
                }
                if (MyBest.CanAttack(EnemyWorst))
                {
                    MyBest.SendPenguins(EnemyWorst, MyBest.PenguinsToSendto(EnemyWorst));
                }
                else
                {
                    if (NeutralBergs.Length > 0)
                    {
                        Iceberg NeutralWorst = NeutralBergs.Worst();
                        if (NeutralWorst.PenguinAmount < MyBest.PenguinAmount&&!MyBest.AlreadyActed)
                        {
                            MyBest.SendPenguins(NeutralWorst, NeutralWorst.PenguinAmount + 1);
                        }
                    }
                }
            }
            catch(System.Exception e)
            {
                System.Console.WriteLine(e);
            }
        }
    }
}
