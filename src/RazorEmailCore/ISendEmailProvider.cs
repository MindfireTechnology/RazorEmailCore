using RazorEmailCore;
using System;
using System.Threading.Tasks;

public interface ISendEmailProvider
{
	bool SendMessage(Email message, ConfigSettings settings);
	Task<bool> SendMessageAsync(Email message, ConfigSettings settings);
}
