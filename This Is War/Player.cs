using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace This_Is_War
{
    class Player: INotifyPropertyChanged
    {
        readonly Random rand = new Random();
        List<Card> cards;
        int score;

        public List<Card> Cards
        {
            get
            {
                return cards;
            }
            set
            {
                cards = value;
                Notify(nameof(Count));
            }
        }

        public int Count { get
            {
                return Cards.Count;
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

        public Player(List<Card> cards, int score)
        {
            Cards = cards;
            Score = score;
            Notify(nameof(Count));
        }

        public Card DrawRandomCard()
        {
            Card card = Cards[rand.Next(0, Cards.Count)];
            Cards.Remove(card);
            Notify(nameof(Count));
            return card;
        }
    }
}
