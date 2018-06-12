using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace RazorEmailCore
{
	[DataContract]
	public class ConfigSettings
	{
		[DataMember(Name ="from")]
		public string From { get; set; }
		[DataMember(Name = "subject")]
		public string Subject { get; set; }
		[DataMember(Name = "cc")]
		public string Cc { get; set; }
		[DataMember(Name = "bcc")]
		public string Bcc { get; set; }

		[DataMember(Name = "server")]
		public string Server { get; set; }
		[DataMember(Name = "username")]
		public string Username { get; set; }
		[DataMember(Name = "password")]
		public string Password { get; set; }

		/// <summary>
		/// Replaces any public string properties in the target with defined properties in the source
		/// </summary>
		/// <param name="source"></param>
		/// <param name="target"></param>
		public static void ReplacePublicStringProperties(ConfigSettings source, ConfigSettings target)
		{
			// loop over all public properties in ConfigSettings
			foreach(PropertyInfo property in typeof(ConfigSettings).GetProperties())
			{
				// try to cast the property as a string
				string sourceValue = property.GetValue(source) as string;
			
				// skip if the cast didn't work (null)
				// or the string is null itself
				if (string.IsNullOrWhiteSpace(sourceValue))
					continue;

				// replace the target's value with the source value
				property.SetValue(target, sourceValue);
			}
		}
	}
}
