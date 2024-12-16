    private void RotateTowardsTarget(Vector3 targetPos)
    {
        Vector3 directionToTarget = (targetPos - transform.position);
        directionToTarget.y = 0;
        directionToTarget.Normalize();

        float targetAngle = Mathf.Atan2(directionToTarget.x, directionToTarget.z) * Mathf.Rad2Deg;
        float smoothedRotation = Mathf.SmoothDampAngle(
            transform.eulerAngles.y,
            targetAngle,
            ref rotationVelocity,
            rotationSmoothTime
        );
        
        transform.rotation = Quaternion.Euler(0, smoothedRotation, 0);
    }
    
    private void StopShip()
    {
        isMoving = false;
        currentSpeed = 0f;
        currentVelocity = Vector3.zero;
        rotationVelocity = 0f;
        
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        
        Debug.Log($"[ShipMovement] {gameObject.name} reached target and stopped");
    }
    
    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;
        
        // Draw target position
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(targetPosition, 0.5f);
        
        // Draw stopping distance
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(targetPosition, targetShip != null ? combatStoppingDistance : stoppingDistance);
        
        // Draw water level
        Gizmos.color = Color.blue;
        Vector3 waterPos = transform.position;
        waterPos.y = waterLevel;
        Gizmos.DrawWireCube(waterPos, new Vector3(2f, 0.1f, 2f));

        // Draw combat-related gizmos
        if (targetShip != null)
        {
            // Draw optimal combat distance
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(targetShip.transform.position, optimalCombatDistance);

            // Draw reposition threshold
            Gizmos.color = new Color(1, 0.5f, 0, 0.5f);
            Gizmos.DrawWireSphere(targetShip.transform.position, optimalCombatDistance + repositionThreshold);
            Gizmos.DrawWireSphere(targetShip.transform.position, optimalCombatDistance - repositionThreshold);

            // Draw ideal combat position
            Vector3 idealPos = CalculateIdealCombatPosition();
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(idealPos, 1f);
            Gizmos.DrawLine(transform.position, idealPos);
        }
    }
}