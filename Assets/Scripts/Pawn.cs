using UnityEngine;
using System.Collections.Generic;

public class Pawn : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY)
    {
        int direction;
        List<Vector2Int> r = new List<Vector2Int>();
      
        if(team == 0)
        {
            direction = -1;
        }
        else
        {
            direction = 1;
        }


        
        // One in front
        // Check if the target position is empty
            if (board[currentX, currentY + direction] == null)
            {
                r.Add(new Vector2Int(currentX, currentY + direction));
            }
        

        //two in front
        
            // Check if the target position is empty
            if (board[currentX, currentY + direction] == null)
            {
                //white team
                if(team == 0 && currentY ==1 && board[currentX, currentY + (direction * 2)] == null)
                {
                    r.Add(new Vector2Int(currentX, currentY + (direction * 2)));

                }

                //black team 
                if (team == 1 && currentY == 6 && board[currentX, currentY + (direction * 2)] == null)
                {
                    r.Add(new Vector2Int(currentX, currentY + (direction * 2)));

                }

            }
            //kill move
            if(currentX != tileCountX -1)
        {
            if (board[currentX + 1, currentY + direction] != null && board[currentX + 1, currentY + direction].team != team)
            {
                r.Add(new Vector2Int(currentX + 1, currentY + direction));
            }
        }
            if(currentX != 0)
        {
            if (board[currentX - 1, currentY + direction] != null && board[currentX - 1, currentY + direction].team != team)
            {
                r.Add(new Vector2Int(currentX - 1, currentY + direction));
            }
        }

        /*
        if (targetY >= 0 && targetY < tileCountY)
        {
            if (board[currentX, targetY] == null)
            {
                r.Add(new Vector2Int(currentX, targetY));
            }

            if (currentY == 1 && team == 0 && board[currentX, targetY] == null && board[currentX, targetY + direction] == null)
            {
                r.Add(new Vector2Int(currentX, targetY + direction));
            }

            if (currentY == 6 && team == 1 && board[currentX, targetY] == null && board[currentX, targetY + direction] == null)
            {
                r.Add(new Vector2Int(currentX, targetY + direction));
            }
        }*/
        return r;
    }

}
