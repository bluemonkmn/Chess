using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BenChess
{
   public class Evaluator
   {
      private static Random random;
      public delegate void Progress(int current, int max);

      public static ChessMove GetBestMove(ChessBoard board, int depth, Progress progress)
      {
         Evaluator e = new Evaluator();
         EvaluatedMove root = new EvaluatedMove(null, board);
         e.Minimax(root, depth, progress);
         EvaluatedMove[] bestMoves = root.Next.Where(m => (m != null) && (m.BestNextMoveValue == root.BestNextMoveValue)).ToArray();
         IOrderedEnumerable<EvaluatedMove> orderedMoves = bestMoves.OrderBy(m => m.MateDepth); //.ThenBy(m => board[m.Move.Source]);
         if (board.IsBlacksTurn)
            orderedMoves = orderedMoves.ThenBy(m=>m.AccumulatedMoveValues);
         else
            orderedMoves = orderedMoves.ThenByDescending(m => m.AccumulatedMoveValues);
         EvaluatedMove[] bestOrderedMoves = orderedMoves.ToArray();

         if (random == null) random = new Random();
         int moveIndex = -1;
         for (int i = 1; i < bestOrderedMoves.Length; i++ )
         {
            if ((bestOrderedMoves[i].AccumulatedMoveValues != bestOrderedMoves[i - 1].AccumulatedMoveValues)
               /*|| (board[bestOrderedMoves[i].Move.Source] != board[bestOrderedMoves[i - 1].Move.Source])*/
               || (bestOrderedMoves[i].MateDepth != bestOrderedMoves[i-1].MateDepth))
            {
               moveIndex = random.Next(i);
               break;
            }
         }
         if (moveIndex < 0)
            moveIndex = random.Next(bestOrderedMoves.Length);
         return bestOrderedMoves[moveIndex].Move;
      }

      private void Minimax(EvaluatedMove predecessor, int depth, Progress progress)
      {
         if ((depth == 0) || (predecessor.ResultingState.BoardValue == -9999) || (predecessor.ResultingState.BoardValue == 9999))
         {
            predecessor.BestNextMoveValue = predecessor.ResultingState.BoardValue;
            return;
         }
         ChessMove[] moves = predecessor.ResultingState.GetValidMoves().ToArray();
         predecessor.Next = new EvaluatedMove[moves.Length];
         if (predecessor.ResultingState.IsBlacksTurn)
         {
            predecessor.BestNextMoveValue = 9999;
            for (int i = 0; i < moves.Length;  i++)
            {
               if (progress != null)
                  progress(i + 1, moves.Length);
               predecessor.Next[i] = new EvaluatedMove(moves[i], predecessor.ResultingState.Move(moves[i]), predecessor);
               Minimax(predecessor.Next[i], depth - 1, null);
               predecessor.AccumulatedMoveValues += predecessor.ResultingState.BoardValue + predecessor.Next[i].AccumulatedMoveValues;
               if (predecessor.Next[i].BestNextMoveValue < predecessor.BestNextMoveValue)
                  predecessor.BestNextMoveValue = predecessor.Next[i].BestNextMoveValue;
               if ((predecessor.BestNextMoveValue == -9999) && (predecessor.Next[i].MateDepth == 0))
                  predecessor.Next[i].MateDepth = 1;
               if (predecessor.Next[i].MateDepth > 0)
                  predecessor.MateDepth = (byte)(predecessor.Next[i].MateDepth + 1);
            }
         }
         else
         {
            predecessor.BestNextMoveValue = -9999;
            for (int i = 0; i < moves.Length; i++)
            {
               if (progress != null)
                  progress(i + 1, moves.Length);
               predecessor.Next[i] = new EvaluatedMove(moves[i], predecessor.ResultingState.Move(moves[i]), predecessor);
               Minimax(predecessor.Next[i], depth - 1, null);
               predecessor.AccumulatedMoveValues += predecessor.ResultingState.BoardValue + predecessor.Next[i].AccumulatedMoveValues;
               if (predecessor.Next[i].BestNextMoveValue > predecessor.BestNextMoveValue)
                  predecessor.BestNextMoveValue = predecessor.Next[i].BestNextMoveValue;
               if ((predecessor.BestNextMoveValue == 9999) && (predecessor.Next[i].MateDepth == 0))
                  predecessor.Next[i].MateDepth = 1;
               if (predecessor.Next[i].MateDepth > 0)
                  predecessor.MateDepth = (byte)(predecessor.Next[i].MateDepth + 1);
            }
         }
      }
   }
}
