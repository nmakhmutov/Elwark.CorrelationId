using System.Net.Mail;

namespace Elwark.People.Abstractions
{
    public static class Extensions
    {
        internal static string ValidateEmail(this string email)
        {
            var result = new MailAddress(email);

            return result.Address;
        }

        public static string GetUser(this Identification.Email email) => 
            new MailAddress(email.Value).User;
    }
}