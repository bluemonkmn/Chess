using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BenChess
{
   class EvaluatedMove
   {
      ChessMove move;
      ChessBoard resultingState;
      public EvaluatedMoves Previous { get; private set; }
      public EvaluatedMoves Next { get; set; }

      public EvaluatedMove(ChessMove move, ChessBoard resultingState, EvaluatedMoves predecessor = null)
      {
         this.move = move;
         this.resultingState = resultingState;
         this.Previous = predecessor;
      }

      public ChessMove Move
      {
         get
         {
            return move;
         }
      }

      public ChessBoard ResultingState
      {
         get
         {
            return resultingState;
         }
      }

      public int FinalValue
      {
         get
         {
            if (Next == null)
               return resultingState.BoardValue;
            else
               return Next.FinalBestValue;
         }
      }
   }
}
