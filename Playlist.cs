using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Esercizio_23_Sciarfy
{
    public class Playlist
    {
        public string Nome { get; set; }
        public string PathImg { get; set; }
        public List<Brano> ListaBrani { get; set; }

        public Playlist(string nome, string pathImg, List<Brano> listaBrani)
        {
            Nome = nome;
            PathImg = pathImg;
            ListaBrani = listaBrani;
        }
    }
}
