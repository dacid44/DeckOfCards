using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DeckOfCardsAPI;

namespace DeckOfCardsGUI {
   /// <summary>
   /// Interaction logic for DeckOfCardsWindow.xaml
   /// </summary>
   public partial class DeckOfCardsWindow : Window {
      private static readonly Regex _numerals = new Regex("[^0-9]+");

      private DeckOfCards _deck;

      public DeckOfCardsWindow() {
         InitializeComponent();
         _deck = new DeckOfCards();
         if (_deck.Success) {
            DeckIDField.Content = "Deck ID: " + _deck.DeckID;
         } else {
            MessageField.Content = _deck.ErrorMessage;
         }
      }
      
      private void DrawInputPreview(object sender, TextCompositionEventArgs e) {
         e.Handled = NumCards.Text.Length >= 4 || _numerals.IsMatch(e.Text);
      }
      
      private void DrawInputKeyDown(object sender, KeyEventArgs e) {
         if (e.Key == Key.Return) {
            DrawEvent(sender, e);
         }
      }
      
      private void DrawEvent(object sender, RoutedEventArgs e) {
         List<Card> drawnCards = _deck.Draw(int.Parse(NumCards.Text == "" ? "0" : NumCards.Text));
         if (drawnCards is null) {
            MessageField.Content = _deck.ErrorMessage;
            return;
         }
         DisplayCards(drawnCards);
         MessageField.Content = $"Drew {drawnCards.Count} cards, {_deck.CardsRemaining} cards remaining.";
      }

      private void ShuffleEvent(object sender, RoutedEventArgs e) {
         bool success = _deck.Shuffle();
         if (!success) {
            MessageField.Content = _deck.ErrorMessage;
            return;
         }
         DisplayCards(new List<Card>());
         MessageField.Content = $"Shuffled the deck, {_deck.CardsRemaining} cards remaining.";
      }

      private void ListEvent(object sender, RoutedEventArgs e) {
         if (!_deck.Success) {
            MessageField.Content = "Deck initialization returned an error: " + _deck.ErrorMessage;
            return;
         }
         DisplayCards(_deck.DrawnCards);
         MessageField.Content =
            $"{_deck.DrawnCards.Count} cards have been drawn, and there are {_deck.CardsRemaining} cards remaining.";
      }

      private void CountEvent(object sender, RoutedEventArgs e) {
         if (!_deck.Success) {
            MessageField.Content = "Deck initialization returned an error: " + _deck.ErrorMessage;
            return;
         }
         MessageField.Content = $"There are {_deck.CardsRemaining} cards remaining.";
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
               BitmapImage imgSrc = new BitmapImage();
               imgSrc.BeginInit();
               imgSrc.UriSource = new Uri(cards[i * cardsPerRow + j].PngImage, UriKind.Absolute);
               imgSrc.EndInit();
               Image img = new Image();
               img.Source = imgSrc;
               Grid.SetColumn(img, j);
               row.Children.Add(img);
            }
         }
      }

      private int CalcMaxCardRow(int rows) {
         if (rows == 0) { return 0; }
         double gridWidth = CardGrid.ActualWidth;
         double gridHeight = CardGrid.ActualHeight;
         return (int) Math.Floor(gridWidth / (gridHeight / rows) * (4 / 3.0));
      }
   }
}