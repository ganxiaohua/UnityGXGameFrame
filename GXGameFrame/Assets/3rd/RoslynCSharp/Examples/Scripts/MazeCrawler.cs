using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RoslynCSharp.Example
{
    /// <summary>
    /// Represents a maze move direction in world space.
    /// </summary>
    public enum MazeDirection
    {
        /// <summary>
        /// Move up.
        /// </summary>
        Up,
        /// <summary>
        /// Move down.
        /// </summary>
        Down,
        /// <summary>
        /// Move left.
        /// </summary>
        Left,
        /// <summary>
        /// Move right.
        /// </summary>
        Right,
    }

    /// <summary>
    /// Base class for the maze crawler example.
    /// Any runtime maze crawler code should inherit from this class.
    /// </summary>
    public abstract class MazeCrawler : MonoBehaviour
    {
        // Private
        private List<GameObject> droppedBreadcrumbs = new List<GameObject>();
        private HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        private Vector2 startPosition;
        private Vector2 currentPosition;
        private Vector2 targetPosition;

        // Public
        /// <summary>
        /// The breadcrum game object that is dropped at every position.
        /// </summary>
        public GameObject breadcrumbPrefab;
        /// <summary>
        /// The move speed of the crawler move.
        /// </summary>
        public float moveSpeed = 2f;

        // Methods
        /// <summary>
        /// When overriden should return the <see cref="MazeDirection"/> that the crawler should move given the current state
        /// </summary>
        /// <param name="position">The current maze index position</param>
        /// <param name="canMoveLeft">Can the crawler move left or is there a wall in the way</param>
        /// <param name="canMoveRight">Can the crawler move right or is there a wall in the way</param>
        /// <param name="canMoveUp">Can the crawler move up or is there a wall in the way</param>
        /// <param name="canMoveDown">Can the crawler move down or is there a wall in the way</param>
        /// <returns></returns>
        public abstract MazeDirection DecideDirection(Vector2Int position, bool canMoveLeft, bool canMoveRight, bool canMoveUp, bool canMoveDown);

        /// <summary>
        /// Called by Unity.
        /// </summary>
        public void Awake()
        {
            startPosition = transform.position;
            currentPosition = transform.position;
            targetPosition = transform.position;
        }

        /// <summary>
        /// Called by Unity.
        /// </summary>
        public void Update()
        {
            if(HasArrived(targetPosition) == true)
            {
                // Update current position
                currentPosition = targetPosition;

                // Check available moves
                bool canMoveLeft = CanMoveInDirection(MazeDirection.Left);
                bool canMoveRight = CanMoveInDirection(MazeDirection.Right);
                bool canMoveUp = CanMoveInDirection(MazeDirection.Up);
                bool canMoveDown = CanMoveInDirection(MazeDirection.Down);

                // Calcualte the current index position
                Vector2Int index = new Vector2Int(Mathf.RoundToInt(currentPosition.x), Mathf.RoundToInt(currentPosition.y));

                // Add to visited
                if (visited.Contains(index) == false)
                {
                    visited.Add(index);
                    DropBreadcrumb();
                }

                try
                {
                    // Get a move decsision
                    MazeDirection moveDirection = DecideDirection(
                        index, 
                        canMoveLeft, 
                        canMoveRight, 
                        canMoveUp, 
                        canMoveDown);

                    // Validate direction
                    switch (moveDirection)
                    {
                        case MazeDirection.Up:
                            {
                                // Validate move
                                if (canMoveUp == false)
                                    throw new InvalidOperationException("Invalid decision: Cannot move up. Game will restart");

                                // Update target position
                                targetPosition = currentPosition + new Vector2(0, 1);
                                transform.localEulerAngles = new Vector3(0, 0, 0);
                                break;
                            }

                        case MazeDirection.Down:
                            {
                                // Validate move
                                if (canMoveDown == false)
                                    throw new InvalidOperationException("Invalid decision: Cannot move down. Game will restart");

                                // Update target position
                                targetPosition = currentPosition + new Vector2(0, -1);
                                transform.localEulerAngles = new Vector3(0, 0, 180);
                                break;
                            }

                        case MazeDirection.Left:
                            {
                                // Validate move
                                if (canMoveLeft == false)
                                    throw new InvalidOperationException("Invalid decision: Cannot move left. Game will restart");

                                // Update target position
                                targetPosition = currentPosition + new Vector2(-1, 0);
                                transform.localEulerAngles = new Vector3(0, 0, 90);
                                break;
                            }

                        case MazeDirection.Right:
                            {
                                // Validate move
                                if (canMoveRight == false)
                                    throw new InvalidOperationException("Invalid decision: Cannot move right. Game will restart");

                                // Update target position
                                targetPosition = currentPosition + new Vector2(1, 0);
                                transform.localEulerAngles = new Vector3(0, 0, -90);
                                break;
                            }
                    }
                }
                catch(Exception e)
                {
                    Debug.LogException(e);
                    Restart();
                }
            }
            else
            {
                // Move to target
                transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            }
        }

        /// <summary>
        /// Resets the maze crawler to its initial start position
        /// </summary>
        public void Restart()
        {
            // Destroy all breadcrumbs
            foreach (GameObject go in droppedBreadcrumbs)
                Destroy(go);

            droppedBreadcrumbs.Clear();
            visited.Clear();
            transform.position = startPosition;
            currentPosition = transform.position;
            targetPosition = transform.position;
        }

        private bool CanMoveInDirection(MazeDirection direction)
        {
            float x = 0;
            float y = 0;

            if (direction == MazeDirection.Left) x -= 1;
            if (direction == MazeDirection.Right) x += 1;
            if (direction == MazeDirection.Up) y += 1;
            if (direction == MazeDirection.Down) y -= 1;

            // Create a ray vector
            Vector2 ray = new Vector2(x, y);

            // Do a raycast
            RaycastHit2D hit = Physics2D.Raycast(transform.position, ray, 0.75f);

            if(hit.collider != null)
            {
                if(hit.collider.gameObject.name == "MazeWallFinish")
                {
                    enabled = false;
                    Debug.Log("Congratulations! Your crawler successfully escaped the maze");
                    return false;
                }

                // Check for wall collision
                if (hit.collider.gameObject.name == "MazeWall")
                    return false;
            }

            // Nothing is obstructing
            return true;
        }

        private bool HasArrived(Vector2 targetPosition)
        {
            return Vector2.Distance(targetPosition, transform.position) < 0.05f;
        }

        private void DropBreadcrumb()
        {
            // Spawn a breadcrumb
            if (breadcrumbPrefab != null)
            {
                // Create a breadcrumb
                GameObject go = Instantiate(breadcrumbPrefab, transform.position, Quaternion.identity);

                // Register dropped object
                droppedBreadcrumbs.Add(go);
            }
        }
    }
}
