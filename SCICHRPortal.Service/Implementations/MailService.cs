using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic;
using System.Net;
using System.Net.Mail;
using SCICHRPortal.Service.Interfaces;
using SCICHRPortal.Utility.Extensions;
using SCICHRPortal.Utility.Settings;

namespace SCICHRPortal.Service.Implementations
{
    public class MailService : IMailService
    {
        private MailSettings MailSettings { get; }
        private AppSettings AppSettings { get; }
        public MailService(IOptions<MailSettings> mailSettings, IOptions<AppSettings> appSettings)
        {
            MailSettings = mailSettings.Value;
            AppSettings = appSettings.Value;
        }

        private SmtpClient ConfigureMailClient()
        {
            return new SmtpClient
            {
                Port = MailSettings.Port!,
                Host = MailSettings.Host!,
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(MailSettings.Mail, MailSettings.Password),
                DeliveryMethod = SmtpDeliveryMethod.Network,
        };
        }

        public async Task SendResetPasswordEmailAsync(string email, string name, string path)
        {
            return; //NOTE: this line is for disabling sending of email;
            StreamReader str = new StreamReader(path);
            string MailText = str.ReadToEnd();
            str.Close();

            MailText = MailText.Replace("[username]", name)
                .Replace("[email]", email)
                .Replace("[resetPassword]", MailSettings.ResetPasswordUrl);

            var smtpClient = ConfigureMailClient();

            MailMessage message = new MailMessage
            {
                From = new MailAddress(MailSettings.Mail!, MailSettings.DisplayName),
                Subject = $"Welcome {name}",
                IsBodyHtml = true,
                Body = MailText
            };

            message.To.Add(new MailAddress(email));

            await smtpClient.SendMailAsync(message);
        }

        public async Task SendResetPasswordEmailAsync(string email, string name, FileStreamResult file)
        {
            return; //NOTE: this line is for disabling sending of email;
            StreamReader str = new StreamReader(file.FileStream);
            string MailText = str.ReadToEnd();
            str.Close();

            MailText = MailText.Replace("[username]", name)
                .Replace("[email]", email)
                .Replace("[resetPassword]", MailSettings.ResetPasswordUrl);

            var smtpClient = ConfigureMailClient();

            MailMessage message = new MailMessage
            {
                From = new MailAddress(MailSettings.Mail!, MailSettings.DisplayName),
                Subject = $"Welcome {name}",
                IsBodyHtml = true,
                Body = MailText
            };

            message.To.Add(new MailAddress(email));

            await smtpClient.SendMailAsync(message);
        }

        public async Task SendForgotPasswordEmailAsync(string email, string name, string newPassword, FileStreamResult file)
        {
            StreamReader str = new StreamReader(file.FileStream);
            string MailText = str.ReadToEnd();
            str.Close();

            MailText = MailText.Replace("[username]", name)
                .Replace("[newPassword]", newPassword);

            var smtpClient = ConfigureMailClient();

            MailMessage message = new MailMessage
            {
                From = new MailAddress(MailSettings.Mail!, MailSettings.DisplayName),
                Subject = $"New Password",
                BodyEncoding = System.Text.Encoding.UTF8,
                SubjectEncoding = System.Text.Encoding.Default,
                IsBodyHtml = true,
                Body = MailText
            };

            message.To.Add(new MailAddress(email));

            await smtpClient.SendMailAsync(message);
        }

        public async Task SendNewUserEmailAsync(string email, string name, string newPassword, FileStreamResult file)
        {
            return; //NOTE: this line is for disabling sending of email;
            StreamReader str = new StreamReader(file.FileStream);
            string MailText = str.ReadToEnd();
            str.Close();

            MailText = MailText.Replace("[username]", name)
                .Replace("[weburl]", AppSettings.WebUrl)
                .Replace("[newPassword]", newPassword);

            var smtpClient = ConfigureMailClient();

            MailMessage message = new MailMessage
            {
                From = new MailAddress(MailSettings.Mail!, MailSettings.DisplayName),
                Subject = $"New User",
                IsBodyHtml = true,
                Body = MailText
            };

            message.To.Add(new MailAddress(email));

            await smtpClient.SendMailAsync(message);
        }

        public async Task SendApprovedRegistrationEmailAsync(string email, string name, string studentName, FileStreamResult file)
        {
            return; //NOTE: this line is for disabling sending of email;
            StreamReader str = new StreamReader(file.FileStream);
            string MailText = str.ReadToEnd();
            str.Close();

            MailText = MailText.Replace("[username]", name)
                .Replace("[studentName]", studentName)
                .Replace("[weburl]", AppSettings.WebUrl);

            var smtpClient = ConfigureMailClient();

            MailMessage message = new MailMessage
            {
                From = new MailAddress(MailSettings.Mail!, MailSettings.DisplayName),
                Subject = $"Application Approved",
                IsBodyHtml = true,
                Body = MailText
            };

            message.To.Add(new MailAddress(email));

            await smtpClient.SendMailAsync(message);
        }

