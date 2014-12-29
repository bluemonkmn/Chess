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
         ChessBoard board = ChessBoard.NewGame();
         while (true)
         {
            Console.WriteLine(board.ToString());
            Dictionary<string, List<string>> validMoves = new Dictionary<string, List<string>>();
            List<string> targetList;
            foreach (ChessMove move in board.GetValidMoves())
            {
               if (!validMoves.TryGetValue(move.Source.ToString(), out targetList))
               {
                  targetList = new List<string>();
                  validMoves[move.Source.ToString()] = targetList;
               }
               targetList.Add(move.ToString());
            }
            string source;
            string target;
            do
            {
               Console.Write(String.Join(",", validMoves.Keys));
               Console.Write(": ");
               source = Console.ReadLine();
            } while (!validMoves.TryGetValue(source, out targetList));
            do
            {
               Console.Write(string.Join(",", targetList.Select(a=>a.Substring(3)).ToArray()));
               Console.Write(": ");
               target = Console.ReadLine();
            } while (!targetList.Contains(source + "-" + target));
            board = board.Move(source + "-" + target);
         }
      }
   }
}
