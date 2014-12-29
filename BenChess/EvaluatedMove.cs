using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BenChess
{
   class EvaluatedMove
   {
      EvaluatedMove priorState;
      ChessMove move;
      ChessBoard resultingState;
      int depth;
      int bestValue;

      public EvaluatedMove(EvaluatedMove priorState, ChessMove move, ChessBoard resultingState)
      {
         this.priorState = priorState;
         this.move = move;
         this.resultingState = resultingState;
         bestValue = resultingState.BoardValue;
         if (priorState != null)
         {
            this.depth = priorState.depth + 1;
            PropagateBestValue();
         }
         else
            this.depth = 0;
      }

      public EvaluatedMove PriorState
      {
         get
         {
            return priorState;
         }
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

      public int Depth
      {
         get
         {
            return depth;
         }
      }

      public int BestValue
      {
         get
         {
            return bestValue;
         }
      }

      private void PropagateBestValue()
      {
         if (priorState == null)
            return;
         if (priorState.resultingState.IsBlacksTurn)
         {
            if (bestValue < priorState.bestValue)
               priorState.bestValue = bestValue;
         }
         else
         {
            if (bestValue > priorState.bestValue)
               priorState.bestValue = bestValue;
         }
         priorState.PropagateBestValue();
      }
   }
}
