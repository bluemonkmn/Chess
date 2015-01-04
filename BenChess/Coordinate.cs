using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BenChess
{
   public struct Coordinate
   {
      public byte row;
      public byte col;
      public Coordinate(int row, int col)
      {
         if ((row > 7) || (row < 0) || (col > 7) || (col < 0))
            throw new ArgumentException("Row and column of coordinate must be 0-7");
         this.row = (byte)row;
         this.col = (byte)col;
      }

      public override string ToString()
      {
         return new string(new char[] {("abcdefgh"[col]), ("87654321"[row])});
      }

      public Coordinate(string shortForm)
      {
         int colInt = "abcdefgh".IndexOf(char.ToLower(shortForm[0]));
         if (colInt < 0) throw new ArgumentException("Col must be a through h.");
         int rowInt = "87654321".IndexOf(shortForm[1]);
         if (rowInt < 0) throw new ArgumentException("Row must be 1 through 8.");
         col = (byte)colInt;
         row = (byte)rowInt;
      }

      public override int GetHashCode()
      {
         return row << 8 | col;
      }

      public static bool operator==(Coordinate a, Coordinate b)
      {
         return (a.row << 8 | a.col) == (b.row << 8 | b.col);
      }

      public static bool operator !=(Coordinate a, Coordinate b)
      {
         return (a.row << 8 | a.col) != (b.row << 8 | b.col);
      }

      public int AsScalar
      {
         get
         {
            return row << 8 | col;
         }
      }
   }
}
