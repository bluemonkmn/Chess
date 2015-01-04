using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BenChess;

namespace ChessTest
{
   class Program
   {
      static void Main(string[] args)
      {
         Play();
      }

      static bool firstProgress;

      public static void Play()
      {
         ChessBoard board;
         do
         {
            Console.Write("Board layout to start with, or enter for default: ");
            string boardKey;
            boardKey = Console.ReadLine();
            try
            {
               if (boardKey.Length > 0)
                  board = ChessBoard.FromUniqueKey(boardKey);
               else
                  board = ChessBoard.NewGame();
            }
            catch (Exception ex)
            {
               board = null;
               Console.WriteLine(ex.Message);
            }
         } while (board == null);
         Dictionary<string, List<string>> validMoves = new Dictionary<string, List<string>>();
         List<string> targetList = null;
         board.WriteToConsole();
         while (board.GetValidMoves().Any())
         {
            foreach (ChessMove move in board.GetValidMoves())
            {
               if (!validMoves.TryGetValue(move.Source.ToString(), out targetList))
               {
                  targetList = new List<string>();
                  validMoves[move.Source.ToString()] = targetList;
               }
               targetList.Add(move.ToString());
            }
            if (!board.GetValidMoves().Any())
               break;
            string source;
            string target;
            if (board.IsBlacksTurn)
            {
               if (board.IsBlackInCheck)
                  Console.WriteLine("Black is in CHECK.");
            }
            else
            {
               if (board.IsWhiteInCheck)
                  Console.WriteLine("White is in CHECK.");
            }
            Console.WriteLine("Press enter to have the computer play {0}'s turn", board.IsBlacksTurn ? "black" : "white");
            do
            {
               Console.Write("Move from coordinate ({0}): ", String.Join(",", validMoves.Keys));
               source = Console.ReadLine();
            } while ((source.Length > 0) && !validMoves.TryGetValue(source, out targetList));
            if (source.Length == 0)
            {
               firstProgress = true;
               ChessMove move = Evaluator.GetBestMove(board, 4, Progress);
               Console.WriteLine(move.ToString());
               board = board.Move(move);
               Console.WriteLine("Board layout code: {0}", board.GetUniqueKey());
               board.WriteToConsole(move.Target);
            }
            else
            {
               do
               {
                  Console.Write("Move to Coordinate or press Enter to go back ({0}): ", string.Join(",", targetList.Select(a => a.Substring(3)).ToArray()));
                  target = Console.ReadLine();
               } while ((target.Length > 0) && !targetList.Contains(source + "-" + target));
               if (target.Length == 0) continue;
               board = board.Move(new ChessMove(source + "-" + target));
               Console.WriteLine("Board layout code: {0}", board.GetUniqueKey());
               board.WriteToConsole(new Coordinate(target));
            }
            validMoves.Clear();
         }
         if (board.IsBlacksTurn)
            Console.WriteLine("Checkmate, white wins.");
         else
            Console.WriteLine("Checkmate, black wins.");
         Console.ReadLine();
      }

      static void Progress(int current, int max)
      {
         if (firstProgress)
         {
            Console.Write("[{0}]", new string(' ', max));
            Console.SetCursorPosition(1, Console.CursorTop);
         }
         Console.Write("-");
         firstProgress = false;
         if (current == max)
            Console.SetCursorPosition(Console.CursorLeft + 2, Console.CursorTop);
      }
   }
}
