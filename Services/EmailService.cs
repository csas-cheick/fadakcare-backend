using System.Net;
using System.Net.Mail;

namespace backend.Services;

public class EmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;
    private readonly string _smtpServer;
    private readonly int _smtpPort;
    private readonly string _fromEmail;
    private readonly string _password;

    public EmailService(ILogger<EmailService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        
        // Charger les paramètres depuis la configuration
        _smtpServer = _configuration["EmailSettings:SmtpServer"] ?? "smtp.gmail.com";
        _smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
        _fromEmail = _configuration["EmailSettings:SmtpUser"] ?? "fadakcare@gmail.com";
        _password = _configuration["EmailSettings:SmtpPass"] ?? "sonnypygkrhapaxi";
    }

    public async Task SendConfirmationEmail(string toEmail, string username)
    {
        using var client = new SmtpClient(_smtpServer, _smtpPort)
        {
            Credentials = new NetworkCredential(_fromEmail, _password),
            EnableSsl = true,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            Timeout = 10000
        };

        var message = new MailMessage
        {
            From = new MailAddress(_fromEmail, "Équipe FadakCare"),
            Subject = "Confirmation de votre inscription",
            Body = BuildEmailBody(username),
            IsBodyHtml = true,
            Priority = MailPriority.Normal
        };

        message.To.Add(new MailAddress(toEmail));

        message.Headers.Add("X-Mailer", "FadakCare Mail Service");
        message.Headers.Add("X-Priority", "3");

        await client.SendMailAsync(message);
    }

    public async Task SendConfirmationEmailMedecinWithCredentials(string toEmail, string username, string email, string password)
    {
        using var client = new SmtpClient(_smtpServer, _smtpPort)
        {
            Credentials = new NetworkCredential(_fromEmail, _password),
            EnableSsl = true,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            Timeout = 10000
        };

        var message = new MailMessage
        {
            From = new MailAddress(_fromEmail, "Équipe FadakCare"),
            Subject = "Vos paramètres de connexion FadakCare",
            Body = BuildEmailBodyMedecinWithCredentials(username, email, password),
            IsBodyHtml = true,
            Priority = MailPriority.Normal
        };

        message.To.Add(new MailAddress(toEmail));

        message.Headers.Add("X-Mailer", "FadakCare Mail Service");
        message.Headers.Add("X-Priority", "3");

        await client.SendMailAsync(message);
    }

   private string BuildEmailBody(string username)
    {
        return @"<!DOCTYPE html>
            <html>
            <head>
                <meta charset='utf-8'>
                <style>
                    body {
                        font-family: Arial, sans-serif;
                        background-color: #f9f9f9;
                        color: #333333;
                        margin: 0;
                        padding: 0;
                    }
                    .container {
                        max-width: 600px;
                        margin: 20px auto;
                        background: #ffffff;
                        padding: 20px;
                        border-radius: 8px;
                        box-shadow: 0 2px 6px rgba(0,0,0,0.1);
                    }
                    h2 {
                        color: #0066cc;
                    }
                    p {
                        line-height: 1.6;
                    }
                    .button {
                        color: #ffffff !important;
                        background-color: #0066cc;
                        padding: 12px 20px;
                        text-decoration: none;
                        border-radius: 5px;
                        display: inline-block;
                        font-weight: bold;
                        margin-top: 15px;
                    }
                    .footer {
                        color: #888888;
                        font-size: 0.85em;
                        margin-top: 30px;
                        border-top: 1px solid #eeeeee;
                        padding-top: 10px;
                    }
                </style>
            </head>
            <body>
                <div class='container'>
                    <h2>Bienvenue sur FadakCare, " + username + @" 🎉</h2>
                    <p>Nous sommes ravis de vous compter parmi nos membres. Votre compte a été créé avec succès.</p>
                    
                    <p>
                        Vous pouvez dès maintenant accéder à votre espace personnel et profiter de nos services :
                    </p>
                    <ul>
                        <li>Consulter et gérer vos rendez-vous</li>
                        <li>Accéder à vos résultats de dépistage</li>
                        <li>Échanger avec votre médecin</li>
                        <li>Participer à vos téléconsultations en toute sécurité</li>
                    </ul>

                    <p style='text-align: center;'>
                        <a href='http://localhost:5173/login' class='button'>Accéder à mon compte</a>
                    </p>

                    <p>
                        Si vous n'êtes pas à l'origine de cette inscription, veuillez ignorer ce message.
                    </p>

                    <p class='footer'>
                        À très bientôt,<br>
                        L'équipe <strong>FadakCare</strong><br>
                        <em>Votre santé, notre priorité</em>
                    </p>
                </div>
            </body>
            </html>";
    }

    public async Task SendResetPasswordEmail(string toEmail, string resetCode)
    {
        using var client = new SmtpClient(_smtpServer, _smtpPort)
        {
            Credentials = new NetworkCredential(_fromEmail, _password),
            EnableSsl = true,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            Timeout = 10000
        };

        var message = new MailMessage
        {
            From = new MailAddress(_fromEmail, "Équipe FadakCare"),
            Subject = "Réinitialisation de votre mot de passe",
            Body = BuildResetPasswordEmailBody(resetCode),
            IsBodyHtml = true,
            Priority = MailPriority.Normal
        };

        message.To.Add(new MailAddress(toEmail));

        message.Headers.Add("X-Mailer", "FadakCare Mail Service");
        message.Headers.Add("X-Priority", "3");

        await client.SendMailAsync(message);
    }

    public async Task SendAccountBlockedNotification(string toEmail, string username, string userRole)
    {
        using var client = new SmtpClient(_smtpServer, _smtpPort)
        {
            Credentials = new NetworkCredential(_fromEmail, _password),
            EnableSsl = true,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            Timeout = 10000
        };

        var message = new MailMessage
        {
            From = new MailAddress(_fromEmail, "Équipe FadakCare"),
            Subject = "Notification de suspension de votre compte FadakCare",
            Body = BuildAccountBlockedEmailBody(username, userRole),
            IsBodyHtml = true,
            Priority = MailPriority.High
        };

        message.To.Add(new MailAddress(toEmail));

        message.Headers.Add("X-Mailer", "FadakCare Mail Service");
        message.Headers.Add("X-Priority", "1");

        await client.SendMailAsync(message);
    }

    public async Task SendAccountPendingValidationEmail(string toEmail, string username, string userRole)
    {
        using var client = new SmtpClient(_smtpServer, _smtpPort)
        {
            Credentials = new NetworkCredential(_fromEmail, _password),
            EnableSsl = true,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            Timeout = 10000
        };

        var message = new MailMessage
        {
            From = new MailAddress(_fromEmail, "Équipe FadakCare"),
            Subject = "Votre demande de création de compte est en cours d'examen",
            Body = BuildAccountPendingValidationEmailBody(username, userRole),
            IsBodyHtml = true,
            Priority = MailPriority.Normal
        };

        message.To.Add(new MailAddress(toEmail));

        message.Headers.Add("X-Mailer", "FadakCare Mail Service");
        message.Headers.Add("X-Priority", "3");

        await client.SendMailAsync(message);
    }

    public async Task SendAccountApprovedEmail(string toEmail, string username, string userRole)
    {
        using var client = new SmtpClient(_smtpServer, _smtpPort)
        {
            Credentials = new NetworkCredential(_fromEmail, _password),
            EnableSsl = true,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            Timeout = 10000
        };

        var message = new MailMessage
        {
            From = new MailAddress(_fromEmail, "Équipe FadakCare"),
            Subject = "🎉 Votre compte FadakCare a été approuvé !",
            Body = BuildAccountApprovedEmailBody(username, userRole),
            IsBodyHtml = true,
            Priority = MailPriority.Normal
        };

        message.To.Add(new MailAddress(toEmail));

        message.Headers.Add("X-Mailer", "FadakCare Mail Service");
        message.Headers.Add("X-Priority", "3");

        await client.SendMailAsync(message);
    }

    private string BuildResetPasswordEmailBody(string resetCode)
    {
        return @"<!DOCTYPE html>
            <html>
            <head>
                <meta charset='utf-8'>
                <style>
                    .button {
                        color: #ffffff;
                        background-color: #0066cc;
                        padding: 8px 16px;
                        text-decoration: none;
                        border-radius: 4px;
                        display: inline-block;
                        font-weight: bold;
                    }
                    .footer {
                        color: #666666;
                        font-size: 0.9em;
                        margin-top: 20px;
                    }
                </style>
            </head>
            <body>
                <h3 style='color:#0066cc;'>Demande de réinitialisation de mot de passe</h3>
                <p>Vous avez demandé la réinitialisation de votre mot de passe.</p>
                <p>Utilisez le code suivant pour continuer :</p>
                <h2 style='color:#ff6600;'>" + resetCode + @"</h2>
                <p>Si vous n'avez pas demandé cette réinitialisation, ignorez cet email.</p>
                <p class='footer'>
                    Cordialement,<br>L'équipe FadakCare
                </p>
            </body>
            </html>";
    }

    private string BuildEmailBodyMedecinWithCredentials(string username, string email, string password)
    {
        return @"<!DOCTYPE html>
            <html>
            <head>
                <meta charset='utf-8'>
                <style>
                    body {
                        font-family: Arial, sans-serif;
                        background-color: #f9f9f9;
                        color: #333333;
                        margin: 0;
                        padding: 0;
                    }
                    .container {
                        max-width: 600px;
                        margin: 20px auto;
                        background: #ffffff;
                        padding: 20px;
                        border-radius: 8px;
                        box-shadow: 0 2px 6px rgba(0,0,0,0.1);
                    }
                    h2 {
                        color: #0066cc;
                    }
                    p {
                        line-height: 1.6;
                    }
                    .credentials {
                        background-color: #f8f9fa;
                        border: 1px solid #dee2e6;
                        border-radius: 5px;
                        padding: 15px;
                        margin: 20px 0;
                        font-family: 'Courier New', monospace;
                    }
                    .button {
                        color: #ffffff !important;
                        background-color: #0066cc;
                        padding: 12px 20px;
                        text-decoration: none;
                        border-radius: 5px;
                        display: inline-block;
                        font-weight: bold;
                        margin-top: 15px;
                    }
                    .warning {
                        background-color: #fff3cd;
                        border: 1px solid #ffeaa7;
                        color: #856404;
                        padding: 10px;
                        border-radius: 5px;
                        margin: 15px 0;
                    }
                    .footer {
                        color: #888888;
                        font-size: 0.85em;
                        margin-top: 30px;
                        border-top: 1px solid #eeeeee;
                        padding-top: 10px;
                    }
                </style>
            </head>
            <body>
                <div class='container'>
                    <h2>Bienvenue sur FadakCare, Dr. " + username + @" 👨‍⚕️👩‍⚕️</h2>
                    <p>Votre compte médecin a été créé avec succès par l'administrateur de la plateforme.</p>
                    
                    <p>Voici vos paramètres de connexion :</p>
                    
                    <div class='credentials'>
                        <strong>Email :</strong> " + email + @"<br>
                        <strong>Mot de passe :</strong> " + password + @"
                    </div>
                    
                    <div class='warning'>
                        <strong>⚠️ Important :</strong> Pour votre sécurité, nous vous recommandons fortement de changer ce mot de passe lors de votre première connexion.
                    </div>
                    
                    <p>
                        Vous pouvez dès maintenant accéder à votre espace professionnel et bénéficier de nos services dédiés :
                    </p>
                    <ul>
                        <li>Gérer vos patients et leurs rendez-vous</li>
                        <li>Consulter et mettre à jour les dossiers médicaux</li>
                        <li>Donner des conseils et suivre vos patients à distance</li>
                        <li>Organiser et animer vos téléconsultations en toute sécurité</li>
                    </ul>

                    <p style='text-align: center;'>
                        <a href='http://localhost:5173/login' class='button'>Accéder à mon espace médecin</a>
                    </p>

                    <p>
                        Si vous n'êtes pas à l'origine de cette création de compte, veuillez contacter immédiatement l'administrateur.
                    </p>

                    <p class='footer'>
                        À très bientôt,<br>
                        L'équipe <strong>FadakCare</strong><br>
                        <em>Votre santé, notre priorité</em>
                    </p>
                </div>
            </body>
            </html>";
    }

    private string BuildAccountBlockedEmailBody(string username, string userRole)
    {
        string roleDisplayName = userRole switch
        {
            "doctor" => "médecin",
            "patient" => "patient",
            _ => "utilisateur"
        };

        string personalTitle = userRole == "doctor" ? $"Dr. {username}" : username;
        string emoji = userRole == "doctor" ? "👨‍⚕️👩‍⚕️" : "👤";

        return @"<!DOCTYPE html>
            <html>
            <head>
                <meta charset='utf-8'>
                <style>
                    body {
                        font-family: Arial, sans-serif;
                        background-color: #f9f9f9;
                        color: #333333;
                        margin: 0;
                        padding: 0;
                    }
                    .container {
                        max-width: 600px;
                        margin: 20px auto;
                        background: #ffffff;
                        padding: 20px;
                        border-radius: 8px;
                        box-shadow: 0 2px 6px rgba(0,0,0,0.1);
                        border-left: 5px solid #dc3545;
                    }
                    h2 {
                        color: #dc3545;
                        margin-bottom: 20px;
                    }
                    p {
                        line-height: 1.6;
                        margin-bottom: 15px;
                    }
                    .alert-box {
                        background-color: #f8d7da;
                        border: 1px solid #f5c6cb;
                        color: #721c24;
                        padding: 15px;
                        border-radius: 5px;
                        margin: 20px 0;
                        font-weight: bold;
                    }
                    .contact-info {
                        background-color: #e2e3e5;
                        border: 1px solid #d6d8db;
                        color: #383d41;
                        padding: 15px;
                        border-radius: 5px;
                        margin: 20px 0;
                    }
                    .footer {
                        color: #888888;
                        font-size: 0.85em;
                        margin-top: 30px;
                        border-top: 1px solid #eeeeee;
                        padding-top: 15px;
                    }
                    .reason-list {
                        background-color: #fff3cd;
                        border: 1px solid #ffeaa7;
                        color: #856404;
                        padding: 15px;
                        border-radius: 5px;
                        margin: 15px 0;
                    }
                </style>
            </head>
            <body>
                <div class='container'>
                    <h2>🚫 Suspension de votre compte FadakCare</h2>
                    
                    <p>Bonjour " + personalTitle + @" " + emoji + @",</p>
                    
                    <div class='alert-box'>
                        ⚠️ Votre compte " + roleDisplayName + @" FadakCare a été temporairement suspendu par l'équipe d'administration.
                    </div>
                    
                    <p>
                        Cette mesure a été prise pour garantir la sécurité et le bon fonctionnement de notre plateforme. 
                        Pendant cette période de suspension, vous n'aurez plus accès à votre espace personnel.
                    </p>
                    
                    <div class='reason-list'>
                        <strong>📋 Raisons possibles de suspension :</strong>
                        <ul>
                            <li>Non-respect des conditions d'utilisation</li>
                            <li>Comportement inapproprié sur la plateforme</li>
                            <li>Violation des règles de confidentialité</li>
                            <li>Activité suspecte détectée</li>
                            <li>Mesure préventive de sécurité</li>
                        </ul>
                    </div>
                    
                    <p>
                        <strong>Que faire maintenant ?</strong>
                    </p>
                    
                    <p>
                        Si vous pensez que cette suspension est une erreur ou si vous souhaitez contester cette décision, 
                        nous vous encourageons à contacter immédiatement notre équipe d'administration.
                    </p>
                    
                    <div class='contact-info'>
                        <strong>📞 Contactez-nous :</strong><br>
                        Email : fadakcare@gmail.com<br>
                    </div>
                    
                    <p>
                        Nous nous efforçons de traiter toutes les demandes de révision dans les plus brefs délais. 
                        Votre satisfaction et la sécurité de tous nos utilisateurs restent nos priorités absolues.
                    </p>
                    
                    <p class='footer'>
                        Cordialement,<br>
                        <strong>L'équipe d'administration FadakCare</strong><br>
                        <em>Votre santé, notre priorité - Votre sécurité, notre devoir</em>
                    </p>
                </div>
            </body>
            </html>";
    }

    private string BuildAccountPendingValidationEmailBody(string username, string userRole)
    {
        string roleDisplayName = userRole switch
        {
            "doctor" => "médecin",
            "patient" => "patient",
            _ => "utilisateur"
        };

        string personalTitle = userRole == "doctor" ? $"Dr. {username}" : username;
        string emoji = userRole == "doctor" ? "👨‍⚕️👩‍⚕️" : "👤";

        return @"<!DOCTYPE html>
            <html>
            <head>
                <meta charset='utf-8'>
                <style>
                    body {
                        font-family: Arial, sans-serif;
                        background-color: #f9f9f9;
                        color: #333333;
                        margin: 0;
                        padding: 0;
                    }
                    .container {
                        max-width: 600px;
                        margin: 20px auto;
                        background: #ffffff;
                        padding: 20px;
                        border-radius: 8px;
                        box-shadow: 0 2px 6px rgba(0,0,0,0.1);
                        border-left: 5px solid #ffc107;
                    }
                    h2 {
                        color: #ff6f00;
                        margin-bottom: 20px;
                    }
                    p {
                        line-height: 1.6;
                        margin-bottom: 15px;
                    }
                    .info-box {
                        background-color: #fff3cd;
                        border: 1px solid #ffeaa7;
                        color: #856404;
                        padding: 15px;
                        border-radius: 5px;
                        margin: 20px 0;
                        font-weight: bold;
                    }
                    .steps-box {
                        background-color: #e7f3ff;
                        border: 1px solid #bee5eb;
                        color: #0c5460;
                        padding: 15px;
                        border-radius: 5px;
                        margin: 20px 0;
                    }
                    .footer {
                        color: #888888;
                        font-size: 0.85em;
                        margin-top: 30px;
                        border-top: 1px solid #eeeeee;
                        padding-top: 15px;
                    }
                </style>
            </head>
            <body>
                <div class='container'>
                    <h2>⏳ Demande de création de compte en cours d'examen</h2>
                    
                    <p>Bonjour " + personalTitle + @" " + emoji + @",</p>
                    
                    <p>
                        Nous avons bien reçu votre demande de création de compte " + roleDisplayName + @" sur la plateforme FadakCare.
                        Merci pour votre confiance !
                    </p>
                    
                    <div class='info-box'>
                        🔍 Votre compte est actuellement en cours d'examen par notre équipe d'administration.
                    </div>
                    
                    <p>
                        <strong>Pourquoi cette étape de validation ?</strong>
                    </p>
                    
                    <p>
                        Pour garantir la sécurité et la qualité des services sur notre plateforme, nous examinons 
                        attentivement chaque nouvelle demande de création de compte. Cette démarche nous permet de :
                    </p>
                    
                    <div class='steps-box'>
                        <strong>✅ Processus de validation :</strong>
                        <ul>
                            <li>Vérification de l'authenticité des informations fournies</li>
                            <li>Validation des qualifications professionnelles (pour les médecins)</li>
                            <li>Contrôle de conformité avec nos standards de sécurité</li>
                            <li>Activation définitive de votre compte</li>
                        </ul>
                    </div>
                    
                    <p>
                        <strong>⏱️ Délai de traitement :</strong><br>
                        Notre équipe traite généralement les demandes sous 24 à 48 heures ouvrables. 
                        Vous recevrez une notification par email dès que votre compte sera approuvé.
                    </p>
                    
                    <p>
                        <strong>📧 Prochaines étapes :</strong><br>
                        Une fois votre compte validé et activé, vous recevrez un email de confirmation 
                        avec un lien direct pour accéder à votre espace personnel.
                    </p>
                    
                    <p>
                        Si vous avez des questions ou des préoccupations, n'hésitez pas à nous contacter 
                        à l'adresse : <strong>fadakcare@gmail.com</strong>
                    </p>
                    
                    <p class='footer'>
                        Merci pour votre patience,<br>
                        <strong>L'équipe d'administration FadakCare</strong><br>
                        <em>Votre santé, notre priorité</em>
                    </p>
                </div>
            </body>
            </html>";
    }

    private string BuildAccountApprovedEmailBody(string username, string userRole)
    {
        string roleDisplayName = userRole switch
        {
            "doctor" => "médecin",
            "patient" => "patient",
            _ => "utilisateur"
        };

        string personalTitle = userRole == "doctor" ? $"Dr. {username}" : username;
        string emoji = userRole == "doctor" ? "👨‍⚕️👩‍⚕️" : "🎉";
        string welcomeMessage = userRole == "doctor" 
            ? "Votre compte médecin a été approuvé avec succès !" 
            : "Votre compte patient a été approuvé avec succès !";

        string servicesContent = userRole == "doctor" 
            ? @"<li>Gérer vos patients et leurs rendez-vous</li>
                            <li>Consulter et mettre à jour les dossiers médicaux</li>
                            <li>Donner des conseils et suivre vos patients à distance</li>
                            <li>Organiser et animer vos téléconsultations en toute sécurité</li>
                            <li>Accéder aux outils de dépistage et de diagnostic</li>"
            : @"<li>Consulter et gérer vos rendez-vous</li>
                            <li>Accéder à vos résultats de dépistage</li>
                            <li>Échanger en toute sécurité avec votre médecin</li>
                            <li>Participer à vos téléconsultations</li>
                            <li>Suivre votre historique médical</li>";

        return @"<!DOCTYPE html>
            <html>
            <head>
                <meta charset='utf-8'>
                <style>
                    body {
                        font-family: Arial, sans-serif;
                        background-color: #f9f9f9;
                        color: #333333;
                        margin: 0;
                        padding: 0;
                    }
                    .container {
                        max-width: 600px;
                        margin: 20px auto;
                        background: #ffffff;
                        padding: 20px;
                        border-radius: 8px;
                        box-shadow: 0 2px 6px rgba(0,0,0,0.1);
                        border-left: 5px solid #28a745;
                    }
                    h2 {
                        color: #28a745;
                        margin-bottom: 20px;
                    }
                    p {
                        line-height: 1.6;
                        margin-bottom: 15px;
                    }
                    .success-box {
                        background-color: #d4edda;
                        border: 1px solid #c3e6cb;
                        color: #155724;
                        padding: 15px;
                        border-radius: 5px;
                        margin: 20px 0;
                        font-weight: bold;
                        text-align: center;
                    }
                    .features-box {
                        background-color: #e7f3ff;
                        border: 1px solid #bee5eb;
                        color: #0c5460;
                        padding: 15px;
                        border-radius: 5px;
                        margin: 20px 0;
                    }
                    .button {
                        color: #ffffff !important;
                        background-color: #28a745;
                        padding: 12px 20px;
                        text-decoration: none;
                        border-radius: 5px;
                        display: inline-block;
                        font-weight: bold;
                        margin-top: 15px;
                    }
                    .footer {
                        color: #888888;
                        font-size: 0.85em;
                        margin-top: 30px;
                        border-top: 1px solid #eeeeee;
                        padding-top: 15px;
                    }
                </style>
            </head>
            <body>
                <div class='container'>
                    <h2>🎉 Félicitations ! Votre compte a été approuvé</h2>
                    
                    <p>Bonjour " + personalTitle + @" " + emoji + @",</p>
                    
                    <div class='success-box'>
                        ✅ " + welcomeMessage + @"
                    </div>
                    
                    <p>
                        Excellente nouvelle ! Après examen attentif de votre dossier, notre équipe d'administration 
                        a validé votre demande de création de compte " + roleDisplayName + @".
                    </p>
                    
                    <p>
                        <strong>🔓 Votre compte est maintenant actif et vous pouvez y accéder immédiatement.</strong>
                    </p>
                    
                    <div class='features-box'>
                        <strong>🌟 Services disponibles :</strong>
                        <ul>
                            " + servicesContent + @"
                        </ul>
                    </div>
                    
                    <p>
                        <strong>🚀 Prêt à commencer ?</strong><br>
                        Cliquez sur le bouton ci-dessous pour accéder à votre espace personnel et découvrir 
                        tous les services FadakCare.
                    </p>
                    
                    <p style='text-align: center;'>
                        <a href='http://localhost:5173/login' class='button'>Accéder à mon compte</a>
                    </p>
                    
                    <p>
                        <strong>💡 Conseil :</strong><br>
                        Nous vous recommandons de compléter votre profil dès votre première connexion 
                        pour bénéficier pleinement de nos services.
                    </p>
                    
                    <p>
                        Si vous avez des questions ou avez besoin d'assistance, notre équipe de support 
                        est à votre disposition à l'adresse : <strong>fadakcare@gmail.com</strong>
                    </p>
                    
                    <p class='footer'>
                        Bienvenue dans la famille FadakCare !<br>
                        <strong>L'équipe FadakCare</strong><br>
                        <em>Votre santé, notre priorité</em>
                    </p>
                </div>
            </body>
            </html>";
    }


}