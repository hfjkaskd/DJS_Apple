using System.Collections;
using System.Collections.Specialized;

public interface EEKMCOHPNKH : IList, ICollection, IEnumerable, IOrderedDictionary, IDictionary
{
	bool IsArray { get; }

	bool IsBoolean { get; }

	bool IsDouble { get; }

	bool IsInt { get; }

	bool IsLong { get; }

	bool IsObject { get; }

	bool IsString { get; }

	bool GetBoolean();

	double GetDouble();

	int GetInt();

	FNENALICLCK GetJsonType();

	long GetLong();

	string GetString();

	void SetBoolean(bool val);

	void SetDouble(double val);

	void SetInt(int val);

	void SetJsonType(FNENALICLCK type);

	void SetLong(long val);

	void SetString(string val);

	string ToJson();

	void ToJson(IANMOKCILIA writer);
}
