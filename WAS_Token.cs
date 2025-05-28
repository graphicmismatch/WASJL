using System;
using System.Collections.Generic;
using System.Text;

namespace WASJL
{

    public enum WAS_TokenType { 
        OPENCURLYBRACKET,
        CLOSECURLYBRACKET,
        OPENSQUAREBRACKET,
        CLOSESQUAREBRACKET,
        COMMA,
        COLON,
        STRINGLITERAL,
        NUMERICLITERAL,
        BOOLEAN,
        EOF
    }
    public struct WAS_Token
    {
        public WAS_TokenType type;
        public object value;

        public WAS_Token(WAS_TokenType classification, object value)
        {
            this.type = classification;
            this.value = value;
        }
    }
}
