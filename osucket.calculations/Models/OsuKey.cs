namespace osucket.Calculations.Models
{
	public class OsuKey
	{
		public int CountKeyLeft { get; set; }
		public int CountKeyRight { get; set; }
		public int CountMouseLeft { get; set; }
		public int CountMouseRight { get; set; }
		
		public bool PressedKeyLeft { get; set; }
		public bool PressedKeyRight { get; set; }
		public bool PressedMouseLeft { get; set; }
		public bool PressedMouseRight { get; set; }

		public bool IsEnabled { get; set; }

	}
}