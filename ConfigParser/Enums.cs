
namespace ConfigParser
{
	public enum TokenType
	{
		BlockIdentifier,
		BlockOpen,
		BlockClosed,
		AssignOperator,
		ValueString,
		ListDelimiter,
		EndOfStatement
	}

	public enum ScannerState
	{
		OutOfBlock,
		InBlockIdent,
		InComment,
		InEOL,
		InQuote,
		InQuoteEscape,
		InValue,
		OutOfValue,
		TryCommentStart,
		TryCommentStop
	}
}
