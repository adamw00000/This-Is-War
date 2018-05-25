using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace This_Is_War
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        readonly ObservableCollection<Card> cards = new ObservableCollection<Card>();
        ObservableCollection<Highscore> Highscores { get; } = new ObservableCollection<Highscore>();
        Player p1;
        Player p2;

        public event PropertyChangedEventHandler PropertyChanged;

        BitmapImage currentBack;
        public BitmapImage CurrentBack
        {
            get
            {
                return currentBack;
            }
            set
            {
                currentBack = value;
                Notify(nameof(CurrentBack));
            }
        }
        double moveSpeed;
        public double MoveSpeed
        {
            get
            {
                return moveSpeed;
            }
            set
            {
                moveSpeed = value;
                Notify(nameof(MoveSpeed));
            }
        }
        double rotateSpeed;
        public double RotateSpeed {
            get
            {
                return rotateSpeed;
            }
            set
            {
                rotateSpeed = value;
                Notify(nameof(RotateSpeed));
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            InitializeCards();
            InitializePlayers();

            highscoreTab.DataContext = Highscores;
            sliders.DataContext = p1Back.DataContext = p2Back.DataContext = p1MovingBack.DataContext = this;
            CurrentBack = new BitmapImage(new Uri("back1.png", UriKind.Relative));
            MoveSpeed = 0.1;
            RotateSpeed = 0.1;
        }

        void Notify(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
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
            CreateDecks(out ObservableCollection<Card> p1Cards, out ObservableCollection<Card> p2Cards);

            p1 = new Player(p1Cards, 0, "Left");
            p2 = new Player(p2Cards, 0, "Right");

            p1Deck.DataContext = p1Panel.DataContext = p1;
            p2Deck.DataContext = p2Panel.DataContext = p2;
        }

        private void NextTurn(object sender, MouseButtonEventArgs e)
        {
            DrawCards();
        }

        private void DrawCards()
        {
            Fight();

            if (p1.Cards.Count == 0 || p2.Cards.Count == 0)
            {
                Player winner = p1.Score > p2.Score ? p1 : p2;
                int pos = Highscores.IndexOf(Highscores.FirstOrDefault(h => h.Result < winner.Score));
                Highscores.Insert(pos != -1 ? pos : Highscores.Count,
                    new Highscore(winner.Name, winner.Score));
            }
        }

        private void Fight()
        {
            DrawNextCard(out Card p1Card, out Card p2Card);

            //TODO: Move animation
            //TODO: Rotate animation

            p1Image.Source = p1Card.Image;
            p2Image.Source = p2Card.Image;

            if (!CheckResult(p1Card, p2Card))
            {
                if (WarBox.IsChecked.Value)
                    MessageBox.Show("War!");

                War();
            }
        }

        private void DrawNextCard(out Card p1Card, out Card p2Card)
        {
            p1Card = p1.DrawCard();
            p2Card = p2.DrawCard();

            p1.Stack.Push(p1Card);
            p2.Stack.Push(p2Card);
        }

        private bool CheckResult(Card p1Card, Card p2Card)
        {
            if (p1Card.Number != p2Card.Number)
            {
                if (p1Card.Number > p2Card.Number)
                {
                    EndWar(p1, p2);
                }
                else if (p1Card.Number < p2Card.Number)
                {
                    EndWar(p2, p1);
                }
                return true;
            }
            return false;
        }

        private void War()
        {
            while (true)
            {
                if (EndOnEmptyDeck())
                    break;

                DrawNextCard(out Card p1Card, out Card p2Card);

                //TODO: Move animation

                if (EndOnEmptyDeck())
                    break;

                //TODO: Move animation
                //TODO: Rotate animation

                DrawNextCard(out p1Card, out p2Card);

                if (CheckResult(p1Card, p2Card))
                    break;
            }
        }

        private bool EndOnEmptyDeck()
        {
            if (p1.Cards.Count == 0 || p2.Cards.Count == 0)
            {
                if (p2.Cards.Count == 0)
                {
                    EndWar(p1, p2);
                }
                else
                {
                    EndWar(p2, p1);
                }
                return true;
            }
            return false;
        }

        private static void EndWar(Player winner, Player loser)
        {
            winner.Score += winner.Stack.Count + loser.Stack.Count;

            while (loser.Stack.Count != 0)
            {
                if (loser.Stack.Count % 2 == 1)
                {
                    //TODO: Rotate animation
                }
                //TODO: Move animation

                winner.Cards.Add(loser.Stack.Pop());
            }
            while (winner.Stack.Count != 0)
            {
                if (loser.Stack.Count % 2 == 1)
                {
                    //TODO: Rotate animation
                }
                //TODO: Move animation

                winner.Cards.Add(winner.Stack.Pop());
            }
        }

        private void Reset(object sender, RoutedEventArgs e)
        {
            CreateDecks(out ObservableCollection<Card> p1Cards, out ObservableCollection<Card> p2Cards);

            p1.SetDeck(p1Cards);
            p2.SetDeck(p2Cards);

            p1.Score = 0;
            p2.Score = 0;

            p1Image.Source = null;
            p2Image.Source = null;
        }

        private void CreateDecks(out ObservableCollection<Card> p1Cards, out ObservableCollection<Card> p2Cards)
        {
            ObservableCollection<Card> cardsCopy = new ObservableCollection<Card>(cards);
            Random rand = new Random();

            p1Cards = new ObservableCollection<Card>();
            p2Cards = new ObservableCollection<Card>();
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

        private void SkipToEnd(object sender, RoutedEventArgs e)
        {
            while (p1.Cards.Count > 0 && p2.Cards.Count > 0)
            {
                DrawCards();
            }
        }

        private void ChangeBack(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog
            {
                Filter = "Image files | *.jpg; *.png; *.bmp; *.tiff; *.gif; *.jpeg"
            };
            List<string> possibleExtensions = new List<string>() { ".jpg", ".png", ".bmp", ".tiff", ".gif", ".jpeg" };

            if (openFileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

            if (!possibleExtensions.Contains(System.IO.Path.GetExtension(openFileDialog.FileName).ToLower()))
            {
                MessageBox.Show("Invalid file extension");
                return;
            }

            CurrentBack = new BitmapImage(new Uri(openFileDialog.FileName));
        }

        private void Simulate(object sender, RoutedEventArgs e)
        {

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
