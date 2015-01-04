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
      public EvaluatedMove Previous { get; private set; }
      public EvaluatedMove[] Next { get; set; }
      public short BestNextMoveValue { get; set; }
      public byte MateDepth { get; set; }
      public int AccumulatedMoveValues { get; set; }

      public EvaluatedMove(ChessMove move, ChessBoard resultingState, EvaluatedMove predecessor = null)
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

      public EvaluatedMove GetRoot()
      {
         if ((Previous == null) || (Previous.Move == null))
            return this;
         return Previous.GetRoot();
      }

      public override int GetHashCode()
      {
         return move.GetHashCode() ^ resultingState.GetHashCode();
      }
   }
}
