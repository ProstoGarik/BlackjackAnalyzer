using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Schema;
using static ConsoleApp1.Program;

namespace ConsoleApp1
{
    internal class Program
    {
        public abstract class Player
        {
            public string name { get; set; }

            public int score { get; set; }
            public abstract string State(int score, List<Card> deck);

            public abstract string GetName();
        }

        public class MonteCarloPlayer : Player
        {
            private Random random = new Random();

            public new string name = "Монте-Карло";
            public new int score {  get; set; }
            private List<float> bestScoreToEndArr { get; set; }
            private List<float> bestCoofToEndArr {  get; set; } 
            private float bestScoreToEnd { get; set; }
            private float bestCoofToEnd { get; set; }
            public MonteCarloPlayer() {}
            public void Train()
            {
                List<Card> deck = new List<Card>();
                deck = ResetDeck(deck);
                score = 0;
                bestScoreToEndArr = new List<float>();
                bestCoofToEndArr = new List<float>();

                for (int i = 0; i < 500; i++)
                {
                    score = 0;
                    deck = ResetDeck(deck);
                    while (score <= 21)
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
                            bestCoofToEndArr.Add(deck.Count/DeckSum(deck));
                            bestScoreToEndArr.Add(score);
                            break;
                        }
                        else { score += MCCardTry.value; }
                    }
                }

            score = 0;
                bestScoreToEnd = FindMostFrequent(bestScoreToEndArr);
                bestCoofToEnd = FindMostFrequent(bestCoofToEndArr);
                Console.WriteLine("Лучший счёт для Монте-Карло: " + bestScoreToEnd + " " + bestCoofToEnd);
            }
            public override string State(int score, List<Card> deck)
            {
                if (score > 21)
                {
                    return "Lost";
                }
                if ( score >= bestScoreToEnd || (bestCoofToEnd <= (deck.Count / DeckSum(deck))) )
                {
                    return "Stopped";
                }
                else
                {
                    return "NotStopped";
                }
            }

            public override string GetName()
            {
                return name;
            }
        }

        public class Dealer : Player
        {
            public new int score { get; set; }

            public new string name = "Дилер";
            public Dealer() { }

            public Dealer(int score)
            {
                this.score = score;
            }

            public override string State(int score, List<Card> deck)
            {
                if(score > 21)
                {
                    return "Lost";
                }   
                if(score >= 17)
                {
                    return "Stopped";
                }
                else
                {
                    return "NotStopped";
                }
            }

            public override string GetName()
            {
                return name;
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
                Console.WriteLine("Колода сброшена");
            }

            private void ResetScores()
            {
                player2.score = 0;
                playerDealer.score = 0;
            }
            

            public int GetMoves()
            {
                while (true)
                {
                    Thread.Sleep(100);
                    if (player2.State(player2.score, deck) == "NotStopped")
                    {
                        Card player2CardTry = new Card(0);
                        do
                        {
                            player2CardTry.value = random.Next(1, 11);
                        }
                        while (!deck.Contains(player2CardTry));
                        //Console.WriteLine("Монте-Карло вытянул " + player2CardTry.value);
                        deck.Remove(player2CardTry);
                        player2.score += player2CardTry.value;
                    }
                    if (playerDealer.State(playerDealer.score, deck) == "NotStopped")
                    {
                        Card DealerCardTry = new Card(0);
                        do
                        {
                            DealerCardTry.value = random.Next(1, 11);
                        }
                        while (!deck.Contains(DealerCardTry));  
                        //Console.WriteLine("Дилер вытянул " + DealerCardTry.value);
                        deck.Remove(DealerCardTry);
                        playerDealer.score += DealerCardTry.value;
                    }
                    if (playerDealer.State(playerDealer.score, deck) == "Lost" && (player2.State(player2.score, deck) != "Lost"))
                    {
                        Console.WriteLine("У Дилера больше 21. Монте-Карло побеждает");
                        ResetScores();
                        return 1;
                    }
                    else if (playerDealer.State(playerDealer.score, deck) != "Lost" && (player2.State(player2.score, deck) == "Lost"))
                    {
                        Console.WriteLine("У Монте-Карло больше 21. Дилер побеждает");
                        ResetScores();
                        return 2;
                    }
                    else if(playerDealer.score > player2.score && (player2.State(player2.score, deck) == "Stopped" && (playerDealer.State(playerDealer.score, deck) == "Stopped")))
                    {
                        Console.WriteLine("Дилер побеждает со счётом " + playerDealer.score + " (Монте-Карло: " + player2.score + ")");
                        ResetScores();
                        return 2;
                    }
                    else if ((playerDealer.score < player2.score) && (player2.State(player2.score, deck) == "Stopped" && (playerDealer.State(playerDealer.score, deck) == "Stopped")))
                    {
                        Console.WriteLine("Монте-Карло побеждает со счётом " + player2.score + " (Дилер: " + playerDealer.score + ")");
                        ResetScores();
                        return 1;
                    }
                    else if (player2.score == playerDealer.score)
                    {
                        Console.WriteLine("Ничья");
                        ResetScores();
                        return 0;
                    }
                }
            }
            public int GetScore() { return playerDealer.score; }
        }

        static float FindMostFrequent(List<float> numbers)
        {
            Dictionary<float, float> frequencyMap = new Dictionary<float, float>();

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

            float mostFrequent = numbers[0];
            float maxCount = 0;

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

        public static float DeckSum(List<Card> deck)
        {
            float sum = 0;
            for (int i = 0; i < deck.Count; i++)
            {
                sum+= deck[i].value;
            }
            return sum;
        }
        static void Main(string[] args)
        {
            const int gameCount = 50;
            float player1Win = 0;
            float playerDealerWin = 0;
            float drawCount = 0;
            float errorCount = 0;

            Player playerDealer = new Dealer();
            Player player2 = new MonteCarloPlayer();
            Game game = new Game(playerDealer,player2);
            //Console.WriteLine(string.Join(" ", game.deck.Select(x => x.value)));

            for(int i = 0; i< gameCount; i++)
            {
                game.ResetDeck_();
                //Console.WriteLine(string.Join(" ", game.deck.Select(x => x.value)));
                Console.WriteLine("Игра " + (i+1));
                if (player2 is MonteCarloPlayer monteCarloPlayer)
                {
                    monteCarloPlayer.Train();
                }
                switch (game.GetMoves())
                {
                    case 1:
                        player1Win++;
                        break;
                    case 2:
                        playerDealerWin++;
                        break;
                    case 0:
                        drawCount++;
                        break;
                    default:
                        errorCount++;
                        break;
                }
                //Console.WriteLine(string.Join(" ", game.deck.Select(x => x.value)));
                Console.WriteLine("\n");
            }
            Console.WriteLine("Процент побед: " + player2.GetName() + " " + player1Win / gameCount * 100);
            Console.WriteLine("Процент побед: " + playerDealer.GetName() + " " + playerDealerWin / gameCount * 100);
            Console.WriteLine("Процент ничей: " + drawCount / gameCount * 100);
            Console.WriteLine("Процент ошибок: " + errorCount / gameCount * 100);

            Console.ReadLine();
        }
    }
}
