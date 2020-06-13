using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EuroMillion.Models
{
    public class InsertLastNumberModel
    {
        [Range(1,50)]
        public int Numero1 { get; set; }
        [Range(1, 50)]
        public int Numero2 { get; set; }
        [Range(1, 50)]
        public int Numero3 { get; set; }
        [Range(1, 50)]
        public int Numero4 { get; set; }
        [Range(1, 50)]
        public int Numero5 { get; set; }
        [Range(1, 12)]
        public int Etoile1 { get; set; }
        [Range(1, 12)]
        public int Etoile2 { get; set; }
    }
}