        public async Task SendCancelRegistrationEmailAsync(string email, string name, string studentName, string reason, FileStreamResult file)
        {
            return; //NOTE: this line is for disabling sending of email;
            StreamReader str = new StreamReader(file.FileStream);
            string MailText = str.ReadToEnd();
            str.Close();

            MailText = MailText.Replace("[username]", name)
                .Replace("[weburl]", AppSettings.WebUrl)
                .Replace("[studentName]", studentName)
                .Replace("[reason]", reason);

            var smtpClient = ConfigureMailClient();

            MailMessage message = new MailMessage
            {
                From = new MailAddress(MailSettings.Mail!, MailSettings.DisplayName),
                Subject = $"Application UnApproved",
                BodyEncoding = System.Text.Encoding.UTF8,
                IsBodyHtml = true,
                Body = MailText
            };

            message.To.Add(new MailAddress(email));

            await smtpClient.SendMailAsync(message);
        }

        public async Task SendAnnouncementEmailAsync(string email, string name, string title, string announcement, IFormFile attachFile, FileStreamResult file)
        {
            StreamReader str = new StreamReader(file.FileStream);
            string MailText = str.ReadToEnd();
            str.Close();
            
            MailText = MailText.Replace("[username]", name)
                .Replace("[weburl]", AppSettings.WebUrl)
                .Replace("[title]", title)
                .Replace("[announcement]", announcement);

            var smtpClient = ConfigureMailClient();
     
            MailMessage message = new MailMessage
            {
                From = new MailAddress(MailSettings.Mail!, MailSettings.DisplayName),
                Subject = $"Announcement -{title}",
                BodyEncoding = System.Text.Encoding.UTF8,
                IsBodyHtml = true,
            Body = MailText
            };

            if(attachFile != null )
            {
                var attachementStream = new MemoryStream(await attachFile.GetBytes());
                Attachment attachment = new Attachment(attachementStream, attachFile.FileName, file.ContentType);
                message.Attachments.Add(attachment);
            }
            message.To.Add(new MailAddress(email));

            await smtpClient.SendMailAsync(message);
        }

        public async Task SendStatementOfAccountAsync(string weburl, string email, string name, string statements, string number, string date, string dueDate, string totalAmount, string admin, FileStreamResult file)
        {
            StreamReader str = new StreamReader(file.FileStream);
            string MailText = str.ReadToEnd();
            str.Close();

            MailText = MailText.Replace("[student]", name)
                .Replace("[weburl]", weburl)
                .Replace("[statements]", statements)
                .Replace("[number]", number)
                .Replace("[date]", date)
                .Replace("[duedate]", dueDate)
                .Replace("[total]", totalAmount)
                .Replace("[admin]", admin);

            var smtpClient = ConfigureMailClient();

            MailMessage message = new MailMessage
            {
                From = new MailAddress(MailSettings.Mail!, MailSettings.DisplayName),
                Subject = $"Statement of Account",
                IsBodyHtml = true,
                Body = MailText
            };

            message.To.Add(new MailAddress(email));

            await smtpClient.SendMailAsync(message);
        }

        public async Task SendDidNotPassRequirementsAsync(string email, string name, string student, string requirements, FileStreamResult file)
        {
            return; //NOTE: this line is for disabling sending of email;
            StreamReader str = new StreamReader(file.FileStream);
            string MailText = str.ReadToEnd();
            str.Close();

            MailText = MailText.Replace("[username]", name)
                .Replace("[student]", student)
                .Replace("[requirements]", requirements);

            var smtpClient = ConfigureMailClient();

            MailMessage message = new MailMessage
            {
                From = new MailAddress(MailSettings.Mail!, MailSettings.DisplayName),
                Subject = $"Requirements",
                IsBodyHtml = true,
                Body = MailText
            };

            message.To.Add(new MailAddress(email));

            await smtpClient.SendMailAsync(message);
        }

        public async Task SendReservationAsync(string email, string studentNumber, string studentName, string schoolYear, string reservationDate, string expiryDate, string gradeLevel, string section, FileStreamResult file)
        {
            StreamReader str = new StreamReader(file.FileStream);
            string MailText = str.ReadToEnd();
            str.Close();

            MailText = MailText.Replace("[studentNumber]", studentNumber)
                .Replace("[studentName]", studentName)
                .Replace("[schoolYear]", schoolYear)
                .Replace("[reservationDate]", reservationDate)
                .Replace("[expiryDate]", expiryDate)
                .Replace("[gradeLevel]", gradeLevel)
                .Replace("[section]", section);

            var smtpClient = ConfigureMailClient();

            MailMessage message = new MailMessage
            {
                From = new MailAddress(MailSettings.Mail!, MailSettings.DisplayName),
                Subject = $"Reservation",
                BodyEncoding = System.Text.Encoding.UTF8,
                IsBodyHtml = true,
                Body = MailText
            };

            message.To.Add(new MailAddress(email));

            await smtpClient.SendMailAsync(message);
        }

