using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

public class MIEJKNMBFKN
{
	private sealed class JONABMGBDDB<T>
	{
		public global::IHFLCLDOJNO<T> exporter;

		internal void _003CRegisterExporter_003Eb__0(object obj, IANMOKCILIA writer)
		{
		}
	}

	private sealed class HHBGCOLDHNI<TJson, TValue>
	{
		public global::FPKHHMOPJKA<TJson, TValue> importer;

		internal object _003CRegisterImporter_003Eb__0(object input)
		{
			return null;
		}
	}

	private static readonly int max_nesting_depth;

	private static readonly IFormatProvider datetime_format;

	private static readonly IDictionary<Type, NKMDFKDEJCP> base_exporters_table;

	private static readonly IDictionary<Type, NKMDFKDEJCP> custom_exporters_table;

	private static readonly IDictionary<Type, IDictionary<Type, HEAMAOMBDJI>> base_importers_table;

	private static readonly IDictionary<Type, IDictionary<Type, HEAMAOMBDJI>> custom_importers_table;

	private static readonly IDictionary<Type, BPCLBPPBGIP> array_metadata;

	private static readonly object array_metadata_lock;

	private static readonly IDictionary<Type, IDictionary<Type, MethodInfo>> conv_ops;

	private static readonly object conv_ops_lock;

	private static readonly IDictionary<Type, JPMPJNEKBKA> object_metadata;

	private static readonly object object_metadata_lock;

	private static readonly IDictionary<Type, IList<NGJPMNPNEFO>> type_properties;

	private static readonly object type_properties_lock;

	private static readonly IANMOKCILIA static_writer;

	private static readonly object static_writer_lock;

	static MIEJKNMBFKN()
	{
	}

	private static void AddArrayMetadata(Type type)
	{
	}

	private static void AddObjectMetadata(Type type)
	{
	}

	private static void AddTypeProperties(Type type)
	{
	}

	private static MethodInfo GetConvOp(Type t1, Type t2)
	{
		return null;
	}

	private static object ReadValue(Type inst_type, KMPCHFOJFKI reader)
	{
		return null;
	}

	private static EEKMCOHPNKH ReadValue(ENDNKACEELM factory, KMPCHFOJFKI reader)
	{
		return null;
	}

	private static void ReadSkip(KMPCHFOJFKI reader)
	{
	}

	private static void RegisterBaseExporters()
	{
	}

	private static void RegisterBaseImporters()
	{
	}

	private static void RegisterImporter(IDictionary<Type, IDictionary<Type, HEAMAOMBDJI>> table, Type json_type, Type value_type, HEAMAOMBDJI importer)
	{
	}

	private static void WriteValue(object obj, IANMOKCILIA writer, bool writer_is_private, int depth)
	{
	}

	public static string ToJson(object obj)
	{
		return null;
	}

	public static void ToJson(object obj, IANMOKCILIA writer)
	{
	}

	public static DPDFFKDDFOG ToObject(KMPCHFOJFKI reader)
	{
		return null;
	}

	public static DPDFFKDDFOG ToObject(TextReader reader)
	{
		return null;
	}

	public static DPDFFKDDFOG ToObject(string json)
	{
		return null;
	}

	public static T ToObject<T>(KMPCHFOJFKI reader)
	{
		return default(T);
	}

	public static T ToObject<T>(TextReader reader)
	{
		return default(T);
	}

	public static T ToObject<T>(string json)
	{
		return default(T);
	}

	public static object ToObject(string json, Type ConvertType)
	{
		return null;
	}

	public static EEKMCOHPNKH ToWrapper(ENDNKACEELM factory, KMPCHFOJFKI reader)
	{
		return null;
	}

	public static EEKMCOHPNKH ToWrapper(ENDNKACEELM factory, string json)
	{
		return null;
	}

	public static void RegisterExporter<T>(global::IHFLCLDOJNO<T> exporter)
	{
	}

	public static void RegisterImporter<TJson, TValue>(global::FPKHHMOPJKA<TJson, TValue> importer)
	{
	}

	public static void UnregisterExporters()
	{
	}

	public static void UnregisterImporters()
	{
	}
}
