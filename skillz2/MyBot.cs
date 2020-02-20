
using System.Linq;
using PenguinGame;

namespace MyBot
{
    class MyBot :ISkillzBot
    {
        public void DoTurn(Game game)
        {
            game.DoSimpleStrategy();
        }
    }
    public static class utils
    {
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
