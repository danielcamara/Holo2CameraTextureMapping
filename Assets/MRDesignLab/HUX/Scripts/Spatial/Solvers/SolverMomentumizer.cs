//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
using UnityEngine;
using System.Collections;

namespace HUX.Spatial
{
    [RequireComponent(typeof(SolverHandler))]

	/// <summary>
	///   Momentumizer solver applies accel/velocity/friction to simulate momentum for an object being moved by other solvers/components
	/// </summary>
	public class SolverMomentumizer : Solver
	{
		[Tooltip("Friction to slow down the current velocity")]
		public float resistance = 0.99f;
		[Tooltip("Apply more resistance when going faster- applied resistance is resistance * (velocity ^ reisistanceVelPower)")]
		public float resistanceVelPower = 1.5f;
		[Tooltip("Accelerate to goal position at this rate")]
		public float accelRate = 10f;
		[Tooltip("Apply more acceleration if farther from target- applied accel is accelRate + springiness * distance")]
		public float springiness = 0;

		[Tooltip("Instantly maintain a constant depth from the view point instead of simulating Z-velocity")]
		public bool SnapZ = true;

		Vector3 m_Velocity;

	    private Transform m_HeadTransform;

        void Start()
        {
            m_HeadTransform = Veil.Instance.HeadTransform;
        }

		public override void SolverUpdate()
		{
            DoWork();
		}

		public override void SnapTo(Vector3 position, Quaternion rotation)
		{
			base.SnapTo(position, rotation);
			m_Velocity = Vector3.zero;
		}

		protected override void OnEnable()
		{
			base.OnEnable();

			m_Velocity = Vector3.zero;
		}

		void FixedUpdate()
		{
			//DoWork();
		}

		void DoWork()
		{
            // Start with SnapZ
            if (SnapZ)
			{
                // Snap the current depth to the goal depth
                var headPos = GetHeadPos();
                float goalDepth = (solverHandler.GoalPosition - headPos).magnitude;
				Vector3 currentDelta = transform.position - headPos;
				float currentDeltaLen = currentDelta.magnitude;
				if (!Mathf.Approximately(currentDeltaLen, 0))
				{
					Vector3 currentDeltaNorm = currentDelta / currentDeltaLen;
					transform.position += currentDeltaNorm * (goalDepth - currentDeltaLen);
				}
			}

			// Determine and apply accel
			Vector3 delta = solverHandler.GoalPosition - transform.position;
			float deltaLen = delta.magnitude;
			if (deltaLen > 0.01f)
			{
				Vector3 deltaNorm = delta / deltaLen;

				m_Velocity += deltaNorm * (solverHandler.DeltaTime * (accelRate + springiness * deltaLen));
			}

			// Resistance
			float velMag = m_Velocity.magnitude;
			if (!Mathf.Approximately(velMag, 0))
			{
				Vector3 velNormal = m_Velocity / velMag;
				float powFactor = velMag > 1f ? Mathf.Pow(velMag, resistanceVelPower) : velMag;
				m_Velocity -= velNormal * (powFactor * resistance * solverHandler.DeltaTime);
			}

			if (m_Velocity.sqrMagnitude < 0.001f)
			{
				m_Velocity = Vector3.zero;
			}

			// Apply vel to the solver... no wait, the actual transform
			transform.position += m_Velocity * solverHandler.DeltaTime;
		}

	    private Vector3 GetHeadPos()
	    {
	        if (m_HeadTransform == null)
	        {
	            m_HeadTransform = Veil.Instance.HeadTransform;
	            if (m_HeadTransform == null)
	            {
	                return Vector3.zero;
	            }
	        }
	        return m_HeadTransform.position;
	    }
	}
}
