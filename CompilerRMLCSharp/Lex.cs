using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;



namespace CompilerRMLCSharp
{
    /// <summary>
    /// Класс лексического анализатора в котором происходит обработка и разбиение исходного потока
    /// на лексемы в соответствии с грамматикой целевого языка (по идее можно это обобщить и реализовать
    /// для любого языка, но похуй, это долго), в данном случа Turbo Pascal.
    /// </summary>
    public class Lex
    {
        private int lastFinalState = 0;
        private int lastPositionWithFinalState = 0;
        private int startingPosition = 0;
        private int i = 0;
        private int[] states;

        private string code;
        private int length;

        // Временно сделал паблик для дебаггинга
        public List<List<string>> tables;

        public Lex(string code)
        {
            this.code = code;
            
            //Длинна исходной строки для считывания
            length = code.Length;

            // Список хеш-таблиц
            tables = new List<List<string>>();

            for (int i = 0; i < (short)LexState.LENGTH; i++)
                tables.Add(new List<string>());
            
            states = initStates();
        }

        private int[] initStates()
        {
            int[] arr = new int[(short)LexState.LENGTH - 1];
            for (int i = 0; i < arr.Length; i++)
                arr[i] = 1;
            return arr;     
        }

        public Token parseNext()
        {
            while (i < length)
            {
                // Передача по ref? нужно проверить!!!
                identify(code[i], states);

                // Проверяем вдруг послед. символ не попадает не под одно состояние и ни один из переходов не дошел до финала
                if (isAllNotValid(states))
                {
                    if (lastFinalState == 0)
                    {
                        int numberOfTable = getNumberOfTable(0);
                        int index = -1;

                        string str = code.Substring(
                            startingPosition, lastPositionWithFinalState + 1 - startingPosition <= 0 ?
                            1 : lastPositionWithFinalState + 1 - startingPosition);

                        tables[numberOfTable].Add(str);

                        // Нужно поправить так как я зачем то сделал то что и идентификатор ошибок и тип INT совпадают =-=
                        Token token = new Token(startingPosition, startingPosition + 1, index, numberOfTable, 0, str);

                        states = initStates();
                        i = startingPosition;
                        startingPosition++;
                        i++;
                        return token;
                    }
                    else
                    {
                        int numberOfTable = getNumberOfTable(lastFinalState);

                        if (numberOfTable == -1)
                            Console.WriteLine(lastFinalState);

                        string str = code.Substring(startingPosition, lastPositionWithFinalState + 1 - startingPosition);
                        if (lastFinalState == (int)LexState.ID)
                            str = stringRestricter(str);
                        int index = -1;
                        tables[numberOfTable].Add(str);
                        index = tables[numberOfTable].IndexOf(str);

                        Token token = new Token(startingPosition, lastPositionWithFinalState + 1, index, numberOfTable, lastFinalState, str);

                        lastFinalState = 0;
                        i = lastPositionWithFinalState;
                        startingPosition = lastPositionWithFinalState + 1;
                        states = initStates();
                        i++;
                        return token;
                    }
                }
                else
                {
                    if (getFinalState(states) != 0)
                    {
                        lastFinalState = getFinalState(states);
                        lastPositionWithFinalState = i;
                    }
                    i++;
                }

            }
            if (lastFinalState != 0 && i >= length)
            {

                //success //todo add to hash table
                int numberOfTable = getNumberOfTable(lastFinalState);
                int index = -1;
                String str = code.Substring(startingPosition, lastPositionWithFinalState + 1 - startingPosition);
                str = stringRestricter(str);
                if (lastFinalState == (int)LexState.ID)
                {
                    str = stringRestricter(str);
                }

                tables[numberOfTable].Add(str);
                index = tables[numberOfTable].IndexOf(str);

                Token token = new Token(startingPosition, lastPositionWithFinalState + 1, index, numberOfTable, lastFinalState, str);
                lastFinalState = 0;
                return token;
            }

            throw new Exception("Нет больше лексем!");
        }

