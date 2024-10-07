using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    internal class Program
    {
        public abstract class Player
        {   
            public abstract bool MakeMove();
        }

        public class MonteCarloPlayer : Player
        {
            private Random random = new Random();
            private int BestScoreToEnd { get; set; }
            public MonteCarloPlayer() { }

            public override bool MakeMove()
            {
                return true;
            }
        }

        public class Card
        {
            public int value { get; set; }
            public Card(int value) { this.value = value; }
        }

        public class RandomPlayer : Player
        {
            private Random random = new Random();
            public RandomPlayer() { }
            public override bool MakeMove()
            {
                if (random.Next(2) == 1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public class Game
        {
            private Player player1 { get; set; }
            private Player player2 { get; set; }

            private int cards = 52;
            public Game(Player player1, Player player2)
            {
                this.player1 = player1;
                this.player2 = player2;
            }


        }

        public static List<Card> ResetDeck(List<Card> deck)
        {
            for (int i = 0; i < 11; i++)
            {
                deck[i] = new Card(i);
                deck[i + 13] = new Card(i);
                deck[i + 26] = new Card(i);
            }
            return deck;
        }
        static void Main(string[] args)
        {
            List<Card> deck = new List<Card>();

        }
    }
}
