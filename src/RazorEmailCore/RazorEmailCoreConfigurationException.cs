using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RazorEmailCore
{
	public class RazorEmailCoreConfigurationException : RazorEmailCoreException
	{
		public RazorEmailCoreConfigurationException() { }
		public RazorEmailCoreConfigurationException(string message) : base(message) { }
		public RazorEmailCoreConfigurationException(string message, Exception inner) : base(message, inner) { }
	}
}