        private int getFinalState(int[] states)
        {

            //keyword
            if (states[2] < 0)
            {
                return states[2];
            }

            //int
            if (states[0] < 0)
            {
                return states[0];
            }

            //identifier
            if (states[1] < 0)
            {
                return states[1];
            }

            //float
            if (states[3] < 0)
            {
                return states[3];
            }

            //space
            if (states[4] < 0)
            {
                return states[4];
            }

            //sign
            if (states[5] < 0)
            {
                return states[5];
            }

            return 0;
        }


        private string stringRestricter(string ID)
        {
            if (ID.Length > (int)LexState.MAX_ID_LENGTH)
                return (ID.Substring(0, (int)LexState.MAX_ID_LENGTH));
            return ID;
        }

        private bool isAllNotValid(int[] states)
        {
            foreach (var state in states)
                if (state != 0)
                    return false;
            return true;
        }

        // Получаем номер в таблице идентификаторов в зависимости от типа
        // Определенной лексемы
        private int getNumberOfTable(int state)
        {

            switch (state)
            {

                case (int)LexState.INT:
                    return (int)LexState.TABLE_INT;
                case (int)LexState.ID:
                    return (int)LexState.TABLE_ID;

                case (int)LexState.REAL_KEYWORD:
                case (int)LexState.AND_KEYWORD:
                case (int)LexState.ARRAY_KEYWORD:
                case (int)LexState.PROGRAM_KEYWORD:
                case (int)LexState.IF_KEYWORD:
                case (int)LexState.BEGIN_KEYWORD:
                case (int)LexState.CONST_KEYWORD:
                case (int)LexState.THEN_KEYWORD:
                case (int)LexState.TO_KEYWORD:
                case (int)LexState.DIV_KEYWORD:
                case (int)LexState.MOD_KEYWORD:
                case (int)LexState.DO_KEYWORD:
                case (int)LexState.DOWNTO_KEYWORD:
                case (int)LexState.NOT_KEYWORD:
                case (int)LexState.ELSE_KEYWORD:
                case (int)LexState.END_KEYWORD:
                case (int)LexState.OF_KEYWORD:
                case (int)LexState.VAR_KEYWORD:
                case (int)LexState.OR_KEYWORD:
                case (int)LexState.FOR_KEYWORD:
                case (int)LexState.FUNCTION_KEYWORD:
                case (int)LexState.PROCEDURE_KEYWORD:
                case (int)LexState.XOR_KEYWORD:
                case (int)LexState.EXIT_KEYWORD:
                    return (int)LexState.TABLE_KEYWORD;
                case -1:
                case -2:
                case -3:
                case -4:
                    return (int)LexState.TABLE_REAL;
                case (int)LexState.SPACE:
                    return (int)LexState.TABLE_SPACE;
                case (int)LexState.CLOSING_CURLY_BRACE:
                case (int)LexState.OPENING_CURLY_BRACE:
                case (int)LexState.OPENING_ROUND_BRACE:
                case (int)LexState.CLOSING_ROUND_BRACE:
                case (int)LexState.SEMICOLON:
                case (int)LexState.DOT:
                case (int)LexState.DOUBLE_DOT:
                case (int)LexState.EQUALITY:
                case (int)LexState.ARIFMETIC_DIV:
                case (int)LexState.ARIFMETIC_MINUS:
                case (int)LexState.ARIFMETIC_MULT:
                case (int)LexState.ARIFMETIC_PLUS:
                case (int)LexState.COMPARE_LESS:
                case (int)LexState.COMPARE_MORE:
                case (int)LexState.SIGN_QUOTE:
                case (int)LexState.SIGN_COMMA:
                    return (int)LexState.TABLE_SIGN;
                case (int)LexState.ERROR:
                    return (int)LexState.TABLE_ERROR;

            }
            return -1;
        }

