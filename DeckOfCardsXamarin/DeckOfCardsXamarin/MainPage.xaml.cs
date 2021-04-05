using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace DeckOfCardsXamarin {
   public partial class MainPage : ContentPage {
      private static readonly Regex _numerals = new Regex("[^0-9]+");
      
      private DeckOfCards _deck;
      
      public MainPage() {
         InitializeComponent();
         _deck = new DeckOfCards();
         if (_deck.Success) {
            DeckIDField.Text = "Deck ID: " + _deck.DeckID;
         } else {
            MessageField.Text = _deck.ErrorMessage;
         }
      }
      
      private void OnDrawInputChanged(object sender, TextChangedEventArgs e) {
         if (NumCards.Text.Length >= 4 || _numerals.IsMatch(e.NewTextValue)) {
            ((Entry) sender).Text = e.OldTextValue;
         }
      }
      
      private void DrawEvent(object sender, EventArgs e) {
         List<Card> drawnCards = _deck.Draw(int.Parse(NumCards.Text == "" ? "0" : NumCards.Text));
         if (drawnCards is null) {
            MessageField.Text = _deck.ErrorMessage;
            return;
         }
         DisplayCards(drawnCards);
         MessageField.Text = $"Drew {drawnCards.Count} cards, {_deck.CardsRemaining} cards remaining.";
      }

      private void ShuffleEvent(object sender, EventArgs e) {
         bool success = _deck.Shuffle();
         if (!success) {
            MessageField.Text = _deck.ErrorMessage;
            return;
         }
         DisplayCards(new List<Card>());
         MessageField.Text = $"Shuffled the deck, {_deck.CardsRemaining} cards remaining.";
      }

      private void ListEvent(object sender, EventArgs e) {
         if (!_deck.Success) {
            MessageField.Text = "Deck initialization returned an error: " + _deck.ErrorMessage;
            return;
         }
         DisplayCards(_deck.DrawnCards);
         MessageField.Text =
            $"{_deck.DrawnCards.Count} cards have been drawn, and there are {_deck.CardsRemaining} cards remaining.";
      }
      
      private void DisplayCards(List<Card> cards) {
         int rows = 0;
         for (; rows * CalcMaxCardRow(rows) < cards.Count; rows++) { }
         int cardsPerRow = CalcMaxCardRow(rows);
         
         CardGrid.RowDefinitions.Clear();
         CardGrid.Children.Clear();
         for (int i = 0; i < rows; i++) {
            int numCards = Math.Min(cardsPerRow, cards.Count - (i * cardsPerRow));
            if (numCards <= 0) { break; }
            CardGrid.RowDefinitions.Add(new RowDefinition());
            Grid row = new Grid();
            Grid.SetRow(row, i);
            CardGrid.Children.Add(row);
            for (int j = 0; j < numCards; j++) {
               row.ColumnDefinitions.Add(new ColumnDefinition());
               Image img = new Image();
               img.Source = new UriImageSource {
                  Uri = new Uri(cards[i * cardsPerRow + j].PngImage, UriKind.Absolute),
                  CachingEnabled = true,
                  CacheValidity = new TimeSpan(0, 30, 0)
               };
               Grid.SetColumn(img, j);
               row.Children.Add(img);
            }
         }
      }
      
      private int CalcMaxCardRow(int rows) {
         if (rows == 0) { return 0; }
         double gridWidth = CardGrid.Width;
         double gridHeight = CardGrid.Height;
         return (int) Math.Floor(gridWidth / (gridHeight / rows) * (4 / 3.0));
      }
   }
}