namespace AutoDispatch.Web.Logic.Email;

/// <summary>
///     Письмо компании (V1 AutoCall.Web.Common.Objects.Email): confirmation, invoice, setup packet, схема трейлера </summary>
public class Email {

    public List<EmailAddress> ToEmailAddresses { get; set; } = new List<EmailAddress>();
    public EmailAddress FromEmailAddress { get; set; }
    public List<EmailAddress> CcEmailAddresses { get; set; } = new List<EmailAddress>();
    public List<EmailAddress> BccEmailAddresses { get; set; } = new List<EmailAddress>();
    public EmailAddress ReplyEmailAddress { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
    /// <summary>
    ///     Текстовая альтернатива HTML (plain view) </summary>
    public string Description { get; set; }
    public bool IsBodyHTML { get; set; }
    public List<EmailAttachment> Attachments { get; set; } = new List<EmailAttachment>();
    public List<EmailInlineImage> InlineImages { get; set; }
    public bool EnableReadReceipt { get; set; }
    public bool EnableDeliveryReceipt { get; set; }

    public Email() { }

    public Email(string toAddress, string fromAddress, string subject, string body) {
        this.ToEmailAddresses.Add(new EmailAddress(toAddress));
        this.FromEmailAddress = new EmailAddress(fromAddress);
        this.Subject = subject;
        this.Body = body;
    }
}

public class EmailAddress {
    private string _address;

    public string Address {
        get => _address;
        set => _address = value?.Trim();
    }
    public string DisplayName { get; set; }

    public EmailAddress() { }

    public EmailAddress(string address) {
        this.Address = address;
        // no "display name" supplied, so use a double-quoted copy of the actual email address
        this.DisplayName = $"\"{address}\"";
    }

    public EmailAddress(string address, string displayName) {
        this.Address = address;
        this.DisplayName = displayName;
    }
}

public class EmailAttachment {
    public string FileName { get; set; }
    /// <summary>
    ///     base64 </summary>
    public string Content { get; set; }
}

public class EmailInlineImage {
    public string FileName { get; set; }
    public string MediaType { get; set; } = "image/png";
    public string ContentId { get; set; }
    public string Content { get; set; }
}

/// <summary>
///     SMTP компании: DispatchCompany.SMTP_* или SMTP_Scheme* </summary>
public class SmtpSettings {
    public string UserName { get; set; }
    public string Password { get; set; }
    public string Host { get; set; }
    public int Port { get; set; }
    public bool EnableSsl { get; set; } = true;
    public int Timeout { get; set; } = 10000;

    public SmtpSettings(string userName, string password, string host, int port) {
        this.UserName = userName;
        this.Password = password;
        this.Host = host;
        this.Port = port;
    }
}
