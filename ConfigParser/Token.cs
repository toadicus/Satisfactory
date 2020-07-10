using System;

namespace ConfigParser
{
	public class Token
	{
		public TokenType Type
		{
			get;
			private set;
		}

		public object Value
		{
			get;
			private set;
		}

		private Token() {}

		public Token(TokenType type, object value)
		{
			this.Type = type;
			this.Value = value;
		}

		public override string ToString()
		{
			return string.Format("[Token: Type={0}, Value='{1}']", Type, Value);
		}
	}
}

