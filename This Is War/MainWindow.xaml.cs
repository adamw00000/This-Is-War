﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
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

        bool simulating;
        bool animated = true;

        Player p1;
        Player p2;

        Player Winner { get; set; }
        Player Loser { get; set; }

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

        void Notify(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public MainWindow()
        {
            InitializeComponent();
            InitializeCards();
            InitializePlayers();

            InitializeBindings();
        }

        #region Initialization
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

            p1 = new Player(p1Cards, 0, "Left", p1Image, p1Back, p1MovingBack);
            p2 = new Player(p2Cards, 0, "Right", p2Image, p2Back, p2MovingBack);

            p1Deck.DataContext = p1;
            p2Deck.DataContext = p2;

            p1Score.DataContext = p1Label.DataContext = p1Image.DataContext = p1PreviousImage.DataContext = p1;
            p2Score.DataContext = p2Label.DataContext = p2Image.DataContext = p2PreviousImage.DataContext = p2;
        }

        private void InitializeBindings()
        {
            highscoreTab.DataContext = Highscores;
            sliders.DataContext = p1Back.DataContext = p2Back.DataContext =
                p1MovingBack.DataContext = p2MovingBack.DataContext = this;
            CurrentBack = new BitmapImage(new Uri("back1.png", UriKind.Relative));
            MoveSpeed = 1;
            RotateSpeed = 1;
            p1MovingBack.Visibility = p2MovingBack.Visibility = Visibility.Hidden;
        }
        #endregion

        private void NextTurn(object sender, MouseButtonEventArgs e)
        {
            DrawCards();
        }

        private void DrawCards()
        {
            HideMovingBacks();

            if (p1.Deck.Count == 0 || p2.Deck.Count == 0)
            {
                simulateButton.IsEnabled = skipButton.IsEnabled = false;
                return;
            }

            resetButton.IsEnabled = skipButton.IsEnabled = false;
            p1Back.IsEnabled = p2Back.IsEnabled = false;

            Fight();
        }
 
        private void CheckForWinner()
        {
            if ((p1.Deck.Count == 0 || p2.Deck.Count == 0) && 
                (p1.Stack.Count == 0 && p2.Stack.Count == 0))
            {
                Player winner = p1.Score > p2.Score ? p1 : p2;
                int pos = Highscores.IndexOf(Highscores.FirstOrDefault(h => h.Result < winner.Score));
                Highscores.Insert(pos != -1 ? pos : Highscores.Count,
                    new Highscore(winner.Name, winner.Score));
                p1Back.IsEnabled = p2Back.IsEnabled = true;
            }
        }

        private void HideMovingBacks()
        {
            p1.MovingBack.Visibility = Visibility.Hidden;
            p2.MovingBack.Visibility = Visibility.Hidden;
        }

        #region Fight
        private void Fight()
        {
            DrawNextCard();

            if (animated)
            {
                Move(p1, p1.Back, p1.Image, null);
                Move(p2, p2.Back, p2.Image, StartStandardBattle);
            }
            else
            {
                StartStandardBattle(this, EventArgs.Empty);
            }
        }

        private void DrawNextCard()
        {
            p1.DrawCard();
            p2.DrawCard();
        }

        #region Standard Move Animation
        private void Move(Player target, Image startingImage, Image endImage, Action<object, EventArgs> OnMoveEnd)
        {
            CreateMove(target.MovingBack, startingImage, endImage, out DoubleAnimation moveAnim, out TranslateTransform tt);

            if (OnMoveEnd != null)
                moveAnim.Completed += (o, e) => OnMoveEnd(o, e);

            target.MovingBack.Visibility = Visibility.Visible;

            tt.BeginAnimation(TranslateTransform.XProperty, moveAnim);
        }

        private void CreateMove(Image target, Image startingImage, Image endImage, out DoubleAnimation moveAnim, out TranslateTransform tt)
        {
            double currentX = target.TransformToAncestor(Application.Current.MainWindow).Transform(new Point(0, 0)).X
                - target.RenderTransform.Value.OffsetX;
            double startX = startingImage.TransformToAncestor(Application.Current.MainWindow).Transform(new Point(0, 0)).X;
            double endX = endImage.TransformToAncestor(Application.Current.MainWindow).Transform(new Point(0, 0)).X;
            if (endImage.Source == null)
                endX -= p1Image.Width / 2;

            moveAnim = new DoubleAnimation()
            {
                From = startX - currentX,
                To = endX - currentX,
                Duration = new Duration(TimeSpan.Parse($"0:0:{MoveSpeed:N2}")),
                EasingFunction = new QuarticEase()
            };
            target.RenderTransformOrigin = new Point(0, 0);
            tt = new TranslateTransform();
            target.RenderTransform = tt;
        }
        #endregion

        private void StartStandardBattle(object sender, EventArgs e)
        {
            HideMovingBacks();
            p1.CurrentImage = CurrentBack;
            p2.CurrentImage = CurrentBack;

            if (animated)
            {
                RotateImage(p1, p1.CurrentCard.Image, null);
                RotateImage(p2, p2.CurrentCard.Image, EndStandardBattle);
            }
            else
            {
                EndStandardBattle(this, EventArgs.Empty);
            }
        }

        #region Standard Rotate Animation
        private void RotateImage(Player target, ImageSource newImage, Action<object, EventArgs> OnRotationEnd)
        {
            CreateRotationFirstPart(target, out DoubleAnimation rotateAnim, out ScaleTransform st);
            
            rotateAnim.Completed += (o, e) => CreateRotationSecondPart(target, newImage, OnRotationEnd);

            st.BeginAnimation(ScaleTransform.ScaleXProperty, rotateAnim);
        }

        private void CreateRotationFirstPart(Player target, out DoubleAnimation rotateAnim, out ScaleTransform st)
        {
            p1.MovingBack.Visibility = Visibility.Hidden;
            p2.MovingBack.Visibility = Visibility.Hidden;

            rotateAnim = new DoubleAnimation()
            {
                From = 1,
                To = 0,
                Duration = new Duration(TimeSpan.Parse($"0:0:{RotateSpeed/2:N2}")),
                EasingFunction = new QuarticEase() { EasingMode=EasingMode.EaseIn  }
            };
            st = new ScaleTransform();
            target.Image.RenderTransformOrigin = new Point(0.5, 0.5);
            target.Image.RenderTransform = st;
        }

        private void CreateRotationSecondPart(Player target, ImageSource newImage, Action<object, EventArgs> OnRotationEnd)
        {
            target.CurrentImage = newImage;

            DoubleAnimation rotateAnim = new DoubleAnimation()
            {
                From = 0,
                To = 1,
                Duration = new Duration(TimeSpan.Parse($"0:0:{RotateSpeed / 2:N2}")),
                EasingFunction = new QuarticEase() { EasingMode = EasingMode.EaseOut }
            };
            ScaleTransform st = new ScaleTransform();
            target.Image.RenderTransformOrigin = new Point(0.5, 0.5);
            target.Image.RenderTransform = st;

            if (OnRotationEnd != null)
                rotateAnim.Completed += (o, e) => OnRotationEnd(o, e);

            st.BeginAnimation(ScaleTransform.ScaleXProperty, rotateAnim);
        }
        #endregion

        private void EndStandardBattle(object sender, EventArgs e)
        {
            if (!CheckResult(p1.CurrentCard, p2.CurrentCard))
            {
                if (WarBox.IsChecked.Value)
                    MessageBox.Show("War!");

                WarFirstPhase();
            }
        }

        private bool CheckResult(Card p1Card, Card p2Card)
        {
            if (p1Card.Number != p2Card.Number)
            {
                if (p1Card.Number > p2Card.Number)
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
        #endregion

        #region War
        private void WarFirstPhase()
        {
            if (EndOnEmptyDeck())
                return;

            DrawNextCard();

            if (animated)
            {
                Move(p1, p1.Back, p1.Image, null);
                Move(p2, p2.Back, p2.Image, WarSecondPhase);
            }
            else
            {
                WarSecondPhase(this, EventArgs.Empty);
            }
        }

        private void WarSecondPhase(object arg1, EventArgs arg2)
        {
            HideMovingBacks();
            p1.CurrentImage = CurrentBack;
            p2.CurrentImage = CurrentBack;

            if (EndOnEmptyDeck())
                return;

            DrawNextCard();

            if (animated)
            {
                Move(p1, p1.Back, p1.Image, null);
                Move(p2, p2.Back, p2.Image, WarThirdPhase);
            }
            else
            {
                WarThirdPhase(this, EventArgs.Empty);
            }
        }

        private void WarThirdPhase(object arg1, EventArgs arg2)
        {
            HideMovingBacks();
            p1.PreviousImage = CurrentBack;
            p2.PreviousImage = CurrentBack;
            //p1.CurrentImage = CurrentBack;
            //p2.CurrentImage = CurrentBack;

            if (animated)
            {
                RotateImage(p1, p1.CurrentCard.Image, null);
                RotateImage(p2, p2.CurrentCard.Image, WarFourthPhase);
            }
            else
            {
                WarFourthPhase(this, EventArgs.Empty);
            }
        }

        private void WarFourthPhase(object arg1, EventArgs arg2)
        {
            p1.PreviousImage = null;
            p2.PreviousImage = null;

            if (CheckResult(p1.CurrentCard, p2.CurrentCard))
                return;
            else
                WarFirstPhase();
        }

        private bool EndOnEmptyDeck()
        {
            if (p1.Deck.Count == 0 || p2.Deck.Count == 0)
            {
                if (p2.Deck.Count == 0)
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

        private void EndWar(Player winner, Player loser)
        {
            Winner = winner;
            Loser = loser;

            CollectCards();
        }

        void CollectCards() => RotateLoserStack(this, EventArgs.Empty);
        #endregion

        #region End War Animations And Logic
        private void RotateLoserStack(object sender, EventArgs e)
        {
            if (!animated)
            {
                MoveLoserStack(this, EventArgs.Empty);
                return;
            }

            if (Loser.Stack.Count > 1)
                Loser.PreviousImage = CurrentBack;

            RotateImage(Loser, CurrentBack, MoveLoserStack);
        }

        private void MoveLoserStack(object sender, EventArgs e)
        {
            Loser.PreviousImage = null;

            Winner.Deck.Add(Loser.Stack.Pop());
            Winner.Score++;

            if (Loser.Stack.Count == 0)
                Loser.CurrentImage = null;

            if (!animated)
            {
                LoserStackNextAction();
                return;
            }

            if (Loser.Stack.Count % 2 == 1)
                Loser.CurrentImage = Loser.CurrentCard.Image;

            Loser.MovingBack.Visibility = Visibility.Visible;

            Move(Loser, Loser.Image, Winner.Back, (o, ee) =>
            {
                HideMovingBacks();
                LoserStackNextAction();
            });
        }

        private void LoserStackNextAction()
        {
            if (Loser.Stack.Count != 0)
            {
                if (Loser.Stack.Count % 2 == 0)
                    MoveLoserStack(this, EventArgs.Empty);
                else
                    RotateLoserStack(this, EventArgs.Empty);
            }
            else
            {
                RotateWinnerStack(this, EventArgs.Empty);
            }
        }

        private void RotateWinnerStack(object sender, EventArgs e)
        {
            if (!animated)
            {
                MoveWinnerStack(this, EventArgs.Empty);
                return;
            }
            
            if (Winner.Stack.Count > 1)
                Winner.PreviousImage = CurrentBack;

            RotateImage(Winner, CurrentBack, MoveWinnerStack);
        }

        private void MoveWinnerStack(object sender, EventArgs e)
        {
            Winner.PreviousImage = null;

            Winner.Deck.Add(Winner.Stack.Pop());
            Winner.Score++;

            if (Winner.Stack.Count == 0)
            {
                Winner.CurrentImage = null;
            }

            if (!animated)
            {
                WinnerStackNextAction();
                return;
            }

            if (Winner.Stack.Count % 2 == 1)
                Winner.CurrentImage = Winner.CurrentCard.Image;

            Winner.MovingBack.Visibility = Visibility.Visible;

            Move(Winner, Winner.Image, Winner.Back, (o, ee) =>
            {
                WinnerStackNextAction();
            });
        }

        private void WinnerStackNextAction()
        {
            if (Winner.Stack.Count != 0)
            {
                if (Winner.Stack.Count % 2 == 0)
                    MoveWinnerStack(this, EventArgs.Empty);
                else
                    RotateWinnerStack(this, EventArgs.Empty);
            }
            else
            {
                if (animated)
                {
                    if (simulating)
                    {
                        SimulationEndWarCleanup();
                    }
                    else
                    {
                        RegularEndWarCleanup();
                    }
                }
                else
                {
                    SkipEndWarCleanup();
                }
                CheckForWinner();
            }
        }

        private void SkipEndWarCleanup()
        {
            p1Back.IsEnabled = p2Back.IsEnabled = true;
            resetButton.IsEnabled = skipButton.IsEnabled = true;
            if (p1.Deck.Count == 0 || p2.Deck.Count == 0)
                skipButton.IsEnabled = simulateButton.IsEnabled = false;
        }

        private void RegularEndWarCleanup()
        {
            HideMovingBacks();

            p1Back.IsEnabled = p2Back.IsEnabled = true;
            resetButton.IsEnabled = skipButton.IsEnabled = true;

            if (p1.Deck.Count == 0 || p2.Deck.Count == 0)
                skipButton.IsEnabled = simulateButton.IsEnabled = false;
        }

        private void SimulationEndWarCleanup()
        {
            if (p1.Deck.Count == 0 || p2.Deck.Count == 0)
            {
                simulateButton.Content = "Simulate";
                simulating = false;
                resetButton.IsEnabled = true;
            }
            else
                DrawCards();
        }
        #endregion

        #region Button Logic
        private void Reset(object sender, RoutedEventArgs e)
        {
            CreateDecks(out ObservableCollection<Card> p1Cards, out ObservableCollection<Card> p2Cards);

            p1.SetDeck(p1Cards);
            p2.SetDeck(p2Cards);

            p1.Score = 0;
            p2.Score = 0;

            p1.CurrentImage = null;
            p2.CurrentImage = null;

            simulateButton.IsEnabled = skipButton.IsEnabled = true;
            p1Back.IsEnabled = p2Back.IsEnabled = true;
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
            animated = false;
            while (p1.Deck.Count > 0 && p2.Deck.Count > 0)
            {
                DrawCards();
            }
            animated = true;
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
            simulating = !simulating;
            if (simulating)
                simulateButton.Content = "Stop";
            else
                simulateButton.Content = "Simulate";

            if (p1Back.IsEnabled)
                DrawCards();
        }
        #endregion
    }

    #region Converters
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
    #endregion
}
