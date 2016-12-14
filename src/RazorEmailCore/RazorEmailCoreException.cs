using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RazorEmailCore
{
	public class RazorEmailCoreException : Exception
	{
		public RazorEmailCoreException() { }
		public RazorEmailCoreException(string message) : base(message) { }
		public RazorEmailCoreException(string message, Exception inner) : base(message, inner) { }
	}
}
