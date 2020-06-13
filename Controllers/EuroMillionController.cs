using EuroMillion.Models;
using EuroMillion.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EuroMillion.Controllers
{
    public class EuroMillionController : Controller
    {
        private readonly NumerosSortiesRepo _repo = new NumerosSortiesRepo();
        private readonly EuroMillionEntities db = new EuroMillionEntities();

        public ActionResult Index()
        {            
            var indexModel = new IndexModel
            {
                Etoiles = db.Numero.Where(x => x.IsEtoile == true).ToList(),
                Numeros = db.Numero.Where(x => x.IsEtoile == false).ToList()
            };
            return View(indexModel);
        }

        public ActionResult Init()
        {
            _repo.InitValue();
            return RedirectToAction("Index");
        }

        public ActionResult NumeroGagnant(int skip = 0)
        {
            var numeroGagnant = _repo.NumeroGagnant(skip);
            return View(numeroGagnant);
        }
        

        [HttpGet]
        public ActionResult InsertLastNumber()
        {
            var lastNumber = new InsertLastNumberModel();
            return View(lastNumber);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> InsertLastNumber(InsertLastNumberModel listNumerosEtoiles)
        {
            if (listNumerosEtoiles == null)
                return RedirectToAction("Error");

            if (!ModelState.IsValid)
                return View(listNumerosEtoiles);

            var listOfNumbers = new List<int>();
            var listOfStars = new List<int>();

            listOfNumbers.Add(listNumerosEtoiles.Numero1);
            listOfNumbers.Add(listNumerosEtoiles.Numero2);
            listOfNumbers.Add(listNumerosEtoiles.Numero3);
            listOfNumbers.Add(listNumerosEtoiles.Numero4);
            listOfNumbers.Add(listNumerosEtoiles.Numero5);

            listOfStars.Add(listNumerosEtoiles.Etoile1);
            listOfStars.Add(listNumerosEtoiles.Etoile2);

            var isInserted = await _repo.InsertNewNumber(listOfNumbers, listOfStars);

            return RedirectToAction(isInserted ? "Index" : "Error");
        }

        public ActionResult Error()
        {
            return View();
        }
    }
}
