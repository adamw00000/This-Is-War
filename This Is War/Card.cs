using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace This_Is_War
{
    class Card
    {
        public string Color { get; set; }
        public int Number { get; set; }
        public ImageSource Image { get; set; }

        public Card(string c, int n, ImageSource i)
        {
            Color = c;
            Number = n;
            Image = i;
        }
    }
}
