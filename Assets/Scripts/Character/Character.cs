using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    public float moveSpeed;

    public bool IsMoving { get; private set; }

    CharacterAnimator animator;

    private void Awake()
    {
        animator = GetComponent<CharacterAnimator>();
    }

    // Coroutine to move the character
    public IEnumerator Move(Vector2 moveVec, Action OnMoveOver=null)
    {
        animator.MoveX = Mathf.Clamp(moveVec.x, -1f, 1f);
        animator.MoveY = Mathf.Clamp(moveVec.y, -1f, 1f);

        var targetPos = transform.position;
        targetPos.x += moveVec.x;
        targetPos.y += moveVec.y;

        if (!IsPathClear(targetPos))
            yield break;

        IsMoving = true;

        while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPos;

        IsMoving = false;

        OnMoveOver?.Invoke();
    }

    public void HandleUpdate()
    {
        animator.IsMoving = IsMoving;
    }

    private bool IsPathClear(Vector3 targetPos)
    {
        var diff = targetPos - transform.position;
        var dir = diff.normalized;

        return !Physics2D.BoxCast(transform.position + dir, new Vector2(0.2f, 0.2f), 0f, dir, diff.magnitude - 1, GameLayers.i.PlayerLayer | GameLayers.i.SolidLayer | GameLayers.i.InteractableLayer);
    }

    private bool IsWalkable(Vector3 targetPos)
    {
        if (Physics2D.OverlapCircle(targetPos, 0.2f, GameLayers.i.SolidLayer | GameLayers.i.InteractableLayer) != null)
        {
            return false;
        }

        return true;
    }

    public void LookTowards (Vector3 targetPos)
    {
        var xdiff = Mathf.Floor(targetPos.x) - Mathf.Floor(transform.position.x);
        var ydiff = Mathf.Floor(targetPos.y) - Mathf.Floor(transform.position.y);

        // Character can only look in horizontal or vertical decisions
        if (xdiff == 0 || ydiff == 0)
        {
            animator.MoveX = Mathf.Clamp(xdiff, -1f, 1f);
            animator.MoveY = Mathf.Clamp(ydiff, -1f, 1f);
        }
        else
        {
            Debug.LogError("Error in Look Towards: You can't ask the character to look diagonally.");
        }
    }

    public CharacterAnimator Animator
    {
        get => animator;
    }
}