        //Epsilon-переходы (правила)
        private void identify(char c, int[] states)
        {
            states[0] = identifyInt(c, states[0]);
            states[1] = identifyIdentifier(c, states[1]);
            states[2] = identifyKeyWord(c, states[2]);
            states[3] = identifyReal(c, states[3]);
            states[4] = identifySpace(c, states[4]);
            states[5] = identifySign(c, states[5]);
        }

        #region Epsilon-правила

        //Проверка на целое число
        private int identifyInt(char ch, int state)
        {
            switch (state)
            {
                case (int)LexState.TABLE_ID:
                        if (char.IsDigit(ch))
                            return (int)LexState.INT;
                    break;
                case (int)LexState.INT:
                    if (char.IsDigit(ch))
                        return (int)LexState.INT;
                    break; 

            }
            return (int)LexState.ERROR;

        }

        // Проверка на идентификатор
        private int identifyIdentifier(char ch, int state)
        {
            switch (state)
            {
                case (int)LexState.TABLE_ID:
                    if (char.IsLetter(ch))
                        return (int)LexState.ID;
                    break;
                case (int)LexState.ID:
                    if (char.IsLetter(ch))
                        return (int)LexState.ID;
                    break;
            }
            return (int)LexState.ERROR;
        }

        //Проверка на ключевое слово
        //Здесь можно добавлять дополнительные состояния для новых ключевых слов
        private int identifyKeyWord(char ch, int state)
        {
            char rch = char.ToLower(ch);
            switch (state)
            {
                case (int)LexState.TABLE_ID:
                    if (rch == 'a')
                        return 2;
                    if (rch == 'b')
                        return 7;
                    if (rch == 'p')
                        return 11;
                    if (rch == 'i')
                        return 17;
                    if (rch == 'c')
                        return 18;
                    if (rch == 't')
                        return 22;
                    if (rch == 'd')
                        return 25;
                    if (rch == 'n')
                        return 31;
                    if (rch == 'e')
                        return 33;
                    if (rch == 'o')
                        return 37;
                    if (rch == 'v')
                        return 38;
                    if (rch == 'f')
                        return 40;
                    if (rch == 'x')
                        return 53;
                    
                    return 0;
                case 2:
                    if (rch == 'n')
                        return 3;
                    if (rch == 'r')
                        return 4;
                   
                    return 0;
                case 3:
                    if (rch == 'd')
                        return (int)LexState.AND_KEYWORD;
                    return 0;
                case 4:
                    if (rch == 'r')
                        return 5;
                    return 0;
                case 5:
                    if (rch == 'a')
                        return 6;
                    return 0;
                case 6:
                    if (rch == 'y')
                        return (int)LexState.ARRAY_KEYWORD;
                    return 0;
                case 7:
                    if (rch == 'e')
                        return 8;
                    return 0;
                case 8:
                    if (rch == 'g')
                        return 9;
                    return 0;
                case 9:
                    if (rch == 'i')
                        return 10;
                    return 0;
                case 10:
                    if (rch == 'n')
                        return (int)LexState.BEGIN_KEYWORD;
                    return 0;
                case 11:
                    if (rch == 'r')
                        return 12;
                    return 0;
                case 12:
                    if (rch == 'o')
                        return 13;
                    return 0;
                case 13:
                    if (rch == 'g')
                        return 14;
                    if (rch == 'c')
                        return 48;
                    return 0;
                case 14:
                    if (rch == 'r')
                        return 15;
                    return 0;
                case 15:
                    if (rch == 'a')
                        return 16;
                    return 0;
                case 16:
                    if (rch == 'm')
                        return (int)LexState.PROGRAM_KEYWORD;
                    return 0;
                case 17:
                    if (rch == 'f')
                        return (int)LexState.IF_KEYWORD;
                    return 0;
                case 18:
                    if (rch == 'o')
                        return 19;
                    return 0;
                case 19:
                    if (rch == 'n')
                        return 20;
                    return 0;
                case 20:
                    if (rch == 's')
                        return 21;
                    return 0;
                case 21:
                    if (rch == 't')
                        return (int)LexState.CONST_KEYWORD;
                    return 0;
                case 22:
                    if (rch == 'o')
                        return (int)LexState.TO_KEYWORD;
                    if (rch == 'h')
                        return 23;
                    return 0;
                case 23:
                    if (rch == 'e')
                        return 24;
                    return 0;
                case 24:
                    if (rch == 'n')
                        return (int)LexState.THEN_KEYWORD;
                    return 0;
                case 25: // do и downto, не совмес понятно как решить коллизию, так как downto включает do
                    if (rch == 'o')
                        return 27;
                    if (rch == 'i')
                        return 26;
                    return 0;
                case 26:
                    if (rch == 'v')
                        return (int)LexState.DIV_KEYWORD;
                    return 0;
                case 27:
                    if (rch == 'w')
                        return 28;
                    if (rch == '\n' || rch == ' ')
                        return (int)LexState.DO_KEYWORD;
                    return 0;
                case 28:
                    if (rch == 'n')
                        return 29;
                    return 0;
                case 29:
                    if (rch == 't')
                        return 30;
                    return 0;
                case 30:
                    if (rch == 'o')
                        return (int)LexState.DOWNTO_KEYWORD;
                    return 0;
                case 31:
                    if (rch == 'o')
                        return 32;
                    return 0;
                case 32:
                    if (rch == 't')
                        return (int)LexState.NOT_KEYWORD;
                    return 0;
                case 33:
                    if (rch == 'l')
                        return 34;
                    if (rch == 'n')
                        return 36;
                    if (rch == 'x')
                        return 55;
                    return 0;
                case 34:
                    if (rch == 's')
                        return 35;
                    return 0;
                case 35:
                    if (rch == 'e')
                        return (int)LexState.ELSE_KEYWORD;
                    return 0;
                case 36:
                    if (rch == 'd')
                        return (int)LexState.END_KEYWORD;
                    return 0;
                case 37:
                    if (rch == 'f')
                        return (int)LexState.OF_KEYWORD;
                    if (rch == 'r')
                        return (int)LexState.OR_KEYWORD;
                    return 0;
                case 38:
                    if (rch == 'a')
                        return 39;
                    return 0;
                case 39:
                    if (rch == 'r')
                        return (int)LexState.VAR_KEYWORD;
                    return 0;
                case 40:
                    if (rch == 'u')
                        return 42;
                    if (rch == 'o')
                        return 41;
                    return 0;
                case 41:
                    if (rch == 'r')
                        return (int)LexState.FOR_KEYWORD;
                    return 0;
                case 42:
                    if (rch == 'n')
                        return 43;
                    return 0;
                case 43:
                    if (rch == 'c')
                        return 44;
                    return 0;
                case 44:
                    if (rch == 't')
                        return 45;
                    return 0;
                case 45:
                    if (rch == 'i')
                        return 46;
                    return 0;
                case 46:
                    if (rch == 'o')
                        return 47;
                    return 0;
                case 47:
                    if (rch == 'n')
                        return (int)LexState.FUNCTION_KEYWORD;
                    return 0;
                case 48:
                    if (rch == 'e')
                        return 49;
                    return 0;
                case 49:
                    if (rch == 'd')
                        return 50;
                    return 0;
                case 50:
                    if (rch == 'u')
                        return 51;
                    return 0;
                case 51:
                    if (rch == 'r')
                        return 52;
                    return 0;
                case 52:
                    if (rch == 'e')
                        return (int)LexState.PROCEDURE_KEYWORD;
                    return 0;
                case 53:
                    if (rch == 'o')
                        return 54;
                    return 0;
                case 54:
                    if (rch == 'r')
                        return (int)LexState.XOR_KEYWORD;
                    return 0;
                case 55:
                    if (rch == 'i')
                        return 56;
                    return 0;
                case 56:
                    if (rch == 't')
                        return (int)LexState.EXIT_KEYWORD;
                    return 0;
                default:
                    break;
            }
            return (int)LexState.ERROR;
        }

