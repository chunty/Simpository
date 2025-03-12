namespace Simpository.Exceptions;

[ExcludeFromCodeCoverage]
public class DataNotFoundException(object? key = null, string keyName = "Id", string entityType = "Object") : Exception
{
	protected readonly string EntityType = entityType;
	protected readonly object? Key = key;
	protected readonly string BaseMessage = "{0} not found";

	public override string Message
	{
		get
		{
			if (Key is null)
			{
				return string.Format(BaseMessage, EntityType);
			}

			var keys = Key.GetType().IsArray ? string.Join(", ", ((object[])Key).Select(o => o.ToString())) : Key.ToString();
			return string.Format(BaseMessage + " with key '{2}' using: {1}", EntityType, keys, keyName);
		}
	}
}

[ExcludeFromCodeCoverage]
public class DataNotFoundException<T>(object? key = null, string keyName = "Id")
	: DataNotFoundException(key, keyName, typeof(T).Name) where T : class;