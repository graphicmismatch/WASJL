using System;

namespace WASJL
{
    public class WAS_Lexer
    {
        public string JsonString { get;  }

        private int _position;
        private char _currentChar;
        private char _nextChar;
        public WAS_Lexer(string jsonString)
        {
            JsonString = jsonString ?? throw new ArgumentNullException(nameof(jsonString));
            _position = -2;
            _currentChar = '\0';
            _nextChar = '\0';
            Next();
        }

        private void ConsumeWhiteSpace()
        {
            while (_currentChar != '\0' && char.IsWhiteSpace(_currentChar)) {
                Next();
            }  
        }
        private void Next() {
            _position++;
            _currentChar = _nextChar;

            if (_position <= (JsonString.Length - 2))
            {
                _nextChar = JsonString[_position + 1];
            }
            else {
                _nextChar = '\0';
            }
        }

        public WAS_Token GetNextToken() {
            Next();
            ConsumeWhiteSpace();
            char currentChar = _currentChar;
            string charString = currentChar.ToString();
            if (currentChar == '\0')
            {
                return new WAS_Token(WAS_TokenType.EOF, charString);
            }
            else if (currentChar == ',')
            {
                return new WAS_Token(WAS_TokenType.COMMA, charString);
            }
            else if (currentChar == ':')
            {
                return new WAS_Token(WAS_TokenType.COLON, charString);
            }
            else if (currentChar == '{')
            {
                return new WAS_Token(WAS_TokenType.OPENCURLYBRACKET, charString);
            }
            else if (currentChar == '}')
            {
                return new WAS_Token(WAS_TokenType.CLOSECURLYBRACKET, charString);
            }
            else if (currentChar == '[')
            {
                return new WAS_Token(WAS_TokenType.OPENSQUAREBRACKET, charString);
            }
            else if (currentChar == ']')
            {
                return new WAS_Token(WAS_TokenType.CLOSESQUAREBRACKET, charString);
            }
            else if (currentChar == 't' || currentChar == 'f') {
                return MakeBooleanLiteral();
            }
            else if (currentChar == '"')
            {
                return MakeStringLiteral();
            }
            else if (char.IsDigit(currentChar))
            {
                return MakeNumericLiteral();
            }

            return new WAS_Token(WAS_TokenType.EOF, '\0');
        }

        public WAS_Token MakeBooleanLiteral()
        {
        
            int currentPos = _position;

            while (_currentChar != '\0' && _currentChar != 'e')
            {
                Next();
            }

            string literal = JsonString.Substring(currentPos, _position - currentPos+1);
            if (literal == "true" || literal == "false")
            {
                return new WAS_Token(WAS_TokenType.BOOLEAN, bool.Parse(literal));
            }
            else {
                    throw new FormatException("Invalid Boolean Format");
            }
        }
        public WAS_Token MakeStringLiteral() {
            Next();
            int currentPos = _position;

            while (_currentChar != '\0' && _currentChar != '"') {
                Next();
            }

            string literal = JsonString.Substring(currentPos, _position - currentPos);
            return new WAS_Token(WAS_TokenType.STRINGLITERAL, literal);
        }
        public WAS_Token MakeNumericLiteral()
        {
            int currentPos = _position;
            int dots = 0;

            while (_currentChar != '\0'&&(char.IsDigit(_nextChar)||_nextChar == '.'))
            {
                if (_currentChar == '.') {
                    dots++;
                }
                Next();
            }
            string literal = JsonString.Substring(currentPos, _position - currentPos+1);
            if (dots > 1) {
                throw new FormatException("Invalid Number Format");
            }

            if (dots == 1) {
                return new WAS_Token(WAS_TokenType.NUMERICLITERAL, double.Parse(literal));
            }
            return new WAS_Token(WAS_TokenType.NUMERICLITERAL, int.Parse(literal));
        }
    }
}
