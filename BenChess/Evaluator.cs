using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BenChess
{
   public class Evaluator
   {
      private static Random random;

      public static ChessMove GetBestMove(ChessBoard board, int depth)
      {
         Evaluator e = new Evaluator();
         EvaluatedMoves moves = e.EvaluateMoves(board, depth);
         if (moves == null)
            return null;
         moves.PopulateFinalBestMoveValues();
         IEnumerable<EvaluatedMove> bestOrder = moves.Where(m => m.FinalValue == moves.FinalBestValue);
         if (board.IsBlacksTurn)
            bestOrder = bestOrder.OrderBy(m=>m.ResultingState.BoardValue);
         else
            bestOrder = bestOrder.OrderByDescending(m=>m.ResultingState.BoardValue);
         EvaluatedMove[] best = ((IOrderedEnumerable<EvaluatedMove>)bestOrder).ThenBy(m=>board[m.Move.Source]).ToArray();
         if (random == null) random = new Random();
         int moveIndex = -1;
         for (int i = 1; i < best.Length; i++ )
         {
            if ((best[i].ResultingState.BoardValue != best[i - 1].ResultingState.BoardValue) ||
               (board[best[i].Move.Source] != board[best[i - 1].Move.Source]))
            {
               moveIndex = random.Next(i);
               break;
            }
         }
         if (moveIndex < 0)
            moveIndex = random.Next(best.Length);
         return best[moveIndex].Move;
      }

      private EvaluatedMoves EvaluateMoves(ChessBoard board, int depth)
      {
         if (depth == 0)
            return null;
         EvaluatedMoves moves = new EvaluatedMoves(board);
         if (moves.Length == 0)
            return null;
         foreach (EvaluatedMove move in moves)
            move.Next = EvaluateMoves(move.ResultingState, depth - 1);
         return moves;
      }
   }
}
