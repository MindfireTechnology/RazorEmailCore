# RazorEmailCore
Use Razor syntax to write emails, compatible with .NET Core

API

var email = new RazorEmail(name: "Welcome", model: obj);

ALTERNATIVE:

var email = new RazorEmail(template: "razor here", model: obj)

IEmailSender interface to send the emails
IMessageGenerator interface for generation of the email messages


var razor = new RazorEmail(); // Defaults for message generators
var email = razor.CreateEmail(name, model);

RazorEmail.Config.EmailEmitter
RazorEmail.Config.MessageGenerator
RazorEmail.Config.SettingsProvider

Dependency Injection


1) SettingsProvider
  -- Get all settings, template, metainfo, etc.
2) Generate Message Text (in both plaintext and HTML) IMessageGenerator
3) Create Mail Message with the combination of Settings and mail messages (internal)
4) Send Email via ISendEmailProvider



