using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace ConfigParser
{
	public class Scanner
	{
		public const string ReservedChars = "{};=,\r\n\"/*\t ";

		internal const char BlockOpenChar = '{';
		internal const char BlockCloseChar = '}';
		internal const char EndofStatementChar = ';';
		internal const char EndofLineCRChar = '\r';
		internal const char EndofLineLFChar = '\n';
		internal const char CommentOpenChar = '/';
		internal const char CommentStopChar = '*';

		internal const char AssignOpChar = '=';
		internal const char ListDelimChar = ',';
		internal const char StringQuoteChar = '"';
		internal const char QuoteEscapeChar = '\\';

		private string inputString;
		private int inputLength;
		private int pointer;
		private ScannerState state;

		private UInt16 commentDepth;
		private UInt16 blockDepth;

		private Scanner()
		{
			this.pointer = 0;
			this.commentDepth = 0;
			this.blockDepth = 0;
			this.state = ScannerState.OutOfBlock;
		}

		public Scanner(string input) : this()
		{
			this.inputString = input;
			this.inputLength = input.Length;
		}

		public Token GetNextToken()
		{
			char c;

			StringBuilder tokenBuilder = new StringBuilder();

			while (this.pointer < this.inputLength)
			{
				c = this.inputString[this.pointer];

				switch (state)
				{
					case ScannerState.OutOfBlock:
						// When we are out of a block, the only valid token is a block identifier.
						// Block identifiers must start with a letter.
						switch (c) {

							case EndofLineCRChar:
							case EndofLineLFChar:
								// tokenBuilder.Append(c);
								this.pointer++;
								// this.state = ScannerState.InEOL;
								break;
							default:
								if (Char.IsLetter(c)) {
									this.state = ScannerState.InBlockIdent;
								}
								else {
									throw new UnexpectedCharacterException(this.state, this.pointer, c, "Any letter");
								}
								break;
						}
						break;
					case ScannerState.OutOfValue:
						switch (c)
						{
							case ' ':
							case '\t':
								this.pointer++;
								break;
							case CommentOpenChar:
								this.pointer++;
								this.state = ScannerState.TryCommentStart;
								break;
							case BlockOpenChar:
								this.blockDepth++;
								this.pointer++;
								return new Token(TokenType.BlockOpen, c);
							case BlockCloseChar:
								this.pointer++;
								this.blockDepth--;
								if (this.blockDepth == 0) {
									this.state = ScannerState.OutOfBlock;
                                }
								return new Token(TokenType.BlockClosed, c);
							case AssignOpChar:
								this.pointer++;
								return new Token(TokenType.AssignOperator, c);
							case ListDelimChar:
								this.pointer++;
								return new Token(TokenType.ListDelimiter, c);
							case EndofStatementChar:
								this.pointer++;
								return new Token(TokenType.EndOfStatement, c);
							case EndofLineCRChar:
							case EndofLineLFChar:
								tokenBuilder.Append(c);
								this.pointer++;
								this.state = ScannerState.InEOL;
								break;
							case StringQuoteChar:
								this.pointer++;
								this.state = ScannerState.InQuote;
								break;
							default:
								if (
									char.IsLetterOrDigit(c) ||
									c == System.Globalization
										.CultureInfo.CurrentCulture
										.NumberFormat.NumberDecimalSeparator[0]
								)
								{
									tokenBuilder.Append(c);
									this.pointer++;
									this.state = ScannerState.InValue;
								}
								else
								{
									this.pointer++;
								}
								break;
						}
						break;
					case ScannerState.InBlockIdent:
						if (ReservedChars.IndexOf(c) != -1) {
							if (c == CommentOpenChar) {
								this.state = ScannerState.TryCommentStart;
								this.pointer++;
								break;
							}
							this.state = ScannerState.OutOfValue;
							return new Token(TokenType.BlockIdentifier, tokenBuilder.ToString());
						}
						else {
							tokenBuilder.Append(c);
							this.pointer++;
						}
						break;
					case ScannerState.InValue:
						if (ReservedChars.IndexOf(c) != -1)
						{
							if (c == CommentOpenChar)
							{
								this.state = ScannerState.TryCommentStart;
								this.pointer++;
								break;
							}
							this.state = ScannerState.OutOfValue;
							return new Token(TokenType.ValueString, tokenBuilder.ToString());
						}
						else
						{
							tokenBuilder.Append(c);
							this.pointer++;
						}
						break;
					case ScannerState.InEOL:
						switch (c)
						{
							case EndofLineCRChar:
							case EndofLineLFChar:
								tokenBuilder.Append(c);
								this.pointer++;
								break;
							default:
								this.state = ScannerState.OutOfValue;
								return new Token(TokenType.EndOfStatement, tokenBuilder.ToString());
						}
						break;
					case ScannerState.InQuote:
						switch (c)
						{
							case StringQuoteChar:
								this.pointer++;
								this.state = ScannerState.OutOfValue;
								return new Token(TokenType.ValueString, tokenBuilder.ToString());
							case QuoteEscapeChar:
								this.pointer++;
								this.state = ScannerState.InQuoteEscape;
								break;
							default:
								tokenBuilder.Append(c);
								this.pointer++;
								break;
						}
						break;
					case ScannerState.InQuoteEscape:
						this.pointer++;
						tokenBuilder.Append(c);
						this.state = ScannerState.InQuote;
						break;
					case ScannerState.InComment:
						switch (c)
						{
							case EndofLineCRChar:
							case EndofLineLFChar:
								if (this.commentDepth < 1)
								{
									this.state = ScannerState.OutOfValue;
								}
								this.pointer++;
								break;
							case CommentOpenChar:
								this.pointer++;
								this.state = ScannerState.TryCommentStart;
								break;
							case CommentStopChar:
								this.pointer++;
								this.state = ScannerState.TryCommentStop;
								break;
							default:
								this.pointer++;
								break;
						}

						if (this.pointer == this.inputString.Length)
						{
							throw new Exception("Unexpected end of input during unterminated block comment.");
						}

						break;
					case ScannerState.TryCommentStart:
						switch (c)
						{
							case CommentStopChar:
							case CommentOpenChar:
								if (c == CommentStopChar)
								{
									this.commentDepth++;
								}
								this.pointer++;
								this.state = ScannerState.InComment;

								break;
							default:
								Console.Error.WriteLine("/ Operator not yet implemented, treating single '/' as text.");
								tokenBuilder.Append(CommentOpenChar);
								this.state = ScannerState.InValue;
								break;
						}
						break;
					case ScannerState.TryCommentStop:
						switch (c)
						{
							case CommentOpenChar:
								this.pointer++;
								this.commentDepth--;
								if (this.commentDepth == 0)
								{
									this.state = ScannerState.OutOfValue;
								}
								break;
							default:
								this.state = ScannerState.InComment;
								break;
						}
						break;
				}
			}

			return null;
		}
	}

	public class UnexpectedTokenException : Exception
	{
		public TokenType[] expectedTypes
		{
			get;
			protected set;
		}

		public Token foundToken
		{
			get;
			protected set;
		}

		private UnexpectedTokenException() {}

		public UnexpectedTokenException(Token found, params TokenType[] expected)
			: base(string.Format("Found token type {0} at '{1}', expected {2}{3}",
				Enum.GetName(typeof(TokenType), found.Type),
				found.Value,
				expected.Length > 1 ? "one of " : "",
				string.Join(", ", expected.Select(t => Enum.GetName(typeof(TokenType), t)).ToArray())
			))
		{
			this.expectedTypes = expected;
			this.foundToken = found;
		}
	}

	public class UnexpectedCharacterException : Exception
	{
		public ScannerState ScannerState
		{
			get;
			protected set;
		}

		public char FoundChar
		{
			get;
			protected set;
		}

		public string Expected;

		public int Position
		{
			get;
			protected set;
		}

		private UnexpectedCharacterException() {}

		public UnexpectedCharacterException(ScannerState state, int position, char found, string expected)
			: base(string.Format("Unexpected character in state '{0}' at position {1}: found '{2}', expected {3}",
				state,
				position,
				found,
				expected
			))
		{
			this.ScannerState = state;
			this.FoundChar = found;
			this.Position = position;
			this.Expected = expected;
		}
	}
}

