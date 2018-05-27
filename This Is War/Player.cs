using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace This_Is_War
{
    class Player: INotifyPropertyChanged
    {
        int score;

        public string Name { get; set; }
        public Image Image { get; set; }
        public Image Back { get; set; }
        public Image MovingBack { get; set; }
        public ObservableCollection<Card> Deck { get; set; }
        public Stack<Card> Stack { get; set; } = new Stack<Card>();

        public Card CurrentCard { get; set; }

        ImageSource currentImage;
        public ImageSource CurrentImage
        {
            get
            {
                return currentImage;
            }
            set
            {
                currentImage = value;
                Notify(nameof(CurrentImage));
            }
        }
        ImageSource previousImage;
        public ImageSource PreviousImage
        {
            get
            {
                return previousImage;
            }
            set
            {
                previousImage = value;
                Notify(nameof(PreviousImage));
            }
        }

        public int Score { get
            {
                return score;
            }
            set
            {
                score = value;
                Notify(nameof(Score));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        void Notify(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public Player(ObservableCollection<Card> cards, int score, string name, Image image, Image back, Image movingBack)
        {
            Deck = cards;
            Score = score;
            Name = name;
            Image = image;
            Back = back;
            MovingBack = movingBack;
        }

        public Card DrawCard()
        {
            Card card = Deck[0];
            Deck.Remove(card);
            return card;
        }

        public void SetDeck(ObservableCollection<Card> cards)
        {
            Deck.Clear();
            foreach (var card in cards)
            {
                Deck.Add(card);
            }
        }
    }
}
