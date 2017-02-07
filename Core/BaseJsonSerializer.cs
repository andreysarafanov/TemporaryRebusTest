using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Rebus.Extensions;
using Rebus.Messages;
using Rebus.Serialization;

namespace Core
{
	public abstract class BaseJsonSerializer : ISerializer
	{
		/// <summary>
		/// Proper content type when a message has been serialized with this serializer (or another compatible JSON serializer) and it uses the standard UTF8 encoding
		/// </summary>
		public const string JsonUtf8ContentType = "application/json;charset=utf-8";

		/// <summary>
		/// Contents type when the content is JSON
		/// </summary>
		public const string JsonContentType = "application/json";

		static readonly JsonSerializerSettings DefaultSettings = new JsonSerializerSettings
		{
		};

		static readonly Encoding DefaultEncoding = Encoding.UTF8;

		protected readonly JsonSerializerSettings Settings;
		readonly Encoding _encoding;

		public BaseJsonSerializer()
			: this(DefaultSettings, DefaultEncoding)
		{
		}

		internal BaseJsonSerializer(Encoding encoding)
			: this(DefaultSettings, encoding)
		{
		}

		internal BaseJsonSerializer(JsonSerializerSettings jsonSerializerSettings)
			: this(jsonSerializerSettings, DefaultEncoding)
		{
		}

		internal BaseJsonSerializer(JsonSerializerSettings jsonSerializerSettings, Encoding encoding)
		{
			Settings = jsonSerializerSettings;
			_encoding = encoding;
		}

		/// <summary>
		/// Serializes the given <see cref="Message"/> into a <see cref="TransportMessage"/>
		/// </summary>
		public async Task<TransportMessage> Serialize(Message message)
		{
			var jsonText = JsonConvert.SerializeObject(message.Body, Settings);
			var bytes = _encoding.GetBytes(jsonText);
			var headers = message.Headers.Clone();
			headers[Headers.ContentType] = $"{JsonContentType};charset={_encoding.HeaderName}";
			return new TransportMessage(headers, bytes);
		}

		/// <summary>
		/// Deserializes the given <see cref="TransportMessage"/> back into a <see cref="Message"/>
		/// </summary>
		public async Task<Message> Deserialize(TransportMessage transportMessage)
		{
			var contentType = transportMessage.Headers.GetValue(Headers.ContentType);

			if (contentType == JsonUtf8ContentType)
			{
				return GetMessage(transportMessage, _encoding);
			}

			if (contentType.StartsWith(JsonContentType))
			{
				var encoding = GetEncoding(contentType);
				return GetMessage(transportMessage, encoding);
			}

			throw new FormatException($"Unknown content type: '{contentType}' - must be '{JsonUtf8ContentType}' for the JSON serialier to work");
		}

		Encoding GetEncoding(string contentType)
		{
			var charset = contentType
				.Split(';')
				.Select(token => token.Split('='))
				.Where(tokens => tokens.Length == 2)
				.FirstOrDefault(tokens => tokens[0] == "charset");

			if (charset == null)
			{
				return _encoding;
			}

			var encodingName = charset[1];

			try
			{
				return Encoding.GetEncoding(encodingName);
			}
			catch (Exception exception)
			{
				throw new FormatException($"Could not turn charset '{encodingName}' into proper encoding!", exception);
			}
		}

		Message GetMessage(TransportMessage transportMessage, Encoding bodyEncoding)
		{
			var bodyString = bodyEncoding.GetString(transportMessage.Body);
			var bodyObject = Deserialize(bodyString);
			var headers = transportMessage.Headers.Clone();
			return new Message(headers, bodyObject);
		}

		protected abstract object Deserialize(string bodyString);
	}
}