using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BenChess
{
   [Flags()]
   public enum ChessPiece : byte
   {
      Empty = 0,
      Pawn = 1,
      Rook = 2,
      Knight = 3,
      Bishop = 4,
      Queen = 5,
      King = 6,
      PieceMask = 7,
      Black = 8,
      ColorMask = 8,
      BlackPawn = Pawn | Black,
      BlackRook = Rook | Black,
      BlackKnight = Knight | Black,
      BlackBishop = Bishop | Black,
      BlackQueen = Queen | Black,
      BlackKing = King | Black
   }
}
