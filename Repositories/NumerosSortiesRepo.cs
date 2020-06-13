using EuroMillion.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EuroMillion.Repositories
{
    public class NumerosSortiesRepo
    {
        private readonly EuroMillionEntities db = new EuroMillionEntities();

        public void InitValue()
        {
            var listOfNumber = db.Numero.ToList();
            var lastSortie = DateTime.Now;

            listOfNumber.ForEach(nb =>
            {
                nb.PourcentageChanceDeSortie = (Double.Parse(nb.NbrFoisSortie.ToString()) / Double.Parse(nb.NbrDeSemaine.ToString())) * 100;
                var ecartDateSortie = 0d;

                lastSortie = db.DateSortie.OrderBy(o => o.DateSortie1).FirstOrDefault(x => x.NumeroId == nb.Id).DateSortie1;
                var listDatesSorties = db.DateSortie.Where(x => x.NumeroId == nb.Id).OrderBy(o => o.DateSortie1).ToList();
                listDatesSorties.ForEach(dt =>
                {
                    ecartDateSortie += dt.DateSortie1.Subtract(lastSortie.Date).TotalDays;
                    lastSortie = dt.DateSortie1;
                });

                nb.EcartDateSortie = ecartDateSortie / nb.NbrFoisSortie;
            });

            db.SaveChanges();
        }

        public List<NumerosGagnant> NumeroGagnant(int skip)
        {
            var listOfNumber = db.Numero.ToList();
            var listNumeroGagnantTemp = new List<NumerosGagnant>();
            var listNumerosGagnant = new List<NumerosGagnant>();
            var aujdh = DateTime.Now.Date;
            var ecartDeJour = 0d;
            var decheance = 0;
            listOfNumber.ForEach(nb =>
            {
                var derniereSortie = db.DateSortie.OrderByDescending(o => o.DateSortie1).FirstOrDefault(x => x.NumeroId == nb.Id);
                ecartDeJour = aujdh.Date.Subtract(derniereSortie.DateSortie1.Date).TotalDays;
                if (ecartDeJour >= nb.EcartDateSortie)
                {
                    listNumeroGagnantTemp.Add(new NumerosGagnant
                    {
                        Valeur = nb.Valeur.Value,
                        DerniereSortie = derniereSortie.DateSortie1,
                        EcartJourSortie = nb.EcartDateSortie.Value,
                        BonusMalus = ecartDeJour,
                        PourcentageSortie = nb.PourcentageChanceDeSortie.Value,
                        IsEtoile = nb.IsEtoile.Value
                    });
                }
            });

            while (listNumeroGagnantTemp.Count < 7 || listNumeroGagnantTemp.Where(x => x.IsEtoile == true).Count() < 2)
            {
                decheance++;
                listNumeroGagnantTemp.Clear();
                listOfNumber.ForEach(nb =>
                {
                    var derniereSortie = db.DateSortie.OrderByDescending(o => o.DateSortie1).FirstOrDefault(x => x.NumeroId == nb.Id);
                    ecartDeJour = aujdh.Date.Subtract(derniereSortie.DateSortie1.Date).TotalDays;
                    if (ecartDeJour >= (nb.EcartDateSortie - decheance) && !nb.IsEtoile.Value)
                    {
                        listNumeroGagnantTemp.Add(new NumerosGagnant
                        {
                            Valeur = nb.Valeur.Value,
                            DerniereSortie = derniereSortie.DateSortie1,
                            EcartJourSortie = nb.EcartDateSortie.Value,
                            BonusMalus = ecartDeJour,
                            PourcentageSortie = nb.PourcentageChanceDeSortie.Value,
                            IsEtoile = nb.IsEtoile.Value
                        });
                    }
                });
            }

            listNumerosGagnant.AddRange(listNumeroGagnantTemp.OrderByDescending(o => o.BonusMalus).ThenByDescending(o => o.PourcentageSortie).Where(nb => nb.IsEtoile == false).Skip(skip * 5).Take(5).ToList());
            listNumerosGagnant.AddRange(listNumeroGagnantTemp.OrderByDescending(o => o.BonusMalus).ThenByDescending(o => o.PourcentageSortie).Where(nb => nb.IsEtoile == true).Skip(skip * 2).Take(2).ToList());

            return listNumerosGagnant;
        }

        public async Task<bool> InsertNewNumber(List<int> numerosSorties, List<int> etoilesSorties)
        {
            var listOfNumber = db.Numero.ToList();

            listOfNumber.ForEach(nb =>
            {
                nb.NbrDeSemaine++;
                nb.PourcentageChanceDeSortie = (Double.Parse(nb.NbrFoisSortie.ToString()) / Double.Parse(nb.NbrDeSemaine.ToString())) * 100;
            });

            //Pour chaque numéro sortie je crée une DateSortie pour enregistrer le jour de sortie du numéro afin de l'utiliser plus tard dans le calcul d'écart de date
            numerosSorties.ForEach(nb =>
            {

                var numero = listOfNumber.FirstOrDefault(n => n.Valeur == nb && n.IsEtoile == false);
                var lastSortie = DateTime.MinValue;
                var ecartDateSortie = numero.EcartDateSortie;
                if (db.DateSortie.Any(x => x.NumeroId == numero.Id))
                {
                    lastSortie = db.DateSortie.FirstOrDefault(x => x.NumeroId == numero.Id).DateSortie1;
                }

                var dtSortie = new DateSortie
                {
                    NumeroId = numero.Id,
                    DateSortie1 = DateTime.Now
                };

                db.DateSortie.Add(dtSortie);

                numero.NbrFoisSortie++;

                numero.PourcentageChanceDeSortie = (Double.Parse(numero.NbrFoisSortie.ToString()) / Double.Parse(numero.NbrDeSemaine.ToString())) * 100;

                if (lastSortie != DateTime.MinValue)
                {
                    ecartDateSortie += dtSortie.DateSortie1.Subtract(lastSortie.Date).TotalDays;
                    numero.EcartDateSortie = ecartDateSortie / numero.NbrFoisSortie;
                }
                else
                {
                    numero.EcartDateSortie = 0;
                }
            });

            //Pour chaque étoile sortie je crée une DateSortie pour enregistrer le jour de sortie de l'étoile afin de l'utiliser plus tard dans le calcul d'écart de date
            etoilesSorties.ForEach(st =>
            {
                var etoile = db.Numero.FirstOrDefault(e => e.Valeur == st && e.IsEtoile == true);
                var lastSortie = DateTime.MinValue;
                var ecartDateSortie = etoile.EcartDateSortie;
                if (db.DateSortie.Any(x => x.NumeroId == etoile.Id))
                {
                    lastSortie = db.DateSortie.FirstOrDefault(x => x.NumeroId == etoile.Id).DateSortie1;
                }

                var dtSortie = new DateSortie
                {
                    NumeroId = etoile.Id,
                    DateSortie1 = DateTime.Now
                };

                db.DateSortie.Add(dtSortie);



                etoile.NbrFoisSortie++;

                etoile.PourcentageChanceDeSortie = (Double.Parse(etoile.NbrFoisSortie.ToString()) / Double.Parse(etoile.NbrDeSemaine.ToString())) * 100;


                if (lastSortie != DateTime.MinValue)
                {
                    ecartDateSortie += dtSortie.DateSortie1.Subtract(lastSortie.Date);
                    etoile.EcartDateSortie = ecartDateSortie / etoile.NbrFoisSortie;
                }
                else
                {
                    etoile.EcartDateSortie = 0;
                }
            });

            await db.SaveChangesAsync();

            return true;
        }
    }
}