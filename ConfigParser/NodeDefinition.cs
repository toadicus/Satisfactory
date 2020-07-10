using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConfigParser;

namespace ConfigParser {
	public class NodeDefinition {
		public string NodeName {
			get;
			private set;
		}

		public IList<NodeDefinition> Nodes {
			get => this.nodes.AsReadOnly();
        }

		private Dictionary<string, List<NodeDefinition>> indexNodesByName;

		private Dictionary<string, List<string>> values;
		private List<NodeDefinition> nodes;

		private NodeDefinition()
		{
			this.values = new Dictionary<string, List<string>>();
			this.nodes = new List<NodeDefinition>();

			this.indexNodesByName = new Dictionary<string, List<NodeDefinition>>();
		}

		public NodeDefinition(string nodeName) : this()
		{
			this.NodeName = nodeName;
		}

		public string[] GetSubNodeNames() {
			return indexNodesByName.Keys.ToArray();
		}

		public int AddValue(string key, string value)
		{
			if (!this.values.ContainsKey(key))
			{
				this.values[key] = new List<string>();
			}

			this.values[key].Add(value);

			return this.values[key].Count;
		}

		public string GetValue(string key)
		{
			return this.GetValues(key)[0];
		}

		public T GetValue<T>(string key, T defaultValue) where T : IConvertible {
			try {
				return this.GetValue<T>(key);
			}
			catch (KeyNotFoundException) {
				return defaultValue;
			}
			catch (FormatException) {
				return defaultValue;
			}
		}

		public T GetValue<T>(string key) where T : IConvertible {
			return (T)(this.GetValue(key) as IConvertible).ToType(typeof(T), null);
        }

		public List<string> GetValues(string key)
		{
			return this.values[key];
		}

		public bool HasValue(string key)
		{
			return this.values.ContainsKey(key);
		}

		public int SetValue(string key, string value)
		{
			if (this.values.ContainsKey(key) && this.values[key].Count > 0)
			{
				this.values[key][0] = value;
				return this.values.Count;
			}
			else
			{
				return this.AddValue(key, value);
			}
		}

		public int SetValue(string key, string value, int index)
		{
			if (this.values.ContainsKey(key) && this.values[key].Count > index)
			{
				this.values[key][index] = value;
				return this.values.Count;
			}

			return 0;
		}

		public void AddNode(NodeDefinition node)
		{
			if (!this.indexNodesByName.ContainsKey(node.NodeName)) {
				this.indexNodesByName[node.NodeName] = new List<NodeDefinition>();
            }

			this.nodes.Add(node);
			this.indexNodesByName[node.NodeName].Add(node);
		}

		public IList<NodeDefinition> GetNodesByName(string nodeName)
		{
			if (this.indexNodesByName.ContainsKey(nodeName) && this.indexNodesByName[nodeName].Count > 0) {
				return this.indexNodesByName[nodeName].AsReadOnly();
			}
			return null;
		}

		public NodeDefinition GetFirstNodeByName(string nodeName) {
			if (this.indexNodesByName.ContainsKey(nodeName) && this.indexNodesByName[nodeName].Count > 0) {
				return this.indexNodesByName[nodeName][0];
            }

			return null;
        }

		public override string ToString()
		{
			return this.ToString(0);
		}

		public string ToString(int indent)
		{
			StringBuilder sb = new StringBuilder();

			for (int i = 0; i < indent; i++)
			{
				sb.Append('\t');
			}

			string idnt = sb.ToString();

			sb.Length = 0;

			sb.AppendFormat("{0}{1}", idnt, this.NodeName);
			sb.AppendLine();

			sb.AppendFormat("{0}{{", idnt);
			sb.AppendLine();

			sb.Append(this.contentsString(indent + 1));

			sb.AppendFormat("{0}}}", idnt);
			sb.AppendLine();

			return sb.ToString();
		}

		private string contentsString(int indent)
		{
			StringBuilder sb = new StringBuilder();

			for (int i = 0; i < indent; i++)
			{
				sb.Append('\t');
			}

			string idnt = sb.ToString();

			sb.Length = 0;

			foreach (var valuePair in this.values)
			{
				sb.AppendFormat("{0}{1} = ", idnt, valuePair.Key);

				if (valuePair.Value.Count == 1)
				{
					sb.Append(this.escapeValueString(valuePair.Value[0]));
				}
				else
				{
					sb.Append(string.Join(", ", valuePair.Value.Select(v => escapeValueString(v)).ToArray()));
				}

				sb.AppendLine();
			}

			foreach (NodeDefinition node in this.nodes)
			{
				sb.Append(node.ToString(indent));
			}

			return sb.ToString();
		}

		private string escapeValueString(string value)
		{
			if (value.IndexOfAny(Scanner.ReservedChars.ToCharArray()) != -1 || value.Length < 1)
			{
				return string.Format("\"{0}\"", value);
			}
			else
			{
				return value;
			}
		}
	}
}

