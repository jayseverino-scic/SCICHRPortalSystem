using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SCICHRPortal.Utility.Interface;

namespace SCICHRPortal.Service.Interfaces
{
    public interface IMailService: IScopedService
    {
        Task SendResetPasswordEmailAsync(string email, string name, string path);
        Task SendResetPasswordEmailAsync(string email, string name, FileStreamResult file);
        Task SendForgotPasswordEmailAsync(string email, string name, string newPassword, FileStreamResult file);
        Task SendNewUserEmailAsync(string email, string name, string newPassword, FileStreamResult file);
        Task SendApprovedDeclinedRegistrationEmailAsync(string email, string parentName, string studentName, DateTime dateRegistered, FileStreamResult file);
        Task SendApprovedRegistrationEmailAsync(string email, string name, string studentName, FileStreamResult file);
        Task SendCancelRegistrationEmailAsync(string email, string name, string studentName, string reason, FileStreamResult file);
        Task SendAnnouncementEmailAsync(string email, string name, string title, string announcement, IFormFile attachFile, FileStreamResult file);
        Task SendStatementOfAccountAsync(string weburl, string email, string name, string statements, string number, string date, string dueDate, string totalAmount, string admin, FileStreamResult file);
        Task SendSOABalanceAsync(string weburl, string email, string name, string totalAmount ,string date, string schoolYear, FileStreamResult file);
        Task<string> GetStatementOfAccountAsync(string weburl, string email, string name, string statements, string number, string date, string dueDate, string totalAmount, string admin, FileStreamResult file);
        Task SendDidNotPassRequirementsAsync(string email, string name, string student, string requirements, FileStreamResult file);
        Task SendReservationAsync(string email, string studentNumber, string studentName, string schoolYear,
            string reservationDate, string expiryDate, string gradeLevel, string section, FileStreamResult file);

        Task SendExpiryReservationAsync(string email, string studentName,string reservationDate, string remainingDays, FileStreamResult file);
        Task SendReservationForfeitedAsync(string weburl, string email, string name, string date, string reservationDate, string reservationAmount, FileStreamResult file);
    }
}
