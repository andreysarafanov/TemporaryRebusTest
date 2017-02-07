using System;
using System.Text;
using Newtonsoft.Json;

namespace Core
{
	public class TypedJsonSerializer<T> : BaseJsonSerializer
	{
		public TypedJsonSerializer()
		{
		}

		public TypedJsonSerializer(Encoding encoding) : base(encoding)
		{
		}

		public TypedJsonSerializer(JsonSerializerSettings jsonSerializerSettings) : base(jsonSerializerSettings)
		{
		}

		public TypedJsonSerializer(JsonSerializerSettings jsonSerializerSettings, Encoding encoding) : base(jsonSerializerSettings, encoding)
		{
		}

		protected override object Deserialize(string bodyString)
		{
			try
			{
				return JsonConvert.DeserializeObject<T>(bodyString, Settings);
			}
			catch (Exception exception)
			{
				throw new FormatException($"Could not deserialize JSON text: '{bodyString}'", exception);
			}
		}
	}
}