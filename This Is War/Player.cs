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
        List<Card> cards;
        public List<Card> Cards
        {
            get
            {
                return cards;
            }
            set
            {
                cards = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Number"));
            }
        }
        public int Number { get
            {
                return Cards.Count;
            }
        }
        int score;
        public int Score { get
            {
                return score;
            }
            set
            {
                score = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Score"));
            }
        }
        Random rand = new Random();

        public event PropertyChangedEventHandler PropertyChanged;

        public Player(List<Card> cards, int score)
        {
            Cards = cards;
            Score = score;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Number"));
        }

        public Card DrawRandomCard()
        {
            Card card = Cards[rand.Next(0, Cards.Count)];
            Cards.Remove(card);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Number"));
            return card;
        }
    }
}
