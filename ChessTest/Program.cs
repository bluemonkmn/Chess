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

      public static void Play()
      {
         ChessBoard board = ChessBoard.NewGame();
         Dictionary<string, List<string>> validMoves = new Dictionary<string, List<string>>();
         List<string> targetList = null;
         board.WriteToConsole();
         while (board.GetValidMoves().Length > 0)
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
            if (board.GetValidMoves().Length == 0)
               break;
            string source;
            string target;
            do
            {
               Console.Write(String.Join(",", validMoves.Keys));
               Console.Write(": ");
               source = Console.ReadLine();
            } while ((source.Length > 0) && !validMoves.TryGetValue(source, out targetList));
            if (source.Length == 0)
            {
               ChessMove move = Evaluator.GetBestMove(board, 4);
               Console.WriteLine(move.ToString());
               board = board.Move(move.ToString());
               board.WriteToConsole(move.Target);
            }
            else
            {
               do
               {
                  Console.Write(string.Join(",", targetList.Select(a => a.Substring(3)).ToArray()));
                  Console.Write(": ");
                  target = Console.ReadLine();
               } while (!targetList.Contains(source + "-" + target));
               board = board.Move(source + "-" + target);
               board.WriteToConsole(new Coordinate(target));
            }
            validMoves.Clear();
         }
         if (board.IsBlacksTurn)
            Console.WriteLine("Checkmate white");
         else
            Console.WriteLine("Checkmate black");
         Console.ReadLine();
      }

      public static void AutoPlay()
      {
         ChessBoard board = ChessBoard.NewGame();
         Console.WriteLine(board.ToString());
         ChessMove move = Evaluator.GetBestMove(board, 4);
         while (move != null)
         {
            Console.WriteLine(move.ToString());
            board = board.Move(move.ToString());
            Console.WriteLine(board.ToString());
            Console.WriteLine(string.Join(",", board.GetValidMoves().Select(m=>m.ToString()).ToArray()));
            Console.ReadLine();
            move = Evaluator.GetBestMove(board, 4);
         }
         board.WriteToConsole(move.Target);
         Console.WriteLine("Checkmate {0}", board.IsBlacksTurn ? "White" : "Black");
         Console.ReadLine();
      }
   }
}
