using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace This_Is_War
{
    class Player: INotifyPropertyChanged
    {
        int score;

        public string Name { get; set; }
        public ObservableCollection<Card> Cards { get; set; }
        public Stack<Card> Stack { get; set; } = new Stack<Card>();

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

        public Player(ObservableCollection<Card> cards, int score, string name)
        {
            Cards = cards;
            Score = score;
            Name = name;
        }

        public Card DrawCard()
        {
            Card card = Cards[0];
            Cards.Remove(card);
            return card;
        }

        public void SetDeck(ObservableCollection<Card> cards)
        {
            Cards.Clear();
            foreach (var card in cards)
            {
                Cards.Add(card);
            }
        }
    }
}
