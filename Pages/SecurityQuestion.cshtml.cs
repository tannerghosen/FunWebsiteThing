using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
#pragma warning disable CS8618

namespace FunWebsiteThing.Pages
{
    public class SecurityQuestionModel : PageModel
    {
        SessionManager _s;
        [BindProperty]
        public string Id { get; set; }
        public string Question { get; set; }

        [BindProperty]
        public string Answer { get; set; }

        public string CorrectAnswer { get; set; }

        [BindProperty]
        public string Result { get; set; }

        public void OnGet()
        {
            Id = TempData["Id"]?.ToString(); 
            try
            {
                int userid = int.TryParse(Id, out int newid) ? newid : 0;
                Question = SQL.Accounts.GetSecurityQuestion(userid)[0];
                CorrectAnswer = SQL.Accounts.GetSecurityQuestion(userid)[1];
            }
            catch
            {
            }
         }
        public async Task<IActionResult> OnPost()
        {
            Id = Request.Form["Id"]; // pass the Id from the form
            int id = int.TryParse(Id, out int newid) ? newid : 0;
            CorrectAnswer = Request.Form["CorrectAnswer"];
            if (!string.IsNullOrEmpty(Answer)) 
            {
                if (Answer.Equals(CorrectAnswer, StringComparison.OrdinalIgnoreCase)) // if answer is roughly the same as the correct answer
                {
                    TempData["Id"] = Id; 
                    Response.Redirect("/ChangePassword");
                    
                }
                else // if answer is incorrect
                {
                    Result = "Incorrect answer. If you are unable to figure out your answer, please contact the admins.";
                    TempData["Id"] = Id;
                    Response.Redirect("/SecurityQuestion");
                }
            }
            else // if null
            {
                Result = "Answer is blank";
                TempData["Id"] = Id;
                Response.Redirect("/SecurityQuestion?id=" + Id);
            }
            return Page();
        }
    }
}
