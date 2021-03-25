using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using Newtonsoft.Json.Linq;

namespace DeckOfCardsAPI {
   public class DeckOfCards {
      public readonly bool Success;
      public readonly string DeckID;

      public bool IsShuffled { get; private set; }
      public int CardsRemaining { get; private set; }
      public string ErrorMessage { get; private set; }

      private List<Card> _drawnCards;
      public List<Card> DrawnCards => _drawnCards.AsReadOnly().ToList();

      public DeckOfCards(int deckCount = 1) {
         JsonResponse response;
         try {
            response = new JsonResponse(
               "https://deckofcardsapi.com/api/deck/new/shuffle/?deck_count=" + deckCount);
         } catch (WebException e) {
            Success = false;
            ErrorMessage = e.Message;
            return;
         }
         Success = (bool) (response.Json["success"] ?? false);
         if (!Success) { return; }
         
         DeckID = (string) response.Json["deck_id"];
         IsShuffled = (bool) (response.Json["shuffled"] ?? false);
         CardsRemaining = (int) response.Json["remaining"];

         _drawnCards = new List<Card>();
      }

      public List<Card> Draw(int count) {
         JsonResponse response;
         try {
            response = new JsonResponse(
               "https://deckofcardsapi.com/api/deck/" + DeckID + "/draw/?count=" + count);
         } catch (WebException e) {
            ErrorMessage = e.Message;
            return null;
         }
         if (!(bool) (response.Json["success"] ?? false)) { return null; }
         
         JArray cards = (JArray) response.Json["cards"];
         List<Card> toReturn = new List<Card>();
         foreach (JObject cardObj in cards) {
            toReturn.Add(new Card(cardObj));
         }
         _drawnCards.AddRange(toReturn);
         
         CardsRemaining = (int) response.Json["remaining"];
         return toReturn;
      }

      public bool Shuffle() {
         JsonResponse response;
         try {
            response = new JsonResponse("https://deckofcardsapi.com/api/deck/" + DeckID + "/shuffle");
         } catch (WebException e) {
            ErrorMessage = e.Message;
            return false;
         }
         if (!(bool) (response.Json["success"] ?? false)) { return false; }
         
         IsShuffled = (bool) (response.Json["shuffled"] ?? false);
         CardsRemaining = (int) response.Json["remaining"];
         _drawnCards.Clear();
         return true;
      }
   }

   public readonly struct Card {
      public readonly string Code;
      public readonly string SvgImage;
      public readonly string PngImage;
      public readonly string Value;
      public readonly string Suit;
      
      public Card(JObject cardObj) {
         Code = (string) cardObj["code"];
         SvgImage = (string) cardObj["images"]["svg"];
         PngImage = (string) cardObj["images"]["png"];

         TextInfo ti = CultureInfo.CurrentCulture.TextInfo;
         if (!(cardObj["images"] is null)) {
            Value = ti.ToTitleCase(ti.ToLower((string) cardObj["value"] ?? ""));
         } else {
            Value = null;
         }
         if (!(cardObj["images"] is null)) {
            Suit = ti.ToTitleCase(ti.ToLower((string) cardObj["suit"] ?? ""));
         } else {
            Suit = null;
         }
      }
      public Card(string code, string svgImage, string pngImage, string value, string suit) {
         Code = code;
         SvgImage = svgImage;
         PngImage = pngImage;
         Value = value;
         Suit = suit;
      }

      public override string ToString() => $"{Value} of {Suit}";
   }
   
   class JsonResponse {
      public readonly JObject Json;
      
      public JsonResponse(string url) {
         WebRequest request = WebRequest.Create(url);
         request.ContentType = "Application/JSON";
         var response = (HttpWebResponse) request.GetResponse();
         var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
         Json = JObject.Parse(responseString);
      }
   }
}