        // Проверка на действительное число, включая к примеру 25.E-3 или 12.e+25
        private int identifyReal(char ch, int state)
        {
            switch (state)
            {
                case 1:
                    if (ch == '0')
                        return 4;
                    if (char.IsDigit(ch))
                        return 2;
                    if (ch == '.')
                        return 3;
                    return 0;
                case 2:
                    if (char.IsDigit(ch))
                        return 2;
                    if (ch == '.')
                        return -1;
                    return 0;
                case -1:
                    if (char.IsDigit(ch))
                        return -2;
                    return 0;
                case -2:
                    if (char.IsDigit(ch))
                        return -2;
                    return 0;
                case 3:
                    if (char.IsDigit(ch))
                        return -3;
                    return 0;
                case -3:
                    if (char.IsDigit(ch))
                        return -3;
                    return 0;
                case 4:
                    if (ch == '.')
                        return 5;
                    return 0;
                case 5:
                    if (char.IsDigit(ch))
                        return 6;
                    return 0;
                case 6:
                    if (char.IsDigit(ch))
                        return 6;
                    if (ch == 'e' || ch == 'E')
                        return 7;
                    return 0;
                case 7:
                    if (ch == '+' || ch == '-') 
                        return 8;
                    return 0;
                case 8:
                    if (char.IsDigit(ch))
                        return -4;
                    return 0;
                case -4:
                    if (char.IsDigit(ch))
                        return -4;
                    return 0;
            }
            return 0;
        }
        
