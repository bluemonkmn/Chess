using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BenChess
{
   public class ChessMove
   {
      private Coordinate source;
      private Coordinate target;
      private ChessPiece promotion;

      public ChessMove(Coordinate source, Coordinate target) : this(source, target, ChessPiece.Empty)
      {
      }

      public Coordinate Source
      {
         get
         {
            return source;
         }
      }

      public Coordinate Target
      {
         get
         {
            return target;
         }
      }

      public ChessPiece Promotion
      {
         get
         {
            return promotion;
         }
      }

      public ChessMove(Coordinate source, Coordinate target, ChessPiece promotion)
      {
         this.source = source;
         this.target = target;
         this.promotion = promotion;
      }

      public static char GetPieceChar(ChessPiece piece)
      {
         switch (piece & ChessPiece.PieceMask)
         {
            case ChessPiece.Empty:
               return ' ';
            case ChessPiece.Pawn:
               return 'P';
            case ChessPiece.Rook:
               return 'R';
            case ChessPiece.Knight:
               return 'N';
            case ChessPiece.Bishop:
               return 'B';
            case ChessPiece.Queen:
               return 'Q';
            case ChessPiece.King:
               return 'K';
            default:
               throw new ArgumentException("Unexpected chess piece.");
         }
      }

      public static ChessPiece GetChessPiece(char piece)
      {
         switch(char.ToUpper(piece))
         {
            case ' ':
               return ChessPiece.Empty;
            case 'P':
               return ChessPiece.Pawn;
            case 'R':
               return ChessPiece.Rook;
            case 'N':
               return ChessPiece.Knight;
            case 'B':
               return ChessPiece.Bishop;
            case 'Q':
               return ChessPiece.Queen;
            case 'K':
               return ChessPiece.King;
            default:
               throw new ArgumentException(string.Format("Invalid piece code {0}", piece));
         }
      }

      public override string ToString()
      {
         return string.Format("{0}-{1}{2}", source.ToString(), target.ToString(), promotion == ChessPiece.Empty
            ? String.Empty : new string(new char[] {GetPieceChar(promotion)}));
      }

      public ChessMove(string moveString)
      {
         string[] coords = moveString.Split('-');
         if (coords.Length != 2)
            throw new ArgumentException("ChessMove must cotnain two coordinates separated by a hyphen");
         if (coords[0].Length != 2)
            throw new ArgumentException("First component of ChessMove must be 2 characters");
         if (coords[1].Length == 2)
            promotion = ChessPiece.Empty;
         else
         {
            if (coords[1].Length != 3)
               throw new ArgumentException("Second component of ChessMove must be 2 or 3 characters");
            promotion = GetChessPiece(coords[1][2]);
            coords[1] = coords[1].Substring(0, 2);
         }
         source = new Coordinate(coords[0]);
         target = new Coordinate(coords[1]);
      }

      public static bool operator==(ChessMove a, ChessMove b)
      {
         return ((a.source.AsScalar << 16 | a.target.AsScalar) == (b.source.AsScalar << 16 | b.target.AsScalar))
            && (a.promotion == b.promotion);
      }

      public static bool operator !=(ChessMove a, ChessMove b)
      {
         return ((a.source.AsScalar << 16 | a.target.AsScalar) != (b.source.AsScalar << 16 | b.target.AsScalar))
            || (a.promotion != b.promotion);
      }

      public override bool Equals(object obj)
      {
         if (obj is ChessMove)
            return this == (ChessMove)obj;
         else
            return false;
      }

      public override int GetHashCode()
      {
         return source.GetHashCode() << 16 | target.GetHashCode() ^ promotion.GetHashCode();
      }
   }
}
