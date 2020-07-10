using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.Unicode;
using System.Linq;

namespace ConfigParser {
    public static class ConfigFile {
        public static List<NodeDefinition> LoadFromString(string cfgString, bool squadCompat = false) {
            List<NodeDefinition> nodes;

            Scanner scanner = new Scanner(cfgString);

            Parser parser = new Parser(scanner, squadCompat);

            nodes = parser.Parse();

            return nodes;
        }

        public static List<NodeDefinition> LoadFromPath(string path, bool squadCompat = false) {
            using (var stream = File.OpenText(path)) {
                return LoadFromString(stream.ReadToEnd(), squadCompat);
            }
        }

        public static void SaveToPath(List<NodeDefinition> nodeDefs, string path) {
            using (var stream = File.OpenWrite(path)) {
                foreach (NodeDefinition nodeDef in nodeDefs) {
                    stream.Write(UTF8Encoding.UTF8.GetBytes(nodeDef.ToString()));
                    stream.Write(UTF8Encoding.UTF8.GetBytes("\n"));
                }
            }
        }

        public static string CleanFileName(string unclean) {
            StringBuilder clean = new StringBuilder(unclean.Length);

            foreach (var c in unclean) {
                switch (c) {
                    case 'A':
                    case 'B':
                    case 'C':
                    case 'D':
                    case 'E':
                    case 'F':
                    case 'G':
                    case 'H':
                    case 'I':
                    case 'J':
                    case 'K':
                    case 'L':
                    case 'M':
                    case 'N':
                    case 'O':
                    case 'P':
                    case 'Q':
                    case 'R':
                    case 'S':
                    case 'T':
                    case 'U':
                    case 'V':
                    case 'W':
                    case 'X':
                    case 'Y':
                    case 'Z':
                    case 'a':
                    case 'b':
                    case 'c':
                    case 'd':
                    case 'e':
                    case 'f':
                    case 'g':
                    case 'h':
                    case 'i':
                    case 'j':
                    case 'k':
                    case 'l':
                    case 'm':
                    case 'n':
                    case 'o':
                    case 'p':
                    case 'q':
                    case 'r':
                    case 's':
                    case 't':
                    case 'u':
                    case 'v':
                    case 'w':
                    case 'x':
                    case 'y':
                    case 'z':
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        clean.Append(c);
                        break;
                    case ' ':
                    case '\t':
                        clean.Append('_');
                        break;
                    default:
                        break;
                }
            }

            return clean.ToString();
        }
    }
}
