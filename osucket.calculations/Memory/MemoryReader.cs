using OsuMemoryDataProvider;

namespace osucket.Calculations.Memory
{
	internal class MemoryReader
	{
		public StructuredOsuMemoryReader StructuredOsuMemoryReader { get; set; }

		public T ReadProperty<T>(object readObj, string propName, T defaultValue = default) where T : struct
		{
			if (StructuredOsuMemoryReader.TryReadProperty(readObj, propName, out object readResult))
				return (T) readResult;

			return defaultValue;
		}

		public T ReadClassProperty<T>(object readObj, string propName, T defaultValue = default) where T : class
		{
			if (StructuredOsuMemoryReader.TryReadProperty(readObj, propName, out object readResult))
				return (T) readResult;

			return defaultValue;
		}

		public bool ReadBool(object readObj, string propName)
		{
			if (StructuredOsuMemoryReader.TryReadProperty(readObj, propName, out object readResult)) return (bool) readResult;
			return false;
		}

		public int ReadInt(object readObj, string propName)
		{
			return ReadProperty(readObj, propName, -5);
		}

		public ushort ReadUShort(object readObj, string propName)
		{
			return ReadProperty<ushort>(readObj, propName);
		}

		public short ReadShort(object readObj, string propName)
		{
			return ReadProperty<short>(readObj, propName);
		}

		public float ReadFloat(object readObj, string propName)
		{
			return ReadProperty(readObj, propName, -5f);
		}

		public string ReadString(object readObj, string propName)
		{
			return ReadClassProperty(readObj, propName, default(string));
		}
	}
}