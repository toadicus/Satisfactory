using System;
using System.Collections.Generic;

namespace ConfigParser
{
	public class Parser
	{
		#if DEBUG
		private List<Token> tokens;
		#endif

		private Scanner scanner;

		private List<string> valueList;

		#if DEBUG
		public IList<Token> Tokens
		{
			get
			{
				return this.tokens.AsReadOnly();
			}
		}
		#endif

		public bool SquadCompat
		{
			get;
			private set;
		}

		private Token GetNextToken()
		{
			Token token = this.scanner.GetNextToken();

			#if DEBUG
			this.tokens.Add(token);
			#endif

			return token;
		}

		private Parser()
		{
			#if DEBUG
			this.tokens = new List<Token>();
			#endif

			this.valueList = new List<string>();
		}

		private Parser(bool squadCompat) : this()
		{
			this.SquadCompat = squadCompat;
		}

		public Parser(Scanner scanner, bool squadCompat) : this(squadCompat)
		{
			this.scanner = scanner;
		}

		public Parser(Scanner scanner) : this(scanner, false) {}

		public List<NodeDefinition> Parse()
		{
			List<NodeDefinition> nodes = new List<NodeDefinition>();

			Token token = this.GetNextToken();

			while (token != null) {
				switch (token.Type) {
					case TokenType.BlockIdentifier:
						nodes.Add(this.ParseBlockDefinition(token.Value.ToString()));
						break;
					default:
						return nodes;
				}
				token = this.GetNextToken();
			}

			return nodes;
		}

		private void ParseBlockContents(NodeDefinition node)
		{
			Token token = this.GetNextToken();

			do
			{
				switch (token.Type)
				{
					case TokenType.ValueString:
						this.ParseStatement(node, (string)token.Value);
						break;
					case TokenType.BlockClosed:
						return;
					case TokenType.EndOfStatement:
						break;
					default:
						throw new UnexpectedTokenException(token, TokenType.ValueString, TokenType.BlockClosed, TokenType.EndOfStatement);
				}

				token = this.GetNextToken();

			} while (token != null && token.Type != TokenType.BlockClosed);
		}

		private void ParseStatement(NodeDefinition node, string name)
		{
			Token token = this.GetNextToken();

			switch (token.Type)
			{
				case TokenType.AssignOperator:
					this.ParseAssignment(node, name);
					break;
				default:
					node.AddNode(this.ParseBlockDefinition(name));
					break;
			}
		}

		private void ParseAssignment(NodeDefinition node, string name)
		{
			Token token = this.GetNextToken();

			bool afterFirst = false;
			bool afterDelim = false;

			this.valueList.Clear();

			do
			{
				switch (token.Type)
				{
					case TokenType.ValueString:
						if (afterFirst && !afterDelim && !this.SquadCompat)
						{
							throw new UnexpectedTokenException(token, TokenType.EndOfStatement, TokenType.ListDelimiter);
						}
						this.valueList.Add((string)token.Value);
						afterFirst = true;
						afterDelim = false;
						break;
					case TokenType.ListDelimiter:
						afterDelim = true;
						break;
					default:
						throw new UnexpectedTokenException(token, TokenType.ValueString, TokenType.ListDelimiter);
				}

				token = this.GetNextToken();

			} while (token.Type != TokenType.EndOfStatement);

			if (this.SquadCompat)
			{
				node.AddValue(name, string.Join(" ", this.valueList.ToArray()));
			}
			else
			{
				foreach (string value in this.valueList)
				{
					node.AddValue(name, value);
				}
			}
		}

		private NodeDefinition ParseBlockDefinition(string name)
		{
			Token token;

			do
			{
				token = this.GetNextToken();

				switch (token.Type)
				{
					case TokenType.EndOfStatement:
					case TokenType.BlockOpen:
						break;
					default:
						throw new UnexpectedTokenException(token, TokenType.EndOfStatement, TokenType.BlockOpen, TokenType.AssignOperator);
				}
			} while (token.Type != TokenType.BlockOpen);

			NodeDefinition node = new NodeDefinition(name);

			this.ParseBlockContents(node);

			return node;
		}
	}
}

