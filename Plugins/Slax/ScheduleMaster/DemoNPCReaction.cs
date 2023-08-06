using System.Collections;
using UnityEngine;

namespace Slax.Schedule
{
    public class DemoNPCReaction : MonoBehaviour
    {
        public float speed = 2.0f;
        public Vector3 movementDirection = Vector3.left;

        private float objectWidth;
        private bool isMoving = false;

        private void Start()
        {
            objectWidth = transform.GetComponent<SpriteRenderer>().bounds.size.x;
        }

        private void Update()
        {
            if (isMoving)
            {
                StartCoroutine(MoveCoroutine());
            }
        }

        private IEnumerator MoveCoroutine()
        {
            Vector3 targetPosition = transform.position + movementDirection * objectWidth * 2;

            float distanceToTarget = Vector3.Distance(transform.position, targetPosition);
            float timeToReachTarget = distanceToTarget / speed;
            float elapsedTime = 0;

            while (elapsedTime < timeToReachTarget)
            {
                transform.position = Vector3.Lerp(transform.position, targetPosition, elapsedTime / timeToReachTarget);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            transform.position = targetPosition;
            isMoving = false;
        }

        public void React()
        {
            isMoving = true;
        }
    }
}
