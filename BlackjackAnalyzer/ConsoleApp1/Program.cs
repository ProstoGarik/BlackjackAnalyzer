using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;
using static ConsoleApp1.Program;

namespace ConsoleApp1
{
    internal class Program
    {
        public abstract class Player
        {
            private string name { get; set; }

            public int score { get; set; }
            public abstract string State(int score);
        }

        public class MonteCarloPlayer : Player
        {
            private Random random = new Random();

            private string name { get; set; }
            public int score {  get; set; }
            private List<int> bestScoreToEndArr { get; set; }
            private int bestScoreToEnd { get; set; }
            public MonteCarloPlayer() {}
            public void Train()
            {
                List<Card> deck = new List<Card>();
                deck = ResetDeck(deck);
                score = 0;
                bestScoreToEndArr = new List<int>();

                for (int i = 0; i < 50; i++)
                {
                    score = 0;
                    deck = ResetDeck(deck);
                    while (score < 21)
                    {
                        Card MCCardTry = new Card(0);
                        do
                        {
                            MCCardTry.value = random.Next(1, 11);
                        }
                        while (!deck.Contains(MCCardTry));

                        deck.Remove(MCCardTry);
                        if ((score + MCCardTry.value) > 21)
                        {
                            bestScoreToEndArr.Add(score);
                        }
                        else { score += MCCardTry.value; }
                    }
                }

                score = 0;
                bestScoreToEnd = FindMostFrequent(bestScoreToEndArr);
                Console.WriteLine("Лучший счёт: " + bestScoreToEnd);
            }
            public override string State(int score)
            {
                if (score > 21)
                {
                    Console.WriteLine("Монте-Карло проиграл со счётом " + score);
                    return "Lost";
                }
                if (score >= bestScoreToEnd)
                {
                    Console.WriteLine("Монте-Карло остановился на " + score);
                    return "Stopped";
                }
                else
                {
                    return "NotStopped";
                }
            }
        }

        public class Dealer : Player
        {
            public int score { get; set; }
            public Dealer() { }
            public override string State(int score)
            {
                if(score > 21)
                {
                    Console.WriteLine("Дилер проиграл со счётом " + score);
                    return "Lost";
                }   
                if(score >= 17)
                {
                    Console.WriteLine("Дилер остановился на " + score);
                    return "Stopped";
                }
                else
                {
                    return "NotStopped";
                }
            }
        }

        public class Card
        {
            public int value { get; set; }
            public Card(int value) { this.value = value; }

            public override bool Equals(object obj)
            {
                if (obj is Card otherCard)
                {
                    return this.value == otherCard.value;
                }
                return false;
            }
        }

        public class Game
        {
            private Player playerDealer { get; set; }
            private Player player2 { get; set; }

            private Random random { get; set; }

            public List<Card> deck {  get; set; }

            private int cards = 52;


            public Game(Player player1, Player player2)
            {
                this.playerDealer = player1;
                this.player2 = player2;
                deck = new List<Card>();
                random = new Random();
            }

            public void ResetDeck_()
            {
                deck = ResetDeck(deck);
            }
            

            public void GetMoves()
            {
                while (true)
                {
                    if (player2.State(player2.score) == "NotStopped")
                    {
                        Card player2CardTry = new Card(0);
                        do
                        {
                            player2CardTry.value = random.Next(1, 11);
                        }
                        while (!deck.Contains(player2CardTry));

                        deck.Remove(player2CardTry);
                        player2.score += player2CardTry.value;
                    }
                    if (playerDealer.State(playerDealer.score) == "NotStopped")
                    {
                        Card DealerCardTry = new Card(0);
                        do
                        {
                            DealerCardTry.value = random.Next(1, 11);
                        }
                        while (!deck.Contains(DealerCardTry));

                        deck.Remove(DealerCardTry);
                        playerDealer.score += DealerCardTry.value;
                    }

                    if (playerDealer.State(playerDealer.score) == "Lost" && (player2.State(player2.score) != "Lost"))
                    {
                        Console.WriteLine("У Дилера больше 21. Монте-Карло побеждает");
                        break;
                    }
                    else if (playerDealer.State(playerDealer.score) != "Lost" && (player2.State(player2.score) == "Lost"))
                    {
                        Console.WriteLine("У Монте-Карло больше 21. Дилер побеждает");
                        break;
                    }
                    else if(playerDealer.score > player2.score && (player2.State(player2.score) == "Stopped" && playerDealer.State(playerDealer.score) == "Stopped"))
                    {
                        Console.WriteLine("Дилер побеждает со счётом " + playerDealer.score + " (Монте-Карло: " + player2.score + ")");

                        break;
                    }
                    else if ((playerDealer.score < player2.score) && (player2.State(player2.score) == "Stopped" && playerDealer.State(playerDealer.score) == "Stopped"))
                    {
                        Console.WriteLine("Монте-Карло побеждает со счётом " + player2.score + " (Дилер: " + playerDealer.score + ")");
                        break;
                    }
                }
                
                
            }
            public int GetScore() { return playerDealer.score; }
        }

        static int FindMostFrequent(List<int> numbers)
        {
            Dictionary<int, int> frequencyMap = new Dictionary<int, int>();

            foreach (var number in numbers)
            {
                if (frequencyMap.ContainsKey(number))
                {
                    frequencyMap[number]++;
                }
                else
                {
                    frequencyMap[number] = 1;
                }
            }

            int mostFrequent = numbers[0];
            int maxCount = 0;

            foreach (var kvp in frequencyMap)
            {
                if (kvp.Value > maxCount)
                {
                    maxCount = kvp.Value;
                    mostFrequent = kvp.Key;
                }
            }

            return mostFrequent;
        }
        public static List<Card> ResetDeck(List<Card> deck)
        {
            deck.Clear();
            for (int i = 0; i < 52; i++)
            {
                deck.Add(new Card(0));
            }
            for (int i = 0; i < 10; i++)
            {
                deck[i].value = i + 1;
                deck[i + 13].value = i + 1;
                deck[i + 26].value = i + 1;
                deck[i + 39].value = i + 1;
            }

            for (int i = 10; i < 13; i++)
            {
                deck[i].value = 10;
                deck[i + 13].value = 10;
                deck[i + 26].value = 10;
                deck[i + 39].value = 10;
            }

            return deck;
        }
        static void Main(string[] args)
        {
            Player playerDealer = new Dealer();
            Player player2 = new MonteCarloPlayer();
            Game game = new Game(playerDealer,player2);
            game.ResetDeck_();
            Console.WriteLine(string.Join(" ", game.deck.Select(x => x.value)));

            for(int i = 0; i<50; i++)
            {
                if (player2 is MonteCarloPlayer monteCarloPlayer)
                {
                    monteCarloPlayer.Train();
                }
                game.GetMoves();
                Console.WriteLine("\n");
            }
            

            //game.GetMoves();
            //Console.WriteLine("Счёт: " + game.GetScore());
            //Console.WriteLine(string.Join(" ", game.deck.Select(x => x.value)));

            Console.ReadLine();
        }
    }
}
