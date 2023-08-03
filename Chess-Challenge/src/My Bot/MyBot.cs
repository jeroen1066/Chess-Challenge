using ChessChallenge.API;
using System;
using System.Collections.Generic;
using System.Linq;

/*
The idea for the final design of this bot is to create a tree of board states, where analyses are distributed to focus on the 
most promising scenario of the board to further analyse that, but thread out of this path too so alternative opportunities can 
be discovered. The analysis function will evaluate all pieces currently on the board, as well as the strategic position of both 
sides. The bot will associate a score to all board state. After an analysis, the score of all moves leading up to that point will
be updated depending on the outcome of the analysis. 
*/

public class MyBot : IChessBot
{
    public Move Think(Board board, Timer timer)
    {   
        Move[] moves = board.GetLegalMoves();
        Random rng = new();
        Move moveToPlay = moves[rng.Next(moves.Length)];
        int currentscore = Eval(board);
        // initialise the bestscore at a very negative number, so that the evaluation of any board will update it.
        int bestscore = -99999;
        //todo implement tree of moves to allow for several steps of analysis 
        foreach (Move move in moves)
        // rudimentary function for evaluating moves. This section will later be replaced by the tree later.
        // works by evaluating all moves available to the bot currently, and then evaluating the best response 
        // by black. The best result after the move by the opponent is chosen.
        {


            int status = Evaluatemove(board,move);

            if (status > bestscore)
            {
                bestscore = status;
                moveToPlay = move;
            }
        
        }
        return moveToPlay;
    }
    static int Eval(Board board)
    //Function that evaluates the board, with positive defined as being advantageous for white. 
    {
        int[] valuelist = {100,300,300,500,900,999900,-100,-300,-300,-500,-900,-999900};
        PieceList[] pieces = board.GetAllPieceLists();
        int whitescore = 0;
        //If the board is in checkmate, then this results in a massively positive score for white
        if (board.IsInCheckmate())
        {
            whitescore = 999900;
        }
        else
        {
            for (int i =0; i < 12; i++)
            {
                PieceList currentmove = pieces[i];
                int piecenum = currentmove.Count;
                whitescore += piecenum * valuelist[i];

            }
            
        }
        return whitescore;
    }
    public static int Evaluatemove(Board boardstate, Move evalmove)
    // function to evaluate the move being played. Currently does the move by black together.
    // later this should be adapted to do only one move at a time, 
    {
        int evalvalue;
        bool white = boardstate.IsWhiteToMove;
        boardstate.MakeMove(evalmove);

        // early returns in case of a checkmate or draw. 
        if (boardstate.IsInCheckmate())
        {
            boardstate.UndoMove(evalmove);
            return 999900;
        }
        if (boardstate.IsDraw())
        {
            boardstate.UndoMove(evalmove);
            return 0;
        }
        Move[] countermoves = boardstate.GetLegalMoves();

        List<int> responsescores = new List<int>();        
        foreach (Move countermove in countermoves)
        {
            // earyly returns in case of a checkmate or draw by the response of the other side
            boardstate.MakeMove(countermove);
            if (boardstate.IsInCheckmate())
            {
                boardstate.UndoMove(countermove);
                boardstate.UndoMove(evalmove);
                return -999900;
            }
            if (boardstate.IsDraw())
            {
                boardstate.UndoMove(countermove);
                boardstate.UndoMove(evalmove);
                return 0;
            }
            responsescores.Add(Eval(boardstate));
            boardstate.UndoMove(countermove);
        }       
        boardstate.UndoMove(evalmove);
        if (white)
        {
            evalvalue = responsescores.Min();
        }
        else
        {
            evalvalue = -responsescores.Max();
        }

        return evalvalue;
    }
    class Treenode
    //class for each node in a tree containing all analysed positions
    {
        Board board; 
        //boardstate at node
        int score; 
        //evaluation of the position
        List<Treenode> children; 
        // list with all possible moves from the current position
        List<int> childrenscores; 
        // list for collecting the scores of all moves that can be played, used for updating the score of the current position
        int maxchildindex; 
        // index of the best possible move in children list
        Boolean solved; 
        // boolean to store if the current position describes an ended game, ie there either has been a draw or a checkmate

        public Treenode(Board board, Move precedingmove) 
        // only the board an score values are initialised in this stage.
        {
            this.board = board;
            this.board.MakeMove(precedingmove);
            this.score = Evaluatemove(board,precedingmove);
        }
        public void update(int updatescore)
        // Update the score of the current nodes depending on changes in lower nodes evaluation due to further analyses
        {
            score = childrenscores.Max();
            maxchildindex = childrenscores.IndexOf(score);
        }
        public void createchildren()
        // Creation of the next moves as children of the current function and storing them in the children list. 
        // The scores of the children are stored in the childrenscores list to determine if the current score of the parent node needs to change.
        // If no moves are available, this node is instead declared solved.
        {   
            Move[] legalmoves = board.GetLegalMoves();
            if (legalmoves.Length == 0)
            {
                this.solved = true;
            }
            else
            {
                foreach (Move legalmove in legalmoves)
                {        
                    children.Append(new Treenode(board,legalmove));
                    childrenscores.Append(children.Last().score);
                }
            }
        }
        public void evalchild()
        // function to determine which child is evaluated. This function will travel to where an analysis is determined to be nessecary, and then expand the analysis over all moves possible on this board. 
        // then it will determine the best move for that board, and update the score of all nodes when travelling back up the chain. This is the main function for running the analysis.
        {
            if (children != null)
            {
                children.ElementAt(maxchildindex).evalchild();
            }
            else if (!board.IsInCheckmate() & !board.IsDraw())
            {
                createchildren();
            }
            //todo handle checkmate and stalemate, make the evalchild function avoid these scenarios in future evaluation steps
            else
            {
                
            }
            //todo add in function to update the score of all nodes according to the results of the step after the analysis has been done. Probably call update to do this. 
            // if an update doesn't result in a change, then no further updating needs to be done at higher steps. 

        }


    }
}

