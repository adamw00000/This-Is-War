using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
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

namespace This_Is_War
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly List<Card> cards = new List<Card>();
        Player p1;
        Player p2;

        public MainWindow()
        {
            InitializeComponent();
            InitializeCards();
            InitializePlayers();
        }

        private void InitializeCards()
        {
            BitmapImage cardsImage = new BitmapImage(new Uri("cards.jpg", UriKind.Relative));

            for (int i = 0; i < 13; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    cards.Add(new Card("", i, new CroppedBitmap(cardsImage,
                        new Int32Rect(i * 225, j * 315, 225, 315))));
                }
            }
        }

        private void InitializePlayers()
        {
            CreateDecks(out List<Card> p1Cards, out List<Card> p2Cards);

            p1 = new Player(p1Cards, 0);
            p2 = new Player(p2Cards, 0);

            p1Panel.DataContext = p1;
            p2Panel.DataContext = p2;
        }

        private void War(object sender, MouseButtonEventArgs e)
        {
            DrawCards();
        }

        private void DrawCards()
        {
            if (p1.Count == 0 || p2.Count == 0)
            {
                return;
            }
            Card p1Card = p1.DrawRandomCard();
            Card p2Card = p2.DrawRandomCard();
            p1Image.Source = p1Card.Image;
            p2Image.Source = p2Card.Image;

            UpdateScore(p1Card, p2Card);
        }

        private void UpdateScore(Card p1Card, Card p2Card)
        {
            if (p1Card.Number > p2Card.Number)
            {
                p1.Score++;
            }
            else if (p1Card.Number < p2Card.Number)
            {
                p2.Score++;
            }
            else
            {
                p1.Score++;
                p2.Score++;
                if (WarBox.IsChecked.Value)
                    MessageBox.Show("War!");
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            CreateDecks(out List<Card> p1Cards, out List<Card> p2Cards);
            p1.Cards = p1Cards;
            p2.Cards = p2Cards;

            p1Image.Source = null;
            p2Image.Source = null;
        }

        private void CreateDecks(out List<Card> p1Cards, out List<Card> p2Cards)
        {
            List<Card> cardsCopy = new List<Card>(cards);
            Random rand = new Random();

            p1Cards = new List<Card>();
            p2Cards = new List<Card>();
            for (int i = 0; i < 26; i++)
            {
                Card card1 = cardsCopy[rand.Next(0, cardsCopy.Count)];
                cardsCopy.Remove(card1);
                Card card2 = cardsCopy[rand.Next(0, cardsCopy.Count)];
                cardsCopy.Remove(card2);

                p1Cards.Add(card1);
                p2Cards.Add(card2);
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            while (p1.Count > 0 && p2.Count > 0)
            {
                DrawCards();
            }
        }
    }

    class CountToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int count = (int)value;
            if (count == 0)
                return Visibility.Hidden;
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
