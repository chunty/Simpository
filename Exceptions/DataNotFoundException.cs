namespace Simpository.Exceptions;

public class DataNotFoundException : Exception
{
	protected readonly string EntityType;
	protected readonly object? Key;
	protected readonly string[] KeyNames;
	protected readonly string BaseMessage = "{0} not found";

	public DataNotFoundException(object? key = null, string keyName = "Id", string entityType = "Object")
	{
		EntityType = entityType;
		Key = key;
		KeyNames = [keyName];
	}

	public DataNotFoundException(object[] keys, string[] keyNames, string entityType = "Object")
	{
		EntityType = entityType;
		Key = keys;
		KeyNames = keyNames;
	}

	public override string Message
	{
		get
		{
			if (Key is null)
				return string.Format(BaseMessage, EntityType);

			var keys = Key.GetType().IsArray
				? string.Join(", ", ((object[])Key).Select(o => o.ToString()))
				: Key.ToString();
			var keyNamesStr = string.Join(", ", KeyNames);
			return string.Format(BaseMessage + " with {1}: {2}", EntityType, keyNamesStr, keys);
		}
	}
}

public class DataNotFoundException<T> : DataNotFoundException where T : class
{
	public DataNotFoundException(object? key = null, string keyName = "Id")
		: base(key, keyName, typeof(T).Name) { }

	public DataNotFoundException(object[] keys, string[] keyNames)
		: base(keys, keyNames, typeof(T).Name) { }
}
