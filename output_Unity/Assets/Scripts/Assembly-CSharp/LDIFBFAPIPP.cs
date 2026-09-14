using System.IO;
using System.Text;

internal class LDIFBFAPIPP
{
	private delegate bool MLFKOCOHKKP(AHCMBOPFGAC ctx);

	private static readonly int[] fsm_return_table;

	private static readonly MLFKOCOHKKP[] fsm_handler_table;

	private bool allow_comments;

	private bool allow_single_quoted_strings;

	private bool end_of_input;

	private AHCMBOPFGAC fsm_context;

	private int input_buffer;

	private int input_char;

	private TextReader reader;

	private int state;

	private StringBuilder string_buffer;

	private string string_value;

	private int token;

	private int unichar;

	public bool AllowComments
	{
		get
		{
			return false;
		}
		set
		{
		}
	}

	public bool AllowSingleQuotedStrings
	{
		get
		{
			return false;
		}
		set
		{
		}
	}

	public bool EndOfInput => false;

	public int Token => 0;

	public string StringValue => null;

	static LDIFBFAPIPP()
	{
	}

	public LDIFBFAPIPP(TextReader reader)
	{
	}

	private static int HexValue(int digit)
	{
		return 0;
	}

	private static void PopulateFsmTables(out MLFKOCOHKKP[] fsm_handler_table, out int[] fsm_return_table)
	{
		fsm_handler_table = null;
		fsm_return_table = null;
	}

	private static char ProcessEscChar(int esc_char)
	{
		return '\0';
	}

	private static bool State1(AHCMBOPFGAC ctx)
	{
		return false;
	}

	private static bool State2(AHCMBOPFGAC ctx)
	{
		return false;
	}

	private static bool State3(AHCMBOPFGAC ctx)
	{
		return false;
	}

	private static bool State4(AHCMBOPFGAC ctx)
	{
		return false;
	}

	private static bool State5(AHCMBOPFGAC ctx)
	{
		return false;
	}

	private static bool State6(AHCMBOPFGAC ctx)
	{
		return false;
	}

	private static bool State7(AHCMBOPFGAC ctx)
	{
		return false;
	}

	private static bool State8(AHCMBOPFGAC ctx)
	{
		return false;
	}

	private static bool State9(AHCMBOPFGAC ctx)
	{
		return false;
	}

	private static bool State10(AHCMBOPFGAC ctx)
	{
		return false;
	}

	private static bool State11(AHCMBOPFGAC ctx)
	{
		return false;
	}

	private static bool State12(AHCMBOPFGAC ctx)
	{
		return false;
	}

	private static bool State13(AHCMBOPFGAC ctx)
	{
		return false;
	}

	private static bool State14(AHCMBOPFGAC ctx)
	{
		return false;
	}

	private static bool State15(AHCMBOPFGAC ctx)
	{
		return false;
	}

	private static bool State16(AHCMBOPFGAC ctx)
	{
		return false;
	}

	private static bool State17(AHCMBOPFGAC ctx)
	{
		return false;
	}

	private static bool State18(AHCMBOPFGAC ctx)
	{
		return false;
	}

	private static bool State19(AHCMBOPFGAC ctx)
	{
		return false;
	}

	private static bool State20(AHCMBOPFGAC ctx)
	{
		return false;
	}

	private static bool State21(AHCMBOPFGAC ctx)
	{
		return false;
	}

	private static bool State22(AHCMBOPFGAC ctx)
	{
		return false;
	}

	private static bool State23(AHCMBOPFGAC ctx)
	{
		return false;
	}

	private static bool State24(AHCMBOPFGAC ctx)
	{
		return false;
	}

	private static bool State25(AHCMBOPFGAC ctx)
	{
		return false;
	}

	private static bool State26(AHCMBOPFGAC ctx)
	{
		return false;
	}

	private static bool State27(AHCMBOPFGAC ctx)
	{
		return false;
	}

	private static bool State28(AHCMBOPFGAC ctx)
	{
		return false;
	}

	private bool GetChar()
	{
		return false;
	}

	private int NextChar()
	{
		return 0;
	}

	public bool NextToken()
	{
		return false;
	}

	private void UngetChar()
	{
	}
}
