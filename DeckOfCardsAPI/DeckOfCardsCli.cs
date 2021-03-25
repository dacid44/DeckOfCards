using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Newtonsoft.Json.Linq;

namespace DeckOfCardsAPI {
   class DeckOfCardsCli {
      static void Main(string[] args) {
         if (args.Length > 0 && args[0] == "lib") {
            Main2(args[1..]);
         } else {
            JsonResponse initial = new JsonResponse("https://deckofcardsapi.com/api/deck/new/shuffle/?deck_count=1");
            string id = (string) initial.Json["deck_id"];
            Console.WriteLine("Deck ID: " + id);
            while (true) {
               Console.Write("Enter number of cards to draw: ");
               int count = int.Parse(Console.ReadLine());
               JObject drawnCards = DrawCards(id, count);
               for (int i = 0; i < count; i++) {
                  string output = "Card " + (i + 1) + ": ";
                  output += (string) drawnCards["cards"][i]["value"] + " of ";
                  output += (string) drawnCards["cards"][i]["suit"];
                  Console.WriteLine(output);
               }
               Console.WriteLine("There are " + (int) drawnCards["remaining"] + " cards remaining in the deck.");
            }
         }
      }

      static void Main2(string[] args) {
         DeckOfCards deck = new DeckOfCards();
         Console.WriteLine($"Deck ID: {deck.DeckID}, {deck.CardsRemaining} cards remaining.");
         while (true) {
            Console.Write("Enter number of cards to draw (or s/l/c/x to shuffle/list/count/exit): ");
            string input = Console.ReadLine();
            if (input == "s") {
               deck.Shuffle();
               Console.WriteLine($"Shuffled the deck, {deck.CardsRemaining} cards remaining.");
            } else if (input == "l") {
               Console.WriteLine($"So far {deck.DrawnCards.Count} cards have been drawn. They are:");
               foreach (Card card in deck.DrawnCards) {
                  Console.WriteLine(card);
               }
               Console.WriteLine($"There are {deck.CardsRemaining} cards remaining.");
            } else if (input == "c") {
               Console.WriteLine($"There are {deck.CardsRemaining} cards remaining.");
            } else if (input == "x") {
               Console.WriteLine("Exiting.");
               break;
            } else {
               int count = int.Parse(input);
               if (count == 0) {
                  Console.WriteLine($"No cards drawn. {deck.CardsRemaining} cards remaining.");
               } else {
                  List<Card> drawnCards = deck.Draw(count);
                  Console.WriteLine($"Drew {drawnCards.Count} cards. They are:");
                  foreach (Card card in drawnCards) {
                     Console.WriteLine(card);
                  }
                  Console.WriteLine($"There are {deck.CardsRemaining} cards remaining.");
               }
            }
         }
      }
      
      static JObject DrawCards(string id, int count) {
         return new JsonResponse("https://deckofcardsapi.com/api/deck/" + id + "/draw/?count=" + count).Json;
      }
   }
}