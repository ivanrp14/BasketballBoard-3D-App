using UnityEngine;

public static class PlayInterpolator
{
    public static void LerpActor(PlayActor actor,
                                 Vector3 start,
                                 Vector3 target,
                                 float t,
                                 bool block,
                                 Transform ball)
    {
        actor.SetPosition(Vector3.Lerp(start, target, t));

        bool isMoving = Vector3.Distance(start, target) > 0.01f;
        actor.SetMoving(isMoving);
        actor.SetBlock(block);

        if (isMoving)
        {
            Vector3 dir = target - actor.transform.position;
            dir.y = 0;
            if (dir != Vector3.zero)
                actor.transform.rotation = Quaternion.LookRotation(dir);
        }
        else
        {
            actor.LookAt(ball.position);
        }
    }
}