        // Проверка, является ли символ допустимым для грамматики языка, скобкой
        private int identifySign(char ch, int state)
        {
            switch (state)
            {
                case 1:
                    if (ch == ';')
                        return (int)LexState.SEMICOLON;
                    if (ch == '{')
                        return (int)LexState.OPENING_CURLY_BRACE;
                    if (ch == '}')
                        return (int)LexState.CLOSING_CURLY_BRACE;
                    if (ch == '(')
                        return (int)LexState.OPENING_ROUND_BRACE;
                    if (ch == ')')
                        return (int)LexState.CLOSING_ROUND_BRACE;
                    if (ch == '=')
                        return (int)LexState.EQUALITY;
                    if (ch == ':')
                        return (int)LexState.DOUBLE_DOT;
                    if (ch == '.')
                        return (int)LexState.DOT;
                    if (ch == '+')
                        return (int)LexState.ARIFMETIC_PLUS;
                    if (ch == '-')
                        return (int)LexState.ARIFMETIC_MINUS;
                    if (ch == '*')
                        return (int)LexState.ARIFMETIC_MULT;
                    if (ch == '/')
                        return (int)LexState.ARIFMETIC_DIV;
                    if (ch == '>')
                        return (int)LexState.COMPARE_MORE;
                    if (ch == '<')
                        return (int)LexState.COMPARE_LESS;
                    if (ch == '\'')
                        return (int)LexState.SIGN_QUOTE;
                    if (ch == ',')
                        return (int)LexState.SIGN_COMMA;
                    return 0;
            }
            return 0;
        }

