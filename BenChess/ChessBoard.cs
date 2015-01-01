using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BenChess
{
   public class ChessBoard
   {

      [Flags()]
      public enum BoardFlags : short
      {
         HasWhiteQueensRookMoved = 1,
         HasWhiteKingsRookMoved = 2,
         HasWhiteKingMoved = 4,
         HasBlackQueensRookMoved = 8,
         HasBlackKingsRookMoved = 16,
         HasBlackKingMoved = 32,
         WhiteInCheck = 64,
         BlackInCheck = 128,
         BlacksTurn = 256
      }

      private ChessPiece[,] board;
      private Dictionary<string, ChessBoard> moveResults = null;
      private BoardFlags flags = 0;
      // For En Passant validation, this is set to the file of
      // any pawn that advances 2 spaces in 1 move, and cleared
      // for any other move.
      private byte pawnJumpCol = 99;
      private short value = short.MinValue;
      private const string newGame = "rnbqkbnr\npppppppp\n\n\n\n\nPPPPPPPP\nRNBQKBNR";
      private const string uniqueKeyDistanceCodes = "0123456789ACDEFGHIJLMOSTUVWXYZacdefghijlmostuvwxyz";

      /* Capitals are white pieces, and lowercase are black pieces.
       * Initial layout looks like:
       * rnbqkbnr
       * pppppppp
       * 
       * 
       * 
       * 
       * PPPPPPPP
       * RNBQKBNR
       */

      public static ChessBoard Parse(string boardString)
      {
         int stringPos = 0;
         ChessBoard result = new ChessBoard();

         for (int row = 0; row < 8; row++)
         {
            for (int col = 0; col < 8; col++)
            {
               if (stringPos >= boardString.Length)
                  throw new ArgumentException(string.Format("Expected row {0}, column {1} at string index {2}, but found end of string.", row, col, stringPos));
               if (boardString[stringPos] == '\n')
               {
                  col = 7;
                  continue;
               }
               else
                  result.board[row, col] = GetChessPiece(boardString[stringPos++]);
            }
            if ((stringPos < boardString.Length) && (boardString[stringPos++] != '\n'))
               throw new ArgumentException(string.Format("Expected row {0} to be terminated by a newline", row));
         }
         return result;
      }

      public override string ToString()
      {
         StringBuilder sb = new StringBuilder(80);

         for (int row = 0; row < 8; row++)
         {
            for (int col = 0; col < 8; col++)
            {
               sb.Append(GetPieceChar(board[row, col]));
            }
            sb.Append('\n');
         }
         return sb.ToString();
      }

      private static ChessPiece GetChessPiece(char piece)
      {
         switch (piece)
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
            case 'p':
               return ChessPiece.BlackPawn;
            case 'r':
               return ChessPiece.BlackRook;
            case 'n':
               return ChessPiece.BlackKnight;
            case 'b':
               return ChessPiece.BlackBishop;
            case 'q':
               return ChessPiece.BlackQueen;
            case 'k':
               return ChessPiece.BlackKing;
            default:
               throw new ArgumentException(string.Format("Unknown piece code {0}", piece));
         }
      }

      private static char GetPieceChar(ChessPiece piece)
      {
         switch (piece)
         {
            case ChessPiece.Empty:
            case ChessPiece.Black:
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
            case ChessPiece.BlackPawn:
               return 'p';
            case ChessPiece.BlackRook:
               return 'r';
            case ChessPiece.BlackKnight:
               return 'n';
            case ChessPiece.BlackBishop:
               return 'b';
            case ChessPiece.BlackQueen:
               return 'q';
            case ChessPiece.BlackKing:
               return 'k';
            default:
               throw new ArgumentException("Invalid chess piece specified");
         }
      }

      public static ChessBoard NewGame()
      {
         return ChessBoard.Parse(newGame);
      }

      private ChessBoard()
      {
         board = new ChessPiece[8, 8];
      }

      public ChessPiece this[Coordinate coord]
      {
         get
         {
            return board[coord.row, coord.col];
         }
      }

      public ChessPiece this[int row, int col]
      {
         get
         {
            return board[row, col];
         }
      }

      public void CopyFrom(ChessBoard source)
      {
         for (int row = 0; row < 8; row++)
            for (int col = 0; col < 8; col++)
            {
               board[row, col] = source.board[row, col];
            }
         flags = source.flags;
         pawnJumpCol = source.pawnJumpCol;
      }

      public ChessBoard Clone()
      {
         ChessBoard clone = new ChessBoard();
         clone.CopyFrom(this);
         return clone;
      }

      private Coordinate[] GetMovesForPiece(Coordinate source)
      {
         List<Coordinate> candidates = new List<Coordinate>(6);
         bool isBlack = PieceColorAt(source) == PieceColor.Black;

         switch (this[source])
         {
            case ChessPiece.Empty:
               return null;
            case ChessPiece.Pawn:
               if (source.row > 0)
               {
                  AddIfNotBlocked(candidates, source, source.row - 1, source.col, isBlack, true);
                  if (source.row == 6)
                     AddIfNotBlocked(candidates, source, 4, source.col, isBlack, true);
                  if (source.col > 0)
                  {
                     if (!AddIfTargetIsOpponent(candidates, source.row - 1, source.col - 1, isBlack))
                     {
                        // En Passant
                        if ((pawnJumpCol < 8) && (pawnJumpCol == source.col - 1) && (source.row == 3))
                        {
                           candidates.Add(new Coordinate(source.row - 1, source.col - 1));
                        }
                     }
                  }
                  if (source.col < 7)
                     if (!AddIfTargetIsOpponent(candidates, source.row - 1, source.col + 1, isBlack))
                     {
                        // En Passant
                        if ((pawnJumpCol < 8) && (pawnJumpCol == source.col + 1) && (source.row == 3))
                        {
                           candidates.Add(new Coordinate(source.row - 1, source.col + 1));
                        }
                     }
               }
               break;
            case ChessPiece.BlackPawn:
               if (source.row < 7)
               {
                  AddIfNotBlocked(candidates, source, source.row + 1, source.col, isBlack, true);
                  if (source.row == 1)
                     AddIfNotBlocked(candidates, source, 3, source.col, isBlack, true);
                  if (source.col > 0)
                     if (!AddIfTargetIsOpponent(candidates, source.row + 1, source.col - 1, isBlack))
                     {
                        // En Passant
                        if ((pawnJumpCol < 8) && (pawnJumpCol == source.col - 1) && (source.row == 4))
                        {
                           candidates.Add(new Coordinate(source.row + 1, source.col - 1));
                        }
                     }
                  if (source.col < 7)
                     if (!AddIfTargetIsOpponent(candidates, source.row + 1, source.col + 1, isBlack))
                     {
                        // En Passant
                        if ((pawnJumpCol < 8) && (pawnJumpCol == source.col + 1) && (source.row == 4))
                        {
                           candidates.Add(new Coordinate(source.row + 1, source.col + 1));
                        }
                     }
               }
               break;
            case ChessPiece.Rook:
            case ChessPiece.BlackRook:
               for (int to = 0; to < 8; to++)
               {
                  AddIfNotBlocked(candidates, source, source.row, to, isBlack);
                  AddIfNotBlocked(candidates, source, to, source.col, isBlack);
               }
               break;
            case ChessPiece.Knight:
            case ChessPiece.BlackKnight:
               if ((source.row > 0) && (source.col > 1))
                  AddIfTargetIsNotSelf(candidates, source.row - 1, source.col - 2, isBlack);
               if ((source.row > 1) && (source.col > 0))
                  AddIfTargetIsNotSelf(candidates, source.row - 2, source.col - 1, isBlack);
               if ((source.row < 7) && (source.col > 1))
                  AddIfTargetIsNotSelf(candidates, source.row + 1, source.col - 2, isBlack);
               if ((source.row < 6) && (source.col > 0))
                  AddIfTargetIsNotSelf(candidates, source.row + 2, source.col - 1, isBlack);
               if ((source.row < 6) && (source.col < 7))
                  AddIfTargetIsNotSelf(candidates, source.row + 2, source.col + 1, isBlack);
               if ((source.row < 7) && (source.col < 6))
                  AddIfTargetIsNotSelf(candidates, source.row + 1, source.col + 2, isBlack);
               if ((source.row > 0) && (source.col < 6))
                  AddIfTargetIsNotSelf(candidates, source.row - 1, source.col + 2, isBlack);
               if ((source.row > 1) && (source.col < 7))
                  AddIfTargetIsNotSelf(candidates, source.row - 2, source.col + 1, isBlack);
               break;
            case ChessPiece.Bishop:
            case ChessPiece.BlackBishop:
            case ChessPiece.Queen:
            case ChessPiece.BlackQueen:
               for (int offset = 1; offset < 8; offset++)
               {
                  if ((source.row + offset < 8) && (source.col + offset < 8))
                     AddIfNotBlocked(candidates, source, source.row + offset, source.col + offset, isBlack);
                  if ((source.row + offset < 8) && (source.col - offset >= 0))
                     AddIfNotBlocked(candidates, source, source.row + offset, source.col - offset, isBlack);
                  if ((source.row - offset >= 0) && (source.col + offset < 8))
                     AddIfNotBlocked(candidates, source, source.row - offset, source.col + offset, isBlack);
                  if ((source.row - offset >= 0) && (source.col - offset >= 0))
                     AddIfNotBlocked(candidates, source, source.row - offset, source.col - offset, isBlack);
               }
               if ((this[source] & ChessPiece.PieceMask) == ChessPiece.Queen)
               {
                  for (int to = 0; to < 8; to++)
                  {
                     AddIfNotBlocked(candidates, source, source.row, to, isBlack);
                     AddIfNotBlocked(candidates, source, to, source.col, isBlack);
                  }
               }
               break;
            case ChessPiece.King:
            case ChessPiece.BlackKing:
               if ((source.row > 0) && (source.col > 0))
                  AddIfTargetIsNotSelf(candidates, source.row - 1, source.col - 1, isBlack);
               if (source.row > 0)
                  AddIfTargetIsNotSelf(candidates, source.row - 1, source.col, isBlack);
               if ((source.row > 0) && (source.col < 7))
                  AddIfTargetIsNotSelf(candidates, source.row - 1, source.col + 1, isBlack);
               if (source.col > 0)
                  AddIfTargetIsNotSelf(candidates, source.row, source.col - 1, isBlack);
               if ((source.col < 7))
                  AddIfTargetIsNotSelf(candidates, source.row, source.col + 1, isBlack);
               if ((source.row < 7) && (source.col > 0))
                  AddIfTargetIsNotSelf(candidates, source.row + 1, source.col - 1, isBlack);
               if (source.row < 7)
                  AddIfTargetIsNotSelf(candidates, source.row + 1, source.col, isBlack);
               if ((source.row < 7) && (source.col < 7))
                  AddIfTargetIsNotSelf(candidates, source.row + 1, source.col + 1, isBlack);
               if ((this[source] == ChessPiece.BlackKing) && ((flags & BoardFlags.BlackInCheck) == 0))
               {
                  if (((flags & (BoardFlags.HasBlackKingMoved | BoardFlags.HasBlackKingsRookMoved)) == 0)
                     && (this[7, 0] == ChessPiece.BlackRook))
                  {
                     AddIfNotBlocked(candidates, source, source.row, source.col + 2, isBlack);
                  }
                  if (((flags & (BoardFlags.HasBlackKingMoved | BoardFlags.HasBlackQueensRookMoved)) == 0)
                     && ((this[source.row, 1] & ChessPiece.PieceMask) == 0) && (this[0, 0] == ChessPiece.BlackRook))
                  {
                     AddIfNotBlocked(candidates, source, source.row, source.col - 2, isBlack);
                  }
               }
               if ((this[source] == ChessPiece.King) && ((flags & BoardFlags.WhiteInCheck) == 0))
               {
                  if (((flags & (BoardFlags.HasWhiteKingMoved | BoardFlags.HasWhiteKingsRookMoved)) == 0)
                     && (this[7, 7] == ChessPiece.Rook))
                  {
                     AddIfNotBlocked(candidates, source, source.row, source.col + 2, isBlack);
                  }
                  if (((flags & (BoardFlags.HasWhiteKingMoved | BoardFlags.HasWhiteQueensRookMoved)) == 0)
                     && ((this[source.row, 1] & ChessPiece.PieceMask) == 0) && (this[0, 7] == ChessPiece.Rook))
                  {
                     AddIfNotBlocked(candidates, source, source.row, source.col - 2, isBlack);
                  }
               }
               break;
         }
         return candidates.ToArray();
      }

      private bool AddIfNotBlocked(IList<Coordinate> moveList, Coordinate source, int targetRow, int targetCol, bool isBlack, bool mustBeEmpty = false)
      {
         Coordinate target = new Coordinate(targetRow, targetCol);
         if (!IsMoveBlocked(source, target, isBlack, mustBeEmpty))
         {
            moveList.Add(target);
            return true;
         }
         return false;
      }

      private bool AddIfTargetIsNotSelf(IList<Coordinate> moveList, int row, int col, bool isBlack)
      {
         Coordinate target = new Coordinate(row, col);
         if (PieceColorAt(target) != (isBlack ? PieceColor.Black : PieceColor.White))
         {
            moveList.Add(target);
            return true;
         }
         return false;
      }

      private bool AddIfTargetIsOpponent(IList<Coordinate> moveList, int row, int col, bool isBlack)
      {
         Coordinate target = new Coordinate(row, col);
         if (PieceColorAt(target) == (isBlack ? PieceColor.White : PieceColor.Black))
         {
            moveList.Add(target);
            return true;
         }
         return false;
      }

      private enum PieceColor
      {
         None,
         White,
         Black
      }

      private PieceColor PieceColorAt(Coordinate target)
      {
         if ((this[target] & ChessPiece.PieceMask) == ChessPiece.Empty)
            return PieceColor.None;
         if ((this[target] & ChessPiece.ColorMask) == ChessPiece.Black)
            return PieceColor.Black;
         return PieceColor.White;
      }

      private bool IsMoveBlocked(Coordinate source, Coordinate target, bool isBlack, bool mustBeEmpty)
      {
         if (mustBeEmpty)
         {
            if (PieceColorAt(target) != PieceColor.None)
               return true;
         }
         else
         {
            if (PieceColorAt(target) == (isBlack ? PieceColor.Black : PieceColor.White))
               return true;
         }
         int horizontalDirection = target.col > source.col ? 1 : target.col == source.col ? 0 : -1;
         int verticalDirection = target.row > source.row ? 1 : target.row == source.row ? 0 : -1;
         int distance = source.row - target.row;
         if (distance == 0)
            distance = source.col - target.col;
         if (distance < 0)
            distance = -distance;
         for (int offset = 1; offset < distance; offset++)
         {
            Coordinate step = new Coordinate(source.row + verticalDirection * offset, source.col + horizontalDirection * offset);
            if ((this[step] & ChessPiece.PieceMask) != ChessPiece.Empty)
               return true;
         }
         return false;
      }

      public ChessBoard Move(string move)
      {
         ChessBoard result;
         if (moveResults == null)
            GetValidMoves();
         if (moveResults.TryGetValue(move, out result))
         {
            return result;
         }
         throw new ArgumentException(string.Format("{0} is not a valid move.", move));
      }

      private void ApplyMove(ChessMove move)
      {
         bool isBlack = PieceColorAt(move.Source) == PieceColor.Black;
         if (IsBlacksTurn)
         {
            if (!isBlack)
               throw new InvalidOperationException("Attempted to move a white piece on black's turn");
         }
         else
         {
            if (isBlack)
               throw new InvalidOperationException("Attempted to move a black piece on white's turn");
         }

         switch (this[move.Source] & ChessPiece.PieceMask)
         {
            case ChessPiece.Pawn:
               if ((move.Source.col != move.Target.col) && ((this[move.Target] & ChessPiece.PieceMask) == ChessPiece.Empty))
               {
                  // En Passant
                  if (isBlack)
                     board[move.Target.row - 1, move.Target.col] = ChessPiece.Empty;
                  else
                     board[move.Target.row + 1, move.Target.col] = ChessPiece.Empty;
               }
               if ((move.Target.row - move.Source.row > 1) || (move.Source.row - move.Target.row > 1))
                  pawnJumpCol = move.Source.col;
               else
                  pawnJumpCol = 99;
               break;
            case ChessPiece.Rook:
               if (move.Source.col == 0)
                  flags |= BoardFlags.HasWhiteQueensRookMoved;
               else flags |= BoardFlags.HasWhiteKingsRookMoved;
               pawnJumpCol = 99;
               break;
            case ChessPiece.BlackRook:
               if (move.Source.col == 0)
                  flags |= BoardFlags.HasBlackQueensRookMoved;
               else flags |= BoardFlags.HasBlackKingsRookMoved;
               pawnJumpCol = 99;
               break;
            case ChessPiece.King:
               if (isBlack)
                  flags |= BoardFlags.HasBlackKingMoved;
               else
                  flags |= BoardFlags.HasWhiteKingMoved;
               if (move.Target.col - move.Source.col > 1)
               {
                  // Castling to king's rook
                  if (isBlack)
                     flags |= BoardFlags.HasBlackKingsRookMoved;
                  else
                     flags |= BoardFlags.HasWhiteKingsRookMoved;
                  board[move.Source.row, 5] = board[move.Source.row, 7];
                  board[move.Source.row, 7] = ChessPiece.Empty;
               }
               else if (move.Source.col - move.Target.col > 1)
               {
                  // Castling to queen's rook
                  if (isBlack)
                     flags |= BoardFlags.HasBlackQueensRookMoved;
                  else
                     flags |= BoardFlags.HasWhiteQueensRookMoved;
                  board[move.Source.row, 3] = board[move.Source.row, 0];
                  board[move.Source.row, 0] = ChessPiece.Empty;
               }
               pawnJumpCol = 99;
               break;
            case ChessPiece.Empty:
               throw new InvalidOperationException("Attempted to move from a square without a piece.");
            default:
               pawnJumpCol = 99;
               break;
         }
         if ((isBlack && (move.Target.row == 7) && (board[move.Source.row, move.Source.col] == ChessPiece.BlackPawn))
            || (!isBlack && (move.Target.row == 0) && (board[move.Source.row, move.Source.col] == ChessPiece.Pawn)))
            board[move.Target.row, move.Target.col] = move.Promotion | (isBlack ? ChessPiece.Black : 0);
         else
            board[move.Target.row, move.Target.col] = board[move.Source.row, move.Source.col];
         board[move.Source.row, move.Source.col] = ChessPiece.Empty;
         flags ^= BoardFlags.BlacksTurn;
         CheckForCheck();
      }

      private void CheckForCheck()
      {
         flags &= ~(BoardFlags.BlackInCheck | BoardFlags.WhiteInCheck);
         for (int row = 0; row < 8; row++)
            for (int col = 0; col < 8; col++)
            {
               Coordinate source = new Coordinate(row, col);
               if ((IsBlacksTurn && (PieceColorAt(source) == PieceColor.Black))
                  || (!IsBlacksTurn && (PieceColorAt(source) == PieceColor.White)))
               {
                  foreach (Coordinate target in GetMovesForPiece(source))
                     if ((this[target] & ChessPiece.PieceMask) == ChessPiece.King)
                     {
                        if (PieceColorAt(target) == PieceColor.Black)
                           flags |= BoardFlags.BlackInCheck;
                        else
                           flags |= BoardFlags.WhiteInCheck;
                     }
               }
            }
      }

      public ChessMove[] GetValidMoves()
      {
         if (moveResults != null)
            return moveResults.Keys.Select((m => new ChessMove(m))).ToArray();
         List<ChessMove> candidates = new List<ChessMove>();
         for (int row = 0; row < 8; row++)
            for (int col = 0; col < 8; col++)
            {
               Coordinate source = new Coordinate(row, col);
               if ((IsBlacksTurn && (PieceColorAt(source) == PieceColor.Black))
                  || (!IsBlacksTurn && (PieceColorAt(source) == PieceColor.White)))
               {
                  foreach (Coordinate target in GetMovesForPiece(source))
                  {
                     if (((this[source] == ChessPiece.Pawn) && (target.row == 0))
                        || ((this[source] == ChessPiece.BlackPawn) && (target.row == 7)))
                     {
                        foreach (ChessPiece promotion in new ChessPiece[] { ChessPiece.Rook, ChessPiece.Knight, ChessPiece.Bishop, ChessPiece.Queen })
                           candidates.Add(new ChessMove(source, target, promotion));
                     }
                     else
                     {
                        candidates.Add(new ChessMove(source, target));
                     }
                  }
               }
            }

         foreach (ChessMove move in candidates.ToArray())
         {
            ChessBoard testMove = Clone();
            testMove.ApplyMove(move);
            if (moveResults == null)
               moveResults = new Dictionary<string, ChessBoard>();
            if (IsBlacksTurn)
            {
               // If this move would move black into check, eliminate it.
               if ((testMove.flags & BoardFlags.BlackInCheck) != 0)
                  candidates.Remove(move);
               else
                  moveResults[move.ToString()] = testMove;
            }
            else
            {
               // If this move would move white into check, eliminate it.
               if ((testMove.flags & BoardFlags.WhiteInCheck) != 0)
                  candidates.Remove(move);
               else
                  moveResults[move.ToString()] = testMove;
            }
         }

         return candidates.ToArray();
      }

      public short BoardValue
      {
         get
         {
            if (value == short.MinValue)
            {
               // Check for checkmate first, if possible
               if ((moveResults != null) && (moveResults.Count == 0))
               {
                  if (IsBlacksTurn)
                     return 9999;
                  else
                     return -9999;
               }

               short whiteValue = 0;
               short blackValue = 0;

               for (int row = 0; row < 8; row++)
                  for (int col = 0; col < 8; col++)
                  {
                     short pieceValue = 0;
                     Coordinate c = new Coordinate(row, col);
                     switch (this[c] & ChessPiece.PieceMask)
                     {
                        case ChessPiece.Pawn:
                           pieceValue = 1;
                           break;
                        case ChessPiece.Rook:
                           pieceValue = 5;
                           break;
                        case ChessPiece.Knight:
                           pieceValue = 3;
                           break;
                        case ChessPiece.Bishop:
                           pieceValue = 3;
                           break;
                        case ChessPiece.Queen:
                           pieceValue = 9;
                           break;
                     }
                     if (PieceColorAt(c) == PieceColor.Black)
                        blackValue += pieceValue;
                     else
                        whiteValue += pieceValue;
                  }
               value = (short)(whiteValue - blackValue);
            }
            return value;
         }
      }

      public string GetUniqueKey()
      {
         int distance = 0;
         StringBuilder sb = new StringBuilder();
         for (int row = 0; row < 8; row++)
            for (int col = 0; col < 8; col++)
            {
               if ((distance >= uniqueKeyDistanceCodes.Length - 1) ||
                  ((this[row, col] & ChessPiece.PieceMask) != ChessPiece.Empty))
               {
                  if (distance > 0)
                     sb.Append(uniqueKeyDistanceCodes[distance - 1]);
                  sb.Append(GetPieceChar(this[row, col]));
                  distance = 0;
               }
               else
                  distance++;
            }
         return sb.ToString();
      }

      public static ChessBoard FromUniqueKey(string key)
      {
         int index = 0;
         int row = 0, col = 0;
         ChessBoard result = new ChessBoard();
         while (index < key.Length)
         {
            int distance = uniqueKeyDistanceCodes.IndexOf(key[index]);
            if (distance >= 0)
            {
               int newCoord = row * 8 + col + distance + 1;
               row = newCoord / 8;
               col = newCoord % 8;
               index++;
            }
            result.board[row, col] = GetChessPiece(key[index++]);
            col++;
            if (col > 7)
            {
               col = 0;
               row++;
            }
         }
         return result;
      }

      public bool IsBlacksTurn
      {
         get
         {
            return ((flags & BoardFlags.BlacksTurn) != 0);
         }
      }

      public void WriteToConsole(Coordinate highlight)
      {
         WriteToConsole(highlight, true);
      }
      
      public void WriteToConsole()
      {
         WriteToConsole(new Coordinate(), false);
      }

      private void WriteToConsole(Coordinate highlight, bool doHighlight)
      {
         Console.ForegroundColor = ConsoleColor.Yellow;
         Console.WriteLine("  abcdefgh");
         for (int row = 0; row < 8; row++)
         {
            Console.Write("{0} ", "87654321"[row]);
            Console.ResetColor();
            for (int col = 0; col < 8; col++)
            {
               if (doHighlight && (highlight.row == row) && (highlight.col == col))
               {
                  Console.BackgroundColor = ConsoleColor.DarkGreen;
                  Console.ForegroundColor = ConsoleColor.White;
               }
               else if ((col + row) % 2 == 0)
               {
                  Console.BackgroundColor = ConsoleColor.Gray;
                  Console.ForegroundColor = ConsoleColor.Black;
               }
               else
               {
                  Console.ForegroundColor = ConsoleColor.Gray;
                  Console.BackgroundColor = ConsoleColor.Black;
               }
               Console.Write(GetPieceChar(this[row, col]));
               Console.ResetColor();
            }
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(" {0}", "87654321"[row]);
         }
         Console.WriteLine("  abcdefgh");
         Console.ResetColor();
      }
   }
}