        public async Task SendExpiryReservationAsync(string email, string studentName, string reservationDate, string remainingDays, FileStreamResult file)
        {
            StreamReader str = new StreamReader(file.FileStream);
            string MailText = str.ReadToEnd();
            str.Close();

            MailText = MailText.Replace("[studentName]", studentName)
                .Replace("[reservationDate]", reservationDate)
                .Replace("[remainingDays]", remainingDays);

            var smtpClient = ConfigureMailClient();

            MailMessage message = new MailMessage
            {
                From = new MailAddress(MailSettings.Mail!, MailSettings.DisplayName),
                Subject = $"Expiry of Reservation and Enrollment Reminder",
                BodyEncoding = System.Text.Encoding.UTF8,
                IsBodyHtml = true,
                Body = MailText
            };

            message.To.Add(new MailAddress(email));

            await smtpClient.SendMailAsync(message);
        }

        public async Task<string> GetStatementOfAccountAsync(string weburl, string email, string name, string statements, string number, string date, string dueDate, string totalAmount, string admin, FileStreamResult file)
        {
            StreamReader str = new StreamReader(file.FileStream);
            string MailText = str.ReadToEnd();
            str.Close();

            MailText = MailText.Replace("[student]", name)
                .Replace("[weburl]", weburl)
                .Replace("[statements]", statements)
                .Replace("[number]", number)
                .Replace("[date]", date)
                .Replace("[duedate]", dueDate)
                .Replace("[total]", totalAmount)
                .Replace("[admin]", admin);

            return MailText;
        }

        public async Task SendReservationForfeitedAsync(string weburl, string email, string name, string date, string reservationDate, string reservationAmount, FileStreamResult file)
        {
            StreamReader str = new StreamReader(file.FileStream);
            string MailText = str.ReadToEnd();
            str.Close();

            MailText = MailText.Replace("[username]", name)
                .Replace("[weburl]", weburl)
                .Replace("[date]", date)
                .Replace("[reservationDate]", reservationDate)
                .Replace("[reservationAmount]", reservationAmount);

            var smtpClient = ConfigureMailClient();

            MailMessage message = new MailMessage
            {
                From = new MailAddress(MailSettings.Mail!, MailSettings.DisplayName),
                Subject = $"Reservation Forfieture",
                IsBodyHtml = true,
                BodyEncoding = System.Text.Encoding.UTF8,
                Body = MailText
            };

            message.To.Add(new MailAddress(email));

            await smtpClient.SendMailAsync(message);
        }

        public async Task SendSOABalanceAsync(string weburl, string email, string name, string totalAmount, string date, string schoolYear, FileStreamResult file)
        {
            StreamReader str = new StreamReader(file.FileStream);
            string MailText = str.ReadToEnd();
            str.Close();

            MailText = MailText.Replace("[student]", name)
                .Replace("[weburl]", weburl)
                .Replace("[total]", totalAmount)
                .Replace("[schoolYear]", schoolYear)
                .Replace("[date]", date);

            var smtpClient = ConfigureMailClient();

            MailMessage message = new MailMessage
            {
                From = new MailAddress(MailSettings.Mail!, MailSettings.DisplayName),
                Subject = $"Statement of Account",
                IsBodyHtml = true,
                Body = MailText
            };

            message.To.Add(new MailAddress(email));

            await smtpClient.SendMailAsync(message);
        }

        public async Task SendApprovedDeclinedRegistrationEmailAsync(string email, string parentName, string studentName, DateTime dateRegistered,FileStreamResult file)
        {
            StreamReader str = new StreamReader(file.FileStream);
            string MailText = str.ReadToEnd();
            str.Close();
            
            MailText = MailText.Replace("[parentName]", parentName)
                .Replace("[studentName]", studentName)
                .Replace("[dateRegistered]", dateRegistered.ToString("dddd, dd MMMM yyyy"))
                .Replace("[weburl]", AppSettings.WebUrl);

            var smtpClient = ConfigureMailClient();

            MailMessage message = new MailMessage
            {
                From = new MailAddress(MailSettings.Mail!, MailSettings.DisplayName),
                Subject = $"LMSI Registration Application Status",
                IsBodyHtml = true,
                Body = MailText
            };

            message.To.Add(new MailAddress(email));

            await smtpClient.SendMailAsync(message);
        }
    }
}
