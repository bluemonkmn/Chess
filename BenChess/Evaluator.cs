using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BenChess
{
   class Evaluator
   {
      private int maxDepth
      public static ChessMove GetBestMove(ChessBoard board, int depth)
      {
         Evaluator e = new Evaluator();
         e.toEvaluate.Enqueue(new EvaluatedMove(null, null, board));
         e.EvaluateMoves(5);
      }

      private List<EvaluatedMove> EvaluateMoves(EvaluatedMove priorState, int maxDepth)
      {
         List<EvaluatedMove> result = new List<EvaluatedMove>();
         ChessBoard board = priorState.ResultingState;
         if (priorState.Depth >= maxDepth - 1)
         {
            int bestMove = board.GetBestMoveValue();
            foreach(ChessMove m in board.GetValidMoves())
            {
               if (board.EvaluateMove(m.ToString()) == bestMove)
                  result.Add(new EvaluatedMove(priorState, m, board.Move(m.ToString())));
            }
         }
         else
         {
            foreach(ChessMove m in board.GetValidMoves())
            {
               EvaluatedMove nextMove = new EvaluatedMove(priorState, m, board.Move(m.ToString()));
               result.AddRange(EvaluateMoves(nextMove, maxDepth));
            }
            foreach(EvaluatedMove m in result.ToArray())
            {
               if (m.BestValue != priorState.BestValue)
                  result.Remove(m);
            }
         }
         return result;
      }
   }
}
