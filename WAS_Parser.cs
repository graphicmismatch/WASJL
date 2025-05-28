using System;
using System.Collections.Generic;
using System.Text;

namespace WASJL
{
    public class WAS_Parser
    {
        public WAS_Lexer Lexer { get; set; }
        private WAS_Token _CurrentToken { get; set; }
        private WAS_Token _NextToken { get; set; }

        public WAS_Parser(WAS_Lexer lexer)
        {
            Lexer = lexer;
            _CurrentToken = new WAS_Token(WAS_TokenType.EOF,'\0');
            _NextToken = new WAS_Token(WAS_TokenType.EOF, '\0');

            Next();
        }

        private void Next() { 
            _CurrentToken = _NextToken;
            _NextToken = Lexer.GetNextToken();
        }

        public object Parse()
        {
            Next();
            switch (_CurrentToken.type)
            {
                case WAS_TokenType.STRINGLITERAL:
                case WAS_TokenType.NUMERICLITERAL:
                case WAS_TokenType.BOOLEAN:
                    return _CurrentToken.value;

                case WAS_TokenType.OPENCURLYBRACKET:
                    return MakeDictionary();

                case WAS_TokenType.OPENSQUAREBRACKET:
                    return MakeList();

                default:
                    return (object)String.Empty;
            }
        }

        private object MakeList()
        {
            List<object> outList = new List<object>();
            while (_CurrentToken.type != WAS_TokenType.CLOSESQUAREBRACKET) { 
                outList.Add(Parse());
                Next();
            }

            return outList;
        }

        private object MakeDictionary()
        {
            Dictionary<object, object> outDict = new Dictionary<object, object>();
            while (_CurrentToken.type != WAS_TokenType.CLOSECURLYBRACKET) {
                object key = Parse();
                if (_NextToken.type != WAS_TokenType.COLON) {
                    throw new Exception("Expected colon, found "+ _NextToken.value);
                }

                Next();
                object value = Parse();
                outDict.Add(key, value);
                Next();
            }

            return outDict;
        }
    }
}
