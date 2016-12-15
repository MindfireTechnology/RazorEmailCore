using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RazorEmailCore
{
	public class MessageGenerationException : RazorEmailCoreException
	{
		public MessageGenerationException() { }
		public MessageGenerationException(string message) : base(message) { }
		public MessageGenerationException(string message, Exception inner) : base(message, inner) { }
	}
}
