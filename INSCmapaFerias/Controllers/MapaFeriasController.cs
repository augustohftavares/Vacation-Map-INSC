using INSCmapaFerias.Areas.Identity.Data;
using INSCmapaFerias.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace INSCmapaFerias.Controllers
{
    [Authorize(Roles = "Utilizadores, Administradores")]
    public class MapaFeriasController : Controller
    {

        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MapaFeriasController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Events.OrderBy(i => i.Title).ToListAsync());
        }

        public IActionResult Pedir_Ferias()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Pedir_Ferias(Events e, ApplicationUser u)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            var events = await _context.Events.ToListAsync();

            try
            {
                e.event_id = Guid.NewGuid().ToString();
                e.user_id = user.Id;
                var total = GetDifDias(Convert.ToDateTime(e.event_start), Convert.ToDateTime(e.event_end));
                var convertString = total.ToString();
                e.Total = convertString;
                e.Title = user.FirstName + " " + user.LastName;
                e.Status = "Enviado";
                e.Email = user.UserName;
                e.backgroundColor = user.Color;

                if(e.event_start > e.event_end || e.event_end < e.event_start)
                {
                    TempData["FeriasInvalidDates"] = "A data de inicio não pode ser maior que a data de fim e vice-versa";
                    return RedirectToAction(nameof(Pedir_Ferias));
                }

                if (User.Identity.IsAuthenticated && Convert.ToInt32(e.Total) > 22)
                {
                    TempData["FeriasErrorLimit22"] = "Erro, só tens direito a 22 dias de férias por ano!";
                    return RedirectToAction(nameof(Pedir_Ferias));
                }

                if(e.Email == user.UserName)
                    e.Team = user.prof_category;

                DateTime startDate = (DateTime)e.event_start;
                DateTime endDate = (DateTime)e.event_end;
                string startDay = startDate.DayOfWeek.ToString();
                string endDay = endDate.DayOfWeek.ToString();

                await _context.Events.AddAsync(e);
                await _context.SaveChangesAsync();
                TempData["FeriasSubmited"] = "O teu pedido foi enviado!";
                return RedirectToAction(nameof(Index));
            } catch {
                return RedirectToAction(nameof(Pedir_Ferias));
            }

        }//end criar_ferias

        [HttpGet]
        public async Task<IActionResult> Editar_Ferias(Events e ,string id)
        {
            var events = await _context.Events.FindAsync(id);

            if(events.Email == User.Identity.Name)
            {
                return View(events);
            }
            else
            {
                return RedirectToAction(nameof(Index));
            }
        }//end editar_ferias

        [HttpPost]
        public async Task<IActionResult> Editar_Ferias(Events model)
        {
            //captura os eventos
            var eventos = await _context.Events.FindAsync(model.event_id);

            if (eventos == null)
            {
                TempData["EventNullError"] = "Não foi possível encontrar estas férias";
                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                eventos.event_id = model.event_id;
                eventos.Title = model.Title;
                eventos.event_start = model.event_start;
                eventos.event_end = model.event_end;

                if (eventos.event_start < eventos.Options || eventos.event_end > eventos.Options_End)
                {
                    TempData["FeriasEditError1"] = "Verifica se as datas estão entre as datas sugeridas pelo administrador!";
                    return RedirectToAction(nameof(Editar_Ferias));
                }

                if (eventos.event_start > eventos.event_end || eventos.event_end < eventos.event_start)
                {
                    TempData["FeriasEditError2"] = "O teu pedido foi enviado!";
                    return RedirectToAction(nameof(Editar_Ferias));
                }

                var total = GetDifDias(Convert.ToDateTime(eventos.event_start), Convert.ToDateTime(eventos.event_end));
                var convertString = total.ToString();
                eventos.Total = convertString;
                eventos.Status = "Enviado";

                if (Convert.ToInt32(eventos.Total) > 22)
                {
                    // MSG ERRO > 22
                    return RedirectToAction(nameof(Editar_Ferias));
                }

                if(eventos.event_start < eventos.Options && eventos.event_end > eventos.Options_End)
                {
                    return BadRequest();
                }

                try
                {
                    await _context.SaveChangesAsync();
                    TempData["PedidoEnviadoNovamente"] = "";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    return RedirectToAction(nameof(BadRequest));
                }
            }
            return View(eventos);
        }//end editar ferias

        //capture the events(vacations)
        [HttpPost]
        public IActionResult GetCalendarEvents()
        {
            var events = _context.Events.Select(e => new
            {
                Title = e.Title,
                Start = e.event_start,
                End = e.event_end,
                backgroundColor = e.backgroundColor

            }).ToList();

            return new JsonResult(events);
        }

        public async Task<IActionResult> Funcionarios(ApplicationUser u, Events e)
        {
            var users = await _context.Users.OrderBy(i => i.AdmissionDate).ToListAsync();
            u.userImage = Convert.ToString(u.userImage);
            return View(users);
        }

        [HttpGet]
        public async Task<IActionResult> Detalhes(ApplicationUser u, Events e, string? id)
        {

            if (id == null)
                return NotFound();

            var user = await _context.Users.FirstOrDefaultAsync(m => m.Id == id);

            if (user == null)
                return NotFound();

            return View(user);
        }

        [Authorize(Roles = "Administradores")]
        public IActionResult Imprimir()
        {
            return View();
        }


        /*
         * counting days excluding holidays and vacation
         * 
         * @return int
         */
        public static int GetDifDias(DateTime initialDate, DateTime finalDate)
        {
            DateTime today = DateTime.Today;
            int year = today.Year;
            DateTime[] feriados = new DateTime[]
            {
                new DateTime(year, 01, 1),  //ano novo
                new DateTime(year, 02, 13), //Carnaval
                new DateTime(year, 04, 19), //Sexta Feira Santa
                new DateTime(year, 04, 21), //Páscoa
                new DateTime(year, 05, 25), //Dia da Liberdade
                new DateTime(year, 06, 1),  //Dia do Trabalhador
                new DateTime(year, 06, 10), //Dia de Portugal, Camões e das Comunidades Portuguesas
                new DateTime(year, 06, 20), //Corpo de Deus
                new DateTime(year, 07, 1),  //Dia da Madeira
                new DateTime(year, 08, 15), //Assunção (Festa do Monte)
                new DateTime(year, 10, 5),  //Dia da República
                new DateTime(year, 11, 1),  // Dia de todos os Santos
                new DateTime(year, 12, 1),  //Dia da Restauração da Independência
                new DateTime(year, 12, 8),  //Imaculada Conceição
                new DateTime(year, 12, 25), //Dia de Natal
                new DateTime(year, 12, 26), //Primeira Oitava
            };//total posicoes array = 16

            var days = 0;
            var daysCount = 0;
            days = initialDate.Subtract(finalDate).Days;

            if (days < 0)
                days = days * -1;

            for (int i = 1; i <= days; i++)
            {
                initialDate = initialDate.AddDays(1);

                if ( initialDate.DayOfWeek != DayOfWeek.Sunday && initialDate.DayOfWeek != DayOfWeek.Saturday &&
                    initialDate != feriados[0] && initialDate != feriados[1] && initialDate != feriados[2] && initialDate != feriados[3] &&
                    initialDate != feriados[4] && initialDate != feriados[5] && initialDate != feriados[6] && initialDate != feriados[7] && initialDate != feriados[8] &&
                    initialDate != feriados[9] && initialDate != feriados[10] && initialDate != feriados[11] && initialDate != feriados[12] && initialDate != feriados[13] && 
                    initialDate != feriados[14] && initialDate != feriados[15] )
                    daysCount++;
            }
            return daysCount;
        }//end GetDifDias

    }//end class MapaFeriasController

}//end namespace INSCmapaFerias.Controllers