        // Разделитили
        private int identifySpace(char ch, int state)
        {
            switch (state)
            {
                case 1:
                    if (ch == ' ')
                    {
                        return (int)LexState.SPACE;
                    }
                    if (ch == '\t')
                    {
                        return (int)LexState.SPACE;
                    }
                    if (ch == '\r')
                    {
                        return (int)LexState.SPACE;
                    }
                    if (ch == '\n')
                    {
                        return (int)LexState.SPACE;
                    }
                    return 0;
                case (int)LexState.SPACE:
                    if (ch == ' ')
                    {
                        return (int)LexState.SPACE;
                    }
                    if (ch == '\t')
                    {
                        return (int)LexState.SPACE;
                    }
                    if (ch == '\r')
                    {
                        return (int)LexState.SPACE;
                    }
                    if (ch == '\n')
                    {
                        return (int)LexState.SPACE;
                    }
                    return 0;
            }
            return 0;
        }
        #endregion
    }

    // Состояния для автомата
    public enum LexState
    {
        LENGTH = 7,
        SPACE = -13,

        #region Ключевые слова
        REAL_KEYWORD = -1000,
        AND_KEYWORD, 
        RETURN_KEYWORD, 
        ARRAY_KEYWORD, 
        PROGRAM_KEYWORD, 
        IF_KEYWORD,
        BEGIN_KEYWORD, 
        CONST_KEYWORD,
        THEN_KEYWORD,
        TO_KEYWORD,
        DIV_KEYWORD,
        MOD_KEYWORD,
        DO_KEYWORD,
        DOWNTO_KEYWORD,
        NOT_KEYWORD,
        ELSE_KEYWORD,
        END_KEYWORD,
        OF_KEYWORD,
        VAR_KEYWORD,
        OR_KEYWORD,
        FOR_KEYWORD,
        FUNCTION_KEYWORD,
        PROCEDURE_KEYWORD,
        XOR_KEYWORD,
        EXIT_KEYWORD,
        #endregion

        OPENING_CURLY_BRACE = -14,
        CLOSING_CURLY_BRACE = -15,
        OPENING_ROUND_BRACE = -16,
        CLOSING_ROUND_BRACE = -17,
        SEMICOLON = -18,
        INT = -19,
        DOUBLE_DOT = -20,
        DOT = -21,
        EQUALITY = -22,


        // Другие знаки, которые я забыл добавить до этого
        ARIFMETIC_PLUS = - 10000,
        ARIFMETIC_MINUS,
        ARIFMETIC_MULT,
        ARIFMETIC_DIV,

        COMPARE_MORE,
        COMPARE_LESS,

        SIGN_QUOTE,
        SIGN_COMMA,

        ID = -6,
        ERROR = 0,
        TABLE_INT = 0,
        TABLE_ID ,
        TABLE_KEYWORD,
        TABLE_REAL ,
        TABLE_SPACE,
        TABLE_SIGN ,
        TABLE_ERROR ,
        MAX_ID_LENGTH = 69
    }

    public class Node<T>
    {
        public T Value { get; private set; }
        public Node<T> Next { get; set; }

        public Node(T val)
        {
            Value = val;
        }


    }

    public class ListNode<T> : IEnumerable<T>
    {
        Node<T> head;
        Node<T> last;


        public void Insert(T newValue)
        {
            Node<T> node = new Node<T>(newValue);

            if (head == null)
                head = node;
            else
                last.Next = node;
            last = node;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)this).GetEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            Node<T> Curr = head;

            while (Curr != null)
            {
                yield return Curr.Value;
                Curr = Curr.Next;
            }
        }

    }

    public class HashTable
    {
        public int Key { get; private set; }

        public ListNode<string>[] values;

        public HashTable(int N)
        {
            Key = N;
            values = new ListNode<string>[N];
            for (int i = 0; i < N; i++)
            {
                values[i] = new ListNode<string>();
            }
        }


        public int Insert(string newValue)
        {
            values[Math.Abs(newValue.GetHashCode()) % Key].Insert(newValue);
                return Math.Abs(newValue.GetHashCode()) % Key;
        }

        public void Show()
        {
            for (int i = 0; i < Key; i++)
            {


                Console.Write(i + ": ");

                foreach (var item in values[i])
                {
                    Console.Write(item + " ");
                }

                Console.WriteLine();

            }
        }
    }
}
