using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompilerRMLCSharp
{
    public class Token
    {
        private int startingPosition;
        private int endingPosition;
        public string name { get; private set; }

        public int state { private set; get; }

        private int index;
        public int numberOfTable { private set; get; }

        // Инициализация лексемы в конструкторе
        public Token(
            int start, 
            int end, 
            int ind, 
            int numberOfTable, 
            int state,
            string name)
        {
            startingPosition = start;
            endingPosition = end;
            index = ind;
            this.numberOfTable = numberOfTable;
            this.state = state;
            this.name = name;
        }

        

    }
}
