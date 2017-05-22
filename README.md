# RazorEmailCore
Use Razor syntax to write emails, compatible with .NET Core.

## Basic Usag e##

### Required Configuration ###
DotNet Core only allows reflection on projects that have the `preserveCompilationContext` enabled. This is a requirement
of the `RazorLight` library which this project uses for the default template generator. 

To enable this setting, open up your `project.json` file for your project (the executing project) and add the following:
```
"buildOptions": {
	"preserveCompilationContext": true
	// ...
},
```

### Base Path ###
The base path is configured via an Environment variable named `BaseTemplatePath`. This path is either relative to the executing 
project or can be an absolute path. In Visual Studio you can set an Environment variable by going into the project properties, 
Debug tab, and adding the value to the Environment variables section. 

Example: `BaseTemplatePath` => `RazorEmail`

### Folder Structure ###
`<BaseTemplatePath>\settings.json` contains the base configuration file (can be named something else `.json`)
`<BaseTemplatePath>\TemplateName\TemplateName.json` contains the template override settings file (optional)

`<BaseTemplatePath>\TemplateName\TemplateName.razor` contains a razor template to produce an HTML mail message content (optional)

`<BaseTemplatePath>\TemplateName\TemplateName.text` contains a razor template to produce a plain text mail message content (optional)

Either the `.razor` file or the `.text` file must exist. You can also supply both to support email messages with both content types. 

**Note: The name of the file is unimportant, there just needs to be only one `.json` file, `.razor` or `.text` file each!**

### Base Configuration ###
A `.json` file is required in `<BaseTemplatePath>` to serve as the master settings for all templates. The idea is that default settings are kept here, and templates will override them if needed. Settings like `subject` will usually be overridden, but settings like `server`, `username`, and `password` will usually be defined once.

 This configuration takes the format of the example below:
```json
{
	"from": "\"RazorEmailCore\" <donotreply@SomeDomain.com>",
	"subject": "New User Created",
	"cc": "",
	"bcc": "nate.zaugg@SomeDomain.com;joe@go.com",

	"server": "smtp://smtp.sendgrid.net:587",
	"username": "",
	"password": ""
}
```

As with any credentials in a project, we recommend not committing them to your source control.

### Template Configuration ###

Templates may override settings in the Base Configuration by adding their own `.json` file of the same format as the Base Configuration.

### Code ###

#### Simple Example (without error handling) ####
```
// Simple Example
var razoremail = new RazorEmail();
razoremail.CreateAndSendEmail("nate.zaugg@SomeDomain.com", "NewUserTemplate", 
	new { Name = "Nate Zaugg" });
Console.WriteLine("Message sent successfully!");
```

You may also generate the message and then send it later (example with error handling):

```
// Full Example with Separate Create and send email steps
try
{
	var emailBuilder = new RazorEmail();
	var email = emailBuilder.CreateEmail("NewUserTemplate", new { Name = "Nate Zaugg" });

	// Modify the email
	email.Sender = "\"Nate Zaugg\" <nate.zaugg@SomeDomain.com>";
	// ...

	// Send the email
	emailBuilder.SendEmail(email);
}
catch (RazorEmailCoreConfigurationException rece)
{
	Console.WriteLine($"Failed to generate message because of configuration error with RazorEmailCore! {rece}");
}
catch (MessageGenerationException mge)
{
	Console.WriteLine($"Failed to generate email message because of a problem with the razor template! {mge}");
}
catch (SmtpException smtpEx)
{
	Console.WriteLine($"Failed to send email message because of an SMTP error! {smtpEx}");
}
catch (Exception ex)
{
	Console.WriteLine($"Failed to generate or send email because of an unknown error {ex}");
}

Console.WriteLine("Message sent successfully!");

```

## Customizations ##

This library was designed to have its main modules replaced or extended based on the needs of the user. 
Below is a summary of the main interfaces.

### IMessageSettingsProvider ###
This class is used to generate the settings required to generate the message content and later send the message. 
It returns a `ConfigSettings` class that can be inherited and expanded. 

This class must provide:
 1. Either the `HtmlEmailTemplate` or the `PlainTextEmailTemplate` values
 2. The `Subject` for the email
 3. The `From` sender for the email
 4. The `Server` in URI format (e.g. `smtp://smtp.sendgrid.net:587`)
 5. Optional login information `Username` and `Password` for the SMTP server
 6. Optional `Cc`, `Bcc` values

**Note: The default option for this class is the built-in `DefaultMessageSettingsProvider` class**

### IMessageGenerator ###
This class is used to generate the content of the mail message from the template and the model data provided. 
It is given the template text and the model and returns the generated output.

**Note: The default option for this class is the built-in `RazorMessageGenerator` class which makes use of the `RazorLight` library**

###ISendEmailProvider###
This class sends the `Email` object based on the config settings collected by the `IMessageSettingsProvider` class. 

**Note: The default option for this class is the built-in `SmtpSendEmailProvider` class which is a very simple SMTP client
that currently only supports basic auth. There is no support for SSL or TSL.**

## Dependency Injection ##
There is a base interface for the `RazorEmail` class called `IRazorEmail` which can be helpful in dependency injection scenerios. 
The constructor of the `RazorEmail` class provides the following prototype: 

```
public RazorEmail(IMessageSettingsProvider settingsProvider = null, IMessageGenerator messageGenerator = null, ISendEmailProvider sendEmailProvider = null)
```

If you wished to replace just one of the default providers with a custom provider, you could do it like this:
```
var emailBuilder = new RazorEmail(sendEmailProvider: sendgridEmailProvider);
//...
```

## License ##
This project is available under the MIT license. 

Copyright (c)2016 Mindfire Technology

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files 
(the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, 
publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do 
so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF 
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE 
FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION 
WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

This software also depends on `RazorLight` project found at <https://github.com/toddams/RazorLight> 
for the default `RazorMessageGenerator` class and is licensed under the Apache 2 License.
