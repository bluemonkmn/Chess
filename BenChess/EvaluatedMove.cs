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
      int leafValue; // What value will this move finally end up at?

      public EvaluatedMove(EvaluatedMove priorState, ChessMove move, ChessBoard resultingState, bool isLeaf = false)
      {
         this.priorState = priorState;
         this.move = move;
         this.resultingState = resultingState;
         if (priorState != null)
         {
            this.depth = priorState.depth + 1;
         }
         else
            this.depth = 0;
         if (isLeaf)
            leafValue = resultingState.BoardValue;
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

      public int LeafValue
      {
         get
         {
            return leafValue;
         }
         set
         {
            leafValue = value;
         }
      }
   }
}
