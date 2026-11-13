using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using AutoDispatch.Web.Common;

namespace AutoDispatch.Web.Logic.Email;

/// <summary>
///     Отправка писем компаний через их SMTP (V1 EmailUtils, System.Net.Mail как в EmailService AutoCall V3) </summary>
public static class EmailUtils {

    public static async Task SendEmailAsync(SmtpSettings smtpSettings, Email email) {
        using var message = ConvertEmailToMailMessage(email);
        using var smtpClient = new SmtpClient(smtpSettings.Host, smtpSettings.Port) {
            EnableSsl = smtpSettings.EnableSsl,
            Timeout = smtpSettings.Timeout,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(smtpSettings.UserName, smtpSettings.Password)
        };
        await smtpClient.SendMailAsync(message);
        _l.i($"Email '{email.Subject}' sent from {email.FromEmailAddress?.Address} to {String.Join(", ", email.ToEmailAddresses.Select(e => e.Address))}");
    }

    public static bool IsValidEmail(string email) {
        try {
            var addr = new MailAddress(email);
            return addr.Address == email;
        } catch {
            return false;
        }
    }

    private static MailMessage ConvertEmailToMailMessage(Email email) {
        var message = new MailMessage();

        #region [ inserts png ]
        if (email.InlineImages != null && email.InlineImages.Any()) {
            foreach (EmailInlineImage emailImage in email.InlineImages) {
                var image = new LinkedResource(new MemoryStream(Convert.FromBase64String(
                    emailImage.Content.Replace("data:image/png;base64,", "").Replace("data:image/jpeg;base64,", ""))), emailImage.MediaType);
                image.ContentId = $"{emailImage.FileName}@{emailImage.ContentId}";
                image.TransferEncoding = TransferEncoding.Base64;
                AlternateView plainView = AlternateView.CreateAlternateViewFromString(email.Description ?? "", Encoding.UTF8, MediaTypeNames.Text.Plain);
                plainView.TransferEncoding = TransferEncoding.QuotedPrintable;
                AlternateView htmlView = AlternateView.CreateAlternateViewFromString(email.Body, Encoding.UTF8, MediaTypeNames.Text.Html);
                htmlView.LinkedResources.Add(image);
                htmlView.TransferEncoding = TransferEncoding.QuotedPrintable;
                message.AlternateViews.Add(plainView);
                message.AlternateViews.Add(htmlView);
            }
        } else {
            message.Body = email.Body;
            message.IsBodyHtml = email.IsBodyHTML;
        }
        #endregion [ inserts png ]

        foreach (EmailAddress emailAddress in email.ToEmailAddresses) {
            if (IsValidEmail(emailAddress.Address))
                message.To.Add(new MailAddress(emailAddress.Address, emailAddress.DisplayName));
        }
        if (email.FromEmailAddress != null && IsValidEmail(email.FromEmailAddress.Address))
            message.From = new MailAddress(email.FromEmailAddress.Address, email.FromEmailAddress.DisplayName);
        if (email.ReplyEmailAddress != null && IsValidEmail(email.ReplyEmailAddress.Address))
            message.ReplyToList.Add(new MailAddress(email.ReplyEmailAddress.Address, email.ReplyEmailAddress.DisplayName));
        foreach (EmailAddress emailAddress in email.CcEmailAddresses) {
            if (IsValidEmail(emailAddress.Address))
                message.CC.Add(new MailAddress(emailAddress.Address, emailAddress.DisplayName));
        }
        // в V1 BCC тоже уходили в CC — сохранено
        foreach (EmailAddress emailAddress in email.BccEmailAddresses) {
            if (IsValidEmail(emailAddress.Address))
                message.CC.Add(new MailAddress(emailAddress.Address, emailAddress.DisplayName));
        }
        message.Subject = String.IsNullOrEmpty(email.Subject) ? "(No Subject)" : email.Subject;
        foreach (EmailAttachment emailAttachment in email.Attachments ?? new List<EmailAttachment>()) {
            byte[] data = Convert.FromBase64String(emailAttachment.Content);
            message.Attachments.Add(new Attachment(new MemoryStream(data), emailAttachment.FileName));
        }
        if (email.EnableDeliveryReceipt) {
            message.DeliveryNotificationOptions = DeliveryNotificationOptions.OnSuccess;
        }
        if (email.EnableReadReceipt) {
            message.Headers.Add("Disposition-Notification-To", email.FromEmailAddress.Address);
        }
        return message;
    }
}
