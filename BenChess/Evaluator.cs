using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BenChess
{
   public class Evaluator
   {
      private int maxDepth;
      private static Random random;

      public static ChessMove GetBestMove(ChessBoard board, int depth)
      {
         Evaluator e = new Evaluator() { maxDepth = depth };
         EvaluatedMove em = new EvaluatedMove(null, null, board);
         EvaluatedMove[] moves = e.EvaluateMoves(em);
         if (moves.Length == 0)
            return null;
         if (random == null) random = new Random();
         int moveIndex = random.Next(moves.Length);
         return moves[moveIndex].Move;
      }

      private EvaluatedMove[] EvaluateMoves(EvaluatedMove priorState)
      {
         List<EvaluatedMove> result = new List<EvaluatedMove>();
         ChessBoard board = priorState.ResultingState;
         if (priorState.Depth >= maxDepth - 1)
         {
            int bestMove = board.GetBestMoveValue();
            foreach (ChessMove m in board.GetValidMoves())
            {
               if (board.EvaluateMove(m.ToString()) == bestMove)
                  result.Add(new EvaluatedMove(priorState, m, board.Move(m.ToString())));
            }
         }
         else
         {
            int bestMove = int.MinValue;

            foreach (ChessMove m in board.GetValidMoves())
            {
               EvaluatedMove thisMove = new EvaluatedMove(priorState, m, board.Move(m.ToString()));
               EvaluateMoves(thisMove);
               result.Add(thisMove);
               if (bestMove == int.MinValue)
                  bestMove = thisMove.BestValue;
               else if (board.IsBlacksTurn)
               {
                  if (thisMove.BestValue < bestMove)
                     bestMove = thisMove.BestValue;
               }
               else
               {
                  if (thisMove.BestValue > bestMove)
                     bestMove = thisMove.BestValue;
               }
            }
            foreach (EvaluatedMove m in result.ToArray())
            {
               if (m.BestValue != bestMove)
                  result.Remove(m);
            }
         }
         return result.ToArray();
      }
   }
}
