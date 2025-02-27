using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GamePlayingAlgorithms
{
    private GameState state;
    private Heuristics heuristics;
    private int coinParityWeight;
    private int mobilityWeight;
    private int cornerWeight;
    private int stabilityWeight;

    public GamePlayingAlgorithms()
    {
        // Initialize the GamePlayingAlgorithms object with default values
        state = new GameState();
        heuristics = new Heuristics();
        coinParityWeight = 1;
        mobilityWeight = 2;
        cornerWeight = 3;
        stabilityWeight = 4;
    }

    // Perform the MiniMax algorithm to determine the best move for the current player
    public MoveInfo MiniMax(GameState state, int depth)
    {
        return Minimax(state, depth, true);
    }

    // Recursive function for MiniMax algorithm
    public MoveInfo Minimax(GameState state, int depth, bool isMaximizingPlayer)
    {
        // Base case: return the evaluated score if depth limit is reached or game is over
        if (depth <= 0 || state.LegalMoves.Count == 0)
        {
            return Evaluate(state);
        }

        // Initialization
        MoveInfo bestMove = null;
        MoveInfo lastMove = null;
        int bestScore = isMaximizingPlayer ? int.MinValue : int.MaxValue;

        // Iterate through all legal moves
        foreach (var move in state.LegalMoves.Keys)
        {
            // Create a copy of the current state and make the move
            GameState nextState = CloneState(state);
            if (nextState.MakeMove(move, out MoveInfo moveInfo))
            {
                // Recursively call Minimax on the next state
                MoveInfo opponentMove = Minimax(nextState, depth - 1, !isMaximizingPlayer);
                int score = -opponentMove.Score;

                lastMove = moveInfo;

                // Update the best move and score based on the maximizing or minimizing player
                if (isMaximizingPlayer && score > bestScore)
                {
                    bestScore = score;
                    bestMove = moveInfo;
                    bestMove.Score = score;
                }
                else if (!isMaximizingPlayer && score < bestScore)
                {
                    bestScore = score;
                    bestMove = moveInfo;
                    bestMove.Score = score;
                }
            }
        }
        if (bestMove == null)
        {
            return lastMove;
        }
        else
        {
            return bestMove;
        }

    }

    // Perform the Alpha-Beta Pruning algorithm to determine the best move for the current player
    public MoveInfo AlphaBetaPruning(GameState state, int depth)
    {
        return AlphaBetaPruning(state, depth, int.MinValue, int.MaxValue, true);
    }

    // Recursive function for Alpha-Beta Pruning algorithm
    private MoveInfo AlphaBetaPruning(GameState state, int depth, int alpha, int beta, bool maximizingPlayer)
    {
        // Base case: return the evaluated score if depth limit is reached or game is over
        if (depth <= 0 || state.LegalMoves.Count == 0)
        {
            return Evaluate(state);
        }

        // Initialization
        MoveInfo bestMove = null;
        MoveInfo lastMove = null;

        if (maximizingPlayer)
        {
            int bestScore = int.MinValue;

            // Iterate through all legal moves
            foreach (var move in state.LegalMoves.Keys)
            {
                // Create a copy of the current state and make the move
                GameState nextState = CloneState(state);
                if (nextState.MakeMove(move, out MoveInfo moveInfo))
                {
                    // Recursively call AlphaBetaPruning on the next state
                    MoveInfo opponentMove = AlphaBetaPruning(nextState, depth - 1, alpha, beta, false);
                    int score = -opponentMove.Score;
                    lastMove = moveInfo;

                    // Update the best move and score
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestMove = moveInfo;
                        bestMove.Score = score;
                    }

                    // Update alpha value
                    alpha = Math.Max(alpha, bestScore);
                    if (alpha >= beta)
                    {
                        break;
                    }
                }
            }
        }
        else
        {
            int bestScore = int.MaxValue;

            // Iterate through all legal moves
            foreach (var move in state.LegalMoves.Keys)
            {
                // Create a copy of the current state and make the move
                GameState nextState = CloneState(state);
                if (nextState.MakeMove(move, out MoveInfo moveInfo))
                {
                    // Recursively call AlphaBetaPruning on the next state
                    MoveInfo opponentMove = AlphaBetaPruning(nextState, depth - 1, alpha, beta, true);
                    int score = -opponentMove.Score;
                    lastMove = moveInfo;

                    // Update the best move and score
                    if (score < bestScore)
                    {
                        bestScore = score;
                        bestMove = moveInfo;
                        bestMove.Score = score;
                    }

                    // Update beta value
                    beta = Math.Min(beta, bestScore);
                    if (beta <= alpha)
                    {
                        break;
                    }
                }
            }
        }

        if (bestMove == null)
        {
            return lastMove;
        }
        else
        {
            return bestMove;
        }
    }

    // Perform the Alpha-Beta Pruning algorithm with iterative deepening to determine the best move for the current player
    public MoveInfo AlphaBetaPruningWithIterativeDeepening(GameState state, int maxDepth)
    {
        // Initialization
        MoveInfo bestMove = null;

        // Iteratively increase the depth and perform AlphaBetaPruning
        for (int depth = 1; depth <= maxDepth; depth++)
        {
            MoveInfo currentMove = AlphaBetaPruning(state, depth);
            if (currentMove == null)
                break;

            bestMove = currentMove;
        }

        return bestMove;
    }

    // Evaluate the given game state using the heuristics and weights
    private MoveInfo Evaluate(GameState state)
    {
		// Return the maximum score if the game is over in the final position in Board
        if (state.GameOver)
        {
            return new MoveInfo { Score = int.MaxValue };
        }

        // Calculate individual heuristic scores
        int coinParityScore = heuristics.CoinParity(state);
        int mobilityScore = heuristics.Mobility(state);
        int cornerScore = heuristics.Corner(state);
        int stabilityScore = heuristics.Stability(state);

        // Calculate the weighted score based on the heuristic scores and weights
        int weightedScore = this.coinParityWeight * coinParityScore +
                            this.mobilityWeight * mobilityScore +
                            this.cornerWeight * cornerScore +
                            this.stabilityWeight * stabilityScore;

        return new MoveInfo { Score = weightedScore };
    }

    // Create a deep copy of the given game state
    private GameState CloneState(GameState state)
    {
        // Create a deep copy of the GameState object to avoid modifying the original state.
        GameState clone = new GameState();

        clone.Board = (Player[,])state.Board.Clone();
        clone.DiscCount = new Dictionary<Player, int>(state.DiscCount);
        clone.CurrentPlayer = state.CurrentPlayer;
        clone.GameOver = state.GameOver;
        clone.Winner = state.Winner;
        clone.LegalMoves = new Dictionary<Position, List<Position>>(state.LegalMoves);
        return clone;
    }
}