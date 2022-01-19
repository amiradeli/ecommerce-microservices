using Ardalis.GuardClauses;
using BuildingBlocks.Email.Configs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace BuildingBlocks.Email;

public class SendGridEmailSender : IEmailSender
{
    private readonly ILogger<SendGridEmailSender> _logger;
    private readonly EmailConfig _config;

    public SendGridEmailSender(IOptions<EmailConfig> emailConfig, ILogger<SendGridEmailSender> logger)
    {
        _logger = logger;
        _config = Guard.Against.Null(emailConfig?.Value, nameof(EmailConfig));
    }

    private SendGridClient SendGridClient => new(_config.SendGridConfig.ApiKey);

    public async Task SendAsync(EmailObject emailObject)
    {
        Guard.Against.Null(emailObject, nameof(EmailObject));

        var message = new SendGridMessage
        {
            Subject = emailObject.Subject,
            HtmlContent = emailObject.MailBody,
        };

        message.AddTo(new EmailAddress(emailObject.ReceiverEmail));

        message.From = new EmailAddress(emailObject.SenderEmail ?? _config.From);
        message.ReplyTo = new EmailAddress(emailObject.SenderEmail ?? _config.From);

        await SendGridClient.SendEmailAsync(message);

        _logger.LogInformation(
            "Email sent. From: {From}, To: {To}, Subject: {Subject}, Content: {Content}",
            _config.From,
            emailObject.ReceiverEmail,
            emailObject.Subject,
            emailObject.MailBody);
    }
}