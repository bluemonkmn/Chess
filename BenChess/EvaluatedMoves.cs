using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BenChess
{
   class EvaluatedMoves : IEnumerable<EvaluatedMove>
   {
      ChessBoard priorState;
      EvaluatedMove[] moves;

      public int FinalBestValue { get; set; }

      public EvaluatedMoves(ChessBoard priorState)
      {
         List<EvaluatedMove> moves = new List<EvaluatedMove>();
         this.priorState = priorState;
         foreach (ChessMove move in priorState.GetValidMoves())
         {
            ChessBoard moveResult = priorState.Move(move.ToString());
            EvaluatedMove thisMove = new EvaluatedMove(move, moveResult, this);
            moves.Add(thisMove);
         }
         if (moves.Count == 0)
            this.moves = null;
         else
            this.moves = moves.ToArray();
      }

      public IEnumerator<EvaluatedMove> GetEnumerator()
      {
         if (moves == null)
            return null;
         return ((IEnumerable<EvaluatedMove>)moves).GetEnumerator();
      }

      System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
      {
         if (moves == null)
            return null;
         return moves.GetEnumerator();
      }

      public int Length
      {
         get
         {
            if (moves == null)
               return 0;
            return moves.Length;
         }
      }

      public int PopulateFinalBestMoveValues()
      {
         FinalBestValue = int.MinValue;
         foreach (EvaluatedMove move in moves)
         {
            int value;
            if (move.Next == null)
            {
               value = move.ResultingState.BoardValue;
            }
            else
            {
               value = move.Next.PopulateFinalBestMoveValues();
            }
            if (FinalBestValue == int.MinValue)
               FinalBestValue = value;
            else if (priorState.IsBlacksTurn)
            {
               if (value < FinalBestValue)
                  FinalBestValue = value;
            }
            else
            {
               if (value > FinalBestValue)
                  FinalBestValue = value;
            }
         }
         if (FinalBestValue == int.MinValue)
            FinalBestValue = priorState.BoardValue;
         return FinalBestValue;
      }

      public EvaluatedMove this[int index]
      {
         get
         {
            return moves[index];
         }
      }
   }
}
