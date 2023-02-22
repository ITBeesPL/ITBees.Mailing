# ITBees.Mailing
This library belongs to the 'FAS - Fast Application Start' framework created by ITBees as part of an open source project. You can explore the capabilities of the framework at https://ITBees.pl/fas or https://github.com/ITBeesPL
# Setup
## Environment setup
Depending on the environment you want the library to run in, configure dependency injection to return an implementation that will only send messages to recipients whose email address contains the following string : "+test" , or is in the list of email addresses to debug returned by the interface implementation :

```
    public interface IPlatformSettingsService
    {
        List<string> GetPlatformDebugEmails();
        Environment GetCurrentEnvironment();
    }

```

Dependecy injection setup :

```
    #if DEBUG
                services.AddTransient<IEmailSendingService, DevEnvironmentEmailSendingService>();
    #else     
                services.AddTransient<IEmailSendingService, EmailSendingService>();
    #endif
```

DevEnvironmentEmailSendingService class will send email messages to recipients meeting the above requirement, and send the other messages only to console.log

## Smtp client setup

Configure dependecy injection to inject for the ISmtpClient interface the SmtpClient class from MailKit.Net.Smtp - if you want to use it to communicate with the stmp server.

 ```
    services.AddTransient<ISmtpClient, SmtpClient>();
 ```
