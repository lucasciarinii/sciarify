using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Esercizio_23_Sciarfy
{
    public class Brano
    {
        public string Titolo { get; set; }
        public string Autore { get; set; }
        public string Durata { get; set; }
        public string PathSong { get; set; }
        public string PathIMG { get; set; }
        public bool Favourite { get; set; }

        public Brano(string titolo, string autore, string durata, string pathSong, string pathIMG)
        {
            Titolo = titolo;
            Autore = autore;
            Durata = durata;
            PathSong = pathSong;
            PathIMG = pathIMG;
            Favourite = false;
        }
    }
}
