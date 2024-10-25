using System;
using System.CodeDom;
using System.Collections.Concurrent;
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
        private static readonly Random random = new Random();
        public abstract class Player
        {
            public string name { get; set; }

            public int score { get; set; }
            public abstract string State(int score, List<Card> deck);

            public abstract string GetName();
        }

        public class MonteCarloPlayer : Player
        {

            public new string name = "Монте-Карло";
            public new int score {  get; set; }
            private List<float> bestScoreToEndArr { get; set; }
            private List<float> bestCoofToEndArr {  get; set; } 
            private float bestScoreToEnd { get; set; }
            private float bestCoofToEnd { get; set; }
            private float bestThreshold { get; set; }
            private float bestWinRate { get; set; }
            public MonteCarloPlayer() {}
            public void Train()
            {
                List<Card> deck = new List<Card>();
                deck = ResetDeck(deck);
                score = 0;
                bestScoreToEndArr = new List<float>();
                object lockObject = new object();
                Parallel.For(0, 500, i =>
                {
                    List<Card> localDeck = ResetDeck(null);
                    int localScore = 0;
                    Random localRandom = new Random();

                    while (localScore <= 21)
                    {
                        try
                        {
                            Card MCCardTry = DrawCard(localDeck, localRandom);
                            localScore += MCCardTry.value;

                            if (localScore + MCCardTry.value >= 21)
                            {
                                lock (lockObject)
                                {
                                    bestScoreToEndArr.Add(localScore);
                                }
                                break;
                            }
                        }
                        catch (InvalidOperationException)
                        {
                            break;
                        }
                    }
                });
                bestScoreToEnd = FindMostFrequent(bestScoreToEndArr);
                //Console.WriteLine("Лучший счёт для Монте-Карло: " + bestScoreToEnd);
            }

            public override string State(int score, List<Card> deck)
            {
                if (score > 21)
                {
                    return "Lost";
                }
                if (score >= bestScoreToEnd)
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

        public class RandomPlayer : Player
        {
            private int scoreToEnd;

            public new string name = "Рандом";
            public RandomPlayer() { }
            public void SetRandom()
            {
                scoreToEnd = random.Next(22);
            }
            public override string State(int score, List<Card> deck)
            {
                if (score > 21)
                {
                    return "Lost";
                }
                if (score >= scoreToEnd)
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

            public override int GetHashCode()
            {
                return -1584136870 + value.GetHashCode();
            }
        }

        public class Game
        {
            private Player playerDealer { get; set; }
            private Player player2 { get; set; }

            public List<Card> deck {  get; set; }

            private int cards = 52;


            public Game(Player player1, Player player2)
            {
                this.playerDealer = player1;
                this.player2 = player2;
                deck = new List<Card>();
            }

            public void ResetDeck_()
            {
                deck = ResetDeck(deck);
                //Console.WriteLine("Колода сброшена");
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
                        Card player2CardTry = DrawCard(deck, random);
                        //Console.WriteLine("Монте-Карло вытянул " + player2CardTry.value);
                        player2.score += player2CardTry.value;
                    }
                    if (player2.State(player2.score, deck) == "Lost")
                    {
                        //Console.WriteLine("У " +player2.GetName()+" больше 21. Дилер побеждает");
                        ResetScores();
                        return 2;
                    }

                    if (playerDealer.State(playerDealer.score, deck) == "NotStopped")
                    {
                        Card DealerCardTry = DrawCard(deck, random);
                        //Console.WriteLine("Дилер вытянул " + DealerCardTry.value);
                        playerDealer.score += DealerCardTry.value;
                    }
                    if (playerDealer.State(playerDealer.score, deck) == "Lost")
                    {
                        //Console.WriteLine("У Дилера больше 21. "+player2.GetName()+" побеждает");
                        ResetScores();
                        return 1;
                    }
                    else if (playerDealer.score > player2.score && (player2.State(player2.score, deck) == "Stopped" && (playerDealer.State(playerDealer.score, deck) == "Stopped")))
                    {
                        //Console.WriteLine("Дилер побеждает со счётом " + playerDealer.score + " ("+player2.GetName()+": " + player2.score + ")");
                        ResetScores();
                        return 2;
                    }
                    else if ((playerDealer.score < player2.score) && (player2.State(player2.score, deck) == "Stopped" && (playerDealer.State(playerDealer.score, deck) == "Stopped")))
                    {
                        //Console.WriteLine(player2.GetName()+" побеждает со счётом " + player2.score + " (Дилер: " + playerDealer.score + ")");
                        ResetScores();
                        return 1;
                    }
                    else if (player2.score == playerDealer.score || (player2.State(player2.score, deck) == "Lost" && playerDealer.State(playerDealer.score, deck) == "Lost"))
                    {
                        //Console.WriteLine("Ничья");
                        ResetScores();
                        return 0;
                    }
                }
            }
            public int GetScore() { return playerDealer.score; }
        }

        static Card DrawCard(List<Card> deck, Random random)
        {
            int index = random.Next(deck.Count);
            Card card = deck[index];
            deck.RemoveAt(index);
            return card;
        }

        static List<Card> ResetDeck()
        {
            List<Card> deck = new List<Card>();
            for (int i = 1; i<=13; i++)
            {
                for(int j = 0; j< 4; j++)
                {
                    deck.Add(new Card(i > 10 ? 10 : 1));
                }
            }
            return deck;
        }

        static float FindMostFrequent(List<float> numbers)
        {
            if (numbers == null || numbers.Count == 0)
                throw new InvalidOperationException("Список не должен быть пустым.");

            ConcurrentDictionary<float, int> frequencyMap = new ConcurrentDictionary<float, int>();
            Parallel.ForEach(numbers, number =>
            {
                frequencyMap.AddOrUpdate(number, 1, (key, oldValue) => oldValue + 1);
            });

            float mostFrequent = numbers[0];
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
            deck = new List<Card>(52);

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
            //Console.WriteLine(string.Join(" ", game.deck.Select(x => x.value)));
            const int gameCount = 50;
            float player1Win = 0;
            float playerDealerWin = 0;
            float drawCount = 0;
            float errorCount = 0;

            Player playerDealer = new Dealer();
            Player player2 = new MonteCarloPlayer();
            Console.WriteLine("Игра с Монте-Карло:");
            Parallel.For(0, gameCount, i =>
            {
                Game game = new Game(playerDealer, player2);
                game.ResetDeck_();
                Console.Write(i + 1 + " ");

                if (player2 is MonteCarloPlayer monteCarloPlayer)
                {
                    monteCarloPlayer.Train();
                }

                switch (game.GetMoves())
                {
                    case 1:
                        IncrementWinCount(ref player1Win);
                        break;
                    case 2:
                        IncrementWinCount(ref playerDealerWin);
                        break;
                    case 0:
                        IncrementWinCount(ref drawCount);
                        break;
                    default:
                        IncrementWinCount(ref errorCount);
                        break;
                }
            });
            Console.WriteLine("\n\nПроцент побед " + player2.GetName() + ": " + player1Win / gameCount * 100);
            Console.WriteLine("Процент побед " + playerDealer.GetName() + ": " + playerDealerWin / gameCount * 100);
            Console.WriteLine("Процент ничей: " + drawCount / gameCount * 100 + "\n");
            if(errorCount!=0)
            {
                Console.WriteLine("Процент ошибок: " + errorCount / gameCount * 100);
            }
            


            player1Win = 0;
            playerDealerWin = 0;
            drawCount = 0;
            errorCount = 0;

            player2 = new RandomPlayer();
            Console.WriteLine("Игра с рандомом:");
            Parallel.For(0, gameCount, i =>
            {
                Game game = new Game(playerDealer, player2);
                game.ResetDeck_();
                Console.Write(i + 1 + " ");

                if (player2 is RandomPlayer randomPlayer)
                {
                    randomPlayer.SetRandom();
                }

                switch (game.GetMoves())
                {
                    case 1:
                        IncrementWinCount(ref player1Win);
                        break;
                    case 2:
                        IncrementWinCount(ref playerDealerWin);
                        break;
                    case 0:
                        IncrementWinCount(ref drawCount);
                        break;
                    default:
                        IncrementWinCount(ref errorCount);
                        break;
                }
            });
            Console.WriteLine("\n");
            Console.WriteLine("Процент побед " + player2.GetName() + ": " + player1Win / gameCount * 100);
            Console.WriteLine("Процент побед " + playerDealer.GetName() + ": " + playerDealerWin / gameCount * 100);
            Console.WriteLine("Процент ничей: " + drawCount / gameCount * 100);
            if (errorCount != 0)
            {
                Console.WriteLine("Процент ошибок: " + errorCount / gameCount * 100);
            }

            Console.ReadLine();
        }

        static void IncrementWinCount(ref float count)
        {
            lock (typeof(Program))
            {
                count++;
            }
        }
    }
}
