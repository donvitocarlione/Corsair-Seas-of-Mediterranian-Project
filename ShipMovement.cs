    private void UpdateCombatMovement()
    {
        if (targetShip == null) return;

        Vector3 directionToTarget = (targetShip.transform.position - transform.position);
        float distanceToTarget = directionToTarget.magnitude;

        // Calculate ideal combat position
        Vector3 idealPosition = CalculateIdealCombatPosition();
        
        // Check if we need to reposition
        bool needsRepositioning = ShouldReposition(distanceToTarget);

        if (needsRepositioning)
        {
            // Move towards ideal position
            targetPosition = idealPosition;
            MoveTowardsTarget();
            lastRepositionTime = Time.time;
        }
        else
        {
            // Maintain position and rotate to face target
            RotateTowardsTarget(targetShip.transform.position);
        }
    }

    private Vector3 CalculateIdealCombatPosition()
    {
        if (targetShip == null) return transform.position;

        // Get the direction from target to our ship
        Vector3 directionFromTarget = (transform.position - targetShip.transform.position).normalized;
        
        // Calculate the ideal position at optimal combat distance
        Vector3 idealPosition = targetShip.transform.position + directionFromTarget * optimalCombatDistance;
        idealPosition.y = waterLevel + buoyancyOffset;

        return idealPosition;
    }

    private bool ShouldReposition(float currentDistance)
    {
        // Check if we're too close or too far from optimal distance
        bool distanceInvalid = Mathf.Abs(currentDistance - optimalCombatDistance) > repositionThreshold;
        
        // Check if we're within the firing arc
        bool inFiringArc = IsInFiringArc();

        return distanceInvalid || !inFiringArc;
    }

    private bool IsInFiringArc()
    {
        if (targetShip == null || ownShip == null) return false;

        Vector3 directionToTarget = (targetShip.transform.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, directionToTarget);

        return angle <= ownShip.FiringArc * 0.5f;
    }
    
    private void MoveTowardsTarget()
    {
        if (rb == null) return;
        
        Vector3 directionToTarget = (targetPosition - transform.position);
        directionToTarget.y = 0;
        float distanceToTarget = directionToTarget.magnitude;
        
        float currentStoppingDistance = targetShip != null ? combatStoppingDistance : stoppingDistance;

        if (distanceToTarget <= currentStoppingDistance)
        {
            if (rb.linearVelocity.magnitude < stoppingThreshold)
            {
                StopShip();
                return;
            }
        }
        
        directionToTarget.Normalize();
        RotateTowardsTarget(targetPosition);
        
        float targetSpeed = distanceToTarget > currentStoppingDistance ? 
            maxSpeed : 
            maxSpeed * (distanceToTarget / currentStoppingDistance);
        
        currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref currentVelocity.x, velocitySmoothTime);
        
        float distanceRatio = Mathf.Clamp01(distanceToTarget / currentStoppingDistance);
        Vector3 movementForce = transform.forward * currentSpeed * distanceRatio;
        
        rb.AddForce(movementForce * (1f - waterDrag * Time.fixedDeltaTime), ForceMode.Acceleration);
        
        if (Debug.isDebugBuild)
        {
            Debug.DrawLine(transform.position, targetPosition, Color.yellow);
            Debug.DrawRay(transform.position, movementForce, Color.green);
        }
    }
