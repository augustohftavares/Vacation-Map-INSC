using INSCmapaFerias.Areas.Identity.Data;
using INSCmapaFerias.Data;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using MimeKit.Utils;
using System.Data;

namespace INSCmapaFerias.Controllers
{
    [Authorize(Roles = "Administradores")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /******************
         *                *
         *      USERS     *
         *                *
         ******************/

        public async Task<IActionResult> Gestor_de_Utilizadores()
        {
            //obtém todos os utilizadores ordenados pela data de admissão
            return View(await _context.Users.OrderBy(i => i.AdmissionDate).ToListAsync());
        }

        [HttpGet]
        public async Task<IActionResult> Editar_Utilizador(string id)
        {
            var user = await _context.Users.FindAsync(id);
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> Editar_Utilizador(ApplicationUser model)
        {
            var user = await _context.Users.FindAsync(model.Id);
            if (user == null)
            {
                TempData["UserEditNull"] = "Não foi possível identificar o utilizador, entre em contato com um administrador.";
                return RedirectToAction(nameof(Gestor_de_Utilizadores));
            }
            //captura o que o utilizador introduziu
            user.AdmissionDate = model.AdmissionDate;
            user.prof_category = model.prof_category;
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.UserName = model.UserName;
            user.userImage = model.userImage;
            try
            {
                await _context.SaveChangesAsync();
                TempData["UserEditSuccess"] = "Utilizador editado com sucesso";
                return RedirectToAction(nameof(Gestor_de_Utilizadores));
            }
            catch (DbUpdateException)
            {
            }
            return View(model);
        }//end editar_utilizador

        [HttpGet]
        public async Task<IActionResult> Eliminar_Utilizador(string id, bool? saveChangesError = false)
        {
            if (id == null)
            {
                TempData["UserEditNull"] = "Não foi possível identificar o utilizador, entre em contato com um administrador.";
                return RedirectToAction(nameof(Gestor_de_Utilizadores));
            }
            var user = await _context.Users.FindAsync(id);
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> Eliminar_Utilizador(string? id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                TempData["UserEditNull"] = "Não foi possível identificar o utilizador, entre em contato com um administrador.";
                return RedirectToAction(nameof(Gestor_de_Utilizadores));
            }

            try
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                TempData["UserRemoveSuccess"] = "Utilizador removido com sucesso";
                return RedirectToAction(nameof(Gestor_de_Utilizadores));
            }
            catch (DbUpdateException)
            {
                TempData["DBerror"] = "";
                return RedirectToAction(nameof(Gestor_de_Utilizadores), new { id = id, saveChangesError = true });
            }
        }

        /***************
         *             *
         *  VACATIONS  *
         *             *
         ***************/

        public async Task<IActionResult> Gestor_de_Ferias(Events e)
        {
            return View(await _context.Events.OrderBy(i => i.event_start).ToListAsync());
        }

        [HttpGet]
        public async Task<IActionResult> Editar_Ferias(string id)
        {
            var user = await _context.Events.FindAsync(id);
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> Editar_Ferias(Events model)
        {
            var eventos = await _context.Events.FindAsync(model.event_id);

            if (eventos == null)
            {
                TempData["FeriasNull"] = "Não foi possível identificar estas férias, entre em contato com um administrador.";
                return RedirectToAction(nameof(Gestor_de_Ferias));
            }

            if (ModelState.IsValid)
            {
                eventos.event_id = model.event_id;
                eventos.Title = model.Title;
                eventos.event_start = model.event_start;
                eventos.event_end = model.event_end;

                var total = GetDifDias(Convert.ToDateTime(eventos.event_start), Convert.ToDateTime(eventos.event_end));
                var totalOptions = GetDifDias(Convert.ToDateTime(eventos.Options), Convert.ToDateTime(eventos.Options_End));

                var convertString = total.ToString();
                eventos.Total = convertString;

                eventos.Status = model.Status;
                eventos.Options = model.Options;
                eventos.Options_End = model.Options_End;

                // checks the user's vacation limit
                if (Convert.ToInt32(eventos.Total) > 22)
                {
                    TempData["FeriasLimit"] = "";
                    return RedirectToAction(nameof(Editar_Ferias));
                }
                // checks if dates are invalid
                if (eventos.Options > eventos.Options_End || eventos.Options_End < eventos.Options || eventos.event_start > eventos.event_end || eventos.event_end < eventos.event_start)
                {
                    TempData["FeriasInvalidDates"] = "";
                    return RedirectToAction(nameof(Editar_Ferias));
                }

                try
                {
                    await _context.SaveChangesAsync();
                    //send email
                    //Body,Subject, EmailFrom, ToEmail
                    var emailName = "desenvolvimento";
                    var ourEmail = "desenvolvimento@insc.pt";
                    var emailSubject = "Pedido de férias";
                    var emailUser = eventos.Email;
                    var mailMessage = new MimeMessage();
                    mailMessage.From.Add(new MailboxAddress(emailName, ourEmail));
                    mailMessage.To.Add(new MailboxAddress("to name", emailUser));
                    mailMessage.Subject = emailSubject;

                    if (eventos.Status == "Aprovado")
                    {
                        var builder = new BodyBuilder();
                        var image = builder.LinkedResources.Add("wwwroot/img/aprovado.jpg");
                        image.ContentId = MimeUtils.GenerateMessageId();
                        builder.Attachments.Add("wwwroot/img/aprovado.jpg");
                        // approval message (email)
                        builder.TextBody = "Olá " + eventos.Title + ",\n\n" + "O seu pedido de férias de " + eventos.event_start + " até " + eventos.event_end + " foi aprovado" + "\n\n" + "Desejamos-lhe boas férias!";
                        builder.HtmlBody = string.Format("Olá <b>" + eventos.Title + "</b>,<br /> <br />" + "O seu pedido de férias de " + eventos.event_start.Value.ToShortDateString() + " até " + eventos.event_end.Value.ToShortDateString() + " foi <span style='color: green;'>aprovado</span>" + "<br /> <br />" + "Desejamos-lhe boas férias!");
                        mailMessage.Body = builder.ToMessageBody();
                        var serverIp = "172.16.100.4";
                        var serverPort = 587;
                        var serverEmail = "desenvolvimento@insc.pt";
                        var serverPassword = "t68E%1EAFaRzBe8@";
                        using (var smtpClient = new SmtpClient())
                        {
                            smtpClient.Connect(serverIp, serverPort, SecureSocketOptions.None);
                            smtpClient.Authenticate(serverEmail, serverPassword);
                            smtpClient.Send(mailMessage);
                            smtpClient.Disconnect(true);
                        }
                    }else if (eventos.Status == "Rejeitado")
                    {
                        var builder = new BodyBuilder();
                        var image = builder.LinkedResources.Add("wwwroot/img/rejeitado.webp");
                        image.ContentId = MimeUtils.GenerateMessageId();
                        builder.Attachments.Add("wwwroot/img/rejeitado.webp");
                        //rejection message (email)
                        builder.HtmlBody = string.Format("Olá <b>" + eventos.Title + "</b>,<br /><br />" + "O seu pedido de férias foi <span style='color: red;'>rejeitado</span> e sugerimos estas datas entre: " + "<br /><br /> " + eventos.Options.Value.ToShortDateString() +" <b>até</b> " + eventos.Options_End.Value.ToShortDateString());
                        mailMessage.Body = builder.ToMessageBody();
                        var serverIp = "172.16.100.4";
                        var serverPort = 587;
                        var serverEmail = "desenvolvimento@insc.pt";
                        var serverPassword = "t68E%1EAFaRzBe8@";

                        using (var smtpClient = new SmtpClient())
                        {
                            smtpClient.Connect(serverIp, serverPort, SecureSocketOptions.None);
                            smtpClient.Authenticate(serverEmail, serverPassword);
                            smtpClient.Send(mailMessage);
                            smtpClient.Disconnect(true);
                        }
                    }//end else

                    //success message
                    TempData["FeriasEditSuccess"] = "Férias editadas com sucesso";
                    return RedirectToAction(nameof(Gestor_de_Ferias));

                } catch (DbUpdateConcurrencyException) {
                    TempData["DBerror"] = "";
                    return RedirectToAction(nameof(Gestor_de_Ferias));
                }
            }//end if
            return View(eventos);
        }//end editar ferias

        public async Task<IActionResult> Eliminar_Ferias(string? id, bool? saveChangesError = false)
        {
            if (id == null)
            {
                TempData["FeriasNull"] = "";
                return RedirectToAction(nameof(Gestor_de_Ferias));
            }

            var events = await _context.Events.AsNoTracking().FirstOrDefaultAsync(m => m.event_id == id);

            if (events == null)
            {
                TempData["FeriasNull"] = "";
                return RedirectToAction(nameof(Gestor_de_Ferias));
            }

            // error message
            if (saveChangesError.GetValueOrDefault())
                ViewData["ErrorMessage"] = "Tentativa falhada ao tentar eliminar estas férias, tente novamente";

            return View(events);
        }//end eliminar_ferias

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar_Ferias(string id)
        {
            var events = await _context.Events.FindAsync(id);

            if (events == null)
            {
                TempData["FeriasNull"] = "";
                return RedirectToAction(nameof(Gestor_de_Ferias));
            }

            try
            {
                _context.Events.Remove(events);
                await _context.SaveChangesAsync();
                TempData["FeriasRemoveSuccess"] = "Férias removidas com sucesso";
                return RedirectToAction(nameof(Gestor_de_Ferias));
            } catch (DbUpdateException /* ex */) {
                TempData["DBerror"] = "";
                return RedirectToAction(nameof(Gestor_de_Ferias), new { id = id, saveChangesError = true });
            }
        }//end Eliminar_Ferias

        /*****************
         *               *
         *   Paternity   *
         *               *
         *****************/

        public async Task<IActionResult> Gestor_de_Paternidades()
        {
            return View(await _context.Paternity.ToListAsync());
        }

        [HttpGet]
        public IActionResult Adicionar_Paternidade()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Adicionar_Paternidade(Paternity p)
        {

            try
            {
                p.PaternityId = Guid.NewGuid().ToString();
                var totalPaternity = GetDifDias(Convert.ToDateTime(p.PaternityStart), Convert.ToDateTime(p.PaternityEnd));
                var convertString = totalPaternity.ToString();
                p.Total = convertString;
                await _context.Paternity.AddAsync(p);
                await _context.SaveChangesAsync();
                TempData["PaternitySubmited"] = "Paternidade adicionada com sucesso";
                return RedirectToAction(nameof(Gestor_de_Paternidades));
            }
            catch
            {
                TempData["DBerror"] = "";
                return RedirectToAction(nameof(Gestor_de_Paternidades));
            }
        }//end adicionar_paternidade

        [HttpGet]
        public async Task<IActionResult> Editar_Paternidade(string id)
        {
            var paternidade = await _context.Paternity.FindAsync(id);
            return View(paternidade);
        }

        [HttpPost]
        public async Task<IActionResult> Editar_Paternidade(Paternity model)
        {
            var paternidade = await _context.Paternity.FindAsync(model.PaternityId);

            if (paternidade == null)
            {
                TempData["PaternidadeNull"] = "";
                return RedirectToAction(nameof(Gestor_de_Paternidades));
            }

            if (ModelState.IsValid)
            {
                paternidade.PaternityId = model.PaternityId;
                paternidade.Name = model.Name;
                paternidade.PaternityStart = model.PaternityStart;
                paternidade.PaternityEnd = model.PaternityEnd;
                var total = GetDifDias(Convert.ToDateTime(paternidade.PaternityStart), Convert.ToDateTime(paternidade.PaternityEnd));
                var convertString = total.ToString();
                paternidade.Total = convertString;

                try
                {
                    await _context.SaveChangesAsync();

                    // MSG sucesso
                    TempData["PaternityEditSuccess"] = "Paternidade editada com sucesso";

                    return RedirectToAction(nameof(Gestor_de_Paternidades));

                } catch (DbUpdateConcurrencyException)
                {
                    TempData["DBerror"] = "";
                    return RedirectToAction(nameof(Gestor_de_Paternidades));
                }
            }
            return View(paternidade);
        }//end editar paternidade

        public async Task<IActionResult> Eliminar_Paternidade(string? id, bool? saveChangesError = false)
        {
            if (id == null)
            {
                TempData["PaternidadeNull"] = "";
                return RedirectToAction(nameof(Gestor_de_Paternidades));
            }

            var paternidade = await _context.Paternity
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.PaternityId == id);

            if (paternidade == null)
            {
                TempData["PaternidadeNull"] = "";
                return RedirectToAction(nameof(Gestor_de_Paternidades));
            }

            //msg erro
            if (saveChangesError.GetValueOrDefault())
            {
                ViewData["ErrorMessage"] = "Tentativa falhada ao tentar eliminar esta paternidade, tenta novamente";
            }

            return View(paternidade);
        }//end eliminar_ferias

        // POST: ELIMINAR_PATERNIDADE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar_Paternidade(string id)
        {
            var paternidade = await _context.Paternity.FindAsync(id);
            if (paternidade == null)
            {
                TempData["PaternidadeNull"] = "";
                return RedirectToAction(nameof(Gestor_de_Paternidades));
            }

            try
            {
                _context.Paternity.Remove(paternidade);
                await _context.SaveChangesAsync();
                TempData["PaternityRemoveSuccess"] = "Férias removidas com sucesso";
                return RedirectToAction(nameof(Gestor_de_Paternidades));
            } catch (DbUpdateException /* ex */) {
                TempData["DBerror"] = "";
                return RedirectToAction(nameof(Gestor_de_Paternidades), new { id = id, saveChangesError = true });
            }
        }//end Eliminar_Paternidade

        /*****************
         *               *
         *   Overtime    *
         *               *
         *****************/

        public async Task<IActionResult> Gestor_Horas_Extra()
        {
            return View(await _context.Overtime.ToListAsync());
        }

        [HttpGet]
        public IActionResult Adicionar_Horas_Extra()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Adicionar_Horas_Extra(Overtime o)
        {
            try
            {
                o.OvertimeId = Guid.NewGuid().ToString();
                var total = GetDifDias(Convert.ToDateTime(o.OvertimeStart), Convert.ToDateTime(o.OvertimeEnd));
                var convertString = total.ToString();
                int control = 0;

                for (var i = 0; i <= total; i++)
                {
                    if(Convert.ToDateTime(o.OvertimeStart).AddDays(i).DayOfWeek == DayOfWeek.Saturday || Convert.ToDateTime(o.OvertimeStart).AddDays(i).DayOfWeek == DayOfWeek.Sunday)
                    {
                        ++control;
                    }
                }
                o.Total = control.ToString();
                await _context.Overtime.AddAsync(o);
                await _context.SaveChangesAsync();
                TempData["OvertimeSubmited"] = "Horas extra adicionadas com sucesso";
                return RedirectToAction(nameof(Gestor_Horas_Extra));
            } catch {
                TempData["DBerror"] = "";
                return RedirectToAction(nameof(Gestor_Horas_Extra));
            }
        }//end adicionar_horas_extra

        [HttpGet]
        public async Task<IActionResult> Editar_Horas_Extra(string id)
        {
            var overtime = await _context.Overtime.FindAsync(id);
            return View(overtime);
        }

        [HttpPost]
        public async Task<IActionResult> Editar_Horas_Extra(Overtime model)
        {
            var overtime = await _context.Overtime.FindAsync(model.OvertimeId);

            if (overtime == null)
            {
                TempData["HorasExtraNull"] = "";
                return RedirectToAction(nameof(Gestor_Horas_Extra));
            }

            if (ModelState.IsValid)
            {
                overtime.OvertimeId = model.OvertimeId;
                overtime.OvertimeName = model.OvertimeName;
                overtime.OvertimeStart = model.OvertimeStart;
                overtime.OvertimeEnd = model.OvertimeEnd;

                var total = GetDifDias(Convert.ToDateTime(overtime.OvertimeStart), Convert.ToDateTime(overtime.OvertimeEnd));
                var convertString = total.ToString();
                int control = 0;

                for (var i = 0; i <= total; i++)
                {
                    if (Convert.ToDateTime(overtime.OvertimeStart).AddDays(i).DayOfWeek == DayOfWeek.Saturday ||
                       Convert.ToDateTime(overtime.OvertimeStart).AddDays(i).DayOfWeek == DayOfWeek.Sunday
                      )
                    {
                        ++control;
                    }
                }
                overtime.Total = control.ToString();
                overtime.Hours = model.Hours;

                try
                {
                    await _context.SaveChangesAsync();
                    TempData["OvertimeEditSuccess"] = "Horas extra editadas com sucesso";
                    return RedirectToAction(nameof(Gestor_Horas_Extra));

                } catch (DbUpdateConcurrencyException) {
                    TempData["DBerror"] = "";
                    return RedirectToAction(nameof(Gestor_de_Paternidades));
                }
            }
            return View(overtime);
        }//end editar paternidade

        public async Task<IActionResult> Eliminar_Horas_Extra(string? id, bool? saveChangesError = false)
        {
            if (id == null)
            {
                TempData["HorasExtraNull"] = "";
                return RedirectToAction(nameof(Gestor_Horas_Extra));
            }

            var overtime = await _context.Overtime.AsNoTracking().FirstOrDefaultAsync(m => m.OvertimeId == id);

            if (overtime == null)
            {
                TempData["HorasExtraNull"] = "";
                return RedirectToAction(nameof(Gestor_Horas_Extra));
            }

            if (saveChangesError.GetValueOrDefault())
            {
                ViewData["ErrorMessage"] = "Tentativa falhada ao tentar eliminar estas horas extra, tenta novamente";
            }

            return View(overtime);
        }//end eliminar_ferias

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar_Horas_Extra(string id)
        {
            var overtime = await _context.Overtime.FindAsync(id);

            if (overtime == null)
            {
                TempData["HorasExtraNull"] = "";
                return RedirectToAction(nameof(Gestor_Horas_Extra));
            }

            try
            {
                _context.Overtime.Remove(overtime);
                await _context.SaveChangesAsync();
                TempData["OvertimeRemoveSuccess"] = "Horas extras removidas com sucesso";
                return RedirectToAction(nameof(Gestor_Horas_Extra));
            }
            catch (DbUpdateException /* ex */)
            {
                TempData["DBerror"] = "";
                return RedirectToAction(nameof(Gestor_Horas_Extra), new { id = id, saveChangesError = true });
            }
        }//end Eliminar_Paternidade

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
            };//total posicoes array = 15

            var days = 0;
            var daysCount = 0;
            days = initialDate.Subtract(finalDate).Days;

            if (days < 0)
                days = days * -1;

            for (int i = 1; i <= days; i++)
            {
                initialDate = initialDate.AddDays(1);

                if (initialDate.DayOfWeek != DayOfWeek.Sunday && initialDate.DayOfWeek != DayOfWeek.Saturday &&
                    initialDate != feriados[0] && initialDate != feriados[1] && initialDate != feriados[2] && initialDate != feriados[3] &&
                    initialDate != feriados[4] && initialDate != feriados[5] && initialDate != feriados[6] && initialDate != feriados[7] && initialDate != feriados[8] &&
                    initialDate != feriados[9] && initialDate != feriados[10] && initialDate != feriados[11] && initialDate != feriados[12] && initialDate != feriados[13] &&
                    initialDate != feriados[14] && initialDate != feriados[15])
                    daysCount++;
            }
            return daysCount;
        }//end GetDifDias
    }//end class AdminController

}//end namespace INSCmapaFerias.Controllers