/*
 * Copyright Matthew Cosand
 */
using System.Collections.Generic;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Sar.Services
{
  public interface ISendEmailService
  {
    Task SendEmail(string to, string subject, string message, bool html = true);
    Task SendEmail(IEnumerable<string> to, string subject, string message, bool html = true);
  }
  public interface ISendTextService
  {
    Task SendText(string number, string message);
  }

  public class DefaultSendMessageService : ISendEmailService
  {
    private readonly IConfigService _config;
    public DefaultSendMessageService(IConfigService configService)
    {
      _config = configService;
    }

    public async Task SendEmail(string to, string subject, string message, bool html = true)
    {
      await SendEmail(new [] { to }, subject, message, html);
    }

    public async Task SendEmail(IEnumerable<string> to, string subject, string message, bool html = true)
    {
      var email = new MailMessage
      {
        From = new MailAddress(_config["email:from"] ?? "noone@example.com"),
        Subject = subject,
        Body = message,
        IsBodyHtml = html
      };
      foreach (var addr in to)
      {
        email.To.Add(addr);
      }

      SmtpClient client = new SmtpClient();
      await client.SendMailAsync(email);
    }
  }
}
