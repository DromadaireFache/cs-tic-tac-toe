using System.Diagnostics;

namespace tictactoe
{
    class TicTacToe
    {
        public const int OWIN = -1;
        public const int DRAW = 0;
        public const int XWIN = 1;
        public const int CONTINUE = 2;

        static void Main(string[] args)
        {
            Console.WriteLine("****************************\n");
            Console.WriteLine("Welcome to tic tac toe in C#\n");
            Console.WriteLine("****************************");

            TicTacToe game = new();

            do
            {
                Bot bot = new(game);
                game.Reset();
                while (game.IsOver() == CONTINUE)
                {
                    int move = (bot.playsX && game.IsXtoPlay() || bot.playsO && game.IsOtoPlay()) ? bot.GetBotMove() : game.AskPlayerForMove();
                    game.PlayMove(move);
                    Console.Write(game);
                }
            } while (AnotherGame(game.IsOver()));
        }

        void Reset()
        {
            for (int i = 0; i < 9; i++)
            {
                board[i] = ' ';
            }
            turn = true;
            Console.Write(this);
        }

        readonly char[] board = {
            ' ', ' ', ' ',
            ' ', ' ', ' ',
            ' ', ' ', ' ',
        };
        bool turn = true;

        public override string ToString()
        {
            return PrintRow(board[0], board[1], board[2]) +
                    "-----------\n" +
                    PrintRow(board[3], board[4], board[5]) +
                    "-----------\n" +
                    PrintRow(board[6], board[7], board[8]) + "\n";
        }

        private static string PrintRow(char a, char b, char c)
        {
            return " " + a + " | " + b + " | " + c + " \n";
        }

        public bool IsXtoPlay()
        {
            return turn;
        }

        public bool IsOtoPlay()
        {
            return !turn;
        }

        char PlayerTurnAsChar()
        {
            return IsXtoPlay() ? 'X' : 'O';
        }

        int AskPlayerForMove()
        {
            int move = 0;
            while (true)
            {
                Console.Write($"{PlayerTurnAsChar()} to play (1-9): ");
                string userInput = Console.ReadLine() ?? string.Empty;

                if (userInput.All(char.IsDigit) && userInput.Length > 0)
                {
                    move = int.Parse(userInput);
                    if (move < 1 || 9 < move || board[move - 1] != ' ')
                        Console.WriteLine("Invalid entry");
                    else
                        break;
                }
                else Console.WriteLine("Not in specified range (1-9)");
            }
            return move - 1;
        }

        public bool PlayMove(int move)
        {
            if (board[move] != ' ')
                return false;
            board[move] = PlayerTurnAsChar();
            turn = !turn;
            return true;
        }

        public void UnMove(int move)
        {
            board[move] = ' ';
            turn = !turn;
        }

        public int IsOver()
        {
            if (CheckWin('X')) return XWIN;
            if (CheckWin('O')) return OWIN;
            if (IsBoardFull()) return DRAW;
            return CONTINUE;
        }

        bool CheckWin(char player)
        {
            return (board[0] == player && board[1] == player && board[2] == player) ||
                (board[3] == player && board[4] == player && board[5] == player) ||
                (board[6] == player && board[7] == player && board[8] == player) ||
                (board[0] == player && board[3] == player && board[6] == player) ||
                (board[1] == player && board[4] == player && board[7] == player) ||
                (board[2] == player && board[5] == player && board[8] == player) ||
                (board[0] == player && board[4] == player && board[8] == player) ||
                (board[2] == player && board[4] == player && board[6] == player);
        }

        bool IsBoardFull()
        {
            foreach (char c in board)
                if (c == ' ')
                    return false;
            return true;
        }

        static bool AnotherGame(int winner)
        {
            Console.WriteLine("\n******************************\n");
            if (winner == DRAW)
                Console.WriteLine("             DRAW             \n");
            else if (winner == XWIN)
                Console.WriteLine("            X WINS            \n");
            else if (winner == OWIN)
                Console.WriteLine("            O WINS            \n");
            Console.WriteLine("******************************\n");
            return AskPlayerForYorN("Another Game?");
        }

        public static bool AskPlayerForYorN(string msg)
        {
            while (true)
            {
                Console.Write(msg + " (Y/N): ");
                string ans = Console.ReadLine() ?? string.Empty;
                if (ans.Equals("Y", StringComparison.OrdinalIgnoreCase) || ans.Equals("YES", StringComparison.OrdinalIgnoreCase))
                    return true;
                else if (ans.Equals("N", StringComparison.OrdinalIgnoreCase) || ans.Equals("NO", StringComparison.OrdinalIgnoreCase))
                    return false;
                else
                    Console.WriteLine("Enter Y or N");
            }
        }
    }
    
    class Bot(TicTacToe game)
    {
        public readonly bool playsX = TicTacToe.AskPlayerForYorN("Bot plays X?"), playsO = TicTacToe.AskPlayerForYorN("Bot plays O?");
        readonly TicTacToe game = game;

        public int GetBotMove() {
            Stopwatch stopwatch = Stopwatch.StartNew();
            float bestEval = -1000;
            var moveEvals = new float[9];
            for (int move = 0; move < 9; move++) {
                if (game.PlayMove(move)) {
                    float eval = game.IsOtoPlay() ? minimax(1, -1000, 1000) : -minimax(1, -1000, 1000);
                    moveEvals[move] = eval;
                    bestEval = Math.Max(eval, bestEval);
                    game.UnMove(move);
                } else {
                    moveEvals[move] = -1000;
                }
            }
            stopwatch.Stop();
            Console.WriteLine($"Bot Played: Calculated in {stopwatch.Elapsed.TotalMilliseconds:F2}ms");
            
            Random rand = new();
            while (true) {
                int move = rand.Next(9);
                if (moveEvals[move] == bestEval)
                    return move;
            }
        }   

        float minimax(int depth, float alpha, float beta) {
            int gameState = game.IsOver();
            if (gameState == TicTacToe.XWIN) {
                return 999 / depth;
            } else if (gameState == TicTacToe.OWIN) {
                return -999 / depth;
            } else if (gameState == TicTacToe.DRAW) {
                return 0;
            } else if (game.IsXtoPlay()) {
                float maxEval = -1000;
                for (int move = 0; move < 9; move++) {
                    if (game.PlayMove(move)) {
                        maxEval = Math.Max(maxEval, minimax(depth + 1, alpha, beta));
                        game.UnMove(move);
                        alpha = Math.Max(alpha, maxEval);
                        if (beta <= maxEval) break;
                    }
                }
                return maxEval;
            } else {
                float minEval = 1000;
                for (int move = 0; move < 9; move++) {
                    if (game.PlayMove(move)) {
                        minEval = Math.Min(minEval, minimax(depth + 1, alpha, beta));
                        game.UnMove(move);
                        beta = Math.Min(beta, minEval);
                        if (minEval <= alpha) break;
                    }
                }
                return minEval;
            }
        }
    }

}