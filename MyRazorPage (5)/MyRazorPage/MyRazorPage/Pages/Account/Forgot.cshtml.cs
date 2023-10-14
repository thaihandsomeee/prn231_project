using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyRazorPage.Common;
using MyRazorPage.Models;
using System.Net.Mail;
using System.Net;
using Microsoft.EntityFrameworkCore;
using System.Net.Mime;
using DocumentFormat.OpenXml.Drawing.Wordprocessing;
using DocumentFormat.OpenXml.Office2016.Drawing.Command;

namespace MyRazorPage.Pages.Account
{
    public class ForgotModel : PageModel
    {
        private readonly string InternetConnectionError = "A connection attempt failed because the connected party did not properly respond after a period of time, or established connection failed because connected host has failed to respond";
        private readonly PRN221_DBContext prn221DBContext;
        private readonly IConfiguration configuration;
        private readonly int passwordLength = 8;
        public ForgotModel(PRN221_DBContext prn221DBContext, IConfiguration configuration)
        {
            this.prn221DBContext = prn221DBContext;
            this.configuration = configuration;
        }

        [BindProperty]
        public Models.Account? account { get; set; }



        public async Task<IActionResult> OnPost()
        {         
            if (account is null) return Page();
            bool isSend = false;
            string resetPassword = "123";
            account = await findByEmail(account.Email);
            if (account is not null)
            {
                try
                {
                    using (MailMessage mail = new MailMessage())
                    {
                        SmtpClient SmtpServer = new SmtpClient();
                        mail.From = new MailAddress(configuration.GetValue<string>("Smtp:FromAddress"));
                        mail.To.Add(account.Email);
                        mail.Subject = "Password Recovery";
                        mail.Body = string
                            .Format(CommonEmailTemplate.RECOVERY_EMAIL_TEMPLATE, account.Email, resetPassword);
                        mail.IsBodyHtml = true;
                        SmtpServer.UseDefaultCredentials = false;
                        NetworkCredential NetworkCred = new NetworkCredential(
                            configuration.GetValue<string>("Smtp:UserName"),
                            configuration.GetValue<string>("Smtp:Password")
                            );
                        SmtpServer.Credentials = NetworkCred;
                        SmtpServer.EnableSsl = true;
                        SmtpServer.Port = configuration.GetValue<int>("Smtp:Port");
                        SmtpServer.Host = configuration.GetValue<string>("Smtp:Server");
                        SmtpServer.Send(mail);
                        ViewData["message"] = "Send mail success";
                        isSend = true;
                    }
                }
                catch (SmtpException ex)
                {


                    string msg = "Mail cannot be sent because of the server problem:";
                    msg += ex.Message;
                    if (ex.ToString().Contains(InternetConnectionError))
                    {
                        ViewData["Error"] = msg + " Please check your internet connection";
                    }
                    else
                    {
                        ViewData["Error"] = msg;
                    }
                }
            }
            else
            {
                ViewData["error"] = "Email doesn't register";
                return Page();
            }

            if (isSend)
            {
                account.Password = HashPassword.encryptPassword(configuration.GetValue<string>("SecretKey"), resetPassword);
                await prn221DBContext.SaveChangesAsync();
            }
            return Page();
        }

        private async Task<Models.Account?> findByEmail(String? email)
        {
            var accountInDB = await prn221DBContext.Accounts
                .FirstOrDefaultAsync(x => x.Email == email);
            if (accountInDB is not null)
            {
                return accountInDB;
            }
            return null;
        }

        private String generatedPassword()
        {
            string validChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*?_-";
            Random random = new Random();

            // Select one random character at a time from the string  
            // and create an array of chars  
            char[] chars = new char[passwordLength];
            for (int i = 0; i < passwordLength; i++)
            {
                chars[i] = validChars[random.Next(0, validChars.Length)];
            }
            return new string(chars);
        }
    }
}
