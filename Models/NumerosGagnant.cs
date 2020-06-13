using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EuroMillion.Models
{
    public class NumerosGagnant
    {
        public int Valeur { get; set; }
        public DateTime DerniereSortie { get; set; }
        public double EcartJourSortie { get; set; }
        public double PourcentageSortie { get; set; }
        public double BonusMalus { get; set; }
        public bool IsEtoile { get; set; }
    }
}