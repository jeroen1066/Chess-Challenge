﻿using ChessChallenge.API;
using System;
using System.Collections.Generic;
using System.Linq;

public class MyBot : IChessBot
{
    public Move Think(Board board, Timer timer)
    {   
        Move[] moves = board.GetLegalMoves();
        Random rng = new();
        Move moveToPlay = moves[rng.Next(moves.Length)];
        int currentscore = Eval(board);
        int bestscore = -99999;
        
        Dictionary<Move[], int> movedict = new Dictionary<Move[], int> ();
        foreach (Move move in moves)
        {


            int status = Evaluatemove(board,move);

            if (status > bestscore)
            {
                bestscore = status;
                moveToPlay = move;
            }

            Move[] movelist = {move};
            
            movedict.Add(movelist,status);
            //board.UndoMove(move);
            /*

            for several layers of analysis
        for (int i = 0; i < 10; i++)
            {
            Move[] bestmoves = movedict.MaxBy(x => x.Value).Key;
            Board evalboard = board;
            foreach (Move onemove in bestmoves)
            {
                evalboard.MakeMove(onemove);
            }

            Move[] nextmoves = evalboard.GetLegalMoves();

            foreach (Move evalmove in nextmoves)
            {
                movelist = (Move[])bestmoves.Append(evalmove); 
                evalboard.MakeMove(evalmove);
                movedict.Add(movelist,Eval(evalboard));
                evalboard.UndoMove(evalmove);
            }
            foreach (Move undomove in bestmoves.Reverse())
            {
                evalboard.UndoMove(undomove);
            }
            */


            //}
            /*
            if (whitescore > bestscore)
            {
                bestscore = whitescore;
                moveToPlay = move;
                //Console.WriteLine("Improved the setting");
                //Console.WriteLine(bestscore);
            }
            */
        
        }
        return moveToPlay;
    }
    int Eval(Board board)
    //Function that evaluates the board, with advantage for white as positive scores. 
    {
        int[] valuelist = {100,300,300,500,900,999900,-100,-300,-300,-500,-900,-999900};
        PieceList[] pieces = board.GetAllPieceLists();
        int whitescore = 0;
        //If the board is in checkmate, then the 
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
    int Evaluatemove(Board boardstate, Move evalmove)
    {
        int evalvalue;
        bool white = boardstate.IsWhiteToMove;
        boardstate.MakeMove(evalmove);

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
            //Console.WriteLine(Eval(boardstate));
            //Console.WriteLine("Evaluated");
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